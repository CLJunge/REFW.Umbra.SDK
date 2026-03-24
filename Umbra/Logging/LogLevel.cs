namespace Umbra.Logging;

/// <summary>
/// Defines the severity levels for log messages emitted via <see cref="PluginLogger"/>.
/// Assign <see cref="PluginLogger.MinLevel"/> to suppress messages below a chosen threshold.
/// </summary>
public enum LogLevel
{
    /// <summary>Informational messages. Lowest severity; all messages are emitted when this level is active.</summary>
    Info = 0,
    /// <summary>Warning messages indicating potential issues or unexpected but recoverable conditions.</summary>
    Warning = 1,
    /// <summary>Error messages indicating failures or unhandled exceptions.</summary>
    Error = 2,
    /// <summary>Suppresses all log output. Assign to silence the logger entirely (e.g., in release builds).</summary>
    None = 3,
}
