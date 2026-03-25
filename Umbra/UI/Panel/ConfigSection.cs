using Umbra.Config.Attributes;
using Umbra.Config.UI;

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
    private readonly int _order;
    private readonly string? _treeNodeLabel;
    private readonly bool _treeNodeDefaultOpen;
    private bool _disposed;

    /// <summary>
    /// Initialises a new config section wrapping a <see cref="ConfigDrawer{TConfig}"/>.
    /// </summary>
    /// <param name="config">
    /// A fully initialised configuration instance, ideally returned by
    /// <see cref="Config.SettingsStore{TConfig}.Load()"/>.
    /// </param>
    /// <param name="idScope">
    /// ImGui widget ID sub-scope for this section. Defaults to
    /// <c>typeof(<typeparamref name="TConfig"/>).Name</c> when not supplied.
    /// </param>
    /// <param name="treeNodeLabel">
    /// Explicit tree node label that overrides any <see cref="ConfigRootNodeAttribute"/>
    /// on the config type. When <see langword="null"/> (the default), the label is read from
    /// the attribute, or no tree node is used when the attribute is absent.
    /// Ignored when <paramref name="suppressTreeNode"/> is <see langword="true"/>.
    /// </param>
    /// <param name="treeNodeDefaultOpen">
    /// Whether the section tree node starts expanded. Ignored when
    /// <paramref name="treeNodeLabel"/> resolves to <see langword="null"/> or
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
        _order = typeof(TConfig).GetDrawerAttribute<SectionOrderAttribute>()?.Order ?? int.MaxValue;

        if (!suppressTreeNode)
        {
            if (treeNodeLabel is not null)
            {
                _treeNodeLabel      = treeNodeLabel;
                _treeNodeDefaultOpen = treeNodeDefaultOpen;
            }
            else
            {
                var attr = typeof(TConfig).GetDrawerAttribute<ConfigRootNodeAttribute>();
                if (attr is not null)
                {
                    _treeNodeLabel      = attr.Label ?? typeof(TConfig).Name.ToDisplayName();
                    _treeNodeDefaultOpen = attr.DefaultOpen;
                }
            }
        }

        // Always suppress the drawer's own root tree node: PluginPanel renders it via
        // IPanelSection.TreeNodeLabel so the wrapping is not duplicated.
        _drawer = new ConfigDrawer<TConfig>(config, idScope ?? typeof(TConfig).Name, suppressRootNode: true);
    }

    /// <inheritdoc/>
    public int Order => _order;

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
