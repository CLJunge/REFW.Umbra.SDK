using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

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
/// When <c>rootNodeLabel</c> is supplied, the entire section list is wrapped inside
/// a single collapsible <c>ImGui.TreeNode</c> at the top of the panel. Individual sections may
/// additionally declare their own tree node via <see cref="IPanelSection.TreeNodeLabel"/>;
/// these per-section nodes are rendered inside the root node when one is present.
/// </para>
/// <para>
/// Sections are rendered in ascending <see cref="IPanelSection.Order"/>. The internal
/// list is re-sorted on each call to <see cref="Add"/>, and equal-order sections preserve
/// their insertion order because a stable sort is used. The panel pushes its <c>idScope</c>
/// as an ImGui ID scope around all section rendering, preventing widget ID collisions when
/// multiple plugins render into the same ImGui window.
/// </para>
/// <para>
/// Always dispose the panel in the plugin's <c>[PluginExitPoint]</c> to release all
/// section drawers and their captured state.
/// </para>
/// </remarks>
public sealed class PluginPanel : IDisposable
{
    private readonly string _idScope;
    private readonly string? _rootNodeLabel;
    private readonly bool _rootNodeDefaultOpen;
    private readonly List<IPanelSection> _sections = [];
    private bool _disposed;

    /// <summary>
    /// Initialises a new panel with the given top-level ImGui ID scope.
    /// </summary>
    /// <param name="idScope">
    /// A plugin-unique identifier string (e.g. <c>"MyPlugin"</c>) used to scope all ImGui
    /// widget IDs rendered by this panel's sections via <c>ImGui.PushID</c> /
    /// <c>ImGui.PopID</c>. Must be non-null and non-whitespace.
    /// </param>
    /// <param name="rootNodeLabel">
    /// When non-<see langword="null"/>, all sections are rendered inside a single collapsible
    /// <c>ImGui.TreeNode</c> with this label. Pass <see langword="null"/> (the default) to
    /// render sections flat with no root-level wrapping node.
    /// </param>
    /// <param name="rootNodeDefaultOpen">
    /// When <see langword="true"/>, the root tree node starts in its expanded state.
    /// Ignored when <paramref name="rootNodeLabel"/> is <see langword="null"/>.
    /// Defaults to <see langword="false"/> (collapsed).
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="idScope"/> is <see langword="null"/> or whitespace.
    /// </exception>
    public PluginPanel(string idScope, string? rootNodeLabel = null, bool rootNodeDefaultOpen = false)
    {
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null or whitespace.", nameof(idScope));

        _idScope             = idScope;
        _rootNodeLabel       = rootNodeLabel;
        _rootNodeDefaultOpen = rootNodeDefaultOpen;
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
        ArgumentNullException.ThrowIfNull(section);

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
    /// <remarks>
    /// <para>
    /// The pushed top-level ImGui ID scope is always popped before this method returns,
    /// even if a section throws while drawing.
    /// </para>
    /// <para>
    /// When a <c>rootNodeLabel</c> was supplied at construction, all sections are rendered
    /// inside a single collapsible <c>ImGui.TreeNode</c>; the tree pop is guarded with
    /// <c>try/finally</c> so ImGui state remains balanced even if a section throws.
    /// Each section that declares a non-<see langword="null"/>
    /// <see cref="IPanelSection.TreeNodeLabel"/> is additionally wrapped in its own nested
    /// tree node rendered inside the root node (or at the top level when no root node is set).
    /// </para>
    /// </remarks>
    public void Draw()
    {
        if (_disposed) return;

        ImGui.PushID(_idScope);
        try
        {
            if (_rootNodeLabel is not null)
            {
                var flags = _rootNodeDefaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
                if (ImGui.TreeNodeEx(_rootNodeLabel, flags))
                {
                    try { DrawSections(); }
                    finally { ImGui.TreePop(); }
                }
            }
            else
            {
                DrawSections();
            }
        }
        finally
        {
            ImGui.PopID();
        }
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

    /// <summary>
    /// Iterates over all sections and renders each one, optionally wrapping it inside a
    /// per-section <c>ImGui.TreeNode</c> when the section declares a
    /// <see cref="IPanelSection.TreeNodeLabel"/>. Each section is pushed into its own
    /// <c>ImGui.PushID(index)</c> scope to prevent label-based ID conflicts between sections
    /// that share the same tree node label string.
    /// </summary>
    private void DrawSections()
    {
        for (int i = 0; i < _sections.Count; i++)
        {
            var section = _sections[i];
            ImGui.PushID(i);
            try
            {
                var label = section.TreeNodeLabel;
                if (label is not null)
                {
                    var flags = section.TreeNodeDefaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
                    if (ImGui.TreeNodeEx(label, flags))
                    {
                        try { section.Draw(); }
                        finally { ImGui.TreePop(); }
                    }
                }
                else
                {
                    section.Draw();
                }
            }
            finally
            {
                ImGui.PopID();
            }
        }
    }
}
