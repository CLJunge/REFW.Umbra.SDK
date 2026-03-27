using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="CustomDrawerAttribute{TDrawer}"/>.
/// Allows custom-drawer detection via
/// <c>property.GetDrawerAttribute&lt;ICustomDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
/// <remarks>
/// This interface is part of the public surface so that helpers such as
/// <see cref="ReflectionExtensions.GetDrawerAttribute{T}(System.Reflection.PropertyInfo)"/>
/// and reflection-based tooling can use it as a generic type argument to detect custom drawers on properties.
/// Plugin authors should not implement this interface themselves; it is implemented only
/// by framework-provided attributes such as <see cref="CustomDrawerAttribute{TDrawer}"/>.
/// </remarks>
public interface ICustomDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="IParameterDrawer"/> type used to render the parameter.</summary>
    Type DrawerType { get; }
}
