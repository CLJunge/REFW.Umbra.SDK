namespace Umbra.Config.UI.ParameterDrawers;

/// <summary>
/// Defines the contract for fully custom rendering of a nested configuration group.
/// </summary>
/// <remarks>
/// <para>
/// Apply <c>[NestedGroupDrawer&lt;TDrawer&gt;]</c> to the nested configuration class to bypass the
/// default recursive parameter expansion performed by <see cref="ConfigDrawer{TConfig}"/> and
/// hand the group instance directly to this drawer each frame.
/// </para>
/// <para>
/// The drawer has complete ImGui layout control; no label, column alignment, or section header
/// is emitted by the factory. Property-level attributes such as <c>[Category]</c>,
/// <c>[SpacingBefore]</c>, <c>[SpacingAfter]</c>, and <c>[HideIf]</c> on the declaration of the
/// nested-group property in the parent config are still honoured. <c>[CollapseAsTree]</c> is
/// applied to the nested group <em>type</em> itself rather than the parent property.
/// </para>
/// <para>
/// Implements <see cref="IDisposable"/> so drawers that hold per-instance state (e.g. cached
/// textures, capture counters) can clean up when the owning
/// <see cref="ConfigDrawer{TConfig}"/> is disposed.
/// </para>
/// </remarks>
/// <typeparam name="T">The concrete nested configuration group type this drawer renders.</typeparam>
public interface INestedGroupDrawer<T> : IDisposable
{
    /// <summary>
    /// Renders ImGui controls for the provided nested configuration group instance.
    /// </summary>
    /// <remarks>
    /// The instance is guaranteed to be non-<see langword="null"/> when this method is called.
    /// </remarks>
    /// <param name="groupInstance">The strongly-typed nested configuration group instance to render.</param>
    void Draw(T groupInstance);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <remarks>
    /// Default implementation calls <see cref="GC.SuppressFinalize"/> to prevent a redundant
    /// finalizer call. Override when the drawer holds state that must be released on plugin
    /// unload (e.g. a capture-mode counter or a cached resource handle).
    /// </remarks>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
