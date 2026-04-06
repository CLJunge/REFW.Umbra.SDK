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
            new ConfigDrawerOptions { ShowSearchBar = false },
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
            new ConfigDrawerOptions { ShowSearchBar = true },
            renderer,
            new ConfigSearchIndex());

        // Act
        controller.DrawControls();
        controller.DrawControls();

        // Assert
        Assert.HasCount(1, renderer.TextWidthRequests);
        Assert.HasCount(2, renderer.ButtonWidthRequests);
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
            new ConfigDrawerOptions { ShowSearchBar = true },
            renderer,
            new ConfigSearchIndex());

        // Act
        controller.DrawControls();
        renderer.AvailableWidth = 360f;
        controller.DrawControls();

        // Assert
        Assert.HasCount(2, renderer.TextWidthRequests);
        Assert.HasCount(4, renderer.ButtonWidthRequests);
        Assert.HasCount(2, renderer.NextItemWidths);
        Assert.AreNotEqual(renderer.NextItemWidths[0], renderer.NextItemWidths[1]);
        Assert.AreEqual(360f - 36f - 40f - 44f - (8f * 3f), renderer.NextItemWidths[1]);
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
            new ConfigDrawerOptions { ShowSearchBar = true },
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
            new ConfigDrawerOptions { ShowSearchBar = true },
            renderer,
            searchIndex);

        // Act
        controller.DrawControls();
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(true);
        controller.DrawControls();
        var focusedAfterFirstNext = controller.CurrentState!.FocusedResultId;
        renderer.ButtonResults.Enqueue(false);
        renderer.ButtonResults.Enqueue(true);
        controller.DrawControls();
        var focusedAfterSecondNext = controller.CurrentState!.FocusedResultId;
        renderer.ButtonResults.Enqueue(true);
        renderer.ButtonResults.Enqueue(false);
        controller.DrawControls();

        // Assert
        Assert.AreEqual("alpha", focusedAfterFirstNext);
        Assert.AreEqual("beta", focusedAfterSecondNext);
        Assert.AreEqual("alpha", controller.CurrentState!.FocusedResultId);
    }
}
