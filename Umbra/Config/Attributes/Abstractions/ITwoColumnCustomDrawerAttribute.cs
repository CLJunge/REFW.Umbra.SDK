using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="TwoColumnCustomDrawerAttribute{TDrawer}"/>.
/// Allows two-column custom-drawer detection via
/// <c>property.GetDrawerAttribute&lt;ITwoColumnCustomDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
/// <remarks>
/// This interface is primarily used by internal framework machinery (such as <see cref="ParameterMetadataReader"/>)
/// to detect two-column custom drawers via
/// <see cref="ReflectionExtensions.GetDrawerAttribute{T}(System.Reflection.PropertyInfo)"/>.
/// Plugin authors should not implement it themselves; it is implemented only
/// by framework-provided attributes such as <see cref="TwoColumnCustomDrawerAttribute{TDrawer}"/>.
/// </remarks>
public interface ITwoColumnCustomDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="ITwoColumnParameterDrawer"/> type used to render the parameter's editing widget.</summary>
    Type DrawerType { get; }
}
