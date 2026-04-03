using Hexa.NET.ImGui;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Pre-builds and renders an ImGui settings UI for a typed configuration class.
/// </summary>
/// <remarks>
/// <para>
/// The draw tree is assembled once at construction time via a single reflection pass;
/// each subsequent call to <see cref="Draw"/> walks the pre-built list of nodes cheaply
/// with no per-frame reflection.
/// </para>
/// <para>
/// Pass a config instance returned by <see cref="Umbra.Config.SettingsStore{TConfig}.Load()"/> so that
/// <see cref="Umbra.Config.ParameterMetadata"/> is already populated on every leaf parameter.
/// Parameter-level presentation such as labels, descriptions, ranges, formats, and custom drawer
/// bindings is taken from that cached metadata; nested-group wrapper attributes are still read from
/// the reflected property/type structure during the one-time build pass.
/// </para>
/// <para>
/// For nested settings groups, prefer applying presentation attributes such as
/// <see cref="UmbraCategoryAttribute"/>, <see cref="UmbraCollapseAsTreeAttribute"/>,
/// <see cref="UmbraLabelMarginAttribute"/>, and <see cref="UmbraNestedDrawerAttribute{TDrawer}"/>
/// to the parent property that exposes the group; equivalent type-level declarations remain
/// supported as backward-compatible fallbacks. Category names are scoped to the group that
/// declares them, so sibling nested groups may reuse the same category label without colliding.
/// When a nested-group property declares its own category, that category renders as a real parent
/// container for the group's uncategorized direct controls and any additional child categories
/// declared inside the group. Every nested-group subtree also receives its own stable ImGui ID
/// scope derived from its structural settings path, so custom nested-group drawers can safely
/// reuse local widget labels in different branches. Apply <see cref="UmbraRootNodeAttribute"/>
/// to the root config class to wrap the entire panel inside a single top-level
/// <see cref="ImGui.TreeNode(string)"/>.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type, following the SDK settings attribute conventions.
/// </typeparam>
public sealed class ConfigDrawer<TConfig> : IDisposable where TConfig : class
{
    private readonly List<IDrawNode> _nodes;
    private readonly List<IDisposable> _disposables;
    private readonly string _idScope;
    private readonly IConfigDrawerScope _scope;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> by reflecting over
    /// <paramref name="config"/> once to build the complete draw tree.
    /// </summary>
    /// <param name="config">
    /// A fully initialized configuration instance, ideally returned by
    /// <see cref="Umbra.Config.SettingsStore{TConfig}.Load()"/> so that <see cref="Umbra.Config.ParameterMetadata"/>
    /// is already populated on every parameter.
    /// </param>
    /// <param name="idScope">
    /// A plugin-unique identifier string (e.g. <c>"MyPlugin"</c>) used to scope all ImGui
    /// widget IDs rendered by this drawer via <see cref="ImGui.PushID(string)"/> / <see cref="ImGui.PopID()"/>.
    /// Every widget ID within a <see cref="Draw"/> call is internally prefixed with this
    /// string, preventing duplicate-ID warnings when multiple plugins render settings panels
    /// in the same ImGui window. Must be non-null and non-whitespace.
    /// </param>
    /// <param name="suppressRootNode">
    /// When <see langword="true"/>, the root-node-attribute-driven
    /// root <see cref="ImGui.TreeNode(string)"/> is not rendered even when such an attribute is present on
    /// <typeparamref name="TConfig"/>. Defaults to <see langword="false"/>.
    /// Pass <see langword="true"/> when the owning <see cref="ConfigSection{TConfig}"/>
    /// is responsible for the tree node so that the wrapping is not duplicated.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public ConfigDrawer(TConfig config, string idScope, bool suppressRootNode = false)
        : this(config, idScope, ImGuiConfigRenderContext.Instance, suppressRootNode)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> by reflecting over
    /// <paramref name="config"/> once to build the complete draw tree, using the specified
    /// draw-scope implementation.
    /// </summary>
    /// <param name="config">The configuration instance whose draw tree should be built.</param>
    /// <param name="idScope">The plugin-unique ImGui ID scope string.</param>
    /// <param name="scope">The scope implementation used to bracket each draw call.</param>
    /// <param name="suppressRootNode">
    /// When <see langword="true"/>, suppresses the root-node-attribute-driven root tree wrapper.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/> or <paramref name="scope"/> is <see langword="null"/>.
    /// </exception>
    internal ConfigDrawer(TConfig config, string idScope, IConfigDrawerScope scope, bool suppressRootNode = false)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(scope);
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null, empty, or whitespace.", nameof(idScope));

