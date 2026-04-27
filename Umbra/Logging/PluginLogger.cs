namespace Umbra.Logging;

/// <summary>
/// Writes plugin-scoped log messages with an optional prefix and minimum-level filter.
/// </summary>
/// <remarks>
/// <para>
/// Each plugin should create and hold its own <see cref="PluginLogger"/> instance so that <see cref="Prefix"/>, <see cref="PrefixFormat"/>, and <see cref="MinLevel"/> remain isolated per plugin. Because all managed plugins load into the same AppDomain, storing those settings on the static <see cref="Logger"/> type would allow one plugin to overwrite another plugin's logging configuration.
/// </para>
/// <para>
/// Declare the logger as a <see langword="private"/> <see langword="static"/> <see langword="readonly"/> field on the plugin class and initialize it inline so it is always available and never shared with other plugins:
/// <code>
/// private static readonly PluginLogger _log = new("MyPlugin");
///
/// [PluginEntryPoint]
/// public static void Load()
/// {
///     _log.Info("Loading...");
/// }
/// </code>
/// </para>
/// <para>
/// All instance methods also honor the global <see cref="Logger.Enabled"/> and <see cref="Logger.Suppress()"/> controls, allowing benchmarks and tests to silence all Umbra logging without mutating each plugin's <see cref="MinLevel"/>.
/// </para>
/// <para>
/// Logging methods are exception-safe. Failures from prefix formatting, message formatting, or sink dispatch are swallowed and may be observed only through <see cref="Logger.SuppressedFailureObserver"/>.
/// </para>
/// </remarks>
public sealed class PluginLogger
{
    /// <summary>
    /// Gets or sets the optional prefix prepended to each emitted message.
    /// </summary>
    /// <value>The prefix text rendered through <see cref="PrefixFormat"/>, or <see langword="null"/> to emit messages without a prefix.</value>
    public string? Prefix { get; set; }

    /// <summary>
    /// Gets or sets the composite format string used to render <see cref="Prefix"/> when a prefix is present.
    /// </summary>
    /// <value>A composite format string whose <c>{0}</c> placeholder receives <see cref="Prefix"/>. The default is <c>"[{0}]"</c>.</value>
    /// <remarks>
    /// This value is ignored when <see cref="Prefix"/> is <see langword="null"/> or empty.
    /// </remarks>
    public string PrefixFormat { get; set; } = "[{0}]";

    /// <summary>
    /// Gets or sets the minimum <see cref="LogLevel"/> a message must meet before it is emitted.
    /// </summary>
    /// <value>The per-instance threshold applied before formatting and sink dispatch. The default is <see cref="LogLevel.Info"/>.</value>
    public LogLevel MinLevel { get; set; } = LogLevel.Info;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLogger"/> class.
    /// </summary>
    public PluginLogger() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginLogger"/> class.
    /// </summary>
    /// <param name="prefix">The prefix text prepended to emitted messages.</param>
    public PluginLogger(string prefix)
    {
        Prefix = prefix;
    }

