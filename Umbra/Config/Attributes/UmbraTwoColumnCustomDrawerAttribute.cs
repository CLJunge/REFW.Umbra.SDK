using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Instructs the UI builder to render this settings parameter's editing widget using a custom
/// <see cref="ITwoColumnParameterDrawer"/> while keeping the standard two-column layout.
/// </summary>
/// <typeparam name="TDrawer">
/// The <see cref="ITwoColumnParameterDrawer"/> implementation to use.
/// </typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraTwoColumnCustomDrawerAttribute<TDrawer> : Attribute, ITwoColumnCustomDrawerAttribute
    where TDrawer : ITwoColumnParameterDrawer, new()
{
    /// <summary>Gets the type of the custom drawer used to render this parameter's editing widget.</summary>
    public Type DrawerType => typeof(TDrawer);
}
