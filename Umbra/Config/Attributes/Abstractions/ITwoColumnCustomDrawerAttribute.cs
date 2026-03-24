using Umbra.Config.UI.ParameterDrawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="TwoColumnCustomDrawerAttribute{TDrawer}"/>.
/// Allows two-column custom-drawer detection via
/// <c>property.GetDrawerAttribute&lt;ITwoColumnCustomDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
public interface ITwoColumnCustomDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="ITwoColumnParameterDrawer"/> type used to render the parameter's editing widget.</summary>
    Type DrawerType { get; }
}
