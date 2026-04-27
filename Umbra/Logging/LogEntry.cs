namespace Umbra.Logging;

/// <summary>
/// Represents a single captured log message with its severity and timestamp.
/// </summary>
/// <param name="Level">The severity level of the log entry.</param>
/// <param name="Message">The fully formatted log message as it was emitted.</param>
/// <param name="Timestamp">The UTC time at which the entry was recorded.</param>
public readonly record struct LogEntry(LogLevel Level, string Message, DateTimeOffset Timestamp);
