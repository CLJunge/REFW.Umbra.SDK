using REFrameworkNET;
using Umbra.Config;
using Umbra.Logging;
using Umbra.Runtime;
using Umbra.SamplePlugin.Config;
using Umbra.UI.Config;
using Umbra.UI.Panel;
#if BENCHMARK
using Umbra.UI.Panel.Benchmark;
#endif

namespace Umbra.SamplePlugin;

/// <summary>
/// Sample REFramework.NET plugin instance that demonstrates Umbra settings registration,
/// automatic deferred persistence, panel-based ImGui rendering, and optional reusable
/// plugin-panel benchmarking in <c>BENCHMARK</c> builds.
/// </summary>
/// <remarks>
/// This type is fully instance based. Static REFramework entry points and callbacks live in
/// <see cref="SamplePluginHost"/> so this class owns only its per-plugin state and behavior. The
/// static host also owns single-instance coordination, while this type inherits shared logger
/// plumbing from <see cref="UmbraPlugin"/>.
/// </remarks>
public sealed class SamplePlugin : UmbraPlugin
{
    private const string _runtimePanelScope = "SamplePlugin.RuntimePanel";
    private const string _runtimeSectionScope = "SamplePlugin.RuntimeConfigSection";
#if BENCHMARK
    private const string _benchmarkPanelScope = "SamplePlugin.BenchmarkPanel";
    private const string _benchmarkSectionScope = "SamplePlugin.BenchmarkConfigSection";
#endif

    private static readonly PluginLogger _log = new("SamplePlugin");
    private PluginPanel? _panel;
#if BENCHMARK
    private PluginPanel? _benchmarkPanel;
    private PluginPanelBenchmark? _panelBenchmark;
#endif
    private SettingsStore<PluginConfig>? _store;
    private DeferredSaveController<PluginConfig>? _saveController;
    private PluginConfig? _config;

    /// <summary>
    /// Initialises a new plugin instance with its dedicated plugin logger.
    /// </summary>
    public SamplePlugin() : base(_log) { }

    /// <summary>
    /// Performs the sample plugin's real initialization work after the runtime host acquires the
    /// mutex and constructs the instance.
    /// </summary>
    public override void Initialize()
    {
        Log.Info("Loading...");

        var configPath = GetConfigPath();
        Log.Info($"Config path: {configPath}");

        _store = new SettingsStore<PluginConfig>(configPath);
        _config = _store.Load();
        _config.LogTestMessage.Value = () => Log.Info("Sample Plugin is active!");

        _saveController = new DeferredSaveController<PluginConfig>(_store);
        _panel = CreateRuntimePanel(_config);
#if BENCHMARK
        _benchmarkPanel = CreateBenchmarkPanel(_config);
        _panelBenchmark = new PluginPanelBenchmark(
            "Sample Plugin Panel Benchmark",
            _benchmarkPanel,
            GetBenchmarkDirectoryPath());
#endif

        Log.Info("Loaded successfully.");
    }

    /// <summary>
    /// Performs the sample plugin's shutdown work before the runtime host releases the mutex.
    /// </summary>
    public override void Shutdown()
    {
        Log.Info("Unloading...");

#if BENCHMARK
        _panelBenchmark?.CompleteActiveRun("PluginUnload");
        _panelBenchmark?.Dispose();
        _panelBenchmark = null;

        _benchmarkPanel?.Dispose();
        _benchmarkPanel = null;
#endif

        _panel?.Dispose();
        _panel = null;

        _saveController?.Flush();
        _saveController?.Dispose();
        _saveController = null;

        _store?.Save();
        _store?.Dispose();
        _store = null;

        _config = null;

        Log.Info("Unloaded.");
    }

    /// <summary>
    /// Performs pre-update logic for the behavior, including handling debug key input.
    /// </summary>
    /// <remarks>
    /// In debug builds, this method checks for the Ctrl+Shift+F12 key combination and launches the
    /// debugger if detected. This allows for convenient debugging during development.
    /// </remarks>
    public override void OnPreUpdateBehavior()
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached
            && Input.KeyboardInput.IsCtrlHeld && Input.KeyboardInput.IsShiftHeld
            && Input.KeyboardInput.TryCaptureKeyboardKey(out var capturedKey)
            && capturedKey == (int)Hexa.NET.ImGui.ImGuiKey.F12)
        {
            Log.Info("Ctrl + Shift + F12 detected, attaching debugger...");
            System.Diagnostics.Debugger.Launch();
        }
