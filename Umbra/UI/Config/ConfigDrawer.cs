using Hexa.NET.ImGui;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Builds and renders an ImGui settings panel for a loaded configuration instance.
/// </summary>
/// <remarks>
/// <para>
/// The draw tree is assembled once at construction time. Later <see cref="Draw"/> calls walk the cached nodes without performing per-frame reflection.
/// </para>
/// <para>
/// Pass a configuration instance returned by <see cref="Umbra.Config.SettingsStore{TConfig}.Load()"/> so each registered parameter already carries resolved <see cref="Umbra.Config.ParameterMetadata"/>. Nested-group wrapper attributes are still read from reflected property and type metadata during the one-time build pass.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration type rendered by the drawer.</typeparam>
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
    /// Renders the cached settings UI for the current ImGui frame.
    /// </summary>
    /// <remarks>
    /// The configured ID scope is always popped before this method returns, even if a node throws while drawing. After <see cref="Dispose"/>, this method becomes a silent no-op.
    /// </remarks>
    public void Draw()
    {
        if (_disposed)
            return;

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
    /// Disposes any stateful resources collected during draw-tree construction and marks the drawer as disposed.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing.
    /// </remarks>
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
