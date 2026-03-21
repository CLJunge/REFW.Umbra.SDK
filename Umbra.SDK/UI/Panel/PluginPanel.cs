using Hexa.NET.ImGui;
using Umbra.SDK;

namespace Umbra.SDK.UI.Panel;

/// <summary>
/// Composes and renders an ordered list of <see cref="IPanelSection"/> instances under a
/// shared top-level ImGui ID scope, and owns the lifetime of every section it holds.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PluginPanel"/> is the recommended top-level UI type for plugins that need to
/// display both configuration settings and live game state in a single panel.
/// For plugins that only require a settings panel, <see cref="Config.UI.ConfigDrawer{TConfig}"/>
/// may be used directly.
/// </para>
/// <para>
/// Sections are rendered in the order they were added via <see cref="Add"/>. The panel
/// pushes its <c>idScope</c> as an ImGui ID scope around all section rendering, preventing
/// widget ID collisions when multiple plugins render into the same ImGui window.
/// </para>
/// <para>
/// Always dispose the panel in the plugin's <c>[PluginExitPoint]</c> to release all
/// section drawers and their captured state.
/// </para>
/// </remarks>
public sealed class PluginPanel : IDisposable
{
    private readonly string               _idScope;
    private readonly List<IPanelSection>  _sections = [];
    private bool                          _disposed;

    /// <summary>
    /// Initialises a new panel with the given top-level ImGui ID scope.
    /// </summary>
    /// <param name="idScope">
    /// A plugin-unique identifier string (e.g. <c>"MyPlugin"</c>) used to scope all ImGui
    /// widget IDs rendered by this panel's sections via <c>ImGui.PushID</c> /
    /// <c>ImGui.PopID</c>. Must be non-null and non-whitespace.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="idScope"/> is <see langword="null"/> or whitespace.
    /// </exception>
    public PluginPanel(string idScope)
    {
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null or whitespace.", nameof(idScope));

        _idScope = idScope;
    }

    /// <summary>
    /// Appends a section to the panel and re-sorts the section list by <see cref="IPanelSection.Order"/>.
    /// </summary>
    /// <remarks>
    /// Sections are rendered in ascending <see cref="IPanelSection.Order"/> order. Equal-order
    /// sections preserve their insertion order (stable sort). To control ordering, apply
    /// <see cref="SectionOrderAttribute"/> to the state or config type, or pass a custom
    /// <see cref="IPanelSection"/> implementation that overrides <see cref="IPanelSection.Order"/>.
    /// </remarks>
    /// <param name="section">The section to add. Must not be <see langword="null"/>.</param>
    /// <returns>This <see cref="PluginPanel"/> instance, enabling fluent chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="section"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when the panel has already been disposed.
    /// </exception>
    public PluginPanel Add(IPanelSection section)
    {
        if (section is null)
            throw new ArgumentNullException(nameof(section));

        if (_disposed)
            throw new ObjectDisposedException(nameof(PluginPanel), "Cannot add sections to a disposed panel.");
        _sections.Add(section);
        _sections.StableSortBy(s => s.Order);
        return this;
    }

    /// <summary>
    /// Renders all sections in order. Must be called from within an active ImGui window or
    /// child window, typically from the plugin's ImGui pre-draw callback each frame.
    /// </summary>
    public void Draw()
    {
        if (_disposed) return;

        ImGui.PushID(_idScope);
        foreach (var section in _sections) section.Draw();
        ImGui.PopID();
    }

    /// <summary>
    /// Disposes all sections and clears the section list.
    /// </summary>
    /// <remarks>
    /// Call this in the plugin's <c>[PluginExitPoint]</c> before nulling the panel reference.
    /// After disposal, calls to <see cref="Draw"/> are silent no-ops.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var section in _sections) section.Dispose();
        _sections.Clear();

        GC.SuppressFinalize(this);
    }
}
