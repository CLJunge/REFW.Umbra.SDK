namespace Umbra.UI.Panel;

/// <summary>
/// Owns the ordered section list for a <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// This type isolates section collection responsibilities from <see cref="PluginPanel"/>, including
/// tree-label validation, stable ordering, and section disposal.
/// </remarks>
internal sealed class PluginPanelSectionCollection : IDisposable
{
    private readonly List<IPanelSection> _sections = [];
    private bool _disposed;

    /// <summary>
    /// Gets the ordered sections currently held by the collection.
    /// </summary>
    internal IReadOnlyList<IPanelSection> Sections => _sections;

    /// <summary>
    /// Appends <paramref name="section"/> and re-sorts the collection by <see cref="IPanelSection.Order"/>.
    /// </summary>
    /// <param name="section">The section to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="section"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the collection has already been disposed.</exception>
    internal void Add(IPanelSection section)
    {
        ArgumentNullException.ThrowIfNull(section);

        if (_disposed)
            throw new ObjectDisposedException(nameof(PluginPanel), "Cannot add sections to a disposed panel.");

        PluginPanelTreeNodeLabels.WarnIfInvalid(section);
        _sections.Add(section);
        _sections.SortBy(static sectionItem => sectionItem.Order);
    }

    /// <summary>
    /// Disposes all sections in their current ordered sequence and clears the collection.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var section in _sections)
            section.Dispose();

        _sections.Clear();
        GC.SuppressFinalize(this);
    }
}
