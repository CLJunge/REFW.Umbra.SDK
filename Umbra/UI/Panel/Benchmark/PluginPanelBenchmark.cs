#if BENCHMARK
using System.Diagnostics;
using Hexa.NET.ImGui;
using Umbra.UI.Config;

namespace Umbra.UI.Panel.Benchmark;

/// <summary>
/// Hosts an ImGui benchmark window that measures one <see cref="PluginPanel.Draw"/> call per frame for a benchmark panel.
/// </summary>
/// <remarks>
/// This type owns the benchmark UI, timing session, and export pipeline. The timed region includes only the benchmark panel's <see cref="PluginPanel.Draw"/> call; the host window chrome and benchmark controls are excluded.
/// </remarks>
public sealed class PluginPanelBenchmark : IDisposable
{
    private static readonly string[] _scenarioLabels =
    [
        "Collapsed Panel",
        "Expanded Panel",
        "Interactive"
    ];

    private readonly PluginPanel _panel;
    private readonly PluginPanelBenchmarkSession _session = new();
    private readonly PluginPanelBenchmarkExporter _exporter;
    private readonly bool _ownsPanel;
    private bool _disposed;

    /// <summary>
    /// Initializes a new benchmark host for the supplied caller-owned <paramref name="panel"/>.
    /// </summary>
    /// <param name="windowTitle">The ImGui window title shown for the benchmark host.</param>
    /// <param name="panel">The benchmark panel whose draw call should be measured.</param>
    /// <param name="outputDirectory">The directory that receives CSV, JSON, and Markdown benchmark artifacts.</param>
    /// <param name="benchmarkName">The stable benchmark name written into exported artifacts.</param>
    /// <exception cref="ArgumentException"><paramref name="windowTitle"/>, <paramref name="outputDirectory"/>, or <paramref name="benchmarkName"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="panel"/> is <see langword="null"/>.</exception>
    public PluginPanelBenchmark(string windowTitle, PluginPanel panel, string outputDirectory, string benchmarkName = "PluginPanel.Draw")
        : this(windowTitle, panel, outputDirectory, benchmarkName, ownsPanel: false)
    {
    }

    /// <summary>
    /// Creates a benchmark host for a generated duplicate configuration panel built from <typeparamref name="TConfig"/>.
    /// </summary>
    /// <typeparam name="TConfig">The configuration type rendered by the generated benchmark section.</typeparam>
    /// <param name="windowTitle">The ImGui window title shown for the benchmark host.</param>
    /// <param name="config">The already loaded configuration instance shared by the benchmark section.</param>
    /// <param name="panelIdScope">The unique ImGui ID scope used for the generated benchmark panel.</param>
    /// <param name="outputDirectory">The directory that receives CSV, JSON, and Markdown benchmark artifacts.</param>
    /// <param name="benchmarkName">The stable benchmark name written into exported artifacts.</param>
    /// <param name="sectionIdScope">The optional stable ImGui widget ID sub-scope used for the generated <see cref="ConfigSection{TConfig}"/>.</param>
    /// <returns>A benchmark host that owns the generated duplicate configuration panel.</returns>
    /// <remarks>
    /// This is the convenience path for benchmarking a second config-backed panel without manually constructing and tracking that duplicate panel.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="config"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="windowTitle"/>, <paramref name="panelIdScope"/>, <paramref name="outputDirectory"/>, or <paramref name="benchmarkName"/> is <see langword="null"/> or whitespace.</exception>
    public static PluginPanelBenchmark CreateForConfig<TConfig>(
        string windowTitle,
        TConfig config,
        string panelIdScope,
        string outputDirectory,
        string benchmarkName = "PluginPanel.Draw",
        string? sectionIdScope = null)
        where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentException.ThrowIfNullOrWhiteSpace(panelIdScope);

        var panel = new PluginPanel(panelIdScope)
            .Add(new ConfigSection<TConfig>(config, sectionIdScope));

