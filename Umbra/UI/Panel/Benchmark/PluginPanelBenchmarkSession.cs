using System.Diagnostics;
using Umbra.UI.Panel;

namespace Umbra.UI.Panel.Benchmark;

/// <summary>
/// Aggregates per-frame timing samples for an in-game <see cref="PluginPanel"/> draw benchmark.
/// </summary>
/// <remarks>
/// One measured draw pass is recorded per ImGui frame so the benchmark follows the real
/// REFramework UI lifecycle. A warmup phase is supported to let ImGui state settle before samples
/// are recorded.
/// </remarks>
internal sealed class PluginPanelBenchmarkSession
{
    private readonly List<long> _recordedTicks = [];
    private long _frameIndex;
    private long _warmupFramesRemaining;
    private int _configuredWarmupFrames;
    private int _configuredSampleFrames;
    private long _targetSampleCount;
    private long _recordedSampleCount;
    private long _totalTicks;
    private long _maxTicks;
    private long _lastTicks;

    /// <summary>
    /// Gets whether the benchmark is currently recording.
    /// </summary>
    internal bool IsRunning { get; private set; }

    /// <summary>
    /// Gets the configured warmup-frame count for the active or most recent run.
    /// </summary>
    internal int ConfiguredWarmupFrames => _configuredWarmupFrames;

    /// <summary>
    /// Gets the configured recorded-sample count for the active or most recent run.
    /// </summary>
    internal int ConfiguredSampleFrames => _configuredSampleFrames;

    /// <summary>
    /// Gets the number of warmup frames still to skip.
    /// </summary>
    internal long WarmupFramesRemaining => _warmupFramesRemaining;

    /// <summary>
    /// Gets the number of recorded samples.
    /// </summary>
    internal long RecordedSampleCount => _recordedSampleCount;

    /// <summary>
    /// Gets the configured sample target for the active run.
    /// </summary>
    internal long TargetSampleCount => _targetSampleCount;

    /// <summary>
    /// Gets the duration of the most recent measured frame in milliseconds.
    /// </summary>
    internal double LastMilliseconds => TicksToMilliseconds(_lastTicks);

    /// <summary>
    /// Gets the average measured frame duration in milliseconds.
    /// </summary>
    internal double AverageMilliseconds
        => _recordedSampleCount == 0
            ? 0d
            : TicksToMilliseconds(_totalTicks) / _recordedSampleCount;

    /// <summary>
    /// Gets the slowest measured frame duration in milliseconds.
    /// </summary>
    internal double MaxMilliseconds => TicksToMilliseconds(_maxTicks);

    /// <summary>
    /// Gets the 95th-percentile steady-state frame duration in milliseconds.
    /// </summary>
    /// <remarks>
    /// Only recorded samples are included; warmup frames are excluded from the percentile
    /// calculation.
    /// </remarks>
    internal double P95Milliseconds => GetPercentileMilliseconds(0.95d);

    /// <summary>
    /// Gets the 99th-percentile steady-state frame duration in milliseconds.
    /// </summary>
    /// <remarks>
    /// Only recorded samples are included; warmup frames are excluded from the percentile
    /// calculation.
    /// </remarks>
    internal double P99Milliseconds => GetPercentileMilliseconds(0.99d);

    /// <summary>
    /// Starts a new benchmark run.
    /// </summary>
    /// <param name="warmupFrames">The number of frames to skip before recording.</param>
    /// <param name="sampleFrames">The number of frames to record.</param>
    internal void Start(int warmupFrames = 300, int sampleFrames = 2000)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(warmupFrames);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(sampleFrames);

        Reset();
        _configuredWarmupFrames = warmupFrames;
        _configuredSampleFrames = sampleFrames;
        _warmupFramesRemaining = warmupFrames;
        _targetSampleCount = sampleFrames;
        IsRunning = true;
    }

    /// <summary>
    /// Stops the active benchmark run without clearing recorded data.
    /// </summary>
    internal void Stop() => IsRunning = false;

    /// <summary>
    /// Clears all recorded data and stops any active run.
    /// </summary>
    internal void Reset()
    {
        IsRunning = false;
        _frameIndex = 0;
        _warmupFramesRemaining = 0;
        _configuredWarmupFrames = 0;
        _configuredSampleFrames = 0;
        _targetSampleCount = 0;
        _recordedSampleCount = 0;
        _recordedTicks.Clear();
        _totalTicks = 0;
        _maxTicks = 0;
        _lastTicks = 0;
    }

    /// <summary>
    /// Records one measured frame.
    /// </summary>
    /// <param name="elapsedTicks">The elapsed stopwatch ticks for the measured draw pass.</param>
    /// <returns>
    /// The captured sample when a run is active; otherwise <see langword="null"/> when the frame
    /// was ignored because the benchmark is not running.
    /// </returns>
    internal PluginPanelBenchmarkSample? RecordFrame(long elapsedTicks)
    {
        if (!IsRunning)
            return null;

        _lastTicks = elapsedTicks;
        var isWarmup = _warmupFramesRemaining > 0;
        var sample = PluginPanelBenchmarkSample.Create(_frameIndex, isWarmup, elapsedTicks);
        _frameIndex++;

        if (isWarmup)
        {
            _warmupFramesRemaining--;
            return sample;
        }

        _recordedSampleCount++;
        _recordedTicks.Add(elapsedTicks);
        _totalTicks += elapsedTicks;

        if (elapsedTicks > _maxTicks)
            _maxTicks = elapsedTicks;

        if (_recordedSampleCount >= _targetSampleCount)
            IsRunning = false;

        return sample;
    }

    /// <summary>
    /// Computes a nearest-rank percentile over the recorded non-warmup samples.
    /// </summary>
    /// <param name="percentile">The percentile to compute as a value between 0 and 1.</param>
    /// <returns>The percentile duration in milliseconds, or <c>0</c> when no recorded samples exist.</returns>
    private double GetPercentileMilliseconds(double percentile)
    {
        if (_recordedTicks.Count == 0)
            return 0d;

        var sorted = _recordedTicks.ToArray();
        Array.Sort(sorted);

        var rank = (int)Math.Ceiling(sorted.Length * percentile) - 1;
        if (rank < 0)
            rank = 0;
        if (rank >= sorted.Length)
            rank = sorted.Length - 1;

        return TicksToMilliseconds(sorted[rank]);
    }

    /// <summary>
    /// Converts stopwatch ticks to milliseconds.
    /// </summary>
    /// <param name="ticks">The stopwatch ticks to convert.</param>
    /// <returns>The duration in milliseconds.</returns>
    private static double TicksToMilliseconds(long ticks)
        => ticks * 1000d / Stopwatch.Frequency;
}
