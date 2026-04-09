using Umbra.Config;

namespace Umbra.UI.Toast;

/// <summary>
/// Stores optional toast-notification settings for config operations such as undo and preset save/load/delete.
/// </summary>
/// <remarks>
/// When supplied as a non-<see langword="null"/> value to a toast-capable options class
/// (such as <see cref="ConfigUndoOptions.Toast"/> or
/// <see cref="Umbra.Config.Presets.ConfigPresetOptions.Toast"/>),
/// toast notifications are enabled with the configured settings.
/// When <see langword="null"/>, toast notifications are disabled for that feature.
/// </remarks>
public sealed class ConfigToastOptions
    (string pluginName)
{
    /// <summary>
    /// Gets the plugin name prepended to every toast message produced by this feature as <c>[PluginName]</c>.
    /// </summary>
    /// <remarks>
    /// Identifies the originating plugin in AppDomain-wide toast systems where multiple plugins
    /// share the same <see cref="ToastQueue"/>.
    /// </remarks>
    public string PluginName { get; } = !string.IsNullOrWhiteSpace(pluginName)
        ? pluginName
        : throw new ArgumentException("Plugin name cannot be null, empty, or whitespace.", nameof(pluginName));

    /// <summary>
    /// Gets an optional override for the toast display duration.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the display duration defaults to <see cref="ToastQueue.DefaultDuration"/>.
    /// </remarks>
    public TimeSpan? Duration { get; init; }
}
