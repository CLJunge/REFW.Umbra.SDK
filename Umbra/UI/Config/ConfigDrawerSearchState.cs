namespace Umbra.UI.Config;

/// <summary>
/// Stores per-drawer search query and navigation state.
/// </summary>
/// <remarks>
/// This state is local to one <see cref="ConfigDrawer{TConfig}"/> instance. Match population and
/// focused-result scrolling are completed in later feature phases.
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
    /// Gets the currently focused result identifier, or <see langword="null"/> when no result is focused.
    /// </summary>
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
    internal void SetQuery(string? query)
    {
        Query = query ?? string.Empty;
        NormalizedQuery = Normalize(Query);
        ClampFocusedMatchIndex();
        PendingScrollResultId = GetFocusedResultId();
    }

    /// <summary>
    /// Replaces the ordered match identifiers used for navigation.
    /// </summary>
    /// <param name="matchIds">The ordered result identifiers.</param>
    internal void SetMatches(IEnumerable<string> matchIds)
    {
        ArgumentNullException.ThrowIfNull(matchIds);

        _matchIds.Clear();
        foreach (var matchId in matchIds)
            _matchIds.Add(matchId);

        ClampFocusedMatchIndex();
        PendingScrollResultId = GetFocusedResultId();
    }

    /// <summary>
    /// Moves focus to the next result, wrapping to the first result when needed.
    /// </summary>
    internal void MoveNext()
    {
        if (_matchIds.Count == 0)
            return;

        _focusedMatchIndex = _focusedMatchIndex < 0
            ? 0
            : (_focusedMatchIndex + 1) % _matchIds.Count;
        PendingScrollResultId = _matchIds[_focusedMatchIndex];
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
    /// Moves focus to the previous result, wrapping to the last result when needed.
    /// </summary>
    internal void MovePrevious()
    {
        if (_matchIds.Count == 0)
            return;

        if (_focusedMatchIndex < 0)
            _focusedMatchIndex = _matchIds.Count - 1;
        else
            _focusedMatchIndex = (_focusedMatchIndex - 1 + _matchIds.Count) % _matchIds.Count;

        PendingScrollResultId = _matchIds[_focusedMatchIndex];
    }

    private void ClampFocusedMatchIndex()
    {
        if (_matchIds.Count == 0)
        {
            _focusedMatchIndex = -1;
            return;
        }

        if (_focusedMatchIndex < 0)
        {
            _focusedMatchIndex = 0;
            return;
        }

        if (_focusedMatchIndex >= _matchIds.Count)
            _focusedMatchIndex = _matchIds.Count - 1;
    }

    private string? GetFocusedResultId()
        => _focusedMatchIndex >= 0 && _focusedMatchIndex < _matchIds.Count
            ? _matchIds[_focusedMatchIndex]
            : null;

    private static string Normalize(string query)
        => string.IsNullOrWhiteSpace(query)
            ? string.Empty
            : query.Trim().ToUpperInvariant();
}
