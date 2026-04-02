using REFrameworkNET;
using Umbra.Config;
using Umbra.Input;
using Umbra.Logging;
using Umbra.Runtime;
using Umbra.SamplePlugin.Config;
using Umbra.UI.Config;
using Umbra.UI.Panel;

namespace Umbra.SamplePlugin;

/// <summary>
/// Sample REFramework.NET plugin instance that demonstrates Umbra settings registration,
/// automatic deferred persistence, and panel-based ImGui rendering.
/// </summary>
/// <remarks>
/// This type is fully instance based. Static REFramework entry points and callbacks live in
/// <see cref="SamplePluginHost"/> so this class owns only its per-plugin state and behavior. The
/// mutex metadata lives on the static host, while this type inherits shared logger plumbing from
/// <see cref="UmbraPlugin"/>.
/// </remarks>
public sealed class SamplePlugin : UmbraPlugin
{
    private static readonly PluginLogger _log = new("SamplePlugin");

    private PluginPanel? _panel;
    private SettingsStore<PluginConfig>? _store;
    private DeferredSaveController<PluginConfig>? _saveController;

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
        var config = _store.Load();
        config.LogTestMessage.Value = () => Log.Info("Sample Plugin is active!");
        _saveController = new DeferredSaveController<PluginConfig>(_store);

        _panel = new PluginPanel("SamplePlugin")
            .Add(new ConfigSection<PluginConfig>(config));

        Log.Info("Loaded successfully.");
    }

    /// <summary>
    /// Performs the sample plugin's shutdown work before the runtime host releases the mutex.
    /// </summary>
    public override void Shutdown()
    {
        Log.Info("Unloading...");

        _saveController?.Flush();
        _saveController?.Dispose();
        _saveController = null;

        _store?.Save();
        _store?.Dispose();
        _store = null;

        _panel?.Dispose();
        _panel = null;

        Log.Info("Unloaded.");
    }

    public override void OnPreUpdateBehavior()
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached
            && KeyboardInput.IsCtrlHeld && KeyboardInput.IsShiftHeld
            && KeyboardInput.TryCaptureKeyboardKey(out var capturedKey)
            && capturedKey == (int)Hexa.NET.ImGui.ImGuiKey.F12)
        {
            Log.Info("Ctrl + Shift + F12 detected, attaching debugger...");

            System.Diagnostics.Debugger.Launch();
        }
#endif
    }

    /// <summary>
    /// Renders the plugin UI and advances deferred persistence when the REFramework UI pass is active.
    /// </summary>
    public override void OnPreImGuiDrawUI()
    {
        DrawPanelIfUiIsActive();
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
    /// Draws the plugin panel only while the REFramework UI draw pass is active.
    /// </summary>
    private void DrawPanelIfUiIsActive()
    {
        if (API.IsDrawingUI())
            _panel?.Draw();
    }

    /// <summary>
    /// Advances deferred-save timing so pending configuration changes can flush to disk.
    /// </summary>
    private void TickDeferredSaveController()
        => _saveController?.Tick();
}
