using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Search;

namespace Umbra.UI.Config;

/// <summary>
/// Composes the optional root tree node wrapper for a config drawer.
/// </summary>
/// <remarks>
/// This type isolates root-node metadata lookup and root-level search-branch composition from
/// <see cref="ConfigDrawer{TConfig}"/> so the drawer can stay focused on orchestration.
/// </remarks>
internal static class ConfigDrawerRootNodeComposer
{
    /// <summary>
    /// Returns the final top-level node list for a config drawer, optionally wrapping it in a root
    /// tree node when <typeparamref name="TConfig"/> declares <see cref="UmbraRootNodeAttribute"/>.
    /// </summary>
    /// <typeparam name="TConfig">The config type being rendered.</typeparam>
    /// <param name="idScope">The drawer's unique ID scope.</param>
    /// <param name="nodes">The already built top-level node list.</param>
    /// <param name="searchIndex">The flat search index built for the drawer.</param>
    /// <param name="suppressRootNode"><see langword="true"/> to suppress root-node wrapping.</param>
    /// <returns>
    /// The original <paramref name="nodes"/> list when no wrapping should occur; otherwise, a new
    /// node list containing a single <see cref="RootTreeNode"/> wrapper.
    /// </returns>
    internal static List<IDrawNode> Compose<TConfig>(
        string idScope,
        List<IDrawNode> nodes,
        ConfigSearchIndex searchIndex,
        bool suppressRootNode)
        where TConfig : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idScope);
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(searchIndex);

        var rootAttr = GetRootNodeMetadata(typeof(TConfig));
        if (!rootAttr.HasValue || suppressRootNode)
            return nodes;

        var label = rootAttr.Value.Label ?? typeof(TConfig).Name.ToDisplayName();
        var rootBranchId = BuildRootBranchId(idScope);
        searchIndex.PrependRootBranch(rootBranchId);
        return [new RootTreeNode(label, rootAttr.Value.ExpandedByDefault, nodes, rootBranchId)];
    }

    /// <summary>
    /// Gets the root-node metadata declared on <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The config type to inspect.</param>
    /// <returns>
    /// The configured root-node label and default-expanded state, or <see langword="null"/> when the
    /// type does not declare <see cref="UmbraRootNodeAttribute"/>.
    /// </returns>
    internal static (string? Label, bool ExpandedByDefault)? GetRootNodeMetadata(Type type)
    {
        foreach (var attr in type.GetCustomAttributes(inherit: true))
            if (attr is UmbraRootNodeAttribute prefixed)
                return (prefixed.Label, prefixed.ExpandedByDefault);

        return null;
    }

    /// <summary>
    /// Builds the synthetic search-branch identifier used for the optional root node.
    /// </summary>
    /// <param name="idScope">The drawer's unique ID scope.</param>
    /// <returns>The root-branch identifier for the drawer.</returns>
    internal static string BuildRootBranchId(string idScope) => $"root:{idScope}";
}
