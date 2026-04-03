namespace Umbra.UI.LiveState;

/// <summary>
/// Exposes the drawer type declared by <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/> without requiring generic attribute inspection.
/// </summary>
/// <remarks>
/// <see cref="LiveStateSectionDrawerResolver"/> uses this internal marker to read the declared drawer type from a live-state type through Umbra's reflection helpers. Plugin authors do not implement or reference this interface directly.
/// </remarks>
internal interface ILiveStateSectionDrawerAttribute
{
    /// <summary>
    /// Gets the concrete drawer type declared on the live-state class.
    /// </summary>
    /// <value>The <see cref="ILiveStateSectionDrawer{T}"/> implementation type associated with the annotated live-state type.</value>
    Type DrawerType { get; }
}
