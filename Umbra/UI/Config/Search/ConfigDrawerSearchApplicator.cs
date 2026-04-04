using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.Search;

/// <summary>
/// Applies the current config-drawer search state to the cached draw-node tree.
/// </summary>
/// <remarks>
/// This type isolates per-frame search render-state creation and searchable-node traversal from
/// <see cref="ConfigDrawer{TConfig}"/> so the drawer can remain focused on orchestration.
/// </remarks>
internal static class ConfigDrawerSearchApplicator
{
    /// <summary>
    /// Applies the current search state to the supplied top-level node list.
    /// </summary>
    /// <param name="nodes">The cached top-level draw nodes.</param>
    /// <param name="searchIndex">The flat search index used to resolve ancestor branch IDs.</param>
    /// <param name="searchState">The current search state, or <see langword="null"/> when search is disabled.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="nodes"/> or <paramref name="searchIndex"/> is <see langword="null"/>.
    /// </exception>
    internal static void Apply(
        List<IDrawNode> nodes,
        ConfigSearchIndex searchIndex,
        ConfigDrawerSearchState? searchState)
    {
        ArgumentNullException.ThrowIfNull(nodes);
        ArgumentNullException.ThrowIfNull(searchIndex);

        var renderState = CreateRenderState(searchIndex, searchState);
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] is IConfigSearchNode searchNode)
                searchNode.ApplySearch(renderState);
        }
    }

    private static ConfigSearchRenderState? CreateRenderState(
        ConfigSearchIndex searchIndex,
        ConfigDrawerSearchState? searchState)
    {
        if (searchState is null || !searchState.HasActiveQuery)
            return null;

        var matchedResultIds = new HashSet<string>(searchState.MatchIds, StringComparer.Ordinal);
        var forcedOpenBranchIds = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < searchState.MatchIds.Count; i++)
        {
            if (!searchIndex.TryGetEntry(searchState.MatchIds[i], out var entry))
                continue;

            for (var j = 0; j < entry.AncestorBranchIds.Length; j++)
                forcedOpenBranchIds.Add(entry.AncestorBranchIds[j]);
        }

        return new ConfigSearchRenderState(searchState, matchedResultIds, forcedOpenBranchIds);
    }
}
