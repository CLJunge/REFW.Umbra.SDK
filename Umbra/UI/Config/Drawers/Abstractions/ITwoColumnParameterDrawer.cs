using Umbra.Config;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the contract for a custom parameter widget that participates in Umbra's two-column layout.
/// </summary>
/// <remarks>
/// The configuration-drawer pipeline renders the label, optional help UI, alignment, and item width before calling <see cref="Draw"/>. Implementations should therefore render only the editing widget itself.
/// </remarks>
public interface ITwoColumnParameterDrawer : IDisposable
{
    /// <summary>
    /// Renders the editing widget for the parameter.
    /// </summary>
    /// <remarks>
    /// All widget IDs are scoped by the owning <see cref="ConfigDrawer{TConfig}"/>. Drawers can therefore use a local ID such as <c>$"##{parameter.Key}"</c> without adding extra cross-plugin uniqueness logic.
    /// </remarks>
    /// <param name="parameter">The parameter whose value is being rendered and edited.</param>
    void Draw(IParameter parameter);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <remarks>
    /// The default implementation calls <see cref="GC.SuppressFinalize(object)"/>. Override it when the drawer owns resources that must be released on plugin unload.
    /// </remarks>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
