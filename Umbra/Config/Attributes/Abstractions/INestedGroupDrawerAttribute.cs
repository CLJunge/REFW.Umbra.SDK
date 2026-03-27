using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="UmbraNestedGroupDrawerAttribute{TDrawer}"/>.
/// Allows nested-group custom-drawer detection on either a parent property declaration or the
/// nested group type itself without runtime generic type inspection.
/// </summary>
/// <remarks>
/// Intended for use by framework-internal machinery such as <see cref="Umbra.UI.Config.ConfigDrawerBuilder"/>
/// and <see cref="Umbra.UI.Config.TypeDrawMetadata"/>. Plugin authors should not implement or
/// reference this interface directly; it is considered internal-use-only even though it is publicly
/// visible so that <see cref="ReflectionExtensions.GetDrawerAttribute{T}(System.Reflection.PropertyInfo)"/>
/// and <see cref="ReflectionExtensions.GetDrawerAttribute{T}(System.Type)"/> can use it as a generic type argument.
/// </remarks>
internal interface INestedGroupDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="INestedGroupDrawer{T}"/> type used to render the nested group instance.</summary>
    Type DrawerType { get; }
}
