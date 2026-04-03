namespace Umbra.UI.LiveState;

/// <summary>
/// Associates a live-state type with the drawer that renders it inside <see cref="LiveStateSection{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to the live-state type, not to the drawer. <see cref="LiveStateSectionDrawerResolver"/> reads the attribute when a <see cref="LiveStateSection{T}"/> is constructed, instantiates the declared drawer, and compiles the delegate used for per-frame drawing.
/// </para>
/// <para>
/// <see cref="LiveStateSection{T}"/> keeps rendering the exact state instance it was constructed with. For hook-driven data, keep that bound instance stable for the section's lifetime and update its contents directly or publish swapped snapshots through members on that stable object.
/// </para>
/// <example>
/// <code>
/// [LiveStateSectionDrawer&lt;CameraStatusDrawer&gt;]
/// public sealed class CameraState
/// {
///     public float      Fov  { get; set; }
///     public CameraMode Mode { get; set; }
/// }
/// </code>
/// </example>
/// </remarks>
/// <typeparam name="TDrawer">The drawer type to instantiate. It must provide a public parameterless constructor. <see cref="LiveStateSectionDrawerResolver"/> validates at runtime that it also implements a compatible <see cref="ILiveStateSectionDrawer{T}"/>.</typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class LiveStateSectionDrawerAttribute<TDrawer> : Attribute, ILiveStateSectionDrawerAttribute
    where TDrawer : class, new()
{
    /// <summary>
    /// Gets the concrete drawer type used to render the live-state instance.
    /// </summary>
    /// <value>The declared drawer type.</value>
    public Type DrawerType => typeof(TDrawer);
}
