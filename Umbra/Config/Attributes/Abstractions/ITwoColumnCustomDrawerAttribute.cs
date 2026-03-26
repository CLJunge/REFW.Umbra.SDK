using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="TwoColumnCustomDrawerAttribute{TDrawer}"/>.
/// Allows two-column custom-drawer detection via
/// <c>property.GetDrawerAttribute&lt;ITwoColumnCustomDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
/// <remarks>
/// Used exclusively by internal framework machinery (<see cref="Umbra.Config.ParameterMetadataReader"/>).
/// Plugin authors never implement or reference this interface directly.
/// </remarks>
internal interface ITwoColumnCustomDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="ITwoColumnParameterDrawer"/> type used to render the parameter's editing widget.</summary>
    Type DrawerType { get; }
}
