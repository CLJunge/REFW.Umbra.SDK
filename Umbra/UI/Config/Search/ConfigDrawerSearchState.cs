namespace Umbra.UI.Config.Search;

/// <summary>
/// Stores per-drawer search query and navigation state.
/// </summary>
/// <remarks>
/// This state is local to one <see cref="ConfigDrawer{TConfig}"/> instance. Query and match refreshes
/// clear focused-result selection so matching rows are only focused after the user explicitly
/// navigates with the previous or next controls. Once a result is focused, this state coordinates the
/// pending scroll and keyboard-focus handoff consumed by searchable draw nodes.
/// </remarks>
internal sealed class ConfigDrawerSearchState
{
    private readonly List<string> _matchIds = [];
    private int _focusedMatchIndex = -1;

    /// <summary>
    /// Gets the raw query text currently shown in the search input.
    /// </summary>
    internal string Query { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the normalized query text used for matching, or <see cref="string.Empty"/> when the query is blank.
    /// </summary>
    internal string NormalizedQuery { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the pending result identifier to scroll into view, or <see langword="null"/> when no scroll is pending.
    /// </summary>
    internal string? PendingScrollResultId { get; private set; }

    /// <summary>
    /// Gets the pending result identifier that should receive keyboard focus, or <see langword="null"/> when no focus transfer is pending.
    /// </summary>
    internal string? PendingFocusResultId { get; private set; }

    /// <summary>
    /// Gets the currently focused result identifier, or <see langword="null"/> when no result is focused.
    /// </summary>
    /// <remarks>
    /// Matching rows remain unfocused after query changes until <see cref="MoveNext"/> or <see cref="MovePrevious"/> selects a result explicitly.
    /// </remarks>
    internal string? FocusedResultId => GetFocusedResultId();

    /// <summary>
    /// Gets the ordered match identifiers used by navigation.
    /// </summary>
    internal IReadOnlyList<string> MatchIds => _matchIds;

    /// <summary>
    /// Gets a value indicating whether a non-empty search query is active.
    /// </summary>
    internal bool HasActiveQuery => NormalizedQuery.Length > 0;

    /// <summary>
    /// Replaces the current query text and normalizes it for matching.
    /// </summary>
    /// <param name="query">The new raw query text.</param>
    /// <remarks>
    /// This clears any previously focused result. Matching rows remain visible, but pending scroll and
    /// keyboard-focus requests stay empty until the user navigates to a specific result.
    /// </remarks>
    internal void SetQuery(string? query)
    {
        Query = query ?? string.Empty;
        NormalizedQuery = Normalize(Query);
        _focusedMatchIndex = -1;
        SetPendingTargetsToFocusedResult();
    }

    /// <summary>
    /// Replaces the ordered match identifiers used for navigation.
    /// </summary>
    /// <param name="matchIds">The ordered result identifiers.</param>
    /// <remarks>
    /// When no result is currently focused, this keeps the drawer in the unfocused state until the user
    /// explicitly moves to the next or previous result.
    /// </remarks>
    internal void SetMatches(IEnumerable<string> matchIds)
    {
        ArgumentNullException.ThrowIfNull(matchIds);

        _matchIds.Clear();
        foreach (var matchId in matchIds)
            _matchIds.Add(matchId);

        ClampFocusedMatchIndex();
        SetPendingTargetsToFocusedResult();
    }

    /// <summary>
    /// Moves focus to the next result, wrapping to the first result when needed.
    /// </summary>
    /// <remarks>
    /// When no result is focused yet, this selects the first matching result and schedules both scroll
    /// and keyboard-focus handoff for that row.
    /// </remarks>
    internal void MoveNext()
    {
        if (_matchIds.Count == 0)
            return;

        _focusedMatchIndex = _focusedMatchIndex < 0
            ? 0
            : (_focusedMatchIndex + 1) % _matchIds.Count;
        SetPendingTargetsToFocusedResult();
    }

    /// <summary>
    /// Clears the pending scroll target when it has been consumed by the corresponding focused result.
    /// </summary>
    /// <param name="resultId">The result identifier that consumed the scroll request.</param>
    internal void ClearPendingScrollTarget(string resultId)
    {
        if (string.Equals(PendingScrollResultId, resultId, StringComparison.Ordinal))
            PendingScrollResultId = null;
    }

    /// <summary>
    /// Clears the pending keyboard-focus target when it has been consumed by the corresponding focused result.
    /// </summary>
    /// <param name="resultId">The result identifier that consumed the focus request.</param>
    internal void ClearPendingFocusTarget(string resultId)
    {
        if (string.Equals(PendingFocusResultId, resultId, StringComparison.Ordinal))
            PendingFocusResultId = null;
    }

    /// <summary>
    /// Moves focus to the previous result, wrapping to the last result when needed.
    /// </summary>
    /// <remarks>
    /// When no result is focused yet, this selects the last matching result and schedules both scroll
    /// and keyboard-focus handoff for that row.
    /// </remarks>
    internal void MovePrevious()
    {
        if (_matchIds.Count == 0)
            return;

        if (_focusedMatchIndex < 0)
            _focusedMatchIndex = _matchIds.Count - 1;
        else
            _focusedMatchIndex = (_focusedMatchIndex - 1 + _matchIds.Count) % _matchIds.Count;

        SetPendingTargetsToFocusedResult();
    }

    private void ClampFocusedMatchIndex()
    {
        if (_matchIds.Count == 0)
        {
            _focusedMatchIndex = -1;
            return;
        }

        if (_focusedMatchIndex < -1)
        {
            _focusedMatchIndex = -1;
            return;
        }

        if (_focusedMatchIndex >= _matchIds.Count)
            _focusedMatchIndex = _matchIds.Count - 1;
    }

    private string? GetFocusedResultId()
        => _focusedMatchIndex >= 0 && _focusedMatchIndex < _matchIds.Count
            ? _matchIds[_focusedMatchIndex]
            : null;

    private void SetPendingTargetsToFocusedResult()
    {
        var focusedResultId = GetFocusedResultId();
        PendingScrollResultId = focusedResultId;
        PendingFocusResultId = focusedResultId;
    }

    private static string Normalize(string query)
        => string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : query.Trim().ToUpperInvariant();
}
