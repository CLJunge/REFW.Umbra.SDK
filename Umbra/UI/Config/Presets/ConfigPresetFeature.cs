using Umbra.Config;
using Umbra.Config.Presets;
using Umbra.Logging;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config.Presets;

/// <summary>
/// Owns the optional built-in config preset UI state for one config section.
/// </summary>
/// <remarks>
/// <para>
/// This feature keeps preset selection state, auto-save-before-switch logic, and
/// file-based import/export execution inside Umbra so plugins do not need to model
/// preset UI as part of their own config object graph.
/// </para>
/// <para>
/// The feature tracks the currently active preset name. Before switching to a different
/// preset: when no preset is currently active, the current config is saved via the
/// supplied save callback; when a preset is currently active, its values are saved
/// to the preset store first.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration type managed by the preset store.</typeparam>
internal sealed class ConfigPresetFeature<TConfig> : IDisposable
    where TConfig : class, new()
{
    private readonly ConfigPresetStore<TConfig> _presetStore;
    private readonly Action _saveConfig;
    private readonly ConfigPresetDrawer _drawer;
    private readonly IConfigTransferFilePicker _filePicker;
    private readonly string? _fallbackBrowseDirectory;
    private string? _activePresetName;
    private List<string> _cachedPresetNames = [];
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="ConfigPresetFeature{TConfig}"/>.
    /// </summary>
    /// <param name="presetStore">The preset store that manages preset persistence.</param>
    /// <param name="saveConfig">A callback that saves the current config to its main config file.</param>
    /// <param name="options">The preset options that control section label, placement, and UI behavior.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="presetStore"/>, <paramref name="saveConfig"/>, or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    internal ConfigPresetFeature(ConfigPresetStore<TConfig> presetStore, Action saveConfig, ConfigPresetOptions options)
        : this(presetStore, saveConfig, options, new ConfigPresetDrawer(), new WindowsConfigTransferFilePicker())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigPresetFeature{TConfig}"/> with explicit drawer and file picker seams.
    /// </summary>
    /// <param name="presetStore">The preset store that manages preset persistence.</param>
    /// <param name="saveConfig">A callback that saves the current config to its main config file.</param>
    /// <param name="options">The preset options that control section label, placement, and UI behavior.</param>
    /// <param name="drawer">The drawer that renders preset UI controls.</param>
    /// <param name="filePicker">The native file picker used for export/import dialogs.</param>
    /// <exception cref="ArgumentNullException">
    /// Any parameter is <see langword="null"/>.
    /// </exception>
    internal ConfigPresetFeature(
        ConfigPresetStore<TConfig> presetStore,
        Action saveConfig,
        ConfigPresetOptions options,
        ConfigPresetDrawer drawer,
        IConfigTransferFilePicker filePicker)
    {
        ArgumentNullException.ThrowIfNull(presetStore);
        ArgumentNullException.ThrowIfNull(saveConfig);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(drawer);
        ArgumentNullException.ThrowIfNull(filePicker);

        _presetStore = presetStore;
        _saveConfig = saveConfig;
        _drawer = drawer;
        _filePicker = filePicker;
        _fallbackBrowseDirectory = presetStore.PresetDirectory;
        SectionLabel = ResolveTreeNodeLabel(options.SectionLabel);
        ExpandedByDefault = options.ExpandedByDefault;
        Placement = options.Placement;
        ShowSeparatorBelowButtons = options.ShowSeparatorBelowButtons;
    }

    /// <summary>
    /// Gets the section label for the preset tree node.
    /// </summary>
    internal string? SectionLabel { get; }

    /// <summary>
    /// Gets a value indicating whether the preset section starts expanded.
    /// </summary>
    internal bool ExpandedByDefault { get; }

    /// <summary>
    /// Gets where the preset UI is rendered relative to the main config UI.
    /// </summary>
    internal ConfigPresetPlacement Placement { get; }

    /// <summary>
    /// Gets a value indicating whether a separator is shown below the preset buttons.
    /// </summary>
    internal bool ShowSeparatorBelowButtons { get; }

    /// <summary>
    /// Gets the currently active preset name, or <see langword="null"/> when no preset is active.
    /// </summary>
    internal string? ActivePresetName => _activePresetName;

    /// <summary>
    /// Draws the preset UI controls.
    /// </summary>
    internal void Draw()
    {
        if (_disposed)
            return;

        RefreshPresetNames();
        _drawer.Draw(
            _cachedPresetNames,
            _activePresetName,
            OnPresetSelected,
            OnExportClicked,
            OnImportClicked,
            ShowSeparatorBelowButtons);
    }

    /// <summary>
    /// Disposes the preset feature resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void OnPresetSelected(string presetName)
    {
        SaveBeforeSwitch();
        if (_presetStore.Load(presetName))
            _activePresetName = presetName;
    }

    private void OnExportClicked()
    {
        if (string.IsNullOrWhiteSpace(_activePresetName))
            return;

        if (!_filePicker.TryPickExportPath(null, _fallbackBrowseDirectory, out var selectedPath)
            || string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        _presetStore.ExportPreset(_activePresetName, selectedPath);
    }

    private void OnImportClicked()
    {
        if (!_filePicker.TryPickImportPath(null, _fallbackBrowseDirectory, out var selectedPath)
            || string.IsNullOrWhiteSpace(selectedPath))
        {
            return;
        }

        SaveBeforeSwitch();
        var importedName = _presetStore.ImportPreset(selectedPath);
        if (importedName is not null && _presetStore.Load(importedName))
            _activePresetName = importedName;
    }

    private void SaveBeforeSwitch()
    {
        if (_activePresetName is null)
        {
            try
            {
                _saveConfig();
                Logger.Info("ConfigPresetFeature: saved current config before switching to preset.");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, "ConfigPresetFeature: failed to save current config before switching to preset.");
            }
        }
        else
        {
            try
            {
                _presetStore.Save(_activePresetName);
                Logger.Info($"ConfigPresetFeature: saved active preset '{_activePresetName}' before switching.");
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"ConfigPresetFeature: failed to save active preset '{_activePresetName}' before switching.");
            }
        }
    }

    private void RefreshPresetNames()
    {
        _cachedPresetNames = _presetStore.List();
    }

    internal static string ResolveTreeNodeLabel(string? treeNodeLabel)
        => string.IsNullOrWhiteSpace(treeNodeLabel) ? ConfigPresetOptions.DefaultSectionLabel : treeNodeLabel;
}
