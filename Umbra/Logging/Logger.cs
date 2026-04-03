namespace Umbra.Logging;

/// <summary>
/// Writes raw Umbra log messages for shared runtime infrastructure and exposes process-wide logging suppression controls.
/// </summary>
/// <remarks>
/// <para>
/// This static API carries no per-plugin prefix or minimum-level configuration and forwards messages exactly as given. Plugin instance code should generally use <see cref="PluginLogger"/> for prefix-tagged, per-plugin-filtered logging.
/// </para>
/// <para>
/// Because all managed plugins share the same AppDomain, mutable per-plugin settings such as a prefix or minimum log level must not live on this static type. The shared state exposed here is limited to the coarse-grained enable and suppression controls that silence all Umbra logging in the current process.
/// </para>
/// <para>
/// Low-level sink replacement and lazy default-sink creation are delegated to <see cref="LoggerSinkRegistry"/> so this type can remain focused on enablement, exception-safe dispatch, and optional suppressed-failure observation.
/// </para>
/// </remarks>
public static class Logger
{
    private static int _enabled = 1;
    private static int _suppressionDepth;

    [ThreadStatic]
    private static bool _reportingFailure;

    /// <summary>
    /// Gets or sets an optional observer that receives exceptions suppressed internally by <see cref="Logger"/> and <see cref="PluginLogger"/>.
    /// </summary>
    /// <value>An observer that receives the failing operation name and suppressed exception, or <see langword="null"/> when suppressed failures are ignored.</value>
    /// <remarks>
    /// This hook is intended for opt-in diagnostics in tests, benchmarks, or advanced debugging sessions where callers want visibility into failures that Umbra normally swallows to protect the game process. Exceptions thrown by the observer itself are swallowed as well.
    /// </remarks>
    public static Action<string, Exception>? SuppressedFailureObserver { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Umbra logging is globally enabled.
    /// </summary>
    /// <value><see langword="true"/> if log writes are allowed to proceed to suppression-depth checks; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// Setting this property to <see langword="false"/> disables output from both <see cref="Logger"/> and <see cref="PluginLogger"/> until it is re-enabled. Prefer <see cref="Suppress()"/> when temporary scoped suppression is desired.
    /// </remarks>
    public static bool Enabled
    {
        get => Volatile.Read(ref _enabled) != 0;
        set => Interlocked.Exchange(ref _enabled, value ? 1 : 0);
    }

    /// <summary>
    /// Gets a value indicating whether Umbra logging is currently effective after combining <see cref="Enabled"/> with active <see cref="Suppress()"/> scopes.
    /// </summary>
    /// <value><see langword="true"/> if logging is globally enabled and no active suppression scope remains; otherwise, <see langword="false"/>.</value>
    public static bool IsEnabled => Enabled && Volatile.Read(ref _suppressionDepth) == 0;

    /// <summary>
    /// Replaces the low-level log sink used by <see cref="Logger"/> and <see cref="PluginLogger"/>.
    /// </summary>
    /// <remarks>
    /// This method is intended for tests and other controlled environments that need Umbra logging to remain executable without the REFramework runtime host. Replacing the sink does not change public logging semantics such as global enablement, suppression, or <see cref="PluginLogger.MinLevel"/> filtering.
    /// </remarks>
    /// <param name="sink">The sink that should receive future enabled log writes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sink"/> is <see langword="null"/>.</exception>
    internal static void SetLogSink(ILogSink sink) => LoggerSinkRegistry.Set(sink);

    /// <summary>
    /// Restores the default REFramework-backed log sink.
    /// </summary>
    /// <remarks>
    /// The next enabled write recreates the default sink lazily so disabled paths remain free from host-specific logging calls.
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
    /// <returns>A disposable scope that removes one active suppression layer when disposed.</returns>
    public static IDisposable Suppress()
    {
        Interlocked.Increment(ref _suppressionDepth);
        return new SuppressionScope();
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <remarks>
    /// When <see cref="IsEnabled"/> is <see langword="false"/>, this method returns before resolving the underlying sink so suppressed test and benchmark paths remain host-independent.
    /// </remarks>
    /// <param name="message">The message to log.</param>
    public static void Info(string message)
    {
        if (!IsEnabled) return;
        try { GetLogSink().Info(message); }
        catch (Exception ex) { ReportSuppressedFailure("Logger.Info", ex); }
    }

    /// <summary>
    /// Logs a formatted informational message.
    /// </summary>
    /// <remarks>
    /// This overload formats the message first, then delegates to <see cref="Info(string)"/>. If logging is globally disabled or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
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
    /// Logs a formatted warning message.
    /// </summary>
    /// <remarks>
    /// This overload formats the message first, then delegates to <see cref="Warning(string)"/>. If logging is globally disabled or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
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
    /// Logs a formatted error message.
    /// </summary>
    /// <remarks>
    /// This overload formats the message first, then delegates to <see cref="Error(string)"/>. If logging is globally disabled or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
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
    /// Logs an error message together with exception details.
    /// </summary>
    /// <remarks>
    /// The emitted text contains the supplied context message followed by the exception type, message, and stack trace. If logging is globally disabled or sink dispatch fails, no log is emitted.
    /// </remarks>
    /// <param name="ex">The exception to describe in the emitted log entry.</param>
    /// <param name="message">The context message to prepend to the exception details.</param>
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
    /// Logs a formatted error message together with exception details.
    /// </summary>
    /// <remarks>
    /// This overload formats the context message first, then delegates to <see cref="Exception(Exception, string)"/>. If logging is globally disabled or <see cref="string.Format(string, object[])"/> throws, no log is emitted.
    /// </remarks>
    /// <param name="ex">The exception to describe in the emitted log entry.</param>
    /// <param name="format">A composite format string that produces the context message.</param>
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
    /// Reports a suppressed internal logging failure to <see cref="SuppressedFailureObserver"/>.
    /// </summary>
    /// <remarks>
    /// A per-thread re-entrancy guard prevents infinite recursion when the observer itself triggers another suppressed logging failure. Re-entrant calls are dropped silently.
    /// </remarks>
    /// <param name="operation">The <see cref="Logger"/> or <see cref="PluginLogger"/> operation that suppressed the exception.</param>
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
