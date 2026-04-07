namespace Umbra;

/// <summary>
/// Defines the instance lifecycle and forwarded frame callbacks consumed by Umbra's plugin-hosting APIs.
/// </summary>
/// <remarks>
/// Static REFramework entry points still live on an assembly-facing host type, but they should delegate their work to a plugin instance that implements <see cref="IUmbraPlugin"/>. Single-instance coordination is typically handled by <see cref="PluginHost{TPlugin}"/> or <see cref="PluginBootstrapper"/>, while implementations keep mutable plugin state on the instance itself.
/// </remarks>
public partial interface IUmbraPlugin
{
    /// <summary>
    /// Performs one-time startup work after the runtime host has acquired the plugin's single-instance mutex.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Performs shutdown work before the runtime host releases the plugin's single-instance mutex.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Runs plugin logic before the game's behavior update phase when the static host forwards that callback.
    /// </summary>
    void OnPreUpdateBehavior();

    /// <summary>
    /// Runs plugin ImGui UI logic before the REFramework UI draw pass when the static host forwards that callback.
    /// </summary>
    void OnPreImGuiDrawUI();

    /// <summary>
    /// Runs plugin overlay drawing logic before the ImGui renderer submits draw calls when the static host forwards that callback.
    /// </summary>
    void OnPreImGuiRenderer();
}
