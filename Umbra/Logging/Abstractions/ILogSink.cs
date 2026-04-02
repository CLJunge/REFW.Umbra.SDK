namespace Umbra.Logging;

/// <summary>
/// Represents the low-level sink that receives fully formatted Umbra log messages.
/// </summary>
/// <remarks>
/// <para>
/// This internal seam isolates host-specific output from the public <see cref="Logger"/> and
/// <see cref="PluginLogger"/> APIs so unit tests can substitute a lightweight in-memory sink
/// without requiring the REFramework runtime host.
/// </para>
/// <para>
/// Implementations should not throw. Callers remain exception-safe and defensively suppress sink
/// failures to avoid disrupting the plugin host.
/// </para>
/// </remarks>
internal interface ILogSink
{
    /// <summary>
    /// Writes an informational message.
    /// </summary>
    /// <param name="message">The fully formatted message to emit.</param>
    void Info(string message);

    /// <summary>
    /// Writes a warning message.
    /// </summary>
    /// <param name="message">The fully formatted message to emit.</param>
    void Warning(string message);

    /// <summary>
    /// Writes an error message.
    /// </summary>
    /// <param name="message">The fully formatted message to emit.</param>
    void Error(string message);
}
