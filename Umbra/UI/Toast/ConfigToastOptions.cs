namespace Umbra.UI.Toast;

/// <summary>
/// Stores optional toast-notification settings for config operations such as undo and preset save/load/delete.
/// </summary>
/// <remarks>
/// When supplied as a non-<see langword="null"/> value to a toast-capable options class
/// (such as <see cref="Umbra.Config.ConfigUndoOptions.Toast"/> or
/// <see cref="Umbra.Config.Presets.ConfigPresetOptions.Toast"/>),
/// toast notifications are enabled with the configured settings.
/// When <see langword="null"/>, toast notifications are disabled for that feature.
/// </remarks>
public sealed class ConfigToastOptions
{
    /// <summary>
    /// Gets an optional override for the toast display duration.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, the display duration defaults to <see cref="ToastQueue.DefaultDuration"/>.
    /// </remarks>
    public TimeSpan? Duration { get; init; }
}
