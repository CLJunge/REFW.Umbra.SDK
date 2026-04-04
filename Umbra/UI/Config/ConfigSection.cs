using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Panel;

namespace Umbra.UI.Config;

/// <summary>
/// Wraps a <see cref="ConfigDrawer{TConfig}"/> as a <see cref="IPanelSection"/> for use inside <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// When <typeparamref name="TConfig"/> carries <see cref="UmbraRootNodeAttribute"/>, this section surfaces the corresponding tree-node label and default-open state to the owning <see cref="PluginPanel"/>, unless tree-node behavior is explicitly suppressed.
/// </remarks>
/// <typeparam name="TConfig">The configuration type rendered by the section.</typeparam>
public sealed class ConfigSection<TConfig> : IPanelSection where TConfig : class
{
    private readonly ConfigDrawer<TConfig> _drawer;
    private readonly string _sectionId;
    private readonly int _order;
    private readonly string? _treeNodeLabel;
    private readonly bool _treeNodeDefaultOpen;
    private ConfigTransferFeature? _transferFeature;
    private bool _disposed;

    /// <summary>
    /// Initialises a new config section wrapping a <see cref="ConfigDrawer{TConfig}"/>.
    /// </summary>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="idScope">
    /// Optional stable ImGui widget ID sub-scope for this section. When omitted,
    /// <c>typeof(<typeparamref name="TConfig"/>).FullName</c> (falling back to
    /// <c>typeof(<typeparamref name="TConfig"/>).Name</c>) is used instead. Must not be empty or
    /// whitespace when supplied.
    /// </param>
    /// <param name="treeNodeLabel">
    /// Optional label for a collapsible tree node wrapped around this section by the owning
    /// <see cref="PluginPanel"/>.
    /// </param>
    /// <param name="treeNodeDefaultOpen">
    /// Whether the optional tree node starts expanded. Ignored when <paramref name="treeNodeLabel"/>
    /// is <see langword="null"/>.
    /// </param>
    /// <param name="suppressTreeNode">
    /// When <see langword="true"/>, suppresses any tree-node metadata inferred from
    /// <see cref="UmbraRootNodeAttribute"/> on <typeparamref name="TConfig"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public ConfigSection(TConfig config, string? idScope = null,
        string? treeNodeLabel = null, bool treeNodeDefaultOpen = false,
        bool suppressTreeNode = false)
        : this(config, ConfigDrawerOptions.Default, idScope, treeNodeLabel, treeNodeDefaultOpen, suppressTreeNode)
    {
    }

    /// <summary>
    /// Initialises a new config section wrapping a <see cref="ConfigDrawer{TConfig}"/>, using the supplied drawer options.
    /// </summary>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="options">
    /// The optional feature flags that customize the wrapped drawer behavior. Section-level tree-node metadata
    /// remains controlled by <paramref name="treeNodeLabel"/>, <paramref name="treeNodeDefaultOpen"/>, and
    /// <paramref name="suppressTreeNode"/>.
    /// </param>
    /// <param name="idScope">
    /// Optional stable ImGui widget ID sub-scope for this section. When omitted,
    /// <c>typeof(<typeparamref name="TConfig"/>).FullName</c> (falling back to
    /// <c>typeof(<typeparamref name="TConfig"/>).Name</c>) is used instead. Must not be empty or
    /// whitespace when supplied.
    /// </param>
    /// <param name="treeNodeLabel">
    /// Optional label for a collapsible tree node wrapped around this section by the owning
    /// <see cref="PluginPanel"/>.
    /// </param>
    /// <param name="treeNodeDefaultOpen">
    /// Whether the optional tree node starts expanded. Ignored when <paramref name="treeNodeLabel"/>
    /// is <see langword="null"/>.
    /// </param>
    /// <param name="suppressTreeNode">
    /// When <see langword="true"/>, suppresses any tree-node metadata inferred from
    /// <see cref="UmbraRootNodeAttribute"/> on <typeparamref name="TConfig"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public ConfigSection(TConfig config, ConfigDrawerOptions options, string? idScope = null,
        string? treeNodeLabel = null, bool treeNodeDefaultOpen = false,
        bool suppressTreeNode = false)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(options);
        if (idScope is not null && string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be empty or whitespace when supplied.", nameof(idScope));

        _sectionId = idScope ?? typeof(TConfig).FullName ?? typeof(TConfig).Name;
        _order = typeof(TConfig).GetDrawerAttribute<UmbraSectionOrderAttribute>()?.Order ?? int.MaxValue;

        if (!suppressTreeNode)
        {
            if (treeNodeLabel is not null)
            {
                _treeNodeLabel = treeNodeLabel;
                _treeNodeDefaultOpen = treeNodeDefaultOpen;
            }
            else
            {
                var attr = GetRootNodeMetadata(typeof(TConfig));
                if (attr.HasValue)
                {
                    _treeNodeLabel = attr.Value.Label ?? typeof(TConfig).Name.ToDisplayName();
                    _treeNodeDefaultOpen = attr.Value.DefaultOpen;
                }
            }
        }

        _drawer = new ConfigDrawer<TConfig>(config, _sectionId, options.WithSuppressRootNode(true));
    }

    /// <summary>
    /// Creates a config section with access to the loaded settings store for optional built-in transfer UI.
    /// </summary>
    /// <remarks>
    /// When enabled through <see cref="ConfigDrawerOptions.Transfer"/>, the built-in transfer UI is rendered
    /// after the normal config nodes rather than as part of the config object graph.
    /// </remarks>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="store">The loaded settings store associated with <paramref name="config"/>.</param>
    /// <param name="idScope">Optional stable ImGui widget ID sub-scope for this section.</param>
    /// <param name="treeNodeLabel">Optional label for a collapsible tree node wrapped around this section by the owning <see cref="PluginPanel"/>.</param>
    /// <param name="treeNodeDefaultOpen">Whether the optional tree node starts expanded.</param>
    /// <param name="suppressTreeNode">When <see langword="true"/>, suppresses any tree-node metadata inferred from <see cref="UmbraRootNodeAttribute"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public static ConfigSection<TConfig> CreateWithStore(
        TConfig config,
        IConfigTransferStore store,
        string? idScope = null,
        string? treeNodeLabel = null,
        bool treeNodeDefaultOpen = false,
        bool suppressTreeNode = false)
        => CreateWithStore(config, store, ConfigDrawerOptions.Default, idScope, treeNodeLabel, treeNodeDefaultOpen, suppressTreeNode);

    /// <summary>
    /// Creates a config section with access to the loaded settings store for optional built-in transfer UI and the supplied drawer options.
    /// </summary>
    /// <remarks>
    /// When enabled through <see cref="ConfigDrawerOptions.Transfer"/>, the built-in transfer UI is rendered
    /// after the normal config nodes rather than as part of the config object graph.
    /// </remarks>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="store">The loaded settings store associated with <paramref name="config"/>.</param>
    /// <param name="options">The optional feature flags that customize the wrapped drawer behavior.</param>
    /// <param name="idScope">Optional stable ImGui widget ID sub-scope for this section.</param>
    /// <param name="treeNodeLabel">Optional label for a collapsible tree node wrapped around this section by the owning <see cref="PluginPanel"/>.</param>
    /// <param name="treeNodeDefaultOpen">Whether the optional tree node starts expanded.</param>
    /// <param name="suppressTreeNode">When <see langword="true"/>, suppresses any tree-node metadata inferred from <see cref="UmbraRootNodeAttribute"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/>, <paramref name="store"/>, or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public static ConfigSection<TConfig> CreateWithStore(
        TConfig config,
        IConfigTransferStore store,
        ConfigDrawerOptions options,
        string? idScope = null,
        string? treeNodeLabel = null,
        bool treeNodeDefaultOpen = false,
        bool suppressTreeNode = false)
    {
        ArgumentNullException.ThrowIfNull(store);
        var section = new ConfigSection<TConfig>(config, options, idScope, treeNodeLabel, treeNodeDefaultOpen, suppressTreeNode);
        section._transferFeature = CreateTransferFeature(store, options);
        return section;
    }

    /// <inheritdoc/>
    public int Order => _order;

    /// <inheritdoc/>
    public string SectionId => _sectionId;

    /// <inheritdoc/>
    public string? TreeNodeLabel => _treeNodeLabel;

    /// <inheritdoc/>
    public bool TreeNodeDefaultOpen => _treeNodeDefaultOpen;

    /// <inheritdoc/>
    /// <remarks>
    /// After <see cref="Dispose"/> has been called, this method becomes a silent no-op.
    /// </remarks>
    public void Draw()
    {
        if (_disposed) return;
        _drawer.Draw();
        _transferFeature?.Draw();
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Repeated calls after the first one do nothing.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _transferFeature?.Dispose();
        _transferFeature = null;
        _drawer.Dispose();
        GC.SuppressFinalize(this);
    }

    private static ConfigTransferFeature? CreateTransferFeature(IConfigTransferStore store, ConfigDrawerOptions options)
    {
        var transferOptions = options.Transfer;
        if (transferOptions is null || !transferOptions.Enabled)
            return null;

        return new ConfigTransferFeature(store, transferOptions);
    }

    private static (string? Label, bool DefaultOpen)? GetRootNodeMetadata(Type type)
    {
        foreach (var attr in type.GetCustomAttributes(inherit: true))
            if (attr is UmbraRootNodeAttribute prefixed)
                return (prefixed.Label, prefixed.DefaultOpen);

        return null;
    }
}
