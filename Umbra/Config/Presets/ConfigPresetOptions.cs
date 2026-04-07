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

    private readonly string _presetFilePrefix = DefaultPresetFilePrefix;

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
}
