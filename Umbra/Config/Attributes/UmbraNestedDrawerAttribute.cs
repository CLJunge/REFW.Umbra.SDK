using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the custom nested-group drawer used instead of Umbra's default recursive expansion for an annotated configuration scope.
/// </summary>
/// <typeparam name="TDrawer">The <see cref="INestedDrawer{TGroup}"/> implementation used to render the nested group.</typeparam>
/// <remarks>
/// This attribute can be applied to a nested-group property or, as a fallback, to the nested-group type itself.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraNestedDrawerAttribute<TDrawer> : Attribute, INestedDrawerAttribute
    where TDrawer : class, new()
{
    /// <summary>
    /// Gets the concrete drawer type used to render the annotated nested-group scope.
    /// </summary>
    /// <value>The declared nested-group drawer type.</value>
    public Type DrawerType => typeof(TDrawer);
}
