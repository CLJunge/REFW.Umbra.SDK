namespace Umbra.SDK.UI.Panel;

/// <summary>
/// Defines the rendering and disposal contract for a live state drawer used by
/// <see cref="LiveSection{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface on a class also providing a public parameterless constructor,
/// then declare the drawer on the live state type with
/// <c>[LiveSectionDrawer&lt;TDrawer&gt;]</c>. <see cref="LiveSection{T}"/> discovers and
/// instantiates the drawer at construction time; <see cref="Draw"/> is called each frame
/// with the current state instance.
/// </para>
/// <para>
/// The drawer has complete ImGui layout control. The state instance is guaranteed
/// non-<see langword="null"/> on every call.
/// </para>
/// </remarks>
/// <typeparam name="T">The live state type this drawer renders.</typeparam>
public interface ILiveSectionDrawer<T> : IDisposable
{
    /// <summary>
    /// Renders ImGui controls for the provided live state instance.
    /// </summary>
    /// <param name="state">
    /// The live state instance bound to the owning <see cref="LiveSection{T}"/> for the
    /// section's lifetime. Hooks or callbacks may update this instance between frames; it is
    /// always non-<see langword="null"/> when this method is called.
    /// </param>
    void Draw(T state);

    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// <remarks>
    /// Default implementation calls <see cref="GC.SuppressFinalize"/> to prevent a redundant
    /// finalizer call. Override when the drawer holds resources that must be released on
    /// plugin unload.
    /// </remarks>
    void IDisposable.Dispose() => GC.SuppressFinalize(this);
}
