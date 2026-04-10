namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the contract for fully custom rendering of a nested configuration group.
/// </summary>
/// <remarks>
/// <para>
/// Prefer applying <see cref="Umbra.Config.Attributes.UmbraNestedDrawerAttribute{TDrawer}"/> to the parent property that exposes the nested group. When <see cref="ConfigDrawer{TConfig}"/> encounters the attribute on the property or, as a fallback, on the nested type, it bypasses the default recursive expansion and hands the group instance directly to this drawer each frame.
/// </para>
/// <para>
/// The drawer has full ImGui layout control. Property-level wrapper attributes such as category, collapse, spacing, hide rules, and disable rules are still honored around the drawer output by the surrounding configuration-drawer pipeline.
/// </para>
/// </remarks>
/// <typeparam name="T">The nested configuration-group type rendered by the drawer.</typeparam>
public interface INestedDrawer<T> : IDisposable
{
    /// <summary>
    /// Renders ImGui content for the provided nested configuration-group instance.
    /// </summary>
    /// <param name="groupInstance">The non-null nested group instance to render.</param>
    void Draw(T groupInstance);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <remarks>
    /// The default implementation calls <see cref="GC.SuppressFinalize(object)"/>. Override it when the drawer owns resources that must be released on plugin unload.
    /// </remarks>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
