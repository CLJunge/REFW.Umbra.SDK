using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

/// <summary>
/// Composes and renders an ordered list of <see cref="IPanelSection"/> instances under a
/// shared top-level ImGui ID scope.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PluginPanel"/> is the recommended top-level UI type for plugins that need to
/// display both configuration settings and live game state in a single panel.
/// For plugins that only require a settings panel, <see cref="Config.ConfigDrawer{TConfig}"/>
/// may be used directly.
/// </para>
/// <para>
/// The constructor-supplied ID scope string is the sole mechanism that separates this panel's
/// ImGui widget IDs from every other panel rendered into the same REFramework window. All
/// managed plugins
/// share one AppDomain and one ImGui context; duplicate-scope detection and release are handled by
/// <see cref="PluginPanelScopeRegistry"/>. Use a value that is guaranteed unique across all plugins,
/// such as <c>nameof(MyPlugin)</c> or <c>typeof(MyPlugin).FullName</c>.
/// The registry emits at most one detailed warning per still-active duplicate scope so accidental
/// repeated panel construction does not flood the REFramework console.
/// </para>
/// <para>
/// Section collection responsibilities such as tree-label validation, stable ordering, and section
/// disposal are delegated to <see cref="PluginPanelSectionCollection"/>. Root-node rendering,
/// per-section tree-node rendering, and separator placement are delegated to
/// <see cref="PluginPanelDrawPipeline"/>.
/// </para>
/// <para>
/// Always dispose the panel in the plugin's
/// <see cref="REFrameworkNET.Attributes.PluginExitPoint"/> (<c>[PluginExitPoint]</c>)
/// to release all section drawers and their captured state.
/// </para>
/// </remarks>
public sealed class PluginPanel : IDisposable
{
    private readonly string _idScope;
    private readonly bool _scopeRegistered;
    private readonly IPluginPanelRenderer _renderer;
    private readonly PluginPanelSectionCollection _sections = new();
    private readonly PluginPanelDrawPipeline _drawPipeline;
    private bool _disposed;

    /// <summary>
    /// Initialises a new panel with the given top-level ImGui ID scope.
    /// </summary>
    /// <param name="idScope">
    /// A globally unique identifier string for this plugin (e.g. <c>nameof(MyPlugin)</c> or
    /// <c>typeof(MyPlugin).FullName</c>). All managed plugins share one AppDomain and one ImGui
    /// context; this is the only separator between this panel's widget IDs and every other panel
    /// in the process. Duplicate-scope detection is handled by <see cref="PluginPanelScopeRegistry"/>,
    /// which warns once per active conflicting scope.
    /// Must be non-null and non-whitespace.
    /// </param>
    /// <param name="rootNodeLabel">
    /// When non-<see langword="null"/>, all sections are rendered inside a single collapsible
    /// <see cref="ImGui.TreeNode(string)"/> with this label. Pass <see langword="null"/>
    /// (the default) to render sections flat with no root-level wrapping node.
    /// </param>
    /// <param name="rootNodeDefaultOpen">
    /// When <see langword="true"/>, the root tree node starts in its expanded state.
    /// Ignored when <paramref name="rootNodeLabel"/> is <see langword="null"/>.
    /// Defaults to <see langword="false"/> (collapsed).
    /// </param>
    /// <param name="drawSeparator">
    /// When <see langword="true"/> (the default), a horizontal separator is drawn after
    /// all sections. Pass <see langword="false"/> to suppress it.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="idScope"/> is <see langword="null"/> or whitespace.
    /// </exception>
    public PluginPanel(string idScope, string? rootNodeLabel = null, bool rootNodeDefaultOpen = false, bool drawSeparator = true)
        : this(idScope, rootNodeLabel, rootNodeDefaultOpen, drawSeparator, new ImGuiPluginPanelRenderer())
    {
    }

