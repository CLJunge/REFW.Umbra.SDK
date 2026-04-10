using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the custom drawer type used to render an annotated parameter instead of Umbra's default control selection.
/// </summary>
/// <typeparam name="TDrawer">The <see cref="IParameterDrawer"/> implementation used for rendering.</typeparam>
/// <remarks>
/// When this attribute is present, Umbra delegates rendering of the annotated parameter entirely to the declared drawer type.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraDrawerAttribute<TDrawer> : Attribute, IDrawerAttribute where TDrawer : IParameterDrawer, new()
{
    /// <summary>
    /// Gets the concrete drawer type used to render the annotated parameter.
    /// </summary>
    /// <value>The declared custom drawer type.</value>
    public Type DrawerType => typeof(TDrawer);
}