#endif
    }

    /// <summary>
    /// Renders the sample plugin UI and advances deferred persistence when the REFramework UI pass
    /// is active.
    /// </summary>
    public override void OnPreImGuiDrawUI()
    {
        DrawUiIfActive();
        TickDeferredSaveController();
    }

    /// <summary>
    /// Resolves the absolute path to the plugin's JSON configuration file.
    /// </summary>
    /// <returns>
    /// The absolute path to <c>config.json</c> inside the plugin's
    /// <c>&lt;PluginDir&gt;/data/Umbra/SamplePlugin/</c> directory.
    /// </returns>
    private string GetConfigPath()
        => Path.Combine(GetConfigDirectoryPath(), "config.json");

    /// <summary>
    /// Resolves the absolute path to the directory where panel benchmark artifacts are written.
    /// </summary>
    /// <returns>
    /// The absolute path to the panel benchmark artifact directory under the sample plugin's data
    /// folder.
    /// </returns>
#if BENCHMARK
    private string GetBenchmarkDirectoryPath()
        => Path.Combine(GetConfigDirectoryPath(), "artifacts", "perf", "runtime", "panel-draw");
#endif

    /// <summary>
    /// Resolves the absolute path to the sample plugin's configuration directory and ensures it exists.
    /// </summary>
    /// <returns>The absolute path to the sample plugin configuration directory.</returns>
    private string GetConfigDirectoryPath()
    {
        var pluginDir = API.GetPluginDirectory(GetType().Assembly);
        var configDir = Path.Combine(pluginDir, "data", "Umbra", nameof(SamplePlugin));
        EnsureConfigDirectoryExists(configDir);
        return configDir;
    }

    /// <summary>
    /// Creates <paramref name="configDir"/> when it does not already exist.
    /// </summary>
    /// <param name="configDir">The absolute configuration directory path.</param>
    private void EnsureConfigDirectoryExists(string configDir)
    {
        if (Directory.Exists(configDir))
            return;

        Log.Info($"Config directory not found, creating: {configDir}");
        Directory.CreateDirectory(configDir);
    }

    /// <summary>
    /// Builds the plugin's normal runtime panel.
    /// </summary>
    /// <param name="config">The loaded config instance shared by the panel sections.</param>
    /// <returns>The runtime panel.</returns>
    private static PluginPanel CreateRuntimePanel(PluginConfig config)
        => new PluginPanel(_runtimePanelScope)
            .Add(new ConfigSection<PluginConfig>(config, _runtimeSectionScope));

    /// <summary>
    /// Builds a duplicate panel for isolated benchmark measurement.
    /// </summary>
    /// <remarks>
    /// The benchmark panel uses the same backing config object as the runtime panel, but it owns a
    /// separate section instance and unique ImGui scopes so widget IDs do not collide when both are
    /// rendered in the same frame.
    /// </remarks>
    /// <param name="config">The loaded config instance shared by the benchmark section.</param>
    /// <returns>The benchmark panel.</returns>
#if BENCHMARK
    private static PluginPanel CreateBenchmarkPanel(PluginConfig config)
        => new PluginPanel(_benchmarkPanelScope)
            .Add(new ConfigSection<PluginConfig>(config, _benchmarkSectionScope));
#endif

    /// <summary>
    /// Draws the runtime panel and the reusable benchmark host window only while the REFramework UI
    /// pass is active.
    /// </summary>
    private void DrawUiIfActive()
    {
        if (!API.IsDrawingUI())
            return;

#if BENCHMARK
        if (_panelBenchmark is null || !_panelBenchmark.ShouldSuppressRuntimePanel)
            _panel?.Draw();

        _panelBenchmark?.DrawWindow();
#else
        _panel?.Draw();
#endif
    }

    /// <summary>
    /// Advances deferred-save timing so pending configuration changes can flush to disk.
    /// </summary>
    private void TickDeferredSaveController()
        => _saveController?.Tick();
}