        return new PluginPanelBenchmark(windowTitle, panel, outputDirectory, benchmarkName, ownsPanel: true);
    }

    /// <summary>
    /// Initializes a new benchmark host for the supplied benchmark panel.
    /// </summary>
    /// <param name="windowTitle">The ImGui window title shown for the benchmark host.</param>
    /// <param name="panel">The benchmark panel whose draw call should be measured.</param>
    /// <param name="outputDirectory">The directory that receives CSV, JSON, and Markdown benchmark artifacts.</param>
    /// <param name="benchmarkName">The stable benchmark name written into exported artifacts.</param>
    /// <param name="ownsPanel"><see langword="true"/> when this instance should dispose <paramref name="panel"/> during <see cref="Dispose"/>; otherwise, <see langword="false"/>.</param>
    /// <exception cref="ArgumentException"><paramref name="windowTitle"/>, <paramref name="outputDirectory"/>, or <paramref name="benchmarkName"/> is <see langword="null"/> or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="panel"/> is <see langword="null"/>.</exception>
    private PluginPanelBenchmark(string windowTitle, PluginPanel panel, string outputDirectory, string benchmarkName, bool ownsPanel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(windowTitle);
        ArgumentNullException.ThrowIfNull(panel);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(benchmarkName);

        WindowTitle = windowTitle;
        BenchmarkName = benchmarkName;
        _panel = panel;
        _exporter = new PluginPanelBenchmarkExporter(outputDirectory);
        _ownsPanel = ownsPanel;
    }

    /// <summary>
    /// Gets the ImGui window title shown for the benchmark host.
    /// </summary>
    public string WindowTitle { get; }

    /// <summary>
    /// Gets the stable benchmark name written into exported artifacts.
    /// </summary>
    public string BenchmarkName { get; }

    /// <summary>
    /// Gets or sets the selected benchmark scenario label.
    /// </summary>
    public PluginPanelBenchmarkScenario Scenario { get; set; } = PluginPanelBenchmarkScenario.ExpandedPanel;

    /// <summary>
    /// Gets or sets a value indicating whether the runtime panel should be suppressed while samples are being recorded.
    /// </summary>
    public bool SuppressRuntimePanelWhileSampling { get; set; } = true;

    /// <summary>
    /// Gets or sets the number of warmup frames skipped before samples are recorded.
    /// </summary>
    public int WarmupFrames { get; set; } = 300;

    /// <summary>
    /// Gets or sets the number of steady-state sample frames recorded per run.
    /// </summary>
    public int SampleFrames { get; set; } = 2000;

    /// <summary>
    /// Gets a value indicating whether the benchmark is currently recording.
    /// </summary>
    public bool IsRunning => !_disposed && _session.IsRunning;

    /// <summary>
    /// Gets a value indicating whether the caller should suppress its normal runtime panel for the current frame.
    /// </summary>
    /// <remarks>
    /// Callers can use this from their normal UI path to avoid drawing both the runtime and benchmark panels in the same frame while a run is active.
    /// </remarks>
    public bool ShouldSuppressRuntimePanel => !_disposed && SuppressRuntimePanelWhileSampling && _session.IsRunning;

    /// <summary>
    /// Gets the path to the most recent CSV export file.
    /// </summary>
    public string? LastCsvPath => _exporter.LastCsvPath;

    /// <summary>
    /// Gets the path to the most recent JSON export file.
    /// </summary>
    public string? LastJsonPath => _exporter.LastJsonPath;

    /// <summary>
    /// Gets the path to the most recent Markdown export file.
    /// </summary>
    public string? LastMarkdownPath => _exporter.LastMarkdownPath;

    /// <summary>
    /// Starts a new benchmark run using the current benchmark settings.
    /// </summary>
    /// <remarks>
    /// If a run is already active, it is finalized first so any partial data is exported with the correct summary statistics before the new run resets the session state.
    /// </remarks>
    public void Start()
    {
        ThrowIfDisposed();

        if (WarmupFrames < 0)
            WarmupFrames = 0;

        if (SampleFrames < 1)
            SampleFrames = 1;

        if (_exporter.IsRunActive)
            _exporter.CompleteRun(_session, "Restarted");

        _session.Start(WarmupFrames, SampleFrames);
        _exporter.StartRun(
            BenchmarkName,
            GetScenarioLabel(Scenario),
            WarmupFrames,
            SampleFrames,
            SuppressRuntimePanelWhileSampling);
    }

    /// <summary>
    /// Stops the active benchmark run and finalizes any pending export artifacts.
    /// </summary>
    /// <param name="completionReason">The reason the current run ended.</param>
    public void Stop(string completionReason = "Stopped")
    {
        ThrowIfDisposed();

        if (!_session.IsRunning && !_exporter.IsRunActive)
            return;

        _session.Stop();
        _exporter.CompleteRun(_session, completionReason);
    }

    /// <summary>
    /// Finalizes any active run as reset and clears the in-memory benchmark state.
    /// </summary>
    public void Reset()
    {
        ThrowIfDisposed();
        CompleteActiveRun("Reset");
    }

    /// <summary>
    /// Completes any active export run, preserving partial data, and then clears benchmark state.
    /// </summary>
    /// <param name="completionReason">The reason the run is being finalized.</param>
    public void CompleteActiveRun(string completionReason)
    {
        ThrowIfDisposed();

        if (_exporter.IsRunActive)
            _exporter.CompleteRun(_session, completionReason);

        _session.Reset();
    }

    /// <summary>
    /// Draws the benchmark host window and, when active, measures one <see cref="PluginPanel.Draw"/> call for the benchmark panel.
    /// </summary>
    /// <remarks>
    /// The selected scenario label is exported with the run so later analysis can separate collapsed, expanded, and interactive measurements. For repeatable results, keep the panel's actual expansion state aligned with the selected scenario and avoid interacting with benchmarked widgets unless the interactive scenario is intended.
    /// </remarks>
    public void DrawWindow()
    {
        ThrowIfDisposed();

        var isWindowVisible = ImGui.Begin(WindowTitle);
        if (!isWindowVisible)
        {
            ImGui.End();
            return;
        }

        var suppressRuntimePanelWhileSampling = SuppressRuntimePanelWhileSampling;
        ImGui.Checkbox("Suppress runtime panel while sampling", ref suppressRuntimePanelWhileSampling);
        SuppressRuntimePanelWhileSampling = suppressRuntimePanelWhileSampling;
        var scenarioIndex = (int)Scenario;
        if (ImGui.Combo("Scenario", ref scenarioIndex, _scenarioLabels, _scenarioLabels.Length))
            Scenario = (PluginPanelBenchmarkScenario)scenarioIndex;

        var warmupFrames = WarmupFrames;
        ImGui.InputInt("Warmup Frames", ref warmupFrames);
        WarmupFrames = warmupFrames;

        var sampleFrames = SampleFrames;
        ImGui.InputInt("Sample Frames", ref sampleFrames);
        SampleFrames = sampleFrames;

        if (WarmupFrames < 0)
            WarmupFrames = 0;

        if (SampleFrames < 1)
            SampleFrames = 1;

        if (ImGui.Button("Start##PluginPanelBenchmark"))
            Start();

        ImGui.SameLine();
        if (ImGui.Button("Stop##PluginPanelBenchmark"))
            Stop();

        ImGui.SameLine();
        if (ImGui.Button("Reset##PluginPanelBenchmark"))
            Reset();

        ImGui.Text($"Running: {_session.IsRunning}");
        ImGui.Text($"Warmup Remaining: {_session.WarmupFramesRemaining}");
        ImGui.Text($"Samples: {_session.RecordedSampleCount}/{_session.TargetSampleCount}");
        ImGui.Text($"Last: {_session.LastMilliseconds:F4} ms");
        ImGui.Text($"Average: {_session.AverageMilliseconds:F4} ms");
        ImGui.Text($"Max: {_session.MaxMilliseconds:F4} ms");
        ImGui.Text($"P95: {_session.P95Milliseconds:F4} ms");
        ImGui.Text($"P99: {_session.P99Milliseconds:F4} ms");
        if (_exporter.LastCsvPath is not null)
            ImGui.TextWrapped($"Last CSV: {_exporter.LastCsvPath}");
        if (_exporter.LastJsonPath is not null)
            ImGui.TextWrapped($"Last JSON: {_exporter.LastJsonPath}");
        if (_exporter.LastMarkdownPath is not null)
            ImGui.TextWrapped($"Last Markdown: {_exporter.LastMarkdownPath}");
        ImGui.Separator();
        ImGui.Text($"Selected Scenario: {GetScenarioLabel(Scenario)}");
        ImGui.TextUnformatted("Benchmark panel output:");

        var startTimestamp = Stopwatch.GetTimestamp();
        _panel.Draw();
        var elapsedTicks = Stopwatch.GetTimestamp() - startTimestamp;

        var wasRunning = _session.IsRunning;
        var sample = _session.RecordFrame(elapsedTicks);
        if (sample.HasValue)
            _exporter.RecordSample(sample.Value);
        if (wasRunning && !_session.IsRunning)
            _exporter.CompleteRun(_session, "Completed");

        ImGui.End();
    }

    /// <summary>
    /// Completes any active export run and disposes the owned benchmark panel when applicable.
    /// </summary>
    /// <remarks>
    /// After disposal, benchmark operations throw through <see cref="ThrowIfDisposed"/>.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        CompleteActiveRun("Disposed");
        if (_ownsPanel)
            _panel.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Returns the stable exported label for <paramref name="scenario"/>.
    /// </summary>
    /// <param name="scenario">The benchmark scenario to label.</param>
    /// <returns>The scenario label written into exported benchmark artifacts.</returns>
    public static string GetScenarioLabel(PluginPanelBenchmarkScenario scenario)
        => scenario switch
        {
            PluginPanelBenchmarkScenario.CollapsedPanel => "CollapsedPanel",
            PluginPanelBenchmarkScenario.ExpandedPanel => "ExpandedPanel",
            _ => "Interactive"
        };

    /// <summary>
    /// Throws when the benchmark host has already been disposed.
    /// </summary>
    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, typeof(PluginPanelBenchmark));
}
#endif
