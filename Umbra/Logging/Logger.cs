using System.Threading;
using System.Runtime.CompilerServices;
using REFrameworkNET;

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
    /// Enables all Umbra logging.
    /// </summary>
    public static void EnableAll()
    {
        Enabled = true;
    }

    /// <summary>
    /// Disables all Umbra logging.
    /// </summary>
    public static void DisableAll()
    {
        Enabled = false;
    }

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
    /// Logs an informational message via <see cref="API.LogInfo"/>.
    /// </summary>
    /// <remarks>
    /// The disabled path is dependency-free: when <see cref="IsEnabled"/> is <see langword="false"/>,
    /// this method returns before touching any REFramework logging bridge so benchmarks and tests
    /// can suppress Umbra logging without requiring the REFramework host assemblies to load.
    /// </remarks>
    /// <param name="message">The message to log.</param>
    public static void Info(string message)
    {
        if (!IsEnabled) return;
        LogBridge.Info(message);
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
    /// Logs a warning message via <see cref="API.LogWarning"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Warning(string message)
    {
        if (!IsEnabled) return;
        LogBridge.Warning(message);
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
    /// Logs an error message via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public static void Error(string message)
    {
        if (!IsEnabled) return;
        LogBridge.Error(message);
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
    /// message, and stack trace, via <see cref="API.LogError"/>.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="message">A descriptive message providing context for the exception.</param>
    public static void Exception(Exception ex, string message)
    {
        if (!IsEnabled) return;
        LogBridge.Exception(ex, message);
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

    /// <summary>
    /// Isolates all direct REFramework API calls from the fast disabled path on <see cref="Logger"/>.
    /// </summary>
    /// <remarks>
    /// Keeping the host-specific calls in non-inlineable methods allows callers such as
    /// <see cref="Info(string)"/> to return immediately when logging is disabled without forcing the
    /// JIT to resolve REFramework logging members on that cold path.
    /// </remarks>
    private static class LogBridge
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Info(string message)
        {
            try { API.LogInfo(message); } catch { }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Warning(string message)
        {
            try { API.LogWarning(message); } catch { }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Error(string message)
        {
            try { API.LogError(message); } catch { }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Exception(Exception ex, string message)
        {
            try
            {
                API.LogError($"{message}\nException: {ex.GetType().Name}: {ex.Message}\nStack Trace:\n{ex.StackTrace}");
            }
            catch { }
        }
    }
}
