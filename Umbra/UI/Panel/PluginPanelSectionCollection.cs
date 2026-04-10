using Umbra.Logging;

namespace Umbra.UI.Panel;

/// <summary>
/// Owns the ordered section list for a <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// This type isolates section collection responsibilities from <see cref="PluginPanel"/>, including tree-label validation, stable ordering, and section disposal.
/// </remarks>
internal sealed class PluginPanelSectionCollection : IDisposable
{
    private readonly List<IPanelSection> _sections = [];
    private bool _disposed;

    /// <summary>
    /// Gets the ordered sections currently held by the collection.
    /// </summary>
    /// <value>The live ordered section list used by the owning panel.</value>
    internal IReadOnlyList<IPanelSection> Sections => _sections;

    /// <summary>
    /// Appends <paramref name="section"/> and re-sorts the collection by <see cref="IPanelSection.Order"/>.
    /// </summary>
    /// <param name="section">The section to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="section"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">The collection has already been disposed.</exception>
    internal void Add(IPanelSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (_disposed)
            throw new ObjectDisposedException(nameof(PluginPanelSectionCollection), "Cannot add sections to a disposed collection.");

        PluginPanelTreeNodeLabels.WarnIfInvalid(section);
        _sections.Add(section);
        _sections.SortBy(static sectionItem => sectionItem.Order);
    }

    /// <summary>
    /// Disposes all sections in their current ordered sequence and clears the collection.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing. If a section throws during disposal, the
    /// exception is logged and disposal continues for the remaining sections.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var section in _sections)
            DisposeSectionSafely(section);

        _sections.Clear();
        GC.SuppressFinalize(this);
    }

    private static void DisposeSectionSafely(IPanelSection section)
    {
        try
        {
            section.Dispose();
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "Plugin panel section '{0}' threw during Dispose().", section.SectionId);
        }
    }
}
