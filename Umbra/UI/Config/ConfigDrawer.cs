using Hexa.NET.ImGui;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Rendering;
using Umbra.UI.Config.Search;

namespace Umbra.UI.Config;

/// <summary>
/// Builds and renders an ImGui config panel for a loaded configuration instance.
/// </summary>
/// <remarks>
/// <para>
/// The draw tree is assembled once at construction time. Later <see cref="Draw"/> calls walk the cached nodes without performing per-frame reflection.
/// </para>
/// <para>
/// Pass a configuration instance returned by <see cref="Umbra.Config.ConfigStore{TConfig}.Load()"/> so each registered parameter already carries resolved <see cref="Umbra.Config.ParameterMetadata"/>. Nested-group wrapper attributes are still read from reflected property and type metadata during the one-time build pass.
/// </para>
/// <para>
/// Root-node composition is delegated to <see cref="ConfigDrawerRootNodeComposer"/>, built-in search-row UI is delegated to <see cref="ConfigDrawerSearchController"/>, and per-frame search-state application is delegated to <see cref="ConfigDrawerSearchApplicator"/>.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration type rendered by the drawer.</typeparam>
public sealed class ConfigDrawer<TConfig> : IDisposable where TConfig : class
{
    private readonly List<IDrawNode> _nodes;
    private readonly List<IDisposable> _disposables;
    private readonly string _idScope;
    private readonly IConfigDrawerRenderer _renderer;
    private readonly ConfigSearchIndex _searchIndex;
    private readonly ConfigDrawerSearchController _searchController;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> by reflecting over
    /// <paramref name="config"/> once to build the complete draw tree.
    /// </summary>
    /// <param name="config">
    /// A fully initialized configuration instance, ideally returned by
    /// <see cref="Umbra.Config.ConfigStore{TConfig}.Load()"/> so that <see cref="Umbra.Config.ParameterMetadata"/>
    /// is already populated on every parameter.
    /// </param>
    /// <param name="idScope">
    /// A plugin-unique identifier string (e.g. <c>"MyPlugin"</c>) used to scope all ImGui
    /// widget IDs rendered by this drawer via <see cref="ImGui.PushID(string)"/> / <see cref="ImGui.PopID()"/>.
    /// Every widget ID within a <see cref="Draw"/> call is internally prefixed with this
    /// string, preventing duplicate-ID warnings when multiple plugins render config panels
    /// in the same ImGui window. Must be non-null and non-whitespace.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public ConfigDrawer(TConfig config, string idScope)
        : this(config, idScope, ConfigDrawerOptions.Default, ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> by reflecting over
    /// <paramref name="config"/> once to build the complete draw tree, using the supplied drawer options.
    /// </summary>
    /// <param name="config">The configuration instance whose draw tree should be built.</param>
    /// <param name="idScope">The plugin-unique ImGui ID scope string.</param>
    /// <param name="options">
    /// The optional feature flags that customize drawer behavior, including whether the built-in search bar is shown
    /// and whether the root-node-attribute-driven root tree wrapper is suppressed.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public ConfigDrawer(TConfig config, string idScope, ConfigDrawerOptions options)
        : this(config, idScope, options, ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> by reflecting over
    /// <paramref name="config"/> once to build the complete draw tree, using the specified
    /// renderer seam.
    /// </summary>
    /// <param name="config">The configuration instance whose draw tree should be built.</param>
    /// <param name="idScope">The plugin-unique ImGui ID scope string.</param>
    /// <param name="options">
    /// The optional feature flags that customize drawer behavior, including whether the built-in search bar is shown
    /// and whether the root-node-attribute-driven root tree wrapper is suppressed.
    /// </param>
    /// <param name="renderer">The renderer seam used for outer drawer chrome, ID-scope operations, and search-row layout.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/>, <paramref name="options"/>, or <paramref name="renderer"/> is <see langword="null"/>.
    /// </exception>
    internal ConfigDrawer(TConfig config, string idScope, ConfigDrawerOptions options, IConfigDrawerRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(renderer);
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null, empty, or whitespace.", nameof(idScope));

        _idScope = idScope;
        _renderer = renderer;
        var builder = new ConfigDrawerBuilder(options.NumericEditSink);
        builder.Collect(config, typeof(TConfig));
        builder.SortAll();
        _searchIndex = builder.SearchIndex;
        _searchController = new ConfigDrawerSearchController(options, _renderer, _searchIndex);
        _disposables = builder.Disposables;

        _nodes = ConfigDrawerRootNodeComposer.Compose<TConfig>(
            _idScope,
            builder.Nodes,
            _searchIndex,
            options.SuppressRootNode);
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> with a pre-built node list.
    /// </summary>
    /// <remarks>
    /// This constructor exists for tests that need to verify draw ordering, disposal behavior, search-state application, and
    /// renderer cleanup without building a runtime-backed ImGui node tree.
    /// </remarks>
    /// <param name="idScope">The plugin-unique ImGui ID scope string.</param>
    /// <param name="nodes">The pre-built nodes to draw each frame.</param>
    /// <param name="disposables">The disposable resources owned by the drawer.</param>
    /// <param name="renderer">The renderer seam used to verify drawer-level UI operations without an active ImGui frame.</param>
    /// <param name="options">Optional feature flags that customize drawer behavior for the test instance. When <see langword="null"/>, <see cref="ConfigDrawerOptions.Default"/> is used.</param>
    /// <param name="searchIndex">The pre-built flat search index used by the test instance. When <see langword="null"/>, an empty index is created.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="nodes"/>, <paramref name="disposables"/>, or <paramref name="renderer"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="idScope"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    internal ConfigDrawer(string idScope, List<IDrawNode> nodes, List<IDisposable> disposables, IConfigDrawerRenderer renderer, ConfigDrawerOptions? options = null, ConfigSearchIndex? searchIndex = null)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(disposables);
        ArgumentNullException.ThrowIfNull(renderer);
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null, empty, or whitespace.", nameof(idScope));

        _idScope = idScope;
        var effectiveOptions = options ?? ConfigDrawerOptions.Default;
        _nodes = nodes;
        _disposables = disposables;
        _renderer = renderer;
        _searchIndex = searchIndex ?? new ConfigSearchIndex();
        _searchController = new ConfigDrawerSearchController(effectiveOptions, _renderer, _searchIndex);
    }

    /// <summary>
    /// Gets a value indicating whether the built-in search row currently has an active non-empty query.
    /// </summary>
    internal bool HasActiveSearchQuery => _searchController.CurrentState?.HasActiveQuery ?? false;

    /// <summary>
    /// Renders the cached config UI for the current ImGui frame.
    /// </summary>
    /// <remarks>
    /// The configured ID scope is always popped before this method returns, even if a node throws while drawing. After <see cref="Dispose"/>, this method becomes a silent no-op.
    /// </remarks>
    public void Draw()
    {
        if (_disposed)
            return;

        _renderer.PushId(_idScope);
        try
        {
            _searchController.DrawControls();
            ConfigDrawerSearchApplicator.Apply(_nodes, _searchIndex, _searchController.CurrentState);
            foreach (var node in _nodes)
                node.Draw();
        }
        finally
        {
            _renderer.PopId();
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
}
