namespace Umbra.UI.LiveState;

/// <summary>
/// Non-generic marker interface implemented by <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>.
/// Enables reflection-based detection of the attribute on a live state type without
/// generic type inspection.
/// </summary>
/// <remarks>
/// Used exclusively by internal framework machinery (<see cref="LiveStateSectionDrawerResolver"/>) to
/// locate the drawer type declared on a live state class via
/// <c>type.GetDrawerAttribute&lt;ILiveStateSectionDrawerAttribute&gt;()</c>.
/// Plugin authors do not implement or reference this interface directly.
/// </remarks>
internal interface ILiveStateSectionDrawerAttribute
{
    /// <summary>
    /// Gets the concrete <see cref="ILiveStateSectionDrawer{T}"/> type declared on the live state class.
    /// </summary>
    Type DrawerType { get; }
}
