namespace Umbra.UI.Panel;

/// <summary>
/// Non-generic marker interface implemented by <see cref="LiveSectionDrawerAttribute{TDrawer}"/>.
/// Enables reflection-based detection of the attribute on a live state type without
/// generic type inspection.
/// </summary>
/// <remarks>
/// Used internally by <see cref="LiveSectionDrawerResolver"/> to locate the drawer type
/// declared on a live state class via <c>type.GetDrawerAttribute&lt;ILiveSectionDrawerAttribute&gt;()</c>.
/// Plugin authors do not implement or reference this interface directly.
/// </remarks>
public interface ILiveSectionDrawerAttribute
{
    /// <summary>
    /// Gets the concrete <see cref="ILiveSectionDrawer{T}"/> type declared on the live state class.
    /// </summary>
    Type DrawerType { get; }
}
