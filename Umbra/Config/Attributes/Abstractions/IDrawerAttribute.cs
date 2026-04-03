using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Exposes the drawer type declared by <see cref="UmbraDrawerAttribute{TDrawer}"/> without requiring generic attribute inspection.
/// </summary>
/// <remarks>
/// Umbra's reflection-based metadata pipeline uses this marker to discover custom parameter drawers through helper methods such as <see cref="ReflectionExtensions.GetDrawerAttribute{T}(System.Reflection.PropertyInfo)"/>. Plugin authors do not implement this interface directly.
/// </remarks>
internal interface IDrawerAttribute
{
    /// <summary>
    /// Gets the concrete <see cref="IParameterDrawer"/> type declared for the annotated member.
    /// </summary>
    /// <value>The drawer type used when the parameter is rendered.</value>
    Type DrawerType { get; }
}
