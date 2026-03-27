using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Instructs the UI builder to render this settings parameter using a custom
/// <see cref="IParameterDrawer"/> instead of the default control inferred from the
/// parameter's value type.
/// </summary>
/// <typeparam name="TDrawer">
/// The <see cref="IParameterDrawer"/> implementation to use.
/// </typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraCustomDrawerAttribute<TDrawer> : Attribute, ICustomDrawerAttribute where TDrawer : IParameterDrawer, new()
{
    /// <summary>Gets the type of the custom drawer used to render this parameter.</summary>
    public Type DrawerType => typeof(TDrawer);
}
