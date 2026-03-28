using Umbra.Config.Attributes;
using Umbra.UI.Panel;

namespace Umbra.UI.Config;

/// <summary>
/// A <see cref="IPanelSection"/> that renders a typed configuration object as a settings
/// panel using <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
/// <remarks>
/// <para>
/// When the config type carries <see cref="UmbraConfigRootNodeAttribute"/>, the section
/// automatically exposes <see cref="IPanelSection.TreeNodeLabel"/> and
/// <see cref="IPanelSection.TreeNodeDefaultOpen"/> so that the owning
/// <see cref="PluginPanel"/> renders the tree node. An explicit <c>treeNodeLabel</c>
/// constructor argument overrides the attribute value. Pass
/// <c>suppressTreeNode = true</c> to opt out entirely even when the
/// attribute is present.
/// </para>
/// <para>
/// The <c>idScope</c> defaults to <c>typeof(<typeparamref name="TConfig"/>).FullName</c>,
/// falling back to <c>typeof(<typeparamref name="TConfig"/>).Name</c> when the full name is
/// unavailable. <see cref="PluginPanel"/> pushes a top-level ImGui ID scope before calling
/// <see cref="Draw"/>; this sub-scope nests inside it, preventing widget ID collisions when two
/// config sections of the same type appear in the same panel.
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
    public ConfigSection(TConfig config, string? idScope = null,
        string? treeNodeLabel = null, bool treeNodeDefaultOpen = false,
        bool suppressTreeNode = false)
    {
        _sectionId = idScope ?? typeof(TConfig).FullName ?? typeof(TConfig).Name;
        _order = typeof(TConfig).GetDrawerAttribute<SectionOrderAttribute>()?.Order ?? int.MaxValue;

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

        _drawer = new ConfigDrawer<TConfig>(config, _sectionId, suppressRootNode: true);
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

    private static (string? Label, bool DefaultOpen)? GetRootNodeMetadata(Type type)
    {
        foreach (var attr in type.GetCustomAttributes(inherit: true))
            if (attr is UmbraConfigRootNodeAttribute prefixed)
                return (prefixed.Label, prefixed.DefaultOpen);

        return null;
    }
}