    /// <summary>
    /// Logs a debug-level message.
    /// </summary>
    /// <remarks>
    /// This method returns without writing anything when global logging is suppressed or when <see cref="MinLevel"/> is above <see cref="LogLevel.Debug"/>. Prefix-format and sink failures are swallowed.
    /// </remarks>
    /// <param name="message">The message to log.</param>
    public void Debug(string message)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Debug) return;
        try
        {
            var formatted = FormatMessage(message);
            Logger.GetLogSink().Debug(formatted);
            Logger.NotifyWriteObserver(LogLevel.Debug, formatted);
        }
        catch (Exception ex) { Logger.ReportSuppressedFailure("PluginLogger.Debug", ex); }
    }

    /// <summary>
    /// Logs a formatted debug-level message.
    /// </summary>
    /// <remarks>
    /// This overload formats the message first, then delegates to <see cref="Debug(string)"/>. If global logging is suppressed, <see cref="MinLevel"/> filters out debug messages, or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Debug(string format, params object[] args)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Debug) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception ex)
        {
            Logger.ReportSuppressedFailure("PluginLogger.Debug(format)", ex);
            return;
        }

        Debug(message);
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <remarks>
    /// This method returns without writing anything when global logging is suppressed or when <see cref="MinLevel"/> is above <see cref="LogLevel.Info"/>. Prefix-format and sink failures are swallowed.
    /// </remarks>
    /// <param name="message">The message to log.</param>
    public void Info(string message)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Info) return;
        try
        {
            var formatted = FormatMessage(message);
            Logger.GetLogSink().Info(formatted);
            Logger.NotifyWriteObserver(LogLevel.Info, formatted);
        }
        catch (Exception ex) { Logger.ReportSuppressedFailure("PluginLogger.Info", ex); }
    }

    /// <summary>
    /// Logs a formatted informational message.
    /// </summary>
    /// <remarks>
    /// This overload formats the message first, then delegates to <see cref="Info(string)"/>. If global logging is suppressed, <see cref="MinLevel"/> filters out informational messages, or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Info(string format, params object[] args)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Info) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception ex)
        {
            Logger.ReportSuppressedFailure("PluginLogger.Info(format)", ex);
            return;
        }

        Info(message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <remarks>
    /// This method returns without writing anything when global logging is suppressed or when <see cref="MinLevel"/> is above <see cref="LogLevel.Warning"/>. Prefix-format and sink failures are swallowed.
    /// </remarks>
    /// <param name="message">The message to log.</param>
    public void Warning(string message)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Warning) return;
        try
        {
            var formatted = FormatMessage(message);
            Logger.GetLogSink().Warning(formatted);
            Logger.NotifyWriteObserver(LogLevel.Warning, formatted);
        }
        catch (Exception ex) { Logger.ReportSuppressedFailure("PluginLogger.Warning", ex); }
    }

    /// <summary>
    /// Logs a formatted warning message.
    /// </summary>
    /// <remarks>
    /// This overload formats the message first, then delegates to <see cref="Warning(string)"/>. If global logging is suppressed, <see cref="MinLevel"/> filters out warning messages, or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Warning(string format, params object[] args)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Warning) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception ex)
        {
            Logger.ReportSuppressedFailure("PluginLogger.Warning(format)", ex);
            return;
        }

        Warning(message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <remarks>
    /// This method returns without writing anything when global logging is suppressed or when <see cref="MinLevel"/> is above <see cref="LogLevel.Error"/>. Prefix-format and sink failures are swallowed.
    /// </remarks>
    /// <param name="message">The message to log.</param>
    public void Error(string message)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Error) return;
        try
        {
            var formatted = FormatMessage(message);
            Logger.GetLogSink().Error(formatted);
            Logger.NotifyWriteObserver(LogLevel.Error, formatted);
        }
        catch (Exception ex) { Logger.ReportSuppressedFailure("PluginLogger.Error", ex); }
    }

    /// <summary>
    /// Logs a formatted error message.
    /// </summary>
    /// <remarks>
    /// This overload formats the message first, then delegates to <see cref="Error(string)"/>. If global logging is suppressed, <see cref="MinLevel"/> filters out error messages, or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Error(string format, params object[] args)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Error) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception ex)
        {
            Logger.ReportSuppressedFailure("PluginLogger.Error(format)", ex);
            return;
        }

        Error(message);
    }

    /// <summary>
    /// Logs an error message together with exception details.
    /// </summary>
    /// <remarks>
    /// The emitted text contains the formatted context message followed by the exception type, message, and stack trace. If global logging is suppressed, <see cref="MinLevel"/> filters out error messages, or sink or prefix formatting fails, no log is emitted.
    /// </remarks>
    /// <param name="ex">The exception to describe in the emitted log entry.</param>
    /// <param name="message">The context message to prepend to the exception details.</param>
    public void Exception(Exception ex, string message)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Error) return;
        try
        {
            var logMessage = $"{FormatMessage(message)}\nException: {ex.GetType().Name}: {ex.Message}\nStack Trace:\n{ex.StackTrace}";
            Logger.GetLogSink().Error(logMessage);
            Logger.NotifyWriteObserver(LogLevel.Error, logMessage);
        }
        catch (Exception sinkException)
        {
            Logger.ReportSuppressedFailure("PluginLogger.Exception", sinkException);
        }
    }

    /// <summary>
    /// Logs a formatted error message together with exception details.
    /// </summary>
    /// <remarks>
    /// This overload formats the context message first, then delegates to <see cref="Exception(Exception, string)"/>. If global logging is suppressed, <see cref="MinLevel"/> filters out error messages, or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
    /// </remarks>
    /// <param name="ex">The exception to describe in the emitted log entry.</param>
    /// <param name="format">A composite format string that produces the context message.</param>
    /// <param name="args">An array of objects to format.</param>
    public void Exception(Exception ex, string format, params object[] args)
    {
        if (!Logger.IsEnabled || MinLevel > LogLevel.Error) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception formatException)
        {
            Logger.ReportSuppressedFailure("PluginLogger.Exception(format)", formatException);
            return;
        }

        Exception(ex, message);
    }

    /// <summary>
    /// Prepends <see cref="Prefix"/> to <paramref name="message"/> using <see cref="PrefixFormat"/> when a prefix is configured.
    /// </summary>
    /// <param name="message">The raw message to format.</param>
    /// <returns>The original <paramref name="message"/> when <see cref="Prefix"/> is <see langword="null"/> or empty; otherwise, the prefixed message.</returns>
    private string FormatMessage(string message)
    {
        if (string.IsNullOrEmpty(Prefix)) return message;
        var prefix = string.Format(PrefixFormat, Prefix);
        return $"{prefix} {message}";
    }
}
