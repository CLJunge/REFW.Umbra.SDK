using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

/// <summary>
/// Composes and renders an ordered list of <see cref="IPanelSection"/> instances under one shared top-level ImGui ID scope.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="PluginPanel"/> is the recommended top-level UI type for plugins that need to display both configuration values and live game state in a single panel. For plugins that only require a config panel, <see cref="Config.ConfigDrawer{TConfig}"/> can be used directly.
/// </para>
/// <para>
/// The constructor-supplied ID scope is the sole separator between this panel's widget IDs and every other panel rendered into the same REFramework ImGui context. Duplicate-scope detection and release are delegated to <see cref="PluginPanelScopeRegistry"/>.
/// </para>
/// <para>
/// Section collection concerns such as tree-label validation, stable ordering, and section disposal are delegated to <see cref="PluginPanelSectionCollection"/>. Root-node rendering, per-section tree-node wrapping, and separator placement are delegated to <see cref="PluginPanelDrawPipeline"/>.
/// </para>
/// <para>
/// For convenient panel creation with built-in config features, use the static factory methods on
/// <see cref="PluginPanelFactory"/> instead of constructing a panel directly.
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
    /// Initializes a new panel with the given top-level ImGui ID scope.
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
    /// The label should not contain ImGui's <c>"##"</c> separator; when it does, the panel logs a
    /// developer warning once per active panel scope and strips the suffix at render time.
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
    /// Initializes a new panel with the given top-level ImGui ID scope and low-level renderer.
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
    /// The label should not contain ImGui's <c>"##"</c> separator; when it does, the panel logs a
    /// developer warning once per active panel scope and strips the suffix at render time.
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
        PluginPanelTreeNodeLabels.WarnIfInvalid(idScope, rootNodeLabel, $"panel '{idScope}' root node");
        _renderer = renderer;
        _drawPipeline = new PluginPanelDrawPipeline(rootNodeLabel, rootNodeDefaultOpen, drawSeparator, renderer);
    }

    /// <summary>
    /// Appends <paramref name="section"/> to the panel and re-sorts the section list by <see cref="IPanelSection.Order"/>.
    /// </summary>
    /// <param name="section">The section to add.</param>
    /// <returns>This panel instance.</returns>
    /// <remarks>
    /// Equal-order sections preserve insertion order through the section collection's stable ordering behavior.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="section"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the panel has already been disposed.</exception>
    public PluginPanel Add(IPanelSection section)
    {
        _sections.Add(section);
        return this;
    }

    /// <summary>
    /// Renders all sections in order for the current ImGui frame.
    /// </summary>
    /// <remarks>
    /// The shared top-level ImGui ID scope is always popped before this method returns, even if a section throws while drawing. After <see cref="Dispose"/>, this method becomes a silent no-op.
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
    /// Disposes all owned sections and releases this panel's registered ID scope.
    /// </summary>
    /// <remarks>
    /// Releasing the scope through <see cref="PluginPanelScopeRegistry"/> prevents spurious duplicate-scope warnings when a plugin reloads and recreates the panel with the same scope. After disposal, calls to <see cref="Draw"/> are silent no-ops.
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
