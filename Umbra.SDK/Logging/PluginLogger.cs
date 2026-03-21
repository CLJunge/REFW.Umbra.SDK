using REFrameworkNET;

namespace Umbra.SDK.Logging;

/// <summary>
/// An instance-based, exception-safe logger for plugin authors.
/// </summary>
/// <remarks>
/// <para>
/// Each plugin should create and hold its own <see cref="PluginLogger"/> instance so that
/// <see cref="Prefix"/>, <see cref="PrefixFormat"/>, and <see cref="MinLevel"/> are fully
/// isolated per plugin. Because all managed plugins load into the same AppDomain, using the
/// static <see cref="Logger"/> properties for these values would cause the last plugin to load
/// to silently overwrite every earlier plugin's configuration.
/// </para>
/// <para>
/// Declare the logger as a <see langword="static"/> field on the plugin class and initialise it
/// in the entry point:
/// <code>
/// private static PluginLogger _log = new("MyPlugin");
///
/// [PluginEntryPoint]
/// public static void Load()
/// {
///     _log.Info("Loading...");
/// }
/// </code>
/// </para>
/// </remarks>
public sealed class PluginLogger
{
    /// <summary>
    /// Gets or sets an optional prefix prepended to every log message in the format
    /// determined by <see cref="PrefixFormat"/>.
    /// When <see langword="null"/> or empty, no prefix is added.
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the composite format string used to render the prefix portion of a log
    /// message. The placeholder <c>{0}</c> is substituted with <see cref="Prefix"/>.
    /// Defaults to <c>"[{0}]"</c>, producing output such as <c>[MyPlugin] message</c>.
    /// Has no effect when <see cref="Prefix"/> is <see langword="null"/> or empty.
    /// </summary>
    public string PrefixFormat { get; set; } = "[{0}]";

    /// <summary>
    /// Gets or sets the minimum <see cref="LogLevel"/> a message must meet in order to be
    /// emitted. Messages below this level are silently discarded before the REFramework API
    /// is called. Defaults to <see cref="LogLevel.Info"/> (all messages emitted).
    /// </summary>
    public LogLevel MinLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Initialises a new <see cref="PluginLogger"/> with no prefix.
    /// </summary>
    public PluginLogger() { }

    /// <summary>
    /// Initialises a new <see cref="PluginLogger"/> with the given prefix.
    /// </summary>
    /// <param name="prefix">
    /// The prefix string prepended to every message, e.g. <c>"MyPlugin"</c> produces
    /// <c>[MyPlugin] message</c> with the default <see cref="PrefixFormat"/>.
    /// </param>
    public PluginLogger(string prefix)
    {
        Prefix = prefix;
    }

    /// <summary>
    /// Logs an informational message via <see cref="API.LogInfo"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Info(string message)
    {
        if (MinLevel > LogLevel.Info) return;
        try { API.LogInfo(FormatMessage(message)); } catch { }
    }

    /// <summary>
    /// Logs a formatted informational message via <see cref="API.LogInfo"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Info(string format, params object[] args)
    {
        Info(string.Format(format, args));
    }

    /// <summary>
    /// Logs a warning message via <see cref="API.LogWarning"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Warning(string message)
    {
        if (MinLevel > LogLevel.Warning) return;
        try { API.LogWarning(FormatMessage(message)); } catch { }
    }

    /// <summary>
    /// Logs a formatted warning message via <see cref="API.LogWarning"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Warning(string format, params object[] args)
    {
        Warning(string.Format(format, args));
    }

    /// <summary>
    /// Logs an error message via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Error(string message)
    {
        if (MinLevel > LogLevel.Error) return;
        try { API.LogError(FormatMessage(message)); } catch { }
    }

    /// <summary>
    /// Logs a formatted error message via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Error(string format, params object[] args)
    {
        Error(string.Format(format, args));
    }

    /// <summary>
    /// Logs an error message accompanied by exception details — the exception type, message,
    /// and stack trace — via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">A descriptive message providing context for the exception.</param>
    public void Exception(Exception ex, string message)
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
    /// Logs a formatted error message accompanied by exception details — the exception type,
    /// message, and stack trace — via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="format">A composite format string providing context for the exception.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Exception(Exception ex, string format, params object[] args)
    {
        Exception(ex, string.Format(format, args));
    }

    /// <summary>
    /// Prepends <see cref="Prefix"/> to <paramref name="message"/> using <see cref="PrefixFormat"/>
    /// when a prefix is set; otherwise returns the message unchanged.
    /// </summary>
    /// <param name="message">The raw message to format.</param>
    /// <returns>
    /// The original <paramref name="message"/> if <see cref="Prefix"/> is <see langword="null"/>
    /// or empty; otherwise the message prefixed according to <see cref="PrefixFormat"/>.
    /// </returns>
    private string FormatMessage(string message)
    {
        if (string.IsNullOrEmpty(Prefix)) return message;
        var prefix = string.Format(PrefixFormat, Prefix);
        return $"{prefix} {message}";
    }
}
