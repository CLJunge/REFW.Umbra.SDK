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
/// Both <see cref="Logger"/> and <see cref="PluginLogger"/> honor <see cref="Enabled"/> and
/// <see cref="Suppress"/>, so callers can disable all SDK and plugin-prefixed output through one
/// process-wide switch.
/// </para>
/// </remarks>
public static class Logger
{
    private static int _enabled = 1;
    private static int _suppressionDepth;
    private static ILogSink? _logSink;

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
    internal static void SetLogSink(ILogSink sink)
    {
        ArgumentNullException.ThrowIfNull(sink);
        Interlocked.Exchange(ref _logSink, sink);
    }

    /// <summary>
    /// Restores the default REFramework-backed log sink.
    /// </summary>
    /// <remarks>
    /// The next enabled write recreates the default sink lazily so disabled paths remain free from
    /// host-specific logging calls.
    /// </remarks>
    internal static void ResetLogSink()
    {
        Interlocked.Exchange(ref _logSink, null);
    }

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
        try { GetLogSink().Info(message); } catch { }
    }

    /// <summary>
    /// Logs a formatted informational message via <see cref="API.LogInfo"/>.
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
        try { message = string.Format(format, args); } catch { return; }
        Info(message);
    }

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Warning(string message)
    {
        if (!IsEnabled) return;
        try { GetLogSink().Warning(message); } catch { }
    }

    /// <summary>
    /// Logs a formatted warning message via <see cref="API.LogWarning"/>.
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
        try { message = string.Format(format, args); } catch { return; }
        Warning(message);
    }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Error(string message)
    {
        if (!IsEnabled) return;
        try { GetLogSink().Error(message); } catch { }
    }

    /// <summary>
    /// Logs a formatted error message via <see cref="API.LogError"/>.
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
        try { message = string.Format(format, args); } catch { return; }
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
        catch { }
    }

    /// <summary>
    /// Logs a formatted error message accompanied by exception details, including the exception
    /// type, message, and stack trace, via <see cref="API.LogError"/>.
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
        try { message = string.Format(format, args); } catch { return; }
        Exception(ex, message);
    }

    /// <summary>
    /// Returns the currently active low-level sink, creating the default REFramework-backed sink on
    /// first use.
    /// </summary>
    /// <returns>The sink that should receive enabled log writes.</returns>
    internal static ILogSink GetLogSink()
    {
        var sink = Volatile.Read(ref _logSink);
        if (sink != null)
            return sink;

        sink = new REFrameworkLogSink();
        var existing = Interlocked.CompareExchange(ref _logSink, sink, null);
        return existing ?? sink;
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
