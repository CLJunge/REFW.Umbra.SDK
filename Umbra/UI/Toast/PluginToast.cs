namespace Umbra.UI.Toast;

/// <summary>
/// Plugin-scoped toast API that wraps the global <see cref="ToastQueue"/> with a
/// consistent <c>[PluginName]</c> prefix and an optional default display duration.
/// </summary>
/// <remarks>
/// <para>
/// Follows the same per-plugin-instance pattern as <see cref="Logging.PluginLogger"/>:
/// each plugin creates its own <see cref="PluginToast"/> and uses it from any context
/// (hooks, callbacks, UI, save operations) to push formatted toast notifications.
/// </para>
/// <para>
/// Declare on the plugin class as:
/// <code>private static readonly PluginToast _toast = new("MyPlugin");</code>
/// </para>
/// </remarks>
public sealed class PluginToast
{
    private readonly TimeSpan? _defaultDuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginToast"/> class.
    /// </summary>
    /// <param name="pluginName">
    /// The plugin name prepended as <c>[PluginName]</c> to every toast message.
    /// </param>
    /// <param name="defaultDuration">
    /// An optional default display duration applied when no per-call duration is supplied.
    /// When <see langword="null"/>, <see cref="ToastQueue.DefaultDuration"/> is used.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="pluginName"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public PluginToast(string pluginName, TimeSpan? defaultDuration = null)
    {
        if (string.IsNullOrWhiteSpace(pluginName))
            throw new ArgumentException("Plugin name cannot be null, empty, or whitespace.", nameof(pluginName));

        PluginName = pluginName;
        _defaultDuration = defaultDuration;
    }

    /// <summary>
    /// Gets the plugin name prepended to every toast message produced by this instance.
    /// </summary>
    public string PluginName { get; }

    /// <summary>
    /// Gets the default display duration applied when no per-call duration is supplied.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, <see cref="ToastQueue.DefaultDuration"/> is used.
    /// </remarks>
    public TimeSpan? DefaultDuration => _defaultDuration;

    /// <summary>
    /// Pushes a toast notification with the configured plugin-name prefix.
    /// </summary>
    /// <param name="message">The notification text (without the plugin-name prefix).</param>
    /// <param name="level">The severity level. Defaults to <see cref="ToastLevel.Info"/>.</param>
    /// <param name="duration">
    /// How long the toast stays visible. When <see langword="null"/>, <see cref="DefaultDuration"/>
    /// is used (which itself falls back to <see cref="ToastQueue.DefaultDuration"/>).
    /// </param>
    public void Push(string message, ToastLevel level = ToastLevel.Info, TimeSpan? duration = null)
    {
        if (string.IsNullOrWhiteSpace(message)) return;

        ToastQueue.Push($"[{PluginName}] {message}", level, duration ?? _defaultDuration);
    }

    /// <summary>
    /// Pushes an <see cref="ToastLevel.Info"/> toast notification.
    /// </summary>
    /// <param name="message">The notification text (without the plugin-name prefix).</param>
    /// <param name="duration">Optional display duration override.</param>
    public void Info(string message, TimeSpan? duration = null)
        => Push(message, ToastLevel.Info, duration);

    /// <summary>
    /// Pushes a <see cref="ToastLevel.Success"/> toast notification.
    /// </summary>
    /// <param name="message">The notification text (without the plugin-name prefix).</param>
    /// <param name="duration">Optional display duration override.</param>
    public void Success(string message, TimeSpan? duration = null)
        => Push(message, ToastLevel.Success, duration);

    /// <summary>
    /// Pushes a <see cref="ToastLevel.Warning"/> toast notification.
    /// </summary>
    /// <param name="message">The notification text (without the plugin-name prefix).</param>
    /// <param name="duration">Optional display duration override.</param>
    public void Warning(string message, TimeSpan? duration = null)
        => Push(message, ToastLevel.Warning, duration);

    /// <summary>
    /// Pushes a <see cref="ToastLevel.Error"/> toast notification.
    /// </summary>
    /// <param name="message">The notification text (without the plugin-name prefix).</param>
    /// <param name="duration">Optional display duration override.</param>
    public void Error(string message, TimeSpan? duration = null)
        => Push(message, ToastLevel.Error, duration);
}
