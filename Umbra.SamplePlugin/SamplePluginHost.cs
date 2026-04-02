using REFrameworkNET;
using REFrameworkNET.Attributes;
using REFrameworkNET.Callbacks;
using Umbra.Runtime;

namespace Umbra.SamplePlugin;

/// <summary>
/// Static REFramework entry points for <see cref="SamplePlugin"/>.
/// </summary>
/// <remarks>
/// The host keeps the live plugin instance in the shared runtime helper because REFramework
/// requires static entry points and callbacks. The plugin itself remains fully instance based.
/// </remarks>
[UmbraPlugin]
public static class SamplePluginHost
{
    private static readonly PluginHost<SamplePlugin> _host = new(
        typeof(SamplePlugin),
        static () => new SamplePlugin());

    /// <summary>
    /// Plugin entry point. Acquires the plugin's AppDomain-local single-instance mutex, constructs
    /// the live plugin instance, and then runs its initialization logic.
    /// </summary>
    [PluginEntryPoint]
    public static void Load()
    {
#if DEBUG
        //System.Diagnostics.Debugger.Launch();
#endif

        _host?.Load();
    }

    /// <summary>
    /// Plugin exit point. Flushes and disposes the live instance, then releases the mutex.
    /// </summary>
    /// <remarks>
    /// The mutex is released even if shutdown throws, so a later reload is not blocked by a
    /// partially failed cleanup path.
    /// </remarks>
    [PluginExitPoint]
    public static void Unload()
        => _host?.Unload();

    /// <summary>
    /// Ingame update callback. Dispatches to the live plugin instance when one is loaded.
    /// </summary>
    [Callback(typeof(UpdateBehavior), CallbackType.Pre)]
    public static void OnPreUpdateBehavior() => _host?.OnPreUpdateBehavior();

    /// <summary>
    /// ImGui pre-draw callback. Dispatches to the live plugin instance when one is loaded.
    /// </summary>
    /// <remarks>
    /// This callback is a no-op after unload or before load completes.
    /// </remarks>
    [Callback(typeof(ImGuiDrawUI), CallbackType.Pre)]
    public static void OnPreImGuiDrawUI() => _host?.OnPreImGuiDrawUI();

    /// <summary>
    /// ImGui pre-renderer callback. Dispatches to the live plugin instance when one is loaded.
    /// </summary>
    [Callback(typeof(ImGuiRender), CallbackType.Pre)]
    public static void OnPreImGuiRenderer() => _host?.OnPreImGuiRenderer();
}
