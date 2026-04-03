using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Exposes the nested-group drawer type declared by <see cref="UmbraNestedDrawerAttribute{TDrawer}"/> without requiring generic attribute inspection.
/// </summary>
/// <remarks>
/// Umbra's configuration-drawer builder uses this marker to discover nested-group drawers declared either on the parent property or on the nested-group type itself. Plugin authors do not implement this interface directly.
/// </remarks>
internal interface INestedDrawerAttribute
{
    /// <summary>
    /// Gets the concrete <see cref="INestedDrawer{TGroup}"/> type declared for the annotated nested-group scope.
    /// </summary>
    /// <value>The drawer type used to render the nested group.</value>
    Type DrawerType { get; }
}
