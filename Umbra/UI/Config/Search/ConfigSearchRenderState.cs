namespace Umbra.UI.Config.Search;

/// <summary>
/// Stores the per-frame search state consumed by searchable config draw nodes.
/// </summary>
internal sealed class ConfigSearchRenderState
{
    private readonly ConfigDrawerSearchState _searchState;
    private readonly HashSet<string> _matchedResultIds;
    private readonly HashSet<string> _forcedOpenBranchIds;

    internal ConfigSearchRenderState(
        ConfigDrawerSearchState searchState,
        HashSet<string> matchedResultIds,
        HashSet<string> forcedOpenBranchIds)
    {
        ArgumentNullException.ThrowIfNull(searchState);
        ArgumentNullException.ThrowIfNull(matchedResultIds);
        ArgumentNullException.ThrowIfNull(forcedOpenBranchIds);

        _searchState = searchState;
        _matchedResultIds = matchedResultIds;
        _forcedOpenBranchIds = forcedOpenBranchIds;
    }

    internal bool HasActiveQuery => _searchState.HasActiveQuery;

    internal string? FocusedResultId => _searchState.FocusedResultId;

    internal bool IsMatch(string? resultId)
        => resultId is not null && _matchedResultIds.Contains(resultId);

    internal bool IsFocused(string? resultId)
        => resultId is not null && string.Equals(resultId, _searchState.FocusedResultId, StringComparison.Ordinal);

    internal bool ShouldScrollIntoView(string? resultId)
        => resultId is not null && string.Equals(resultId, _searchState.PendingScrollResultId, StringComparison.Ordinal);

    internal bool ShouldFocusControl(string? resultId)
        => resultId is not null && string.Equals(resultId, _searchState.PendingFocusResultId, StringComparison.Ordinal);

    internal void MarkScrolled(string? resultId)
    {
        if (resultId is null)
            return;

        _searchState.ClearPendingScrollTarget(resultId);
    }

    internal void MarkFocused(string? resultId)
    {
        if (resultId is null)
            return;

        _searchState.ClearPendingFocusTarget(resultId);
    }

    internal bool IsBranchForcedOpen(string? branchId)
        => branchId is not null && _forcedOpenBranchIds.Contains(branchId);
}
