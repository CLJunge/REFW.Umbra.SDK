using Umbra.SDK.Config.UI.ParameterDrawers;

namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="NestedGroupDrawerAttribute{TDrawer}"/>.
/// Allows nested-group custom-drawer detection via
/// <c>propType.GetDrawerAttribute&lt;INestedGroupDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
public interface INestedGroupDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="INestedGroupDrawer{T}"/> type used to render the nested group instance.</summary>
    Type DrawerType { get; }
}
