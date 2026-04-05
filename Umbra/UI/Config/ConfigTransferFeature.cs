using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config;

/// <summary>
/// Owns the optional built-in config transfer UI state for one settings section.
/// </summary>
/// <remarks>
/// This feature keeps transfer-path sidecar persistence, browse workflow state, and store-backed
/// import/export execution inside Umbra so plugins do not need to model transfer UI as part of their
/// own config object graph.
/// </remarks>
internal sealed class ConfigTransferFeature : IDisposable
{
    private readonly IConfigTransferStore _store;
    private readonly SettingsStore<ConfigTransferSidecarState> _sidecarStore;
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
            throw new InvalidOperationException("Built-in config transfer UI requires a loaded settings store.");

        _store = store;
        _drawer = drawer;
        _fallbackBrowseDirectory = ResolveFallbackBrowseDirectory(store.FilePath, options.BrowseInitialDirectory);
        var sidecarFilePath = ResolveSidecarFilePath(store.FilePath, options.SidecarFilePath);
        _sidecarStore = new SettingsStore<ConfigTransferSidecarState>(sidecarFilePath);
        var sidecarState = _sidecarStore.Load();
        _sidecarSaveController = new DeferredSaveController<ConfigTransferSidecarState>(_sidecarStore);
        ConfigFilePath = sidecarState.ConfigFilePath;
        ImportConfig = new(ImportFromPath);
        ExportConfig = new(ExportToPath);
    }

    public Parameter<string> ConfigFilePath { get; }

    public Parameter<Action> ImportConfig { get; }

    public Parameter<Action> ExportConfig { get; }

    internal void Draw()
    {
        if (_disposed)
            return;

        _drawer.Draw(ConfigFilePath, ImportConfig.Value, ExportConfig.Value, _fallbackBrowseDirectory);
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

    private void ImportFromPath()
    {
        var filePath = ConfigFilePath.Value;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger.Warning("Config import ignored because the configured config file path is empty.");
            return;
        }

        try
        {
            var report = _store.Import(filePath);
            if (!report.Success)
            {
                Logger.Warning("Config import from '{0}' failed: {1}", filePath, report.FailureReason ?? "Unknown failure.");
                return;
            }

            Logger.Info(
                "Imported config from '{0}'. Applied={1}, Ignored={2}, Rejected={3}, Saved={4}, Legacy={5}.",
                filePath,
                report.AppliedCount,
                report.IgnoredCount,
                report.RejectedCount,
                report.Saved,
                report.IsLegacyDocument);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "Config import from '{0}' threw unexpectedly.", filePath);
        }
    }

    private void ExportToPath()
    {
        var filePath = ConfigFilePath.Value;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Logger.Warning("Config export ignored because the configured config file path is empty.");
            return;
        }

        try
        {
            _store.Export(filePath);
            Logger.Info("Requested config export to '{0}'.", filePath);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "Config export to '{0}' threw unexpectedly.", filePath);
        }
    }
}

/// <summary>
/// Persists the last-used built-in transfer path separately from the main settings file.
/// </summary>
[UmbraAutoRegister]
internal sealed record ConfigTransferSidecarState
{
    /// <summary>
    /// Gets or sets the last-used config transfer file path.
    /// </summary>
    [UmbraParameter]
    [UmbraDisplayName("Config File")]
    [UmbraDescription("The JSON file path used by Umbra's built-in config import and export UI.")]
    [UmbraRequired]
    public Parameter<string> ConfigFilePath { get; set; } = new(string.Empty);
}
