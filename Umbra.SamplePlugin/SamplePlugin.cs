using REFrameworkNET;
using REFrameworkNET.Attributes;
using REFrameworkNET.Callbacks;
using Umbra.Config;
using Umbra.Input;
using Umbra.Logging;
using Umbra.SamplePlugin.Config;
using Umbra.UI.Config;
using Umbra.UI.Panel;

namespace Umbra.SamplePlugin;

/// <summary>
/// Sample REFramework.NET plugin that demonstrates Umbra settings registration,
/// automatic deferred persistence, and panel-based ImGui rendering.
/// </summary>
/// <remarks>
/// The sample keeps its logger, settings store, deferred-save controller, and panel in static
/// fields because REFramework plugin entry points and callbacks are static. The implementation is
/// intentionally small so each concern in the Umbra plugin lifecycle is easy to map back to the
/// corresponding SDK feature.
/// </remarks>
public static class SamplePlugin
{
    private static readonly PluginLogger _log = new("SamplePlugin");

    private static PluginPanel? _panel;
    private static SettingsStore<PluginConfig>? _store;
    private static DeferredSaveController<PluginConfig>? _saveController;

    /// <summary>
    /// Plugin entry point. Resolves the configuration file path, loads persisted settings from disk
    /// (or writes defaults if no file exists), wires the sample button action, starts deferred
    /// save handling, and constructs the plugin panel.
    /// </summary>
    /// <remarks>
    /// The sample binds the <see cref="PluginConfig.LogTestMessage"/> button action after
    /// <see cref="SettingsStore{TConfig}.Load()"/> so the persisted configuration instance owns the
    /// callback invoked by the settings UI.
    /// </remarks>
    [PluginEntryPoint]
    public static void Load()
    {
#if DEBUG
        //System.Diagnostics.Debugger.Launch();
#endif

        _log.Info("Loading...");

        var configPath = GetConfigPath();
        _log.Info($"Config path: {configPath}");

        _store = new SettingsStore<PluginConfig>(configPath);
        var config = _store.Load();
        config.LogTestMessage.Value = () => _log.Info("Sample Plugin is active!");
        _saveController = new DeferredSaveController<PluginConfig>(_store);

        _panel = new PluginPanel("SamplePlugin")
            .Add(new ConfigSection<PluginConfig>(config));

        _log.Info("Loaded successfully.");
    }

    /// <summary>
    /// Plugin exit point. Flushes and disposes deferred-save handling before the settings store,
    /// performs a final explicit save, then disposes and nulls all static resources to prevent
    /// stale state if the plugin is reloaded in the same process session.
    /// </summary>
    /// <remarks>
    /// The explicit flush before disposal ensures that a pending debounce interval does not drop
    /// the user's last in-memory edits when the plugin unloads.
    /// </remarks>
    [PluginExitPoint]
    public static void Unload()
    {
        _log.Info("Unloading...");

        _saveController?.Flush();
        _saveController?.Dispose();
        _saveController = null;

        _store?.Save();
        _store?.Dispose();
        _store = null;

        _panel?.Dispose();
        _panel = null;

        _log.Info("Unloaded.");
    }

    /// <summary>
    /// Resolves the absolute path to the plugin's JSON configuration file.
    /// </summary>
    /// <returns>
    /// The absolute path to <c>config.json</c> inside the plugin's
    /// <c>&lt;PluginDir&gt;/data/Umbra/SamplePlugin/</c> directory.
    /// </returns>
    private static string GetConfigPath()
        => Path.Combine(GetConfigDirectoryPath(), "config.json");

    /// <summary>
    /// Resolves the absolute path to the sample plugin's configuration directory and ensures it exists.
    /// </summary>
    /// <returns>The absolute path to the sample plugin configuration directory.</returns>
    private static string GetConfigDirectoryPath()
    {
        var pluginDir = API.GetPluginDirectory(typeof(SamplePlugin).Assembly);
        var configDir = Path.Combine(pluginDir, "data", "Umbra", nameof(SamplePlugin));
        EnsureConfigDirectoryExists(configDir);
        return configDir;
    }

    /// <summary>
    /// Creates <paramref name="configDir"/> when it does not already exist.
    /// </summary>
    /// <param name="configDir">The absolute configuration directory path.</param>
    private static void EnsureConfigDirectoryExists(string configDir)
    {
        if (Directory.Exists(configDir))
            return;

        _log.Info($"Config directory not found, creating: {configDir}");
        Directory.CreateDirectory(configDir);
    }

    /// <summary>
    /// ImGui pre-draw callback. Delegates drawing and deferred-save ticking to focused helpers so
    /// UI rendering and persistence timing remain separate concerns.
    /// </summary>
    /// <remarks>
    /// This callback runs on every REFramework UI draw pass. The helper split keeps the callback
    /// itself declarative while preserving the sample's one-responsibility-per-method layout.
    /// </remarks>
    [Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
    public static void PreDrawUI()
    {
#if DEBUG
        if (!System.Diagnostics.Debugger.IsAttached
            && KeyboardInput.IsCtrlHeld && KeyboardInput.IsShiftHeld
            && KeyboardInput.TryCaptureKeyboardKey(out var capturedKey)
            && capturedKey == (int)Hexa.NET.ImGui.ImGuiKey.F12)
        {
            _log.Info("Ctrl + Shift + F12 detected, attaching debugger...");

            System.Diagnostics.Debugger.Launch();
        }

#endif
        DrawPanelIfUiIsActive();
        TickDeferredSaveController();
    }

    /// <summary>
    /// Draws the plugin panel only while the REFramework UI draw pass is active.
    /// </summary>
    private static void DrawPanelIfUiIsActive()
    {
        if (API.IsDrawingUI())
            _panel?.Draw();
    }

    /// <summary>
    /// Advances deferred-save timing so pending configuration changes can flush to disk.
    /// </summary>
    private static void TickDeferredSaveController()
        => _saveController?.Tick();
}
