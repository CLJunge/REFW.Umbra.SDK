namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the search-aware state update operation used by searchable configuration draw nodes.
/// </summary>
internal interface IConfigSearchNode
{
    /// <summary>
    /// Applies the current search state to this node and returns whether the node should remain visible.
    /// </summary>
    /// <param name="searchState">The active per-drawer search render state, or <see langword="null"/> when search is disabled.</param>
    /// <returns><see langword="true"/> when the node should be visible for the current search state; otherwise, <see langword="false"/>.</returns>
    bool ApplySearch(ConfigSearchRenderState? searchState);
}
