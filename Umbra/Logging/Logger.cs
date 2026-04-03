namespace Umbra.Logging;

/// <summary>
/// Provides static, unconditional logging methods for SDK-internal code and a process-wide
/// suppression switch for all Umbra logging.
/// </summary>
/// <remarks>
/// <para>
/// This class carries no per-plugin prefix or minimum-level configuration and forwards messages
/// exactly as given. Plugin code should use <see cref="PluginLogger"/> for prefix-tagged,
/// filterable logging.
/// </para>
/// <para>
/// Because all managed plugins share the same AppDomain, mutable per-plugin configuration such as
/// a prefix or minimum log level must not live on this static type. The only shared state exposed
/// here is the coarse-grained global enable/suppress switch used to silence all Umbra logging
/// during benchmarks, tests, or other measurement-sensitive runs.
/// </para>
/// <para>
/// Low-level sink replacement and lazy default-sink creation are delegated to
/// <see cref="LoggerSinkRegistry"/> so this type can remain focused on enablement and write dispatch.
/// </para>
/// <para>
/// Both <see cref="Logger"/> and <see cref="PluginLogger"/> honor <see cref="Enabled"/> and
/// <see cref="Suppress"/>, so callers can disable all SDK and plugin-prefixed output through one
/// process-wide switch.
/// </para>
/// </remarks>
public static class Logger
{
    private static int _enabled = 1;
    private static int _suppressionDepth;

    [ThreadStatic]
    private static bool _reportingFailure;

    /// <summary>
    /// Gets or sets an optional observer that receives exceptions suppressed internally by
    /// <see cref="Logger"/> and <see cref="PluginLogger"/>.
    /// </summary>
    /// <remarks>
    /// This hook is intended for opt-in diagnostics in tests, benchmarks, or advanced debugging
    /// sessions where callers want visibility into failures that Umbra normally swallows to protect
    /// the game process. The first argument identifies the write path that failed, such as
    /// <c>"Logger.Info"</c> or <c>"PluginLogger.Error(format)"</c>. Exceptions thrown by the
    /// observer itself are swallowed as well.
    /// </remarks>
    public static Action<string, Exception>? SuppressedFailureObserver { get; set; }

    /// <summary>
    /// Gets or sets whether Umbra logging is globally enabled.
    /// </summary>
    /// <remarks>
    /// This is a coarse process-wide switch. Setting it to <see langword="false"/> disables all
    /// output from both <see cref="Logger"/> and <see cref="PluginLogger"/> until re-enabled.
    /// Prefer <see cref="Suppress"/> when temporary scoped suppression is desired.
    /// </remarks>
    public static bool Enabled
    {
        get => Volatile.Read(ref _enabled) != 0;
        set => Interlocked.Exchange(ref _enabled, value ? 1 : 0);
    }

    /// <summary>
    /// Gets whether Umbra logging is currently effective after combining <see cref="Enabled"/>
    /// with any active <see cref="Suppress"/> scopes.
    /// </summary>
    public static bool IsEnabled => Enabled && Volatile.Read(ref _suppressionDepth) == 0;

    /// <summary>
    /// Replaces the low-level log sink used by <see cref="Logger"/> and <see cref="PluginLogger"/>.
    /// </summary>
    /// <remarks>
    /// This is intended for tests and other controlled environments that need Umbra logging to
    /// remain executable without the REFramework runtime host. Passing a replacement sink does not
    /// affect the public logging API or the global enable/suppression rules.
    /// </remarks>
    /// <param name="sink">The sink that should receive future log writes.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="sink"/> is <see langword="null"/>.</exception>
    internal static void SetLogSink(ILogSink sink) => LoggerSinkRegistry.Set(sink);

    /// <summary>
    /// Restores the default REFramework-backed log sink.
    /// </summary>
    /// <remarks>
    /// The next enabled write recreates the default sink lazily so disabled paths remain free from
    /// host-specific logging calls.
    /// </remarks>
    internal static void ResetLogSink() => LoggerSinkRegistry.Reset();

    /// <summary>
    /// Enables all Umbra logging.
    /// </summary>
    public static void EnableAll() => Enabled = true;

    /// <summary>
    /// Disables all Umbra logging.
    /// </summary>
    public static void DisableAll() => Enabled = false;

