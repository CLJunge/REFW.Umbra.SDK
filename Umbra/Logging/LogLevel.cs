namespace Umbra.Logging;

/// <summary>
/// Defines the severity threshold used by <see cref="PluginLogger"/> to decide which messages are emitted.
/// </summary>
/// <remarks>
/// <see cref="PluginLogger.MinLevel"/> compares each write against these values before formatting and sink dispatch. <see cref="Logger"/> does not use this enum because its static API does not expose per-logger filtering.
/// </remarks>
public enum LogLevel
{
    /// <summary>
    /// Emits informational, warning, and error messages.
    /// </summary>
    Info = 0,

    /// <summary>
    /// Emits warning and error messages, while suppressing informational messages.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Emits only error messages.
    /// </summary>
    Error = 2,

    /// <summary>
    /// Suppresses all messages written through the owning <see cref="PluginLogger"/> instance.
    /// </summary>
    None = 3,
}
