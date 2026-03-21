using REFrameworkNET;
using REFrameworkNET.Attributes;
using REFrameworkNET.Callbacks;
using Umbra.SamplePlugin.Config;
using Umbra.SDK.Config;
using Umbra.SDK.Config.UI;
using Umbra.SDK.Logging;

namespace Umbra.SamplePlugin;

public static class SamplePlugin
{
    private static readonly PluginLogger _log = new("SamplePlugin");

    private static ConfigDrawer<PluginConfig>? _drawer;
    private static SettingsStore<PluginConfig>? _store;
    private static DeferredSaveController<PluginConfig>? _saveController;

    /// <summary>
    /// Plugin entry point. Resolves the configuration file path, loads persisted settings from disk
    /// (or writes defaults if no file exists), and constructs the ImGui settings drawer.
    /// </summary>
    [PluginEntryPoint]
    public static void Load()
    {
        //System.Diagnostics.Debugger.Launch();

        _log.Info("Loading...");

        var configPath = GetConfigPath();
        _log.Info($"Config path: {configPath}");

        _store = new SettingsStore<PluginConfig>(configPath);
        var config = _store.Load();
        _saveController = new DeferredSaveController<PluginConfig>(_store);

        _drawer = new ConfigDrawer<PluginConfig>(config, idScope: "SamplePlugin");

        _log.Info("Loaded successfully.");
    }

    /// <summary>
    /// Plugin exit point. Persists the current configuration to disk, then disposes and nulls
    /// all static resources to prevent stale state if the plugin is reloaded within the same process session.
    /// </summary>
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

        _drawer?.Dispose();
        _drawer = null;

        _log.Info("Unloaded.");
    }

    /// <summary>
    /// Resolves the absolute path to the plugin's JSON configuration file,
    /// creating the configuration directory if it does not already exist.
    /// </summary>
    /// <returns>
    /// The absolute path to <c>config.json</c> inside the plugin's
    /// <c>&lt;PluginDir&gt;/data/Umbra/SamplePlugin/</c> directory.
    /// </returns>
    private static string GetConfigPath()
    {
        var pluginDir = API.GetPluginDirectory(typeof(SamplePlugin).Assembly);
        var configDir = Path.Combine(pluginDir, "data", "Umbra", nameof(SamplePlugin));

        if (!Directory.Exists(configDir))
        {
            _log.Info($"Config directory not found, creating: {configDir}");
            Directory.CreateDirectory(configDir);
        }

        return Path.Combine(configDir, "config.json");
    }

    /// <summary>
    /// ImGui pre-draw callback. Renders the plugin settings UI each frame when the game's UI draw pass is active.
    /// </summary>
    [Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
    public static void PreDrawUI()
    {
        if (API.IsDrawingUI())
            _drawer?.Draw();

        _saveController?.Tick();
    }
}
