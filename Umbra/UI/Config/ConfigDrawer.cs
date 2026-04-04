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
    private const uint SearchBarMaxLength = 256;
    private const float MinimumSearchInputWidth = 64f;
    private const string SearchLabel = "Search";
    private const string SearchInputLabel = "##ConfigDrawerSearch";
    private const string PreviousButtonLabel = "<##ConfigDrawerSearchPrevious";
    private const string NextButtonLabel = ">##ConfigDrawerSearchNext";

    private readonly List<IDrawNode> _nodes;
    private readonly List<IDisposable> _disposables;
    private readonly string _idScope;
    private readonly ConfigDrawerOptions _options;
    private readonly IConfigDrawerRenderer _renderer;
    private readonly ConfigSearchIndex _searchIndex;
    private readonly ConfigDrawerSearchState? _searchState;
    private readonly ConfigDrawerSearchLayoutState? _searchLayoutState;
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
        : this(config, idScope, ConfigDrawerOptions.Default, ImGuiConfigRenderContext.Instance, suppressRootNode)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> by reflecting over
    /// <paramref name="config"/> once to build the complete draw tree, using the supplied drawer options.
    /// </summary>
    /// <param name="config">The configuration instance whose draw tree should be built.</param>
    /// <param name="idScope">The plugin-unique ImGui ID scope string.</param>
    /// <param name="options">The optional feature flags that customize drawer behavior.</param>
    /// <param name="suppressRootNode">
    /// When <see langword="true"/>, suppresses the root-node-attribute-driven root tree wrapper.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public ConfigDrawer(TConfig config, string idScope, ConfigDrawerOptions options, bool suppressRootNode = false)
        : this(config, idScope, options, ImGuiConfigRenderContext.Instance, suppressRootNode)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ConfigDrawer{TConfig}"/> by reflecting over
    /// <paramref name="config"/> once to build the complete draw tree, using the specified
    /// renderer seam.
    /// </summary>
    /// <param name="config">The configuration instance whose draw tree should be built.</param>
    /// <param name="idScope">The plugin-unique ImGui ID scope string.</param>
    /// <param name="options">The optional feature flags that customize drawer behavior.</param>
    /// <param name="renderer">The renderer seam used for outer drawer chrome, ID-scope operations, and search-row layout.</param>
    /// <param name="suppressRootNode">
    /// When <see langword="true"/>, suppresses the root-node-attribute-driven root tree wrapper.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config"/>, <paramref name="options"/>, or <paramref name="renderer"/> is <see langword="null"/>.
    /// </exception>
    internal ConfigDrawer(TConfig config, string idScope, ConfigDrawerOptions options, IConfigDrawerRenderer renderer, bool suppressRootNode = false)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(renderer);
        if (string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be null, empty, or whitespace.", nameof(idScope));

        _idScope = idScope;
        _options = options;
        _renderer = renderer;
        _searchState = options.ShowSearchBar ? new ConfigDrawerSearchState() : null;
        _searchLayoutState = options.ShowSearchBar ? new ConfigDrawerSearchLayoutState() : null;
        var builder = new ConfigDrawerBuilder();
        builder.Collect(config, typeof(TConfig));
        builder.SortAll();
        _searchIndex = builder.SearchIndex;
        _disposables = builder.Disposables;

        var rootAttr = GetRootNodeMetadata(typeof(TConfig));
        if (rootAttr.HasValue && !suppressRootNode)
        {
            var label = rootAttr.Value.Label ?? typeof(TConfig).Name.ToDisplayName();
            var rootBranchId = BuildRootBranchId(_idScope);
            _searchIndex.PrependRootBranch(rootBranchId);
            _nodes = [new RootTreeNode(label, rootAttr.Value.DefaultOpen, builder.Nodes, rootBranchId)];
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
        _options = options ?? ConfigDrawerOptions.Default;
        _searchState = _options.ShowSearchBar ? new ConfigDrawerSearchState() : null;
        _searchLayoutState = _options.ShowSearchBar ? new ConfigDrawerSearchLayoutState() : null;
        _nodes = nodes;
        _disposables = disposables;
        _renderer = renderer;
        _searchIndex = searchIndex ?? new ConfigSearchIndex();
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

        _renderer.PushId(_idScope);
        try
        {
            DrawSearchBar();
            ApplySearchState();
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

    private static (string? Label, bool DefaultOpen)? GetRootNodeMetadata(Type type)
    {
        foreach (var attr in type.GetCustomAttributes(inherit: true))
            if (attr is UmbraRootNodeAttribute prefixed)
                return (prefixed.Label, prefixed.DefaultOpen);

        return null;
    }

    private static string BuildRootBranchId(string idScope) => $"root:{idScope}";

    private void DrawSearchBar()
    {
        var searchState = _searchState;
        var layoutState = _searchLayoutState;
        if (searchState is null || layoutState is null)
            return;

        EnsureSearchLayout(layoutState);

        _renderer.Text(SearchLabel);
        _renderer.SameLine();

        var query = searchState.Query;
        _renderer.SetNextItemWidth(layoutState.SearchInputWidth);
        if (_renderer.InputText(SearchInputLabel, ref query, SearchBarMaxLength))
        {
            searchState.SetQuery(query);
            RefreshSearchMatches(searchState);
        }

        _renderer.SameLine();
        if (_renderer.Button(PreviousButtonLabel))
            searchState.MovePrevious();

        _renderer.SameLine();
        if (_renderer.Button(NextButtonLabel))
            searchState.MoveNext();
    }

    private void EnsureSearchLayout(ConfigDrawerSearchLayoutState layoutState)
    {
        var availableWidth = _renderer.GetAvailableWidth();
        if (layoutState.IsInitialized && layoutState.LastAvailableWidth == availableWidth)
            return;

        layoutState.LastAvailableWidth = availableWidth;
        layoutState.PreviousButtonWidth = _renderer.GetButtonWidth(PreviousButtonLabel);
        layoutState.NextButtonWidth = _renderer.GetButtonWidth(NextButtonLabel);

        var labelWidth = _renderer.GetTextWidth(SearchLabel);
        var spacingX = _renderer.GetItemSpacingX();
        var searchInputWidth = availableWidth
            - labelWidth
            - layoutState.PreviousButtonWidth
            - layoutState.NextButtonWidth
            - (spacingX * 3f);
        layoutState.SearchInputWidth = Math.Max(MinimumSearchInputWidth, searchInputWidth);
        layoutState.IsInitialized = true;
    }

    private void RefreshSearchMatches(ConfigDrawerSearchState searchState)
        => searchState.SetMatches(_searchIndex.FindMatches(searchState.NormalizedQuery));

    private void ApplySearchState()
    {
        var searchState = _searchState;
        ConfigSearchRenderState? renderState = null;
        if (searchState is not null && searchState.HasActiveQuery)
        {
            var matchedResultIds = new HashSet<string>(searchState.MatchIds, StringComparer.Ordinal);
            var forcedOpenBranchIds = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < searchState.MatchIds.Count; i++)
            {
                if (!_searchIndex.TryGetEntry(searchState.MatchIds[i], out var entry))
                    continue;

                for (var j = 0; j < entry.AncestorBranchIds.Length; j++)
                    forcedOpenBranchIds.Add(entry.AncestorBranchIds[j]);
            }

            renderState = new ConfigSearchRenderState(searchState, matchedResultIds, forcedOpenBranchIds);
        }

        for (var i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i] is IConfigSearchNode searchNode)
                searchNode.ApplySearch(renderState);
        }
    }
}
