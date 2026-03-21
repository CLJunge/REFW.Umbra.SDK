using REFrameworkNET;

namespace Umbra.SDK.Logging;

/// <summary>
/// Provides static logging methods that forward messages to the REFramework logging API.
/// All methods are exception-safe and will silently suppress errors to avoid disrupting
/// the game process in the event the logging API is unavailable.
/// </summary>
public static class Logger
{
    /// <summary>
    /// Gets or sets an optional prefix that is prepended to every log message in the format <c>[Prefix] message</c>.
    /// When <see langword="null"/> or empty, no prefix is added.
    /// </summary>
    public static string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the minimum <see cref="LogLevel"/> a message must meet in order to be emitted.
    /// Messages below this level are silently discarded before the REFramework API is called.
    /// Defaults to <see cref="LogLevel.Info"/> (all messages emitted).
    /// </summary>
    public static LogLevel MinLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Gets or sets the composite format string used to render the prefix portion of a log message.
    /// The placeholder <c>{0}</c> is substituted with <see cref="Prefix"/>.
    /// Defaults to <c>"[{0}]"</c>, producing output such as <c>[MyPlugin] message</c>.
    /// Has no effect when <see cref="Prefix"/> is <see langword="null"/> or empty.
    /// </summary>
    public static string PrefixFormat { get; set; } = "[{0}]";

    /// <summary>
    /// Logs an informational message via <see cref="API.LogInfo"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Info(string message)
    {
        if (MinLevel > LogLevel.Info) return;
        try
        {
            API.LogInfo(FormatMessage(message));
        }
        catch { }
    }

    /// <summary>
    /// Logs a formatted informational message via <see cref="API.LogInfo"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Info(string format, params object[] args)
    {
        Info(string.Format(format, args));
    }

    /// <summary>
    /// Logs a warning message via <see cref="API.LogWarning"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Warning(string message)
    {
        if (MinLevel > LogLevel.Warning) return;
        try
        {
            API.LogWarning(FormatMessage(message));
        }
        catch { }
    }

    /// <summary>
    /// Logs a formatted warning message via <see cref="API.LogWarning"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Warning(string format, params object[] args)
    {
        Warning(string.Format(format, args));
    }

    /// <summary>
    /// Logs an error message via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Error(string message)
    {
        if (MinLevel > LogLevel.Error) return;
        try
        {
            API.LogError(FormatMessage(message));
        }
        catch { }
    }

    /// <summary>
    /// Logs a formatted error message via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Error(string format, params object[] args)
    {
        Error(string.Format(format, args));
    }

    /// <summary>
    /// Logs an error message accompanied by exception details, including the exception type,
    /// message, and stack trace, via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">A descriptive message providing context for the exception.</param>
    public static void Exception(Exception ex, string message)
    {
        if (MinLevel > LogLevel.Error) return;
        try
        {
            var logMessage = $"{FormatMessage(message)}\nException: {ex.GetType().Name}: {ex.Message}\nStack Trace:\n{ex.StackTrace}";
            API.LogError(logMessage);
        }
        catch { }
    }

    /// <summary>
    /// Logs a formatted error message accompanied by exception details, including the exception type,
    /// message, and stack trace, via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="format">A composite format string providing context for the exception.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Exception(Exception ex, string format, params object[] args)
    {
        Exception(ex, string.Format(format, args));
    }

    /// <summary>
    /// Formats a log message by prepending the <see cref="Prefix"/> when one is set.
    /// </summary>
    /// <param name="message">The raw message to format.</param>
    /// <returns>
    /// The original <paramref name="message"/> if <see cref="Prefix"/> is <see langword="null"/> or empty;
    /// otherwise, the message prefixed with <c>[Prefix] </c>.
    /// </returns>
    private static string FormatMessage(string message)
    {
        if (string.IsNullOrEmpty(Prefix)) return message;
        var prefix = string.Format(PrefixFormat, Prefix);
        return $"{prefix} {message}";
    }
}
