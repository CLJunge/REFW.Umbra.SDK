using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config.Transfer;

/// <summary>
/// Identifies the active built-in config transfer action.
/// </summary>
internal enum ConfigTransferMode
{
    Import,
    Export
}

/// <summary>
/// Owns the optional built-in config transfer UI state for one config section.
/// </summary>
/// <remarks>
/// This feature keeps transfer-path sidecar persistence, browse workflow state, and store-backed
/// import/export execution inside Umbra so plugins do not need to model transfer UI as part of their
/// own config object graph.
/// </remarks>
internal sealed class ConfigTransferFeature : IDisposable
{
    private readonly IConfigTransferStore _store;
    private readonly ConfigStore<ConfigTransferSidecarState> _sidecarStore;
    private readonly DeferredSaveController<ConfigTransferSidecarState> _sidecarSaveController;
    private readonly ConfigTransferDrawer _drawer;
    private readonly string? _fallbackBrowseDirectory;
    private bool _disposed;

    internal ConfigTransferFeature(IConfigTransferStore store, ConfigTransferOptions options)
        : this(store, options, new ConfigTransferDrawer())
    {
    }

    internal ConfigTransferFeature(IConfigTransferStore store, ConfigTransferOptions options, ConfigTransferDrawer drawer)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(drawer);
        ObjectDisposedException.ThrowIf(store.IsDisposed, store);
        if (!store.IsLoaded)
            throw new InvalidOperationException("Built-in config transfer UI requires a loaded config store.");

