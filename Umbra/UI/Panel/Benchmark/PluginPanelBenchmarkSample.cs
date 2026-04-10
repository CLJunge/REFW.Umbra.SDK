#if BENCHMARK
using System.Diagnostics;

namespace Umbra.UI.Panel.Benchmark;

/// <summary>
/// Stores one measured frame from a <see cref="PluginPanel"/> benchmark run.
/// </summary>
/// <remarks>
/// Samples are created by <see cref="PluginPanelBenchmarkSession"/> as frames are measured and then forwarded to <see cref="PluginPanelBenchmarkExporter"/> for persistence. Warmup frames are retained so exports preserve the full run timeline.
/// </remarks>
internal readonly record struct PluginPanelBenchmarkSample(
    long FrameIndex,
    bool IsWarmup,
    long ElapsedTicks,
    double ElapsedMilliseconds)
{
    /// <summary>
    /// Creates a benchmark sample from a raw stopwatch measurement.
    /// </summary>
    /// <param name="frameIndex">The zero-based frame index within the current run.</param>
    /// <param name="isWarmup"><see langword="true"/> if the frame belongs to the warmup phase; otherwise, <see langword="false"/>.</param>
    /// <param name="elapsedTicks">The elapsed stopwatch ticks measured for the panel draw pass.</param>
    /// <returns>The created benchmark sample.</returns>
    internal static PluginPanelBenchmarkSample Create(long frameIndex, bool isWarmup, long elapsedTicks)
        => new(frameIndex, isWarmup, elapsedTicks, elapsedTicks * 1000d / Stopwatch.Frequency);
}
#endif
