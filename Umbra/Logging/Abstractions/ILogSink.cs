namespace Umbra.Logging;

/// <summary>
/// Defines the low-level sink contract that receives fully formatted Umbra log messages.
/// </summary>
/// <remarks>
/// This internal seam isolates host-specific output from the public <see cref="Logger"/> and <see cref="PluginLogger"/> APIs so tests can substitute a lightweight in-memory sink without requiring the REFramework runtime host. Implementations should not throw because callers defensively suppress sink failures to avoid disrupting the plugin host.
/// </remarks>
internal interface ILogSink
{
    /// <summary>
    /// Writes an informational message.
    /// </summary>
    /// <param name="message">The fully formatted informational message to emit.</param>
    void Info(string message);

    /// <summary>
    /// Writes a warning message.
    /// </summary>
    /// <param name="message">The fully formatted warning message to emit.</param>
    void Warning(string message);

    /// <summary>
    /// Writes an error message.
    /// </summary>
    /// <param name="message">The fully formatted error message to emit.</param>
    void Error(string message);
}