        _store = store;
        _drawer = drawer;
        _drawer.StatusVisibilityTimeout = ResolveStatusVisibilityTimeout(options.StatusDisplayDuration);
        _fallbackBrowseDirectory = ResolveFallbackBrowseDirectory(store.FilePath, options.BrowseFallbackDirectory);
        var transferStateFilePath = ResolveSidecarFilePath(store.FilePath, options.ConfigFilePath);
        _sidecarStore = new ConfigStore<ConfigTransferSidecarState>(transferStateFilePath);
        var sidecarState = _sidecarStore.Load();
        _sidecarSaveController = new DeferredSaveController<ConfigTransferSidecarState>(_sidecarStore);
        ConfigFilePath = sidecarState.ConfigFilePath;
        TransferMode = sidecarState.TransferMode;
        ImportConfig = new(ImportFromPath);
        ExportConfig = new(ExportToPath);
        SectionLabel = ResolveTreeNodeLabel(options.SectionLabel);
        ExpandedByDefault = options.ExpandedByDefault;
        Placement = options.Placement;
        ShowSeparatorBelowButtons = options.ShowSeparatorBelowButtons;
    }

    public Parameter<string> ConfigFilePath { get; }

    internal Parameter<ConfigTransferMode> TransferMode { get; }

    public Parameter<Action> ImportConfig { get; }

    public Parameter<Action> ExportConfig { get; }

    internal string? SectionLabel { get; }

    internal bool ExpandedByDefault { get; }

    internal ConfigTransferPlacement Placement { get; }

    internal bool ShowSeparatorBelowButtons { get; }

    internal void Draw()
    {
        if (_disposed)
            return;

        _drawer.Draw(TransferMode, ConfigFilePath, ExecuteImportFromPath, ExecuteExportToPath, _fallbackBrowseDirectory, ShowSeparatorBelowButtons);
        _sidecarSaveController.Tick();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _sidecarSaveController.Flush();
        _sidecarSaveController.Dispose();
        _sidecarStore.Save();
        _sidecarStore.Dispose();
        _drawer.Dispose();
        GC.SuppressFinalize(this);
    }

    internal static string ResolveSidecarFilePath(string mainFilePath, string? sidecarFilePathOverride)
    {
        if (!string.IsNullOrWhiteSpace(sidecarFilePathOverride))
            return sidecarFilePathOverride;

        var directoryPath = Path.GetDirectoryName(mainFilePath);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(mainFilePath);
        var extension = Path.GetExtension(mainFilePath);
        if (string.IsNullOrWhiteSpace(extension))
            extension = ".json";

        var sidecarFileName = string.IsNullOrWhiteSpace(fileNameWithoutExtension)
            ? $"config-transfer{extension}"
            : $"{fileNameWithoutExtension}-transfer{extension}";
        return string.IsNullOrWhiteSpace(directoryPath)
            ? sidecarFileName
            : Path.Combine(directoryPath, sidecarFileName);
    }

    internal static string? ResolveFallbackBrowseDirectory(string mainFilePath, string? browseInitialDirectoryOverride)
    {
        if (!string.IsNullOrWhiteSpace(browseInitialDirectoryOverride))
            return browseInitialDirectoryOverride;

        var directoryPath = Path.GetDirectoryName(mainFilePath);
        return string.IsNullOrWhiteSpace(directoryPath) ? null : directoryPath;
    }

    internal static string ResolveTreeNodeLabel(string? treeNodeLabel)
        => string.IsNullOrWhiteSpace(treeNodeLabel) ? ConfigTransferOptions.DefaultSectionLabel : treeNodeLabel;

    internal static TimeSpan ResolveStatusVisibilityTimeout(TimeSpan statusVisibilityTimeout)
        => statusVisibilityTimeout > TimeSpan.Zero
            ? statusVisibilityTimeout
            : ConfigTransferOptions.DefaultStatusVisibilityTimeout;

    private void ImportFromPath()
        => _ = ExecuteImportFromPath();

    private bool ExecuteImportFromPath()
    {
        var filePath = ConfigFilePath.Value;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger.Warning("Config import ignored because the configured config file path is empty.");
            return false;
        }

        if (!CanImportFromPath(filePath))
            return false;

        try
        {
            var report = _store.Import(filePath);
            if (!report.Success)
            {
                Logger.Warning("Config import from '{0}' failed: {1}", filePath, report.FailureReason ?? "Unknown failure.");
                return false;
            }

            Logger.Info(
                "Imported config from '{0}'. Applied={1}, Ignored={2}, Rejected={3}, Saved={4}, Legacy={5}.",
                filePath,
                report.AppliedCount,
                report.IgnoredCount,
                report.RejectedCount,
                report.Saved,
                report.IsLegacyDocument);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "Config import from '{0}' threw unexpectedly.", filePath);
            return false;
        }
    }

    private static bool CanImportFromPath(string filePath)
    {
        if (File.Exists(filePath))
            return true;

        Logger.Warning("Config import ignored because the configured config file '{0}' does not exist.", filePath);
        return false;
    }

    private void ExportToPath()
        => _ = ExecuteExportToPath();

    private bool ExecuteExportToPath()
    {
        var filePath = ConfigFilePath.Value;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger.Warning("Config export ignored because the configured config file path is empty.");
            return false;
        }

        if (!CanExportToPath(filePath))
            return false;

        try
        {
            _store.Export(filePath);
            Logger.Info("Requested config export to '{0}'.", filePath);
            return true;
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "Config export to '{0}' threw unexpectedly.", filePath);
            return false;
        }
    }

    private static bool CanExportToPath(string filePath)
    {
        if (HasJsonExtension(filePath))
            return true;

        Logger.Warning("Config export ignored because the configured config file '{0}' does not end with .json.", filePath);
        return false;
    }

    private static bool HasJsonExtension(string filePath)
        => string.Equals(Path.GetExtension(filePath), ".json", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Persists the last-used built-in transfer path separately from the main config file.
/// </summary>
[UmbraAutoRegister]
internal sealed record ConfigTransferSidecarState
{
    /// <summary>
    /// Gets or sets the last-used config transfer action.
    /// </summary>
    [UmbraParameter]
    [UmbraDisplayName("Transfer Mode")]
    public Parameter<ConfigTransferMode> TransferMode { get; set; } = new(ConfigTransferMode.Import);

    /// <summary>
    /// Gets or sets the last-used config transfer file path.
    /// </summary>
    [UmbraParameter]
    [UmbraDisplayName("Config File")]
    [UmbraDescription("The JSON file path used by Umbra's built-in config import and export UI.")]
    [UmbraRequired]
    public Parameter<string> ConfigFilePath { get; set; } = new(string.Empty);
}
