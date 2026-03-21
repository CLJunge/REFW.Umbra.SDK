namespace Umbra.SDK.Config.UI.ParameterDrawers;

/// <summary>
/// Defines a custom parameter widget renderer that participates in the two-column layout.
/// The factory handles label rendering, optional <c>(?)</c> help-marker placement, column
/// alignment, and <c>SetNextItemWidth</c>; the drawer only needs to render the editing widget.
/// </summary>
/// <remarks>
/// <para>
/// Use this interface with <c>[TwoColumnCustomDrawer&lt;TDrawer&gt;]</c> when you need a fully
/// custom control but still want the label aligned with all other parameters in the same
/// category or root scope. For complete layout control (custom label rendering, non-standard
/// row structure), use <see cref="IParameterDrawer"/> with <c>[CustomDrawer&lt;TDrawer&gt;]</c>
/// instead.
/// </para>
/// <para>
/// When <see cref="Draw"/> is called, <c>ImGui.SetNextItemWidth</c> has already been applied
/// (honouring any <c>[ControlWidth]</c> on the parameter, or <c>-1f</c> fill-to-right-edge by
/// default), and the cursor is positioned at the shared column x for the owning scope.
/// The drawer should call its ImGui widget immediately without any additional layout setup.
/// Use <c>$"##{parameter.Key}"</c> as the ImGui widget ID to avoid label collisions.
/// </para>
/// <para>
/// Implements <see cref="IDisposable"/> so drawers that hold per-instance state (e.g. capture
/// counters, cached textures) can clean up when the owning
/// <see cref="ConfigDrawer{TConfig}"/> is disposed.
/// </para>
/// </remarks>
public interface ITwoColumnParameterDrawer : IDisposable
{
    /// <summary>
    /// Renders the editing widget for the parameter. Called each frame after the label
    /// and optional help marker have been drawn and the cursor positioned at the shared
    /// column x. <c>ImGui.SetNextItemWidth</c> is already set; call the ImGui widget directly.
    /// </summary>
    /// <remarks>
    /// All widget IDs are scoped by the owning <see cref="ConfigDrawer{TConfig}"/> via
    /// <c>ImGui.PushID</c> / <c>ImGui.PopID</c>. Use <c>$"##{parameter.Key}"</c>
    /// as the ImGui widget ID; cross-plugin uniqueness is guaranteed without any extra
    /// effort here.
    /// </remarks>
    /// <param name="parameter">The parameter whose value is to be rendered and edited.</param>
    void Draw(IParameter parameter);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <remarks>
    /// Default implementation calls <see cref="GC.SuppressFinalize"/> to prevent a redundant
    /// finalizer call when a concrete class follows the full Dispose pattern. Override when
    /// the drawer holds shared state that must be released on plugin unload
    /// (e.g. a capture-mode counter or a cached resource handle).
    /// </remarks>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
