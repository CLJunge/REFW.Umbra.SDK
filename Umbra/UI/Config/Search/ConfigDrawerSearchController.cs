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
    private const string _searchLabel = "Search";
    private const string _searchInputLabel = "##ConfigDrawerSearch";
    private const string _clearButtonLabel = "\u00d7##ConfigDrawerSearchClear";
    private const string _previousButtonLabel = "<##ConfigDrawerSearchPrevious";
    private const string _nextButtonLabel = ">##ConfigDrawerSearchNext";

    private readonly IConfigDrawerRenderer _renderer;
    private readonly ConfigSearchIndex _searchIndex;
    private readonly ConfigDrawerSearchState? _searchState;
    private readonly ConfigDrawerSearchLayoutState? _searchLayoutState;
    private readonly uint _maxInputLength;
    private readonly float _minimumSearchInputWidth;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigDrawerSearchController"/> class.
    /// </summary>
    /// <param name="options">The drawer options that contain the optional search settings.</param>
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
        var searchOptions = options.Search;
        _searchState = searchOptions is not null ? new ConfigDrawerSearchState() : null;
        _searchLayoutState = searchOptions is not null ? new ConfigDrawerSearchLayoutState() : null;
        _maxInputLength = searchOptions?.MaxInputLength ?? ConfigSearchOptions.DefaultMaxInputLength;
        _minimumSearchInputWidth = searchOptions?.MinimumSearchInputWidth ?? ConfigSearchOptions.DefaultMinimumSearchInputWidth;
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

        _renderer.Text(_searchLabel);
        _renderer.SameLine();

        var query = searchState.Query;
        _renderer.SetNextItemWidth(layoutState.SearchInputWidth);
        if (_renderer.InputText(_searchInputLabel, ref query, _maxInputLength))
        {
            searchState.SetQuery(query);
            RefreshSearchMatches(searchState);
        }

        _renderer.SameLine();
        _renderer.BeginDisabled(!searchState.HasActiveQuery);
        if (_renderer.Button(_clearButtonLabel))
        {
            searchState.SetQuery(string.Empty);
            RefreshSearchMatches(searchState);
        }
        _renderer.EndDisabled();

        _renderer.SameLine();
        _renderer.BeginDisabled(!searchState.CanNavigate);
        if (_renderer.Button(_previousButtonLabel))
            searchState.MovePrevious();

        _renderer.SameLine();
        if (_renderer.Button(_nextButtonLabel))
            searchState.MoveNext();
        _renderer.EndDisabled();
    }

    private void EnsureSearchLayout(ConfigDrawerSearchLayoutState layoutState)
    {
        var availableWidth = _renderer.GetAvailableWidth();
        if (layoutState.IsInitialized && layoutState.LastAvailableWidth == availableWidth)
            return;

        layoutState.LastAvailableWidth = availableWidth;
        layoutState.ClearButtonWidth = _renderer.GetButtonWidth(_clearButtonLabel);
        layoutState.PreviousButtonWidth = _renderer.GetButtonWidth(_previousButtonLabel);
        layoutState.NextButtonWidth = _renderer.GetButtonWidth(_nextButtonLabel);

        var labelWidth = _renderer.GetTextWidth(_searchLabel);
        var spacingX = _renderer.GetItemSpacingX();
        var searchInputWidth = availableWidth
            - labelWidth
            - layoutState.ClearButtonWidth
            - layoutState.PreviousButtonWidth
            - layoutState.NextButtonWidth
            - (spacingX * 4f);
        layoutState.SearchInputWidth = Math.Max(_minimumSearchInputWidth, searchInputWidth);
        layoutState.IsInitialized = true;
    }

    private void RefreshSearchMatches(ConfigDrawerSearchState searchState)
        => searchState.SetMatches(_searchIndex.FindMatches(searchState.NormalizedQuery));
}
