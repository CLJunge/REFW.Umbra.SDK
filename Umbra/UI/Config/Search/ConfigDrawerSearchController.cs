namespace Umbra.UI.Config.Search;

/// <summary>
/// Owns the built-in config-drawer search controls and their local state.
/// </summary>
/// <remarks>
/// This type isolates search-row rendering, cached layout measurement, query mutation, and
/// match refresh behavior from <see cref="ConfigDrawer{TConfig}"/> so the drawer can remain
/// focused on high-level orchestration.
/// </remarks>
internal sealed class ConfigDrawerSearchController
{
    private const uint SearchBarMaxLength = 256;
    private const float MinimumSearchInputWidth = 64f;
    private const string SearchLabel = "Search";
    private const string SearchInputLabel = "##ConfigDrawerSearch";
    private const string PreviousButtonLabel = "<##ConfigDrawerSearchPrevious";
    private const string NextButtonLabel = ">##ConfigDrawerSearchNext";

    private readonly IConfigDrawerRenderer _renderer;
    private readonly ConfigSearchIndex _searchIndex;
    private readonly ConfigDrawerSearchState? _searchState;
    private readonly ConfigDrawerSearchLayoutState? _searchLayoutState;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigDrawerSearchController"/> class.
    /// </summary>
    /// <param name="options">The drawer options that enable or disable the built-in search row.</param>
    /// <param name="renderer">The renderer used for search-row UI and measurement operations.</param>
    /// <param name="searchIndex">The flat search index used to refresh matches from the current query.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/>, <paramref name="renderer"/>, or
    /// <paramref name="searchIndex"/> is <see langword="null"/>.
    /// </exception>
    internal ConfigDrawerSearchController(
        ConfigDrawerOptions options,
        IConfigDrawerRenderer renderer,
        ConfigSearchIndex searchIndex)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(searchIndex);

        _renderer = renderer;
        _searchIndex = searchIndex;
        _searchState = options.ShowSearchBar ? new ConfigDrawerSearchState() : null;
        _searchLayoutState = options.ShowSearchBar ? new ConfigDrawerSearchLayoutState() : null;
    }

    /// <summary>
    /// Gets the current search state used by the owning drawer.
    /// </summary>
    internal ConfigDrawerSearchState? CurrentState => _searchState;

    /// <summary>
    /// Draws the built-in search controls when search is enabled.
    /// </summary>
    internal void DrawControls()
    {
        var searchState = _searchState;
        var layoutState = _searchLayoutState;
        if (searchState is null || layoutState is null)
            return;

        EnsureSearchLayout(layoutState);

        _renderer.Text(SearchLabel);
        _renderer.SameLine();

        var query = searchState.Query;
        _renderer.SetNextItemWidth(layoutState.SearchInputWidth);
        if (_renderer.InputText(SearchInputLabel, ref query, SearchBarMaxLength))
        {
            searchState.SetQuery(query);
            RefreshSearchMatches(searchState);
        }

        _renderer.SameLine();
        if (_renderer.Button(PreviousButtonLabel))
            searchState.MovePrevious();

        _renderer.SameLine();
        if (_renderer.Button(NextButtonLabel))
            searchState.MoveNext();
    }

    private void EnsureSearchLayout(ConfigDrawerSearchLayoutState layoutState)
    {
        var availableWidth = _renderer.GetAvailableWidth();
        if (layoutState.IsInitialized && layoutState.LastAvailableWidth == availableWidth)
            return;

        layoutState.LastAvailableWidth = availableWidth;
        layoutState.PreviousButtonWidth = _renderer.GetButtonWidth(PreviousButtonLabel);
        layoutState.NextButtonWidth = _renderer.GetButtonWidth(NextButtonLabel);

        var labelWidth = _renderer.GetTextWidth(SearchLabel);
        var spacingX = _renderer.GetItemSpacingX();
        var searchInputWidth = availableWidth
            - labelWidth
            - layoutState.PreviousButtonWidth
            - layoutState.NextButtonWidth
            - (spacingX * 3f);
        layoutState.SearchInputWidth = Math.Max(MinimumSearchInputWidth, searchInputWidth);
        layoutState.IsInitialized = true;
    }

    private void RefreshSearchMatches(ConfigDrawerSearchState searchState)
        => searchState.SetMatches(_searchIndex.FindMatches(searchState.NormalizedQuery));
}
