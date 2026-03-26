using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="NestedGroupDrawerAttribute{TDrawer}"/>.
/// Allows nested-group custom-drawer detection on either a parent property declaration or the
/// nested group type itself without runtime generic type inspection.
/// </summary>
/// <remarks>
/// Used exclusively by internal framework machinery (<see cref="Umbra.UI.Config.ConfigDrawerBuilder"/>
/// and <see cref="Umbra.UI.Config.TypeDrawMetadata"/>). Plugin authors never implement or
/// reference this interface directly.
/// </remarks>
internal interface INestedGroupDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="INestedGroupDrawer{T}"/> type used to render the nested group instance.</summary>
    Type DrawerType { get; }
}