        _idScope = idScope;
        _scope = scope;
        var builder = new ConfigDrawerBuilder();
        builder.Collect(config, typeof(TConfig));
        builder.SortAll();
        _disposables = builder.Disposables;

        var rootAttr = GetRootNodeMetadata(typeof(TConfig));
        if (rootAttr.HasValue && !suppressRootNode)
        {
            var label = rootAttr.Value.Label ?? typeof(TConfig).Name.ToDisplayName();
            _nodes = [new RootTreeNode(label, rootAttr.Value.DefaultOpen, builder.Nodes)];
        }
        else
        {
            _nodes = builder.Nodes;
        }
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> with a pre-built node list.
    /// </summary>
    /// <remarks>
    /// This constructor exists for tests that need to verify draw ordering, disposal behavior, and
    /// scope cleanup without building a runtime-backed ImGui node tree.
    /// </remarks>
    /// <param name="idScope">The plugin-unique ImGui ID scope string.</param>
    /// <param name="nodes">The pre-built nodes to draw each frame.</param>
    /// <param name="disposables">The disposable resources owned by the drawer.</param>
    /// <param name="scope">The scope implementation used to bracket each draw call.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="nodes"/>, <paramref name="disposables"/>, or
    /// <paramref name="scope"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="idScope"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConfigDrawer(string idScope, List<IDrawNode> nodes, List<IDisposable> disposables, IConfigDrawerScope scope)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(disposables);
        ArgumentNullException.ThrowIfNull(scope);
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null, empty, or whitespace.", nameof(idScope));

        _idScope = idScope;
        _nodes = nodes;
        _disposables = disposables;
        _scope = scope;
    }

    /// <summary>
    /// Renders the full settings UI for one ImGui frame.
    /// Must be called from within an active ImGui window or child window.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All widget IDs rendered during this call are bracketed by
    /// the configured draw scope's push/pop operations, making every
    /// <c>##key</c> label unique across plugins without any changes to individual controls or
    /// custom drawers. The scope is always popped before this method returns, even if a node
    /// throws while drawing.
    /// </para>
    /// <para>
    /// A no-op when the instance has been disposed; logs a warning rather than throwing so
    /// a stale render callback in the game loop does not raise an unhandled exception in-process.
    /// </para>
    /// </remarks>
    public void Draw()
    {
        if (_disposed)
        {
            Logger.Warning($"ConfigDrawer<{typeof(TConfig).Name}>.Draw called on a disposed instance; skipping.");
            return;
        }
        _scope.PushId(_idScope);
        try
        {
            foreach (var node in _nodes)
                node.Draw();
        }
        finally
        {
            _scope.PopId();
        }
    }

    /// <summary>
    /// Disposes all stateful custom drawers collected during the draw-tree build pass,
    /// then marks this instance as disposed.
    /// Subsequent calls to <see cref="Draw"/> will log a warning and return without rendering.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var d in _disposables) d.Dispose();
        GC.SuppressFinalize(this);
    }

    private static (string? Label, bool DefaultOpen)? GetRootNodeMetadata(Type type)
    {
        foreach (var attr in type.GetCustomAttributes(inherit: true))
            if (attr is UmbraRootNodeAttribute prefixed)
                return (prefixed.Label, prefixed.DefaultOpen);

        return null;
    }
}
