using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Rendering;
using Umbra.UI.Config.Transfer;
using Umbra.UI.Panel;

namespace Umbra.UI.Config;

/// <summary>
/// Wraps a <see cref="ConfigDrawer{TConfig}"/> as a <see cref="IPanelSection"/> for use inside <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// When <typeparamref name="TConfig"/> carries <see cref="UmbraRootNodeAttribute"/>, this section surfaces the corresponding section label and default-open state to the owning <see cref="PluginPanel"/>, unless tree-node behavior is explicitly suppressed.
/// </remarks>
/// <typeparam name="TConfig">The configuration type rendered by the section.</typeparam>
public sealed class ConfigSection<TConfig> : IPanelSection where TConfig : class
{
    private static readonly ImGuiConfigRenderContext _renderContext = ImGuiConfigRenderContext.Instance;
    private readonly ConfigDrawer<TConfig> _drawer;
    private readonly string _sectionId;
    private readonly int _order;
    private readonly string? _sectionLabel;
    private readonly bool _expandedByDefault;
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
    /// <param name="sectionLabel">
    /// Optional label for a collapsible section wrapped around this section by the owning
    /// <see cref="PluginPanel"/>.
    /// </param>
    /// <param name="expandedByDefault">
    /// Whether the optional section starts expanded. Ignored when <paramref name="sectionLabel"/>
    /// is <see langword="null"/>.
    /// </param>
    /// <param name="suppressTreeNode">
    /// When <see langword="true"/>, suppresses any tree-node metadata inferred from
    /// <see cref="UmbraRootNodeAttribute"/> on <typeparamref name="TConfig"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public ConfigSection(TConfig config, string? idScope = null,
        string? sectionLabel = null, bool expandedByDefault = false,
        bool suppressTreeNode = false)
        : this(config, ConfigDrawerOptions.Default, idScope, sectionLabel, expandedByDefault, suppressTreeNode)
    {
    }

    /// <summary>
    /// Initialises a new config section wrapping a <see cref="ConfigDrawer{TConfig}"/>, using the supplied drawer options.
    /// </summary>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="options">
    /// The optional feature flags that customize the wrapped drawer behavior. Section-level presentation metadata
    /// remains controlled by <paramref name="sectionLabel"/>, <paramref name="expandedByDefault"/>, and
    /// <paramref name="suppressTreeNode"/>.
    /// </param>
    /// <param name="idScope">
    /// Optional stable ImGui widget ID sub-scope for this section. When omitted,
    /// <c>typeof(<typeparamref name="TConfig"/>).FullName</c> (falling back to
    /// <c>typeof(<typeparamref name="TConfig"/>).Name</c>) is used instead. Must not be empty or
    /// whitespace when supplied.
    /// </param>
    /// <param name="sectionLabel">
    /// Optional label for a collapsible section wrapped around this section by the owning
    /// <see cref="PluginPanel"/>.
    /// </param>
    /// <param name="expandedByDefault">
    /// Whether the optional section starts expanded. Ignored when <paramref name="sectionLabel"/>
    /// is <see langword="null"/>.
    /// </param>
    /// <param name="suppressTreeNode">
    /// When <see langword="true"/>, suppresses any tree-node metadata inferred from
    /// <see cref="UmbraRootNodeAttribute"/> on <typeparamref name="TConfig"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public ConfigSection(TConfig config, ConfigDrawerOptions options, string? idScope = null,
        string? sectionLabel = null, bool expandedByDefault = false,
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
            if (sectionLabel is not null)
            {
                _sectionLabel = sectionLabel;
                _expandedByDefault = expandedByDefault;
            }
            else
            {
                var attr = GetRootNodeMetadata(typeof(TConfig));
                if (attr.HasValue)
                {
                    _sectionLabel = attr.Value.Label ?? typeof(TConfig).Name.ToDisplayName();
                    _expandedByDefault = attr.Value.ExpandedByDefault;
                }
            }
        }

        _drawer = new ConfigDrawer<TConfig>(config, _sectionId, options.WithSuppressRootNode(true));
    }

    /// <summary>
    /// Creates a config section with access to the loaded config store for optional built-in transfer UI.
    /// </summary>
    /// <remarks>
    /// When enabled through <see cref="ConfigDrawerOptions.Transfer"/>, the built-in transfer UI is rendered
    /// in its own Umbra-owned tree node, before or after the normal config nodes according to the configured
    /// transfer placement option, rather than as part of the config object graph.
    /// </remarks>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="store">The loaded config store associated with <paramref name="config"/>.</param>
    /// <param name="idScope">Optional stable ImGui widget ID sub-scope for this section.</param>
    /// <param name="sectionLabel">Optional label for a collapsible section wrapped around this section by the owning <see cref="PluginPanel"/>.</param>
    /// <param name="expandedByDefault">Whether the optional section starts expanded.</param>
    /// <param name="suppressTreeNode">When <see langword="true"/>, suppresses any tree-node metadata inferred from <see cref="UmbraRootNodeAttribute"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public static ConfigSection<TConfig> CreateWithStore(
        TConfig config,
        IConfigTransferStore store,
        string? idScope = null,
        string? sectionLabel = null,
        bool expandedByDefault = false,
        bool suppressTreeNode = false)
        => CreateWithStore(config, store, ConfigDrawerOptions.Default, idScope, sectionLabel, expandedByDefault, suppressTreeNode);

    /// <summary>
    /// Creates a config section with access to the loaded config store for optional built-in transfer UI and the supplied drawer options.
    /// </summary>
    /// <remarks>
    /// When enabled through <see cref="ConfigDrawerOptions.Transfer"/>, the built-in transfer UI is rendered
    /// in its own Umbra-owned tree node, before or after the normal config nodes according to the configured
    /// transfer placement option, rather than as part of the config object graph.
    /// </remarks>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="store">The loaded config store associated with <paramref name="config"/>.</param>
    /// <param name="options">The optional feature flags that customize the wrapped drawer behavior.</param>
    /// <param name="idScope">Optional stable ImGui widget ID sub-scope for this section.</param>
    /// <param name="sectionLabel">Optional label for a collapsible section wrapped around this section by the owning <see cref="PluginPanel"/>.</param>
    /// <param name="expandedByDefault">Whether the optional section starts expanded.</param>
    /// <param name="suppressTreeNode">When <see langword="true"/>, suppresses any tree-node metadata inferred from <see cref="UmbraRootNodeAttribute"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/>, <paramref name="store"/>, or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public static ConfigSection<TConfig> CreateWithStore(
        TConfig config,
        IConfigTransferStore store,
        ConfigDrawerOptions options,
        string? idScope = null,
        string? sectionLabel = null,
        bool expandedByDefault = false,
        bool suppressTreeNode = false)
    {
        ArgumentNullException.ThrowIfNull(store);
        var section = new ConfigSection<TConfig>(config, options, idScope, sectionLabel, expandedByDefault, suppressTreeNode)
        {
            _transferFeature = CreateTransferFeature(store, options)
        };
        return section;
    }

    /// <inheritdoc/>
    public int Order => _order;

    /// <inheritdoc/>
    public string SectionId => _sectionId;

    /// <inheritdoc/>
    public string? SectionLabel => _sectionLabel;

    /// <inheritdoc/>
    public bool ExpandedByDefault => _expandedByDefault;

    /// <inheritdoc/>
    /// <remarks>
    /// After <see cref="Dispose"/> has been called, this method becomes a silent no-op.
    /// </remarks>
    public void Draw()
    {
        if (_disposed) return;

        if (ShouldDrawTransferFeature(_transferFeature is not null, _drawer.HasActiveSearchQuery)
            && _transferFeature!.Placement == ConfigTransferPlacement.BeforeConfig)
            DrawTransferFeature();

        _drawer.Draw();

        if (ShouldDrawTransferFeature(_transferFeature is not null, _drawer.HasActiveSearchQuery)
            && _transferFeature!.Placement != ConfigTransferPlacement.BeforeConfig)
            DrawTransferFeature();
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

    private void DrawTransferFeature()
    {
        var transferFeature = _transferFeature;
        if (transferFeature is null)
            return;

        PluginPanelTreeNodeLabels.WarnIfInvalid($"{_sectionId}.Transfer", transferFeature.SectionLabel, $"config-transfer section for '{_sectionId}'");
        var transferSectionLabel = $"{PluginPanelTreeNodeLabels.Sanitize(transferFeature.SectionLabel)}##{_sectionId}.Transfer";
        if (!_renderContext.TreeNode(transferSectionLabel, transferFeature.ExpandedByDefault))
            return;

        try
        {
            transferFeature.Draw();
        }
        finally
        {
            _renderContext.TreePop();
        }
    }

    internal static bool ShouldDrawTransferFeature(bool hasTransferFeature, bool hasActiveSearchQuery)
        => hasTransferFeature && !hasActiveSearchQuery;

    private static (string? Label, bool ExpandedByDefault)? GetRootNodeMetadata(Type type)
    {
        foreach (var attr in type.GetCustomAttributes(inherit: true))
            if (attr is UmbraRootNodeAttribute prefixed)
                return (prefixed.Label, prefixed.ExpandedByDefault);

        return null;
    }
}
