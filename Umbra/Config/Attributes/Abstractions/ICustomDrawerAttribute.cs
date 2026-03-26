using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="CustomDrawerAttribute{TDrawer}"/>.
/// Allows custom-drawer detection via
/// <c>property.GetDrawerAttribute&lt;ICustomDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
/// <remarks>
/// Used exclusively by internal framework machinery (<see cref="Umbra.Config.ParameterMetadataReader"/>).
/// Plugin authors never implement or reference this interface directly.
/// </remarks>
internal interface ICustomDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="IParameterDrawer"/> type used to render the parameter.</summary>
    Type DrawerType { get; }
}
