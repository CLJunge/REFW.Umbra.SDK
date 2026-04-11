using Umbra.UI.Config.UnitTests;

namespace Umbra.UI.Config.Search.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigDrawerSearchController"/>.
/// </summary>
[TestClass]
public sealed class ConfigDrawerSearchControllerTests
{
    /// <summary>
    /// Verifies that the built-in search controls are skipped when search is disabled.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenSearchBarDisabled_DoesNotRenderSearchControls()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions(),
            renderer,
            new ConfigSearchIndex());

        // Act
        controller.DrawControls();

        // Assert
        Assert.IsNull(controller.CurrentState);
        Assert.IsEmpty(renderer.InputTextLabels);
        Assert.IsEmpty(renderer.ButtonLabels);
        Assert.IsEmpty(renderer.RenderedTexts);
    }

    /// <summary>
    /// Verifies that cached search-row measurements are reused while the available width is unchanged.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenAvailableWidthIsUnchanged_ReusesCachedSearchLayout()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        renderer.TextWidths["Search"] = 36f;
        renderer.ButtonWidths["<##ConfigDrawerSearchPrevious"] = 40f;
        renderer.ButtonWidths[">##ConfigDrawerSearchNext"] = 44f;
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            new ConfigSearchIndex());

        // Act
        controller.DrawControls();
        controller.DrawControls();

        // Assert
        Assert.HasCount(1, renderer.TextWidthRequests);
        Assert.HasCount(3, renderer.ButtonWidthRequests);
        Assert.HasCount(2, renderer.NextItemWidths);
        Assert.AreEqual(renderer.NextItemWidths[0], renderer.NextItemWidths[1]);
    }

    /// <summary>
    /// Verifies that cached search-row measurements are recomputed when the available width changes.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenAvailableWidthChanges_RecomputesSearchLayout()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        renderer.TextWidths["Search"] = 36f;
        renderer.ButtonWidths["<##ConfigDrawerSearchPrevious"] = 40f;
        renderer.ButtonWidths[">##ConfigDrawerSearchNext"] = 44f;
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            new ConfigSearchIndex());

        // Act
        controller.DrawControls();
        renderer.AvailableWidth = 360f;
        controller.DrawControls();

        // Assert
        Assert.HasCount(2, renderer.TextWidthRequests);
        Assert.HasCount(6, renderer.ButtonWidthRequests);
        Assert.HasCount(2, renderer.NextItemWidths);
        Assert.AreNotEqual(renderer.NextItemWidths[0], renderer.NextItemWidths[1]);
        Assert.AreEqual(360f - 36f - 40f - 40f - 44f - (8f * 4f), renderer.NextItemWidths[1]);
    }

    /// <summary>
    /// Verifies that entering a query refreshes the ordered match list from the search index.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenQueryChanges_RefreshesMatchesFromSearchIndex()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "audio"
        };
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Master Volume", "Adjusts output level.", "Audio", "config.audio");
        searchIndex.AddParameterResult("beta", "Gamma", "Adjusts display brightness.", "Graphics", "config.graphics");
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            searchIndex);

        // Act
        controller.DrawControls();

        // Assert
        Assert.IsNotNull(controller.CurrentState);
        Assert.AreEqual("audio", controller.CurrentState.Query);
        var matchIds = new List<string>();
        foreach (var matchId in controller.CurrentState.MatchIds)
            matchIds.Add(matchId);
        CollectionAssert.AreEqual(new List<string> { "alpha" }, matchIds);
        Assert.IsNull(controller.CurrentState.FocusedResultId);
    }

    /// <summary>
    /// Verifies that previous and next navigation update the focused result through the controller.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenNavigationButtonsAreClicked_UpdatesFocusedResult()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "ga"
        };
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Gamma", null, "Graphics", "config.graphics");
        searchIndex.AddParameterResult("beta", "Game Speed", null, "Gameplay", "config.gameplay");
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            searchIndex);

        // Act
        controller.DrawControls();
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(true);
        controller.DrawControls();
        var focusedAfterFirstNext = controller.CurrentState!.FocusedResultId;
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(true);
        controller.DrawControls();
        var focusedAfterSecondNext = controller.CurrentState!.FocusedResultId;
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(true);
        renderer.ButtonResults.Enqueue(false);
        controller.DrawControls();

        // Assert
        Assert.AreEqual("alpha", focusedAfterFirstNext);
        Assert.AreEqual("beta", focusedAfterSecondNext);
        Assert.AreEqual("alpha", controller.CurrentState!.FocusedResultId);
    }

    /// <summary>
    /// Verifies that the clear button resets the query and refreshes matches.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenClearButtonIsClicked_ResetsQueryAndClearsMatches()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "audio"
        };
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Master Volume", "Adjusts output level.", "Audio", "config.audio");
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            searchIndex);

        // Act
        controller.DrawControls();
        renderer.ButtonResults.Enqueue(true);
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(false);
        controller.DrawControls();

        // Assert
        Assert.IsNotNull(controller.CurrentState);
        Assert.AreEqual(string.Empty, controller.CurrentState.Query);
        Assert.AreEqual(0, controller.CurrentState.MatchCount);
    }

    /// <summary>
    /// Verifies that the clear button is disabled when the query is empty.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenQueryIsEmpty_ClearButtonIsDisabled()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope();
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            new ConfigSearchIndex());

        // Act
        controller.DrawControls();

        // Assert
        Assert.IsGreaterThanOrEqualTo(2, renderer.DisabledStack.Count);
        Assert.IsTrue(renderer.DisabledStack[0]);
    }

    /// <summary>
    /// Verifies that the clear button is enabled when the query contains only whitespace characters.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenQueryIsWhitespaceOnly_ClearButtonIsEnabled()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "   "
        };
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            new ConfigSearchIndex());

        // Act
        controller.DrawControls();

        // Assert
        Assert.IsGreaterThanOrEqualTo(2, renderer.DisabledStack.Count);
        Assert.IsFalse(renderer.DisabledStack[0]);
    }

    /// <summary>
    /// Verifies that prev/next buttons are disabled when no matches exist.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenNoMatchesExist_NavigationButtonsAreDisabled()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "zzz"
        };
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Volume", null, "Audio", "config.audio");
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            searchIndex);

        // Act
        controller.DrawControls();

        // Assert
        Assert.AreEqual(0, controller.CurrentState!.MatchCount);
        Assert.IsGreaterThanOrEqualTo(4, renderer.DisabledStack.Count);
        Assert.IsTrue(renderer.DisabledStack[2]);
    }

    /// <summary>
    /// Verifies that navigation buttons become disabled once the only match is focused.
    /// </summary>
    [TestMethod]
    public void DrawControls_WhenSingleMatchIsFocused_NavigationButtonsAreDisabled()
    {
        // Arrange
        var renderer = new TestConfigDrawerScope
        {
            NextInputTextResult = true,
            NextInputTextValue = "volume"
        };
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Master Volume", null, "Audio", "config.audio");
        var controller = new ConfigDrawerSearchController(
            new ConfigDrawerOptions { Search = new ConfigSearchOptions() },
            renderer,
            searchIndex);

        // Act
        controller.DrawControls();
        renderer.DisabledStack.Clear();
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(true);
        controller.DrawControls();
        renderer.DisabledStack.Clear();
        controller.DrawControls();

        // Assert
        Assert.AreEqual("alpha", controller.CurrentState!.FocusedResultId);
        Assert.IsGreaterThanOrEqualTo(4, renderer.DisabledStack.Count);
        Assert.IsTrue(renderer.DisabledStack[2]);
    }
}
