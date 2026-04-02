namespace Umbra.Runtime;

/// <summary>
/// Defines the lifecycle contract for an Umbra plugin instance.
/// </summary>
/// <remarks>
/// The managed REFramework host still requires static entry points, but those entry points should
/// only forward into a plugin instance that implements this interface. Implementations are expected
/// to keep all mutable plugin state on the instance itself. Plugins may optionally expose additional
/// REFramework engine callback handlers using an <c>On&lt;CallbackName&gt;()</c> naming convention,
/// but such callbacks are not defined or required by this interface.
/// </remarks>
public partial interface IUmbraPlugin
{
    /// <summary>
    /// Performs one-time plugin startup work after the host has acquired the plugin mutex.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Performs plugin shutdown work before the host releases the plugin mutex.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Performs custom logic before the behavior update phase of the game loop.
    /// </summary>
    /// <remarks>
    /// Use this callback to implement any custom behavior that should run before the game updates its
    /// internal behavior state.
    /// </remarks>
    void OnPreUpdateBehavior();

    /// <summary>
    /// Performs custom logic immediately before the ImGui UI is drawn.
    /// </summary>
    /// <remarks>
    /// Use this callback to draw ImGui elements.
    /// </remarks>
    void OnPreImGuiDrawUI();

    /// <summary>
    /// Performs custom logic immediately before the ImGui renderer executes its draw calls.
    /// </summary>
    /// <remarks>
    /// Use this callback to draw an ingame overlay using ImGui elements.
    /// </remarks>
    void OnPreImGuiRenderer();
}
