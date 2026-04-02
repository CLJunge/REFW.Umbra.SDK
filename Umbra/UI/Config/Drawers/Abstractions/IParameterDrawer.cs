using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines a contract for drawing a UI control for a configuration parameter using ImGui.
/// Implements <see cref="IDisposable"/> so drawers that hold captured input state (e.g.
/// <see cref="HotkeyDrawer"/>) can release shared resources when the owning
/// <see cref="ConfigDrawer{TConfig}"/> is disposed. Implementations that hold no
/// per-instance state do not need to override <see cref="IDisposable.Dispose"/>.
/// </summary>
/// <remarks>
/// <para>
/// Use this interface with
/// <see cref="Umbra.Config.Attributes.UmbraDrawerAttribute{TDrawer}"/> (<c>[UmbraDrawer&lt;TDrawer&gt;]</c>)
/// for complete layout control (custom label rendering, non-standard row structure), use
/// <see cref="ITwoColumnParameterDrawer"/> with
/// <see cref="Umbra.Config.Attributes.UmbraTwoColumnDrawerAttribute{TDrawer}"/> 
/// (<c>[UmbraTwoColumnDrawer&lt;TDrawer&gt;]</c>) when you need a fully custom control but still
/// want the label aligned with all other parameters in the same category or root scope instead.
/// </para>
/// </remarks>
public interface IParameterDrawer : IDisposable
{
    /// <summary>
    /// Draws an ImGui control for the specified configuration parameter.
    /// </summary>
    /// <remarks>
    /// All widget IDs are scoped by the owning <see cref="ConfigDrawer{TConfig}"/> via
    /// <see cref="ImGui.PushID(string)"/> / <see cref="ImGui.PopID()"/>. Use <c>$"##{parameter.Key}"</c>
    /// (or any suffix of your choice) as the ImGui widget ID; cross-plugin uniqueness
    /// is guaranteed without any extra effort here.
    /// </remarks>
    /// <param name="label">The human-readable label displayed alongside the control.</param>
    /// <param name="parameter">The configuration parameter to render and interact with.</param>
    void Draw(string label, IParameter parameter);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <remarks>
    /// Default implementation calls <see cref="GC.SuppressFinalize"/> to prevent a redundant
    /// finalizer call when a concrete class follows the full Dispose pattern. Override when
    /// the drawer holds shared state that must be released on plugin unload
    /// (e.g. a capture-mode counter or a cached resource handle).
    /// </remarks>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
