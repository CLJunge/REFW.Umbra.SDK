namespace Umbra.UI.LiveState;

/// <summary>
/// Defines the rendering contract for a live-state drawer used by <see cref="LiveStateSection{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface on a drawer type with a public parameterless constructor, then declare that drawer on the live-state type with <see cref="LiveStateSectionDrawerAttribute{TDrawer}"/>. <see cref="LiveStateSection{T}"/> resolves and instantiates the drawer once at construction time.
/// </para>
/// <para>
/// The drawer has full ImGui layout control. <see cref="Draw(T)"/> is called each frame with the bound live-state instance, which is always non-<see langword="null"/>.
/// </para>
/// </remarks>
/// <typeparam name="T">The live-state type rendered by the drawer.</typeparam>
public interface ILiveStateSectionDrawer<T> : IDisposable
{
    /// <summary>
    /// Renders ImGui content for the provided live-state instance.
    /// </summary>
    /// <param name="state">The live-state instance bound to the owning <see cref="LiveStateSection{T}"/> for the section's lifetime.</param>
    void Draw(T state);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <remarks>
    /// The default implementation calls <see cref="GC.SuppressFinalize(object)"/>. Override it when the drawer owns resources that must be released on plugin unload.
    /// </remarks>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