    /// <summary>
    /// Initialises a new panel with the given top-level ImGui ID scope and low-level renderer.
    /// </summary>
    /// <param name="idScope">
    /// A globally unique identifier string for this plugin (e.g. <c>nameof(MyPlugin)</c> or
    /// <c>typeof(MyPlugin).FullName</c>). All managed plugins share one AppDomain and one ImGui
    /// context; this is the only separator between this panel's widget IDs and every other panel
    /// in the process. Duplicate-scope detection is handled by <see cref="PluginPanelScopeRegistry"/>,
    /// which warns once per active conflicting scope.
    /// Must be non-null and non-whitespace.
    /// </param>
    /// <param name="rootNodeLabel">
    /// When non-<see langword="null"/>, all sections are rendered inside a single collapsible
    /// <see cref="ImGui.TreeNode(string)"/> with this label. Pass <see langword="null"/>
    /// to render sections flat with no root-level wrapping node.
    /// </param>
    /// <param name="rootNodeDefaultOpen">
    /// When <see langword="true"/>, the root tree node starts in its expanded state.
    /// Ignored when <paramref name="rootNodeLabel"/> is <see langword="null"/>.
    /// </param>
    /// <param name="drawSeparator">
    /// When <see langword="true"/>, a horizontal separator is drawn after all sections.
    /// </param>
    /// <param name="renderer">
    /// The low-level renderer used for ImGui ID-scope, tree-node, and separator operations.
    /// Tests can replace this dependency to verify draw behavior without an active ImGui frame.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="idScope"/> is <see langword="null"/> or whitespace.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> is <see langword="null"/>.
    /// </exception>
    internal PluginPanel(string idScope, string? rootNodeLabel, bool rootNodeDefaultOpen, bool drawSeparator, IPluginPanelRenderer renderer)
    {
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null or whitespace.", nameof(idScope));

        ArgumentNullException.ThrowIfNull(renderer);

        _idScope = idScope;
        _scopeRegistered = PluginPanelScopeRegistry.TryRegister(idScope);
        _renderer = renderer;
        _drawPipeline = new PluginPanelDrawPipeline(rootNodeLabel, rootNodeDefaultOpen, drawSeparator, renderer);
    }

    /// <summary>
    /// Appends a section to the panel and re-sorts the section list by <see cref="IPanelSection.Order"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Sections are rendered in ascending <see cref="IPanelSection.Order"/> order. Equal-order
    /// sections preserve their insertion order (stable sort). To control ordering, apply
    /// <see cref="UmbraSectionOrderAttribute"/> to the state or config type, or pass a custom
    /// <see cref="IPanelSection"/> implementation that overrides <see cref="IPanelSection.Order"/>.
    /// </para>
    /// <para>
    /// Tree-node label validation is delegated to <see cref="PluginPanelSectionCollection"/>, which
    /// uses <see cref="PluginPanelTreeNodeLabels"/> before storing the section.
    /// </para>
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
        _sections.Add(section);
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
    /// Root-node rendering, per-section tree-node wrapping, and separator placement are handled by
    /// <see cref="PluginPanelDrawPipeline"/> once the shared panel ID scope is active.
    /// </para>
    /// </remarks>
    public void Draw()
    {
        if (_disposed) return;

        _renderer.PushId(_idScope);
        try
        {
            _drawPipeline.Draw(_sections.Sections);
        }
        finally
        {
            _renderer.PopId();
        }
    }

    /// <summary>
    /// Disposes all sections, clears the section list, and releases this panel's ID scope
    /// through <see cref="PluginPanelScopeRegistry"/> so a reloaded plugin can register the same
    /// scope without a spurious duplicate warning.
    /// </summary>
    /// <remarks>
    /// Call this in the plugin's
    /// <see cref="REFrameworkNET.Attributes.PluginExitPoint"/> (<c>[PluginExitPoint]</c>)
    /// before nulling the panel reference.
    /// After disposal, calls to <see cref="Draw"/> are silent no-ops.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_scopeRegistered)
            PluginPanelScopeRegistry.Release(_idScope);

        _sections.Dispose();

        GC.SuppressFinalize(this);
    }
}