    /// <summary>
    /// Temporarily suppresses all Umbra logging until the returned scope is disposed.
    /// </summary>
    /// <returns>
    /// A disposable suppression scope. Disposing it removes one active suppression layer.
    /// </returns>
    public static IDisposable Suppress()
    {
        Interlocked.Increment(ref _suppressionDepth);
        return new SuppressionScope();
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <remarks>
    /// The disabled path is host-independent: when <see cref="IsEnabled"/> is <see langword="false"/>,
    /// this method returns before resolving or invoking the underlying sink so benchmarks and tests
    /// can suppress Umbra logging without requiring an active REFramework runtime host.
    /// </remarks>
    /// <param name="message">The message to log.</param>
    public static void Info(string message)
    {
        if (!IsEnabled) return;
        try { GetLogSink().Info(message); }
        catch (Exception ex) { ReportSuppressedFailure("Logger.Info", ex); }
    }

    /// <summary>
    /// Logs a formatted informational message via <see cref="REFrameworkNET.API.LogInfo"/>.
    /// </summary>
    /// <remarks>
    /// This overload is exception-safe: if logging is globally disabled, or if
    /// <see cref="string.Format(string, object[])"/> throws during formatting, the exception is
    /// silently suppressed and no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Info(string format, params object[] args)
    {
        if (!IsEnabled) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception ex)
        {
            ReportSuppressedFailure("Logger.Info(format)", ex);
            return;
        }

        Info(message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Warning(string message)
    {
        if (!IsEnabled) return;
        try { GetLogSink().Warning(message); }
        catch (Exception ex) { ReportSuppressedFailure("Logger.Warning", ex); }
    }

    /// <summary>
    /// Logs a formatted warning message via <see cref="REFrameworkNET.API.LogWarning"/>.
    /// </summary>
    /// <remarks>
    /// This overload is exception-safe: if logging is globally disabled, or if
    /// <see cref="string.Format(string, object[])"/> throws during formatting, the exception is
    /// silently suppressed and no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Warning(string format, params object[] args)
    {
        if (!IsEnabled) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception ex)
        {
            ReportSuppressedFailure("Logger.Warning(format)", ex);
            return;
        }

        Warning(message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Error(string message)
    {
        if (!IsEnabled) return;
        try { GetLogSink().Error(message); }
        catch (Exception ex) { ReportSuppressedFailure("Logger.Error", ex); }
    }

    /// <summary>
    /// Logs a formatted error message via <see cref="REFrameworkNET.API.LogError"/>.
    /// </summary>
    /// <remarks>
    /// This overload is exception-safe: if logging is globally disabled, or if
    /// <see cref="string.Format(string, object[])"/> throws during formatting, the exception is
    /// silently suppressed and no log is emitted.
    /// </remarks>
    /// <param name="format">A composite format string.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Error(string format, params object[] args)
    {
        if (!IsEnabled) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception ex)
        {
            ReportSuppressedFailure("Logger.Error(format)", ex);
            return;
        }

        Error(message);
    }

    /// <summary>
    /// Logs an error message accompanied by exception details, including the exception type,
    /// message, and stack trace.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">A descriptive message providing context for the exception.</param>
    public static void Exception(Exception ex, string message)
    {
        if (!IsEnabled) return;
        try
        {
            GetLogSink().Error($"{message}\nException: {ex.GetType().Name}: {ex.Message}\nStack Trace:\n{ex.StackTrace}");
        }
        catch (Exception sinkException)
        {
            ReportSuppressedFailure("Logger.Exception", sinkException);
        }
    }

    /// <summary>
    /// Logs a formatted error message accompanied by exception details, including the exception
    /// type, message, and stack trace, via <see cref="REFrameworkNET.API.LogError"/>.
    /// </summary>
    /// <remarks>
    /// This overload is exception-safe: if logging is globally disabled, or if
    /// <see cref="string.Format(string, object[])"/> throws during formatting, the exception is
    /// silently suppressed and no log is emitted.
    /// </remarks>
    /// <param name="ex">The exception to log.</param>
    /// <param name="format">A composite format string providing context for the exception.</param>
    /// <param name="args">An array of objects to format.</param>
    public static void Exception(Exception ex, string format, params object[] args)
    {
        if (!IsEnabled) return;
        string message;
        try { message = string.Format(format, args); }
        catch (Exception formatException)
        {
            ReportSuppressedFailure("Logger.Exception(format)", formatException);
            return;
        }

        Exception(ex, message);
    }

    /// <summary>
    /// Returns the currently active low-level sink through <see cref="LoggerSinkRegistry"/>.
    /// </summary>
    /// <returns>The sink that should receive enabled log writes.</returns>
    internal static ILogSink GetLogSink() => LoggerSinkRegistry.Get();

    /// <summary>
    /// Reports a suppressed internal logging failure to the optional observer.
    /// </summary>
    /// <remarks>
    /// A per-thread re-entrancy guard prevents infinite recursion when the observer itself
    /// triggers a suppressed logging failure (e.g., by calling <see cref="Logger"/> methods
    /// while the sink is still throwing). Re-entrant calls are silently dropped.
    /// </remarks>
    /// <param name="operation">The Logger or PluginLogger operation that suppressed the exception.</param>
    /// <param name="exception">The suppressed exception.</param>
    internal static void ReportSuppressedFailure(string operation, Exception exception)
    {
        var observer = SuppressedFailureObserver;
        if (observer is null || _reportingFailure)
            return;

        _reportingFailure = true;
        try
        {
            observer(operation, exception);
        }
        catch
        {
        }
        finally
        {
            _reportingFailure = false;
        }
    }

    private sealed class SuppressionScope : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;
            Interlocked.Decrement(ref _suppressionDepth);
            GC.SuppressFinalize(this);
        }
    }
}
