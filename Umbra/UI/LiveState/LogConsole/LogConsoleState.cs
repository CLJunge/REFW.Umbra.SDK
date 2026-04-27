using Umbra.Logging;

namespace Umbra.UI.LiveState.LogConsole;

/// <summary>
/// Live-state model for the in-game log console rendered by <see cref="LogConsoleDrawer"/>.
/// </summary>
/// <remarks>
/// <para>
/// This class holds the <see cref="LogBuffer"/> that captures log messages through
/// <see cref="Logger.WriteObserver"/> and the display settings used by the drawer to
/// filter and scroll the console output.
/// </para>
/// <para>
/// Create an instance during plugin initialization, wire it to
/// <see cref="Logger.WriteObserver"/>, and pass it to a
/// <see cref="LiveStateSection{T}"/> inside the plugin's
/// <see cref="Panel.PluginPanel"/>. On unload, set
/// <see cref="Logger.WriteObserver"/> to <see langword="null"/> to stop capturing.
/// </para>
/// </remarks>
[LiveStateSectionDrawer<LogConsoleDrawer>]
public sealed class LogConsoleState
{
    /// <summary>
    /// The default buffer capacity for captured log entries.
    /// </summary>
    public const int DefaultBufferCapacity = 512;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogConsoleState"/> class with the specified buffer capacity.
    /// </summary>
    /// <param name="bufferCapacity">Maximum number of log entries to retain.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferCapacity"/> is less than 1.</exception>
    public LogConsoleState(int bufferCapacity = DefaultBufferCapacity)
    {
        Buffer = new LogBuffer(bufferCapacity);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogConsoleState"/> class with a caller-supplied buffer.
    /// </summary>
    /// <param name="buffer">The log buffer that receives captured entries.</param>
    /// <exception cref="ArgumentNullException"><paramref name="buffer"/> is <see langword="null"/>.</exception>
    public LogConsoleState(LogBuffer buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        Buffer = buffer;
    }

    /// <summary>
    /// Gets the ring buffer that stores captured log entries.
    /// </summary>
    public LogBuffer Buffer { get; }

    /// <summary>
    /// Gets or sets the minimum severity level displayed in the console.
    /// </summary>
    /// <value>Entries below this level are hidden from the console output. The default is <see cref="LogLevel.Debug"/>.</value>
    public LogLevel MinDisplayLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Gets or sets a value indicating whether the console auto-scrolls to the newest entry each frame.
    /// </summary>
    /// <value><see langword="true"/> to keep the scroll position at the bottom; otherwise, <see langword="false"/>. The default is <see langword="true"/>.</value>
    public bool AutoScroll { get; set; } = true;
}
