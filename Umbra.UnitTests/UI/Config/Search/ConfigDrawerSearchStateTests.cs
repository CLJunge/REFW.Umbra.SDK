namespace Umbra.UI.Config.Search.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigDrawerSearchState"/>.
/// </summary>
[TestClass]
public sealed class ConfigDrawerSearchStateTests
{
    /// <summary>
    /// Tests that setting a query normalizes non-empty text and marks the query as active.
    /// </summary>
    [TestMethod]
    public void SetQuery_WithNonEmptyText_NormalizesAndActivatesQuery()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();

        // Act
        state.SetQuery("  GaMmA  ");

        // Assert
        Assert.AreEqual("  GaMmA  ", state.Query);
        Assert.AreEqual("GAMMA", state.NormalizedQuery);
        Assert.IsTrue(state.HasActiveQuery);
        Assert.IsNull(state.PendingScrollResultId);
        Assert.IsNull(state.PendingFocusResultId);
    }

    /// <summary>
    /// Tests that setting a whitespace-only query clears the normalized query.
    /// </summary>
    [TestMethod]
    public void SetQuery_WithWhitespaceOnlyText_ClearsNormalizedQuery()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();

        // Act
        state.SetQuery("   ");

        // Assert
        Assert.AreEqual("   ", state.Query);
        Assert.AreEqual(string.Empty, state.NormalizedQuery);
        Assert.IsFalse(state.HasActiveQuery);
        Assert.IsNull(state.PendingScrollResultId);
        Assert.IsNull(state.PendingFocusResultId);
    }

    /// <summary>
    /// Tests that setting matches without explicit navigation does not auto-focus the first result.
    /// </summary>
    [TestMethod]
    public void SetMatches_WithResults_DoesNotAutoFocusFirstResult()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();

        // Act
        state.SetMatches(["alpha", "beta", "gamma"]);

        // Assert
        Assert.IsNull(state.PendingScrollResultId);
        Assert.IsNull(state.PendingFocusResultId);
        Assert.IsNull(state.FocusedResultId);
    }

    /// <summary>
    /// Tests that moving to the next result from an unfocused state selects the first result.
    /// </summary>
    [TestMethod]
    public void MoveNext_WithoutExistingFocus_SelectsFirstResult()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetMatches(["alpha", "beta", "gamma"]);

        // Act
        state.MoveNext();

        // Assert
        Assert.AreEqual("alpha", state.PendingScrollResultId);
        Assert.AreEqual("alpha", state.PendingFocusResultId);
    }

    /// <summary>
    /// Tests that moving to the next result wraps back to the start.
    /// </summary>
    [TestMethod]
    public void MoveNext_AtLastResult_WrapsToFirstResult()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetMatches(["alpha", "beta", "gamma"]);
        state.MoveNext();
        state.MoveNext();
        state.MoveNext();

        // Act
        state.MoveNext();

        // Assert
        Assert.AreEqual("alpha", state.PendingScrollResultId);
        Assert.AreEqual("alpha", state.PendingFocusResultId);
    }

    /// <summary>
    /// Tests that moving to the previous result without an existing focus selects the last result.
    /// </summary>
    [TestMethod]
    public void MovePrevious_WithoutExistingFocus_SelectsLastResult()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetMatches(["alpha", "beta", "gamma"]);
        state.SetMatches([]);
        state.SetMatches(["alpha", "beta", "gamma"]);

        // Act
        state.MovePrevious();

        // Assert
        Assert.AreEqual("gamma", state.PendingScrollResultId);
        Assert.AreEqual("gamma", state.PendingFocusResultId);
    }

    /// <summary>
    /// Tests that replacing matches clamps focus to the available results when a result was already selected explicitly.
    /// </summary>
    [TestMethod]
    public void SetMatches_WhenFocusedIndexExceedsNewResultCount_ClampsFocus()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetMatches(["alpha", "beta", "gamma"]);
        state.MoveNext();
        state.MoveNext();
        state.MoveNext();

        // Act
        state.SetMatches(["alpha"]);

        // Assert
        Assert.AreEqual("alpha", state.PendingScrollResultId);
        Assert.AreEqual("alpha", state.PendingFocusResultId);
    }

    /// <summary>
    /// Tests that changing the query clears an existing focused result until the user navigates again.
    /// </summary>
    [TestMethod]
    public void SetQuery_WhenResultWasFocused_ClearsFocusedResultAndPendingTargets()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetMatches(["alpha", "beta"]);
        state.MoveNext();

        // Act
        state.SetQuery("alpha");

        // Assert
        Assert.IsNull(state.FocusedResultId);
        Assert.IsNull(state.PendingScrollResultId);
        Assert.IsNull(state.PendingFocusResultId);
    }

    /// <summary>
    /// Tests that clearing a consumed focus target leaves unrelated pending focus requests intact.
    /// </summary>
    [TestMethod]
    public void ClearPendingFocusTarget_WhenResultMatches_ClearsPendingFocusOnlyForThatResult()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetMatches(["alpha", "beta"]);
        state.MoveNext();

        // Act
        state.ClearPendingFocusTarget("alpha");

        // Assert
        Assert.IsNull(state.PendingFocusResultId);
        Assert.AreEqual("alpha", state.PendingScrollResultId);
    }

    /// <summary>
    /// Tests that navigation is disabled when no query is active.
    /// </summary>
    [TestMethod]
    public void CanNavigate_WithoutActiveQuery_ReturnsFalse()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetMatches(["alpha", "beta"]);

        // Act & Assert
        Assert.IsFalse(state.CanNavigate);
    }

    /// <summary>
    /// Tests that navigation is disabled when a query is active but produces no matches.
    /// </summary>
    [TestMethod]
    public void CanNavigate_WithActiveQueryAndNoMatches_ReturnsFalse()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetQuery("test");

        // Act & Assert
        Assert.IsFalse(state.CanNavigate);
    }

    /// <summary>
    /// Tests that navigation is enabled when a single match exists but has not been focused.
    /// </summary>
    [TestMethod]
    public void CanNavigate_WithSingleUnfocusedMatch_ReturnsTrue()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetQuery("test");
        state.SetMatches(["alpha"]);

        // Act & Assert
        Assert.IsTrue(state.CanNavigate);
    }

    /// <summary>
    /// Tests that navigation is disabled once the sole match is already focused.
    /// </summary>
    [TestMethod]
    public void CanNavigate_WithSingleFocusedMatch_ReturnsFalse()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetQuery("test");
        state.SetMatches(["alpha"]);
        state.MoveNext();

        // Act & Assert
        Assert.IsFalse(state.CanNavigate);
    }

    /// <summary>
    /// Tests that navigation stays enabled when multiple matches exist regardless of focus.
    /// </summary>
    [TestMethod]
    public void CanNavigate_WithMultipleMatches_ReturnsTrue()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetQuery("test");
        state.SetMatches(["alpha", "beta"]);
        state.MoveNext();

        // Act & Assert
        Assert.IsTrue(state.CanNavigate);
    }

    /// <summary>
    /// Tests that <see cref="ConfigDrawerSearchState.MatchCount"/> reflects the current match list size.
    /// </summary>
    [TestMethod]
    public void MatchCount_AfterSetMatches_ReflectsCurrentCount()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();

        // Act
        state.SetMatches(["alpha", "beta", "gamma"]);

        // Assert
        Assert.AreEqual(3, state.MatchCount);
    }
}
