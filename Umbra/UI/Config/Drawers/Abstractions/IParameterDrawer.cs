using Umbra.Config;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the contract for a custom parameter drawer with full row-layout control.
/// </summary>
/// <remarks>
/// Use this interface with <see cref="Umbra.Config.Attributes.UmbraDrawerAttribute{TDrawer}"/> when a parameter needs custom label rendering or a non-standard row structure. For custom widgets that should still participate in Umbra's standard two-column layout, use <see cref="ITwoColumnParameterDrawer"/> instead. Property-level wrapper attributes such as <see cref="Umbra.Config.Attributes.UmbraHideIfAttribute{T}"/> and <see cref="Umbra.Config.Attributes.UmbraDisableIfAttribute{T}"/> are still honored around the drawer output by the surrounding configuration-drawer pipeline.
/// </remarks>
public interface IParameterDrawer : IDisposable
{
    /// <summary>
    /// Draws the ImGui control for the specified parameter.
    /// </summary>
    /// <remarks>
    /// All widget IDs are scoped by the owning <see cref="ConfigDrawer{TConfig}"/>. Drawers can therefore use a local ID such as <c>$"##{parameter.Key}"</c> without adding extra cross-plugin uniqueness logic.
    /// </remarks>
    /// <param name="label">The human-readable label associated with the parameter.</param>
    /// <param name="parameter">The parameter to render and edit.</param>
    void Draw(string label, IParameter parameter);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <remarks>
    /// The default implementation calls <see cref="GC.SuppressFinalize(object)"/>. Override it when the drawer owns resources that must be released on plugin unload.
    /// </remarks>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
