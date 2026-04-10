namespace Umbra.Logging;

/// <summary>
/// Owns the process-wide low-level sink selection used by <see cref="Logger"/> and <see cref="PluginLogger"/>.
/// </summary>
/// <remarks>
/// This registry isolates sink replacement and lazy default-sink creation from <see cref="Logger"/>, leaving the public logging APIs focused on enablement, filtering, and write dispatch.
/// </remarks>
internal static class LoggerSinkRegistry
{
    private static ILogSink? s_logSink;

    /// <summary>
    /// Replaces the currently active low-level sink.
    /// </summary>
    /// <param name="sink">The sink that should receive future enabled log writes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="sink"/> is <see langword="null"/>.</exception>
    internal static void Set(ILogSink sink)
    {
        ArgumentNullException.ThrowIfNull(sink);
        Interlocked.Exchange(ref s_logSink, sink);
    }

    /// <summary>
    /// Clears any replacement sink so the default REFramework-backed sink is recreated on the next enabled write.
    /// </summary>
    internal static void Reset() => Interlocked.Exchange(ref s_logSink, null);

    /// <summary>
    /// Returns the currently active sink, creating the default <see cref="REFrameworkLogSink"/> on first use.
    /// </summary>
    /// <returns>The sink that should receive enabled log writes.</returns>
    internal static ILogSink Get()
    {
        var sink = Volatile.Read(ref s_logSink);
        if (sink != null)
            return sink;

        sink = new REFrameworkLogSink();
        var existing = Interlocked.CompareExchange(ref s_logSink, sink, null);
        return existing ?? sink;
    }
}
