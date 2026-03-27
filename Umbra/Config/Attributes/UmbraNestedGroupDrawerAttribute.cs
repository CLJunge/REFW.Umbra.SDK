using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Instructs the UI builder to render the decorated nested configuration property or type using a
/// custom <see cref="INestedGroupDrawer{TGroup}"/> instead of the default recursive property expansion.
/// </summary>
/// <typeparam name="TDrawer">
/// The <see cref="INestedGroupDrawer{T}"/> implementation to use.
/// </typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraNestedGroupDrawerAttribute<TDrawer> : Attribute, INestedGroupDrawerAttribute
    where TDrawer : class, new()
{
    /// <summary>Gets the type of the custom drawer used to render the nested group instance.</summary>
    public Type DrawerType => typeof(TDrawer);
}
