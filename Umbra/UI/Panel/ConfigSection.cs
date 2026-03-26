using Umbra.Config.Attributes;
using Umbra.UI.Config;

namespace Umbra.UI.Panel;

/// <summary>
/// A <see cref="IPanelSection"/> that renders a typed configuration object as a settings
/// panel using <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
/// <remarks>
/// <para>
/// When the config type carries <see cref="ConfigRootNodeAttribute"/>, the section
/// automatically exposes <see cref="IPanelSection.TreeNodeLabel"/> and
/// <see cref="IPanelSection.TreeNodeDefaultOpen"/> so that the owning
/// <see cref="PluginPanel"/> renders the tree node. An explicit <c>treeNodeLabel</c>
/// constructor argument overrides the attribute value. Pass
/// <c>suppressTreeNode = true</c> to opt out entirely even when the
/// attribute is present.
/// </para>
/// <para>
/// The <c>idScope</c> defaults to the config type name when not supplied.
/// <see cref="PluginPanel"/> pushes a top-level ImGui ID scope before calling
/// <see cref="Draw"/>; this sub-scope nests inside it, preventing widget ID collisions
/// when two config sections of the same type appear in the same panel.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type. Must have a public parameterless constructor.
/// </typeparam>
public sealed class ConfigSection<TConfig> : IPanelSection where TConfig : class, new()
{
    private readonly ConfigDrawer<TConfig> _drawer;
    private readonly string _sectionId;
    private readonly int _order;
    private readonly string? _treeNodeLabel;
    private readonly bool _treeNodeDefaultOpen;
    private bool _disposed;

    /// <summary>
    /// Initialises a new config section wrapping a <see cref="ConfigDrawer{TConfig}"/>.
    /// </summary>
    /// <param name="config">
    /// A fully initialised configuration instance, ideally returned by
    /// <see cref="Umbra.Config.SettingsStore{TConfig}.Load()"/>.
    /// </param>
    /// <param name="idScope">
    /// Stable ImGui widget ID scope for this section, used as the
    /// <see cref="IPanelSection.SectionId"/> and as the inner scope passed to
    /// <see cref="ConfigDrawer{TConfig}"/>. When a tree node is rendered,
    /// <see cref="PluginPanel"/> embeds this value as a <c>##</c> disambiguation
    /// suffix on the tree node label (e.g. <c>"Settings##MyConfig"</c>) rather than
    /// pushing an extra <c>ImGui.PushID</c> scope before the node.
    /// Defaults to <c>typeof(<typeparamref name="TConfig"/>).FullName</c> (falling back to
    /// <c>typeof(<typeparamref name="TConfig"/>).Name</c> when <c>FullName</c> is
    /// <see langword="null"/>) when not supplied — a namespace-qualified value that prevents
    /// two config types with the same short name from colliding.
    /// </param>
    /// <param name="treeNodeLabel">
    /// Explicit tree node label that overrides any <see cref="ConfigRootNodeAttribute"/>
    /// on the config type. When <see langword="null"/> (the default), the label is read from
    /// the attribute, or no tree node is used when the attribute is absent.
    /// Ignored when <paramref name="suppressTreeNode"/> is <see langword="true"/>.
    /// </param>
    /// <param name="treeNodeDefaultOpen">
    /// Whether the section tree node starts expanded. Only applies when an explicit
    /// <paramref name="treeNodeLabel"/> is provided. When the tree node label is derived from
    /// <see cref="ConfigRootNodeAttribute"/>, the attribute's own <c>DefaultOpen</c> value
    /// controls the initial state and this parameter is ignored. Also ignored when
    /// <paramref name="suppressTreeNode"/> is <see langword="true"/>.
    /// </param>
    /// <param name="suppressTreeNode">
    /// When <see langword="true"/>, no tree node is rendered for this section even when
    /// <see cref="ConfigRootNodeAttribute"/> is present on <typeparamref name="TConfig"/>.
    /// </param>
    public ConfigSection(TConfig config, string? idScope = null,
        string? treeNodeLabel = null, bool treeNodeDefaultOpen = false,
        bool suppressTreeNode = false)
    {
        _sectionId = idScope ?? typeof(TConfig).FullName ?? typeof(TConfig).Name;
        _order     = typeof(TConfig).GetDrawerAttribute<SectionOrderAttribute>()?.Order ?? int.MaxValue;

        if (!suppressTreeNode)
        {
            if (treeNodeLabel is not null)
            {
                _treeNodeLabel       = treeNodeLabel;
                _treeNodeDefaultOpen = treeNodeDefaultOpen;
            }
            else
            {
                var attr = typeof(TConfig).GetDrawerAttribute<ConfigRootNodeAttribute>();
                if (attr is not null)
                {
                    _treeNodeLabel       = attr.Label ?? typeof(TConfig).Name.ToDisplayName();
                    _treeNodeDefaultOpen = attr.DefaultOpen;
                }
            }
        }

        // Always suppress the drawer's own root tree node: PluginPanel renders it via
        // IPanelSection.TreeNodeLabel so the wrapping is not duplicated.
        _drawer = new ConfigDrawer<TConfig>(config, _sectionId, suppressRootNode: true);
    }

    /// <inheritdoc/>
    public int Order => _order;

    /// <inheritdoc/>
    /// <remarks>
    /// Returns the effective <c>idScope</c> value — either the explicitly supplied scope or
    /// <c>typeof(<typeparamref name="TConfig"/>).FullName</c> (falling back to
    /// <c>typeof(<typeparamref name="TConfig"/>).Name</c>). This value is the same string
    /// passed to the inner <see cref="ConfigDrawer{TConfig}"/>, so the
    /// <see cref="PluginPanel"/>-level push and the drawer's own internal push nest cleanly
    /// under the same named scope.
    /// </remarks>
    public string SectionId => _sectionId;

    /// <inheritdoc/>
    public string? TreeNodeLabel => _treeNodeLabel;

    /// <inheritdoc/>
    public bool TreeNodeDefaultOpen => _treeNodeDefaultOpen;

    /// <inheritdoc/>
    public void Draw()
    {
        if (_disposed) return;
        _drawer.Draw();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _drawer.Dispose();
        GC.SuppressFinalize(this);
    }
}
