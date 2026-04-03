#if BENCHMARK
using System.Globalization;
using System.Text;
using System.Text.Json;
using Umbra.Logging;

namespace Umbra.UI.Panel.Benchmark;

/// <summary>
/// Persists <see cref="PluginPanel"/> benchmark runs to CSV, JSON, and Markdown files.
/// </summary>
/// <remarks>
/// CSV output is streamed incrementally as frames are measured so partially completed runs still
/// leave behind useful raw timing data. A JSON export is written when the run completes, stops,
/// resets, or is disposed, combining run metadata, summary statistics, and the collected samples
/// into one analysis-friendly document. A sibling Markdown export summarizes the same run in a
/// format that is easy to inspect directly in the repository or attach to later analysis.
/// </remarks>
internal sealed class PluginPanelBenchmarkExporter
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string _outputDirectory;
    private readonly List<PluginPanelBenchmarkSample> _samples = [];
    private StreamWriter? _csvWriter;
    private string? _runId;
    private string? _benchmarkName;
    private string? _scenario;
    private string? _csvPath;
    private string? _jsonPath;
    private string? _markdownPath;
    private DateTimeOffset _startedUtc;
    private int _configuredWarmupFrames;
    private int _configuredSampleFrames;
    private bool _suppressedRuntimePanel;

    /// <summary>
    /// Initializes a new exporter that writes benchmark artifacts to <paramref name="outputDirectory"/>.
    /// </summary>
    /// <param name="outputDirectory">The directory that will receive CSV, JSON, and Markdown benchmark exports.</param>
    internal PluginPanelBenchmarkExporter(string outputDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        _outputDirectory = outputDirectory;
    }

    /// <summary>
    /// Gets whether an export run is currently active.
    /// </summary>
    internal bool IsRunActive => _runId is not null;

    /// <summary>
    /// Gets the path to the most recent CSV export file.
    /// </summary>
    internal string? LastCsvPath { get; private set; }

    /// <summary>
    /// Gets the path to the most recent JSON export file.
    /// </summary>
    internal string? LastJsonPath { get; private set; }

    /// <summary>
    /// Gets the path to the most recent Markdown export file.
    /// </summary>
    internal string? LastMarkdownPath { get; private set; }

    /// <summary>
    /// Starts a new export run and opens the CSV stream for incremental sample writes.
    /// </summary>
    /// <param name="benchmarkName">A stable benchmark name describing the measured panel draw call.</param>
    /// <param name="scenario">A stable scenario label describing the benchmark target.</param>
    /// <param name="warmupFrames">The configured warmup-frame count.</param>
    /// <param name="sampleFrames">The configured recorded-sample count.</param>
    /// <param name="suppressedRuntimePanel">Whether the normal runtime panel is suppressed while sampling.</param>
    internal void StartRun(string benchmarkName, string scenario, int warmupFrames, int sampleFrames, bool suppressedRuntimePanel)
    {
        if (IsRunActive)
            CompleteRun(null, "Restarted");

        try
        {
            Directory.CreateDirectory(_outputDirectory);

            _runId = DateTimeOffset.UtcNow.ToString("yyyyMMdd-HHmmss-fff");
            _benchmarkName = benchmarkName;
            _scenario = scenario;
            _startedUtc = DateTimeOffset.UtcNow;
            _configuredWarmupFrames = warmupFrames;
            _configuredSampleFrames = sampleFrames;
            _suppressedRuntimePanel = suppressedRuntimePanel;
            _samples.Clear();

            _csvPath = Path.Combine(_outputDirectory, $"plugin-panel-benchmark-{_runId}.csv");
            _jsonPath = Path.Combine(_outputDirectory, $"plugin-panel-benchmark-{_runId}.json");
            _markdownPath = Path.Combine(_outputDirectory, $"plugin-panel-benchmark-{_runId}.md");
            LastCsvPath = _csvPath;
            LastJsonPath = _jsonPath;
            LastMarkdownPath = _markdownPath;

            _csvWriter = new StreamWriter(_csvPath, append: false)
            {
                AutoFlush = true
            };
            _csvWriter.WriteLine("runId,benchmarkName,scenario,frameIndex,isWarmup,elapsedTicks,elapsedMilliseconds,suppressedRuntimePanel");
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"PluginPanelBenchmark: failed to start benchmark export in '{_outputDirectory}'.");
            ResetState();
        }
    }

    /// <summary>
    /// Appends one measured frame to the active CSV stream and in-memory JSON sample list.
    /// </summary>
    /// <param name="sample">The measured frame to persist.</param>
    internal void RecordSample(PluginPanelBenchmarkSample sample)
    {
        if (!IsRunActive || _csvWriter is null || _runId is null || _benchmarkName is null || _scenario is null)
            return;

        try
        {
            _samples.Add(sample);
            _csvWriter.Write(EscapeCsvField(_runId));
            _csvWriter.Write(',');
            _csvWriter.Write(EscapeCsvField(_benchmarkName));
            _csvWriter.Write(',');
            _csvWriter.Write(EscapeCsvField(_scenario));
            _csvWriter.Write(',');
            _csvWriter.Write(sample.FrameIndex);
            _csvWriter.Write(',');
            _csvWriter.Write(sample.IsWarmup);
            _csvWriter.Write(',');
            _csvWriter.Write(sample.ElapsedTicks);
            _csvWriter.Write(',');
            _csvWriter.Write(sample.ElapsedMilliseconds.ToString("F6", CultureInfo.InvariantCulture));
            _csvWriter.Write(',');
            _csvWriter.WriteLine(_suppressedRuntimePanel);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"PluginPanelBenchmark: failed to append benchmark sample to '{_csvPath}'.");
        }
    }

    /// <summary>
    /// Completes the active export run and writes the JSON and Markdown summary documents.
    /// </summary>
    /// <param name="session">The benchmark session providing aggregate statistics, or <see langword="null"/> when unavailable.</param>
    /// <param name="completionReason">The reason the export run ended.</param>
    internal void CompleteRun(PluginPanelBenchmarkSession? session, string completionReason)
    {
        if (!IsRunActive || _runId is null || _benchmarkName is null || _scenario is null || _jsonPath is null || _markdownPath is null)
            return;

        try
        {
            _csvWriter?.Dispose();
            _csvWriter = null;

            var exportDocument = new PluginPanelBenchmarkExportDocument
            {
                RunId = _runId,
                BenchmarkName = _benchmarkName,
                Scenario = _scenario,
                StartedUtc = _startedUtc,
                FinishedUtc = DateTimeOffset.UtcNow,
                CompletionReason = completionReason,
                ConfiguredWarmupFrames = _configuredWarmupFrames,
                ConfiguredSampleFrames = _configuredSampleFrames,
                SuppressedRuntimePanel = _suppressedRuntimePanel,
                RecordedSampleCount = session?.RecordedSampleCount ?? 0,
                ExportedFrameCount = _samples.Count,
                AverageMilliseconds = session?.AverageMilliseconds ?? 0d,
                MaxMilliseconds = session?.MaxMilliseconds ?? 0d,
                LastMilliseconds = session?.LastMilliseconds ?? 0d,
                P95Milliseconds = session?.P95Milliseconds ?? 0d,
                P99Milliseconds = session?.P99Milliseconds ?? 0d,
                Samples = [.. _samples]
            };

            var json = JsonSerializer.Serialize(exportDocument, _jsonOptions);

            File.WriteAllText(_jsonPath, json);
            File.WriteAllText(_markdownPath, BuildMarkdownSummary(exportDocument));
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"PluginPanelBenchmark: failed to finalize benchmark exports for run '{_runId}'.");
        }
        finally
        {
            ResetState();
        }
    }

    /// <summary>
    /// Builds the human-readable Markdown summary written next to the CSV and JSON exports.
    /// </summary>
    /// <param name="document">The finalized export document for the completed run.</param>
    /// <returns>A Markdown summary of the completed benchmark run.</returns>
    private static string BuildMarkdownSummary(PluginPanelBenchmarkExportDocument document)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# Plugin Panel Benchmark Summary");
        builder.AppendLine();
        builder.AppendLine($"- Run ID: `{document.RunId}`");
        builder.AppendLine($"- Benchmark Name: `{document.BenchmarkName}`");
        builder.AppendLine($"- Scenario: `{document.Scenario}`");
        builder.AppendLine($"- Started (UTC): `{document.StartedUtc:O}`");
        builder.AppendLine($"- Finished (UTC): `{document.FinishedUtc:O}`");
        builder.AppendLine($"- Completion Reason: `{document.CompletionReason}`");
        builder.AppendLine($"- Suppressed Runtime Panel: `{document.SuppressedRuntimePanel}`");
        builder.AppendLine($"- Configured Warmup Frames: `{document.ConfiguredWarmupFrames}`");
        builder.AppendLine($"- Configured Sample Frames: `{document.ConfiguredSampleFrames}`");
        builder.AppendLine($"- Exported Frame Count: `{document.ExportedFrameCount}`");
        builder.AppendLine($"- Recorded Sample Count: `{document.RecordedSampleCount}`");
        builder.AppendLine();
        builder.AppendLine("## Summary Statistics");
        builder.AppendLine();
        builder.AppendLine("| Metric | Value (ms) |");
        builder.AppendLine("|---|---:|");
        builder.AppendLine($"| Last | {document.LastMilliseconds:F6} |");
        builder.AppendLine($"| Average | {document.AverageMilliseconds:F6} |");
        builder.AppendLine($"| Max | {document.MaxMilliseconds:F6} |");
        builder.AppendLine($"| P95 | {document.P95Milliseconds:F6} |");
        builder.AppendLine($"| P99 | {document.P99Milliseconds:F6} |");
        return builder.ToString();
    }

    /// <summary>
    /// Clears active-run state while preserving the last export paths for UI display.
    /// </summary>
    private void ResetState()
    {
        _csvWriter?.Dispose();
        _csvWriter = null;
        _runId = null;
        _benchmarkName = null;
        _scenario = null;
        _csvPath = null;
        _jsonPath = null;
        _markdownPath = null;
        _samples.Clear();
    }

    /// <summary>
    /// Returns a RFC 4180-compliant CSV-escaped representation of <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The raw string value to escape.</param>
    /// <returns>
    /// The value wrapped in double quotes, with any embedded double-quote characters doubled.
    /// </returns>
    private static string EscapeCsvField(string value)
        => "\"" + value.Replace("\"", "\"\"") + "\"";

    /// <summary>
    /// JSON-serializable export document written at the end of each benchmark run.
    /// </summary>
    private sealed class PluginPanelBenchmarkExportDocument
    {
        public string RunId { get; init; } = string.Empty;
        public string BenchmarkName { get; init; } = string.Empty;
        public string Scenario { get; init; } = string.Empty;
        public DateTimeOffset StartedUtc { get; init; }
        public DateTimeOffset FinishedUtc { get; init; }
        public string CompletionReason { get; init; } = string.Empty;
        public int ConfiguredWarmupFrames { get; init; }
        public int ConfiguredSampleFrames { get; init; }
        public bool SuppressedRuntimePanel { get; init; }
        public long RecordedSampleCount { get; init; }
        public int ExportedFrameCount { get; init; }
        public double AverageMilliseconds { get; init; }
        public double MaxMilliseconds { get; init; }
        public double LastMilliseconds { get; init; }
        public double P95Milliseconds { get; init; }
        public double P99Milliseconds { get; init; }
        public List<PluginPanelBenchmarkSample> Samples { get; init; } = [];
    }
}
#endif
