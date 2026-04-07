using Umbra.UI.Toast;

namespace Umbra.Config.Presets;

/// <summary>
/// Stores optional preset-store settings for a config drawer or section.
/// </summary>
/// <remarks>
/// When supplied as a non-<see langword="null"/> value to
/// <see cref="Umbra.UI.Config.ConfigDrawerOptions.Presets"/>,
/// the preset store is created with the configured settings. When <see langword="null"/>,
/// the preset feature is disabled.
/// </remarks>
public sealed class ConfigPresetOptions
{
    /// <summary>
    /// The default file-name prefix prepended to preset names when generating preset file names.
    /// </summary>
    public const string DefaultPresetFilePrefix = "config-preset-";

    /// <summary>
    /// The default visible section label used for the built-in preset UI.
    /// </summary>
    public const string DefaultSectionLabel = "Presets";

    private readonly string _presetFilePrefix = DefaultPresetFilePrefix;
    private string? _sectionLabel;
    private bool _expandedByDefault;
    private bool _showSeparatorBelowButtons = true;

    /// <summary>
    /// Gets the file-name prefix prepended to preset names.
    /// </summary>
    /// <remarks>
    /// When set to <see langword="null"/> or whitespace, <see cref="DefaultPresetFilePrefix"/> is used.
    /// </remarks>
    public string PresetFilePrefix
    {
        get => _presetFilePrefix;
        init => _presetFilePrefix = string.IsNullOrWhiteSpace(value) ? DefaultPresetFilePrefix : value;
    }

    /// <summary>
    /// Gets an optional override for the directory where preset files are stored.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the directory containing the config store's file is used.
    /// </remarks>
    public string? PresetDirectory { get; init; }

    /// <summary>
    /// Gets the optional toast notification settings for preset operations.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, no toast notification is displayed on preset save, load, or delete.
    /// When non-<see langword="null"/>, toasts are displayed using the configured settings.
    /// </remarks>
    public ConfigToastOptions? Toast { get; init; }

    /// <summary>
    /// Gets or sets the visible section label used to wrap the built-in preset UI.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/> or whitespace, Umbra uses <see cref="DefaultSectionLabel"/>.
    /// </remarks>
    public string? SectionLabel
    {
        get => _sectionLabel;
        init => _sectionLabel = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the built-in preset section starts expanded.
    /// </summary>
    public bool ExpandedByDefault
    {
        get => _expandedByDefault;
        init => _expandedByDefault = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether a separator is shown below the built-in preset buttons.
    /// </summary>
    public bool ShowSeparatorBelowButtons
    {
        get => _showSeparatorBelowButtons;
        init => _showSeparatorBelowButtons = value;
    }
}
