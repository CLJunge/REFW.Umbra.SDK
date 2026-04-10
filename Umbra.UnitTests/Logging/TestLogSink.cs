namespace Umbra.Logging.UnitTests;

/// <summary>
/// Records log messages in memory for unit tests.
/// </summary>
/// <remarks>
/// Logger tests install this sink through <see cref="Logger.SetLogSink(ILogSink)"/> so they can
/// assert emitted messages without depending on the REFramework runtime host.
/// </remarks>
internal sealed class TestLogSink : ILogSink
{
    /// <summary>
    /// Gets the debug messages written during a test.
    /// </summary>
    public List<string> DebugMessages { get; } = [];

    /// <summary>
    /// Gets the informational messages written during a test.
    /// </summary>
    public List<string> InfoMessages { get; } = [];

    /// <summary>
    /// Gets the warning messages written during a test.
    /// </summary>
    public List<string> WarningMessages { get; } = [];

    /// <summary>
    /// Gets the error messages written during a test.
    /// </summary>
    public List<string> ErrorMessages { get; } = [];

    /// <inheritdoc/>
    public void Debug(string message) => DebugMessages.Add(message);

    /// <inheritdoc/>
    public void Info(string message) => InfoMessages.Add(message);

    /// <inheritdoc/>
    public void Warning(string message) => WarningMessages.Add(message);

    /// <inheritdoc/>
    public void Error(string message) => ErrorMessages.Add(message);
}
