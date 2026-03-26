namespace Umbra.UI.LiveState;

/// <summary>
/// Associates a live state class with the <see cref="ILiveStateSectionDrawer{T}"/> implementation
/// that renders it within a <see cref="LiveStateSection{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to the live state class — not to the drawer — so that the type is
/// self-describing. <see cref="LiveStateSectionDrawerResolver"/> reads the attribute once at
/// <see cref="LiveStateSection{T}"/> construction time to instantiate the drawer and compile a
/// zero-overhead per-frame draw delegate.
/// </para>
/// <para>
/// The live state class itself should be a stable object identity for the lifetime of the
/// owning <see cref="LiveStateSection{T}"/>. Hooks and callbacks may mutate fields on that object,
/// or the object may expose a field or property that points to an immutable snapshot updated
/// atomically between frames. Do not replace the bound state object itself unless the section
/// is also reconstructed, because <see cref="LiveStateSection{T}"/> renders the exact instance it
/// was created with.
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
/// <typeparam name="TDrawer">
/// The drawer implementation to use. Must provide a public parameterless constructor;
/// this constraint is enforced at compile time. The additional requirement that
/// <typeparamref name="TDrawer"/> implements <see cref="ILiveStateSectionDrawer{T}"/> is
/// validated at runtime by <see cref="LiveStateSectionDrawerResolver"/> when
/// <see cref="LiveStateSection{T}"/> is constructed.
/// </typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class LiveStateSectionDrawerAttribute<TDrawer> : Attribute, ILiveStateSectionDrawerAttribute
    where TDrawer : class, new()
{
    /// <summary>Gets the concrete drawer type used to render the live state instance.</summary>
    public Type DrawerType => typeof(TDrawer);
}
