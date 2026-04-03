using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the custom drawer used to render a parameter's editing widget while preserving Umbra's standard two-column layout.
/// </summary>
/// <typeparam name="TDrawer">The <see cref="ITwoColumnParameterDrawer"/> implementation used for rendering.</typeparam>
/// <remarks>
/// When this attribute is present, Umbra keeps the standard label layout and delegates only widget rendering to the declared drawer type.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraTwoColumnDrawerAttribute<TDrawer> : Attribute, ITwoColumnDrawerAttribute
    where TDrawer : ITwoColumnParameterDrawer, new()
{
    /// <summary>
    /// Gets the concrete drawer type used to render the annotated parameter's editing widget.
    /// </summary>
    /// <value>The declared two-column drawer type.</value>
    public Type DrawerType => typeof(TDrawer);
}
