using REFrameworkNET;
using Umbra.Config;
using Umbra.Logging;
using Umbra.SamplePlugin.Config;
using Umbra.UI.Config;
using Umbra.UI.Config.Transfer;
using Umbra.UI.Panel;
using Umbra.UI.Toast;
#if BENCHMARK
using Umbra.UI.Panel.Benchmark;
#endif

namespace Umbra.SamplePlugin;

/// <summary>
/// Sample REFramework.NET plugin instance that demonstrates Umbra config registration,
/// automatic persistence, panel-based ImGui rendering, and optional reusable
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
    private ConfigStore<PluginConfig>? _store;
    private PluginConfig? _config;

    /// <summary>
    /// Initializes a new plugin instance with its dedicated plugin logger.
    /// </summary>
    public SamplePlugin() : base(_log) { }

    /// <summary>
    /// Performs the sample plugin's real initialization work after the runtime host acquires the
    /// mutex and constructs the instance.
    /// </summary>
    public override void Initialize()
    {
        Log.Info("Loading...");

        (_config, _store) = LoadConfig();
        InitializeRuntimePanel(_config, _store);
#if BENCHMARK
        InitializeBenchmarking(_config);
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
        RunShutdownStep("complete active benchmark run", CompleteActiveBenchmarkRun);
        RunShutdownStep("dispose panel benchmark", DisposePanelBenchmark);
        RunShutdownStep("dispose benchmark panel", DisposeBenchmarkPanel);
#endif

        RunShutdownStep("dispose runtime panel", DisposeRuntimePanel);
        RunShutdownStep("save config store", SaveConfigStore);
        RunShutdownStep("dispose config store", DisposeConfigStore);

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
            && capturedKey == (int)Input.UmbraKey.F12)
        {
            Log.Info("Ctrl + Shift + F12 detected, attaching debugger...");
            System.Diagnostics.Debugger.Launch();
        }

        if (Input.KeyboardInput.IsCtrlHeld && Input.KeyboardInput.IsShiftHeld
            && Input.KeyboardInput.TryCaptureKeyboardKey(out capturedKey)
            && capturedKey == (int)Input.UmbraKey.F11)
        {
            Log.Info("Ctrl + Shift + F11 detected, posting test toast notifications...");
            ToastQueue.Push("This is a test info toast.");
            ToastQueue.Push("This is a test warning toast.", ToastLevel.Warning);
            ToastQueue.Push("This is a test error toast.", ToastLevel.Error);
            ToastQueue.Push("This is a test success toast.", ToastLevel.Success);
            ToastQueue.Push("This is a test toast with a custom duration of 5 seconds.", ToastLevel.Info, TimeSpan.FromSeconds(5));
            ToastQueue.Push("This is a test toast with a custom duration of 1 second.", ToastLevel.Info, TimeSpan.FromSeconds(1));
        }
#endif
    }

    /// <summary>
    /// Renders the sample plugin UI when the REFramework UI pass is active.
    /// </summary>
    public override void OnPreImGuiDrawUI() => DrawUiIfActive();

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
    /// Creates and loads the sample plugin config store, then binds runtime-backed sample actions.
    /// </summary>
    /// <returns>The loaded config instance and its owning config store.</returns>
    private (PluginConfig Config, ConfigStore<PluginConfig> Store) LoadConfig()
    {
        var configPath = GetConfigPath();
        Log.Info($"Config path: {configPath}");

        var store = new ConfigStore<PluginConfig>(configPath);
        var config = store.Load();
        PluginConfigActionBinder.Bind(config, store, Log);

        return (config, store);
    }

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
    /// Creates the runtime panel for the loaded sample config.
    /// </summary>
    /// <remarks>
    /// Batch-undo wrapping for reset actions is handled automatically by the undo stack via
    /// <see cref="Config.Attributes.UmbraBatchUndoAttribute"/> on the reset properties.
    /// </remarks>
    /// <param name="config">The loaded config instance.</param>
    /// <param name="store">The loaded config store.</param>
    private void InitializeRuntimePanel(PluginConfig config, ConfigStore<PluginConfig> store)
    {
        var section = CreateRuntimeSection(config, store);
        _panel = new PluginPanel(_runtimePanelScope).Add(section);
    }

    /// <summary>
    /// Builds the config section for the runtime panel.
    /// </summary>
    /// <param name="config">The loaded config instance shared by the panel sections.</param>
    /// <param name="store">The loaded config store used for event-driven persistence, transfer UI, and undo support.</param>
    /// <returns>The config section with undo, search, and transfer support.</returns>
    private static ConfigSection<PluginConfig> CreateRuntimeSection(PluginConfig config, ConfigStore<PluginConfig> store)
    {
        var toast = new PluginToast("Sample Plugin", TimeSpan.FromSeconds(2));
        return ConfigSection<PluginConfig>.CreateWithStore(
            config,
            store,
            new ConfigDrawerOptions
            {
                Search = new UI.Config.Search.ConfigSearchOptions(),
                Transfer = new ConfigTransferOptions { Enabled = true },
                Undo = new ConfigUndoOptions() { Toast = toast }
            },
            _runtimeSectionScope);
    }

    /// <summary>
    /// Creates the benchmark panel and benchmark host for isolated panel-draw measurement.
    /// </summary>
    /// <param name="config">The loaded config instance shared by the benchmark section.</param>
#if BENCHMARK
    private void InitializeBenchmarking(PluginConfig config)
    {
        _benchmarkPanel = CreateBenchmarkPanel(config);
        _panelBenchmark = new PluginPanelBenchmark(
            "Sample Plugin Panel Benchmark",
            _benchmarkPanel,
            GetBenchmarkDirectoryPath());
    }

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

#if BENCHMARK
    private void CompleteActiveBenchmarkRun()
        => _panelBenchmark?.CompleteActiveRun("PluginUnload");

    private void DisposePanelBenchmark()
    {
        var panelBenchmark = _panelBenchmark;
        _panelBenchmark = null;
        panelBenchmark?.Dispose();
    }

    private void DisposeBenchmarkPanel()
    {
        var benchmarkPanel = _benchmarkPanel;
        _benchmarkPanel = null;
        benchmarkPanel?.Dispose();
    }
#endif

    private void DisposeRuntimePanel()
    {
        var panel = _panel;
        _panel = null;
        panel?.Dispose();
    }

    private void SaveConfigStore()
        => _store?.Save();

    private void DisposeConfigStore()
    {
        var store = _store;
        _store = null;
        store?.Dispose();
    }
}
