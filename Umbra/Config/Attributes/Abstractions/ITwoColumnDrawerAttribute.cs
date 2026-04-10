using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Exposes the drawer type declared by <see cref="UmbraTwoColumnDrawerAttribute{TDrawer}"/> without requiring generic attribute inspection.
/// </summary>
/// <remarks>
/// Umbra's metadata pipeline uses this marker to detect two-column custom drawers while preserving the standard label layout. Plugin authors do not implement this interface directly.
/// </remarks>
internal interface ITwoColumnDrawerAttribute
{
    /// <summary>
    /// Gets the concrete <see cref="ITwoColumnParameterDrawer"/> type declared for the annotated member.
    /// </summary>
    /// <value>The drawer type used to render the parameter's editing widget.</value>
    Type DrawerType { get; }
}
