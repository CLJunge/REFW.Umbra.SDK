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
public sealed class ConfigSection<TConfig> : IPanelSection where TConfig : class, new()
{
    private static readonly ImGuiConfigRenderContext _renderContext = ImGuiConfigRenderContext.Instance;
    private readonly ConfigDrawer<TConfig> _drawer;
    private readonly string _sectionId;
    private readonly int _order;
    private readonly string? _sectionLabel;
    private readonly bool _expandedByDefault;
    private ConfigTransferFeature? _transferFeature;
    private ConfigUndoStack<TConfig>? _undoStack;
    private readonly IUndoShortcutInputSource? _undoInputSource;
    private ConfigSaveController<TConfig>? _saveController;
    private bool _disposed;
#if DEBUG
    private readonly bool _hasSearch;
    private readonly bool _enableDebugOverlay;
#endif

    /// <summary>
    /// Gets the undo stack owned by this section, or <see langword="null"/> when undo was not
    /// enabled through <see cref="ConfigDrawerOptions.Undo"/>.
    /// </summary>
    public ConfigUndoStack<TConfig>? UndoStack => _undoStack;

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
    /// <param name="enableDebugOverlay">
    /// When <see langword="true"/> (the default), a debug overlay is rendered at the top in <c>DEBUG</c> builds
    /// showing which optional features are enabled or disabled. When <see langword="false"/>, the overlay is suppressed.
    /// This parameter is ignored in <c>Release</c> builds.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public ConfigSection(
        TConfig config,
        string? idScope = null,
        string? sectionLabel = null,
        bool expandedByDefault = false,
        bool suppressTreeNode = false,
        bool enableDebugOverlay = true)
        : this(config, ConfigDrawerOptions.Default, idScope, sectionLabel, expandedByDefault, suppressTreeNode, enableDebugOverlay)
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
    /// <param name="enableDebugOverlay">
    /// When <see langword="true"/> (the default), a debug overlay is rendered at the top in <c>DEBUG</c> builds
    /// showing which optional features are enabled or disabled. When <see langword="false"/>, the overlay is suppressed.
    /// This parameter is ignored in <c>Release</c> builds.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public ConfigSection(
        TConfig config,
        ConfigDrawerOptions options,
        string? idScope = null,
        string? sectionLabel = null,
        bool expandedByDefault = false,
        bool suppressTreeNode = false,
        bool enableDebugOverlay = true)
    {
        ArgumentNullException.ThrowIfNull(config);
        ArgumentNullException.ThrowIfNull(options);
        if (idScope is not null && string.IsNullOrWhiteSpace(idScope))
            throw new ArgumentException("idScope cannot be empty or whitespace when supplied.", nameof(idScope));

        _undoInputSource = options.UndoInputSource;
        _sectionId = idScope ?? typeof(TConfig).FullName ?? typeof(TConfig).Name;
        _order = typeof(TConfig).GetDrawerAttribute<UmbraSectionOrderAttribute>()?.Order ?? int.MaxValue;
#if DEBUG
        _hasSearch = options.Search is not null;
        _enableDebugOverlay = enableDebugOverlay;
#endif

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
    /// Creates a config section backed by the loaded config store, with automatic event-driven
    /// persistence and optional transfer UI and undo stack.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <paramref name="store"/> is a <see cref="ConfigStore{TConfig}"/>, a
    /// <see cref="ConfigSaveController{TConfig}"/> is always created internally to handle
    /// event-driven persistence. The section owns the controller's lifetime.
    /// </para>
    /// <para>
    /// Optional features are created based on the default <see cref="ConfigDrawerOptions"/>:
    /// transfer UI when <see cref="ConfigDrawerOptions.Transfer"/> is non-null and enabled
    /// and undo stack when <see cref="ConfigDrawerOptions.Undo"/> is non-null.
    /// When both the undo stack and save controller implement <see cref="INumericEditSink"/>,
    /// they are composed so slider/drag interactions are tracked by both systems.
    /// </para>
    /// </remarks>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="store">The loaded config store associated with <paramref name="config"/>.</param>
    /// <param name="idScope">Optional stable ImGui widget ID sub-scope for this section.</param>
    /// <param name="sectionLabel">Optional label for a collapsible section wrapped around this section by the owning <see cref="PluginPanel"/>.</param>
    /// <param name="expandedByDefault">Whether the optional section starts expanded.</param>
    /// <param name="suppressTreeNode">When <see langword="true"/>, suppresses any tree-node metadata inferred from <see cref="UmbraRootNodeAttribute"/>.</param>
    /// <param name="enableDebugOverlay">
    /// When <see langword="true"/> (the default), a debug overlay is rendered at the top in <c>DEBUG</c> builds
    /// showing which optional features are enabled or disabled. When <see langword="false"/>, the overlay is suppressed.
    /// This parameter is ignored in <c>Release</c> builds.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> or <paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public static ConfigSection<TConfig> CreateWithStore(
        TConfig config,
        IConfigTransferStore store,
        string? idScope = null,
        string? sectionLabel = null,
        bool expandedByDefault = false,
        bool suppressTreeNode = false,
        bool enableDebugOverlay = true)
        => CreateWithStore(config, store, ConfigDrawerOptions.Default, idScope, sectionLabel, expandedByDefault, suppressTreeNode, enableDebugOverlay);

    /// <summary>
    /// Creates a config section backed by the loaded config store, with automatic event-driven
    /// persistence and optional transfer UI and undo stack using the supplied
    /// drawer options.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When <paramref name="store"/> is a <see cref="ConfigStore{TConfig}"/>, a
    /// <see cref="ConfigSaveController{TConfig}"/> is always created internally to handle
    /// event-driven persistence. The section owns the controller's lifetime.
    /// </para>
    /// <para>
    /// Optional features are created based on <paramref name="options"/>:
    /// transfer UI when <see cref="ConfigDrawerOptions.Transfer"/> is non-null and enabled
    /// and undo stack when <see cref="ConfigDrawerOptions.Undo"/> is non-null.
    /// When both the undo stack and save controller implement <see cref="INumericEditSink"/>,
    /// they are composed so slider/drag interactions are tracked by both systems.
    /// </para>
    /// </remarks>
    /// <param name="config">The already loaded configuration instance to render.</param>
    /// <param name="store">The loaded config store associated with <paramref name="config"/>.</param>
    /// <param name="options">The optional feature flags that customize the wrapped drawer behavior.</param>
    /// <param name="idScope">Optional stable ImGui widget ID sub-scope for this section.</param>
    /// <param name="sectionLabel">Optional label for a collapsible section wrapped around this section by the owning <see cref="PluginPanel"/>.</param>
    /// <param name="expandedByDefault">Whether the optional section starts expanded.</param>
    /// <param name="suppressTreeNode">When <see langword="true"/>, suppresses any tree-node metadata inferred from <see cref="UmbraRootNodeAttribute"/>.</param>
    /// <param name="enableDebugOverlay">
    /// When <see langword="true"/> (the default), a debug overlay is rendered at the top in <c>DEBUG</c> builds
    /// showing which optional features are enabled or disabled. When <see langword="false"/>, the overlay is suppressed.
    /// This parameter is ignored in <c>Release</c> builds.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/>, <paramref name="store"/>, or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="idScope"/> is supplied but is empty or whitespace.</exception>
    public static ConfigSection<TConfig> CreateWithStore(
        TConfig config,
        IConfigTransferStore store,
        ConfigDrawerOptions options,
        string? idScope = null,
        string? sectionLabel = null,
        bool expandedByDefault = false,
        bool suppressTreeNode = false,
        bool enableDebugOverlay = true)
    {
        ArgumentNullException.ThrowIfNull(store);
        var undoStack = CreateUndoStack(store, options);
        var saveController = store is IConfigStore<TConfig> configStore
            ? new ConfigSaveController<TConfig>(configStore)
            : null;

        INumericEditSink? numericSink;
        if (undoStack is null)
        {
            numericSink = saveController;
        }
        else
        {
            numericSink = NumericEditSinkComposer.Compose(undoStack, saveController);
        }

        ITextEditSink? textSink;
        if (undoStack is null)
        {
            textSink = saveController;
        }
        else
        {
            textSink = TextEditSinkComposer.Compose(undoStack, saveController);
        }

        var effectiveOptions = undoStack is null
            ? options.WithUndoInputSource(null).WithNumericEditSink(numericSink).WithTextEditSink(textSink)
            : options
                .WithUndoInputSource(options.UndoInputSource ?? new KeyboardUndoShortcutInputSource())
                .WithNumericEditSink(numericSink)
                .WithTextEditSink(textSink);

        var section = new ConfigSection<TConfig>(config, effectiveOptions, idScope, sectionLabel, expandedByDefault, suppressTreeNode, enableDebugOverlay)
        {
            _transferFeature = CreateTransferFeature(store, options),
            _undoStack = undoStack,
            _saveController = saveController
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

#if DEBUG
        if (_enableDebugOverlay)
            ConfigSectionDebugOverlay.Draw(
                _hasSearch,
                _transferFeature is not null,
                _undoStack is not null,
                _saveController is not null);
#endif

        TryHandleBuiltInUndo();

        var hasActiveSearch = _drawer.HasActiveSearchQuery;

        if (ShouldDrawFeatureSection(_transferFeature is not null, hasActiveSearch)
            && _transferFeature!.Placement == ConfigTransferPlacement.BeforeConfig)
            DrawTransferFeature();

        _drawer.Draw();

        if (ShouldDrawFeatureSection(_transferFeature is not null, hasActiveSearch)
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
        _saveController?.Dispose();
        _saveController = null;
        _undoStack?.Dispose();
        _undoStack = null;
        _transferFeature?.Dispose();
        _transferFeature = null;
        _drawer.Dispose();
        GC.SuppressFinalize(this);
    }

    internal void TryHandleBuiltInUndo()
    {
        if (_undoStack is null || _undoInputSource is null)
            return;

        UndoShortcutCoordinator.TryProcessShortcut(_undoInputSource);
    }

    private static ConfigTransferFeature? CreateTransferFeature(IConfigTransferStore store, ConfigDrawerOptions options)
    {
        var transferOptions = options.Transfer;
        if (transferOptions is null || !transferOptions.Enabled)
            return null;

        return new ConfigTransferFeature(store, transferOptions);
    }

    private static ConfigUndoStack<TConfig>? CreateUndoStack(IConfigTransferStore store, ConfigDrawerOptions options)
    {
        if (options.Undo is not { } undoOptions || store is not ConfigStore<TConfig> configStore)
            return null;

        return new ConfigUndoStack<TConfig>(configStore, undoOptions);
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

    internal static bool ShouldDrawFeatureSection(bool hasFeature, bool hasActiveSearchQuery)
        => hasFeature && !hasActiveSearchQuery;

    private static (string? Label, bool ExpandedByDefault)? GetRootNodeMetadata(Type type)
    {
        foreach (var attr in type.GetCustomAttributes(inherit: true))
            if (attr is UmbraRootNodeAttribute prefixed)
                return (prefixed.Label, prefixed.ExpandedByDefault);

        return null;
    }
}
