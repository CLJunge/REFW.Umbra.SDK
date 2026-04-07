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
    /// The default value for <see cref="ShowToastNotifications"/>.
    /// </summary>
    public const bool DefaultShowToastNotifications = true;

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
    /// Gets a value indicating whether toast notifications are shown on preset save, load, and delete.
    /// </summary>
    public bool ShowToastNotifications { get; init; } = DefaultShowToastNotifications;
}
