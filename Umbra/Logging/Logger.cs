using REFrameworkNET;

namespace Umbra.Logging;

/// <summary>
/// Provides static, unconditional logging methods that forward raw messages to the REFramework
/// logging API. All methods are exception-safe and will silently suppress errors to avoid
/// disrupting the game process in the event the logging API is unavailable.
/// </summary>
/// <remarks>
/// This class carries no per-plugin configuration and forwards messages exactly as given.
/// Use <see cref="PluginLogger"/> for prefix-tagged, filterable plugin logging. Because all
/// managed plugins share the same AppDomain, static mutable configuration such as a prefix
/// or minimum log level would be silently overwritten by every plugin that loads —
/// <see cref="PluginLogger"/> solves this by keeping all configuration in a per-plugin instance.
/// </remarks>
public static class Logger
{
    /// <summary>
    /// Logs an informational message via <see cref="API.LogInfo"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Info(string message)
    {
        try { API.LogInfo(message); } catch { }
    }

    /// <summary>
    /// Logs a formatted informational message via <see cref="API.LogInfo"/>.
    /// </summary>
    /// <remarks>
    /// This overload is exception-safe: if <see cref="string.Format(string, object[])"/> throws
    /// during formatting, the exception is silently suppressed and no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Info(string format, params object[] args)
    {
        string message;
        try { message = string.Format(format, args); } catch { return; }
        Info(message);
    }

    /// <summary>
    /// Logs a warning message via <see cref="API.LogWarning"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Warning(string message)
    {
        try { API.LogWarning(message); } catch { }
    }

    /// <summary>
    /// Logs a formatted warning message via <see cref="API.LogWarning"/>.
    /// </summary>
    /// <remarks>
    /// This overload is exception-safe: if <see cref="string.Format(string, object[])"/> throws
    /// during formatting, the exception is silently suppressed and no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Warning(string format, params object[] args)
    {
        string message;
        try { message = string.Format(format, args); } catch { return; }
        Warning(message);
    }

    /// <summary>
    /// Logs an error message via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Error(string message)
    {
        try { API.LogError(message); } catch { }
    }

    /// <summary>
    /// Logs a formatted error message via <see cref="API.LogError"/>.
    /// </summary>
    /// <remarks>
    /// This overload is exception-safe: if <see cref="string.Format(string, object[])"/> throws
    /// during formatting, the exception is silently suppressed and no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Error(string format, params object[] args)
    {
        string message;
        try { message = string.Format(format, args); } catch { return; }
        Error(message);
    }

    /// <summary>
    /// Logs an error message accompanied by exception details, including the exception type,
    /// message, and stack trace, via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">A descriptive message providing context for the exception.</param>
    public static void Exception(Exception ex, string message)
    {
        try
        {
            API.LogError($"{message}\nException: {ex.GetType().Name}: {ex.Message}\nStack Trace:\n{ex.StackTrace}");
        }
        catch { }
    }

    /// <summary>
    /// Logs a formatted error message accompanied by exception details, including the exception type,
    /// message, and stack trace, via <see cref="API.LogError"/>.
    /// </summary>
    /// <remarks>
    /// This overload is exception-safe: if <see cref="string.Format(string, object[])"/> throws
    /// during formatting, the exception is silently suppressed and no log is emitted.
    /// </remarks>
    /// <param name="ex">The exception to log.</param>
    /// <param name="format">A composite format string providing context for the exception.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Exception(Exception ex, string format, params object[] args)
    {
        string message;
        try { message = string.Format(format, args); } catch { return; }
        Exception(ex, message);
    }
}
