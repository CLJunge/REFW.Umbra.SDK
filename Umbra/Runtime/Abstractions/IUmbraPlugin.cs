namespace Umbra.Runtime;

/// <summary>
/// Defines the lifecycle contract for an Umbra plugin instance.
/// </summary>
/// <remarks>
/// The managed REFramework host still requires static entry points, but those entry points should
/// only forward into a plugin instance that implements this interface. Implementations are expected
/// to keep all mutable plugin state on the instance itself. Additional REFramework engine callback
/// hooks are exposed in the companion partial declaration of this interface as optional default
/// no-op members named <c>On&lt;CallbackName&gt;()</c>.
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
    /// Performs custom logic immediately before the ImGui UI is drawn.
    /// </summary>
    void OnPreImGuiDrawUI();
}
