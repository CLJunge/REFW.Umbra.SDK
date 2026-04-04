namespace Umbra.UI.Config.UnitTests;

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
    /// Tests that setting matches initializes focus to the first result.
    /// </summary>
    [TestMethod]
    public void SetMatches_WithResults_FocusesFirstResult()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();

        // Act
        state.SetMatches(["alpha", "beta", "gamma"]);

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
    /// Tests that replacing matches clamps focus to the available results.
    /// </summary>
    [TestMethod]
    public void SetMatches_WhenFocusedIndexExceedsNewResultCount_ClampsFocus()
    {
        // Arrange
        var state = new ConfigDrawerSearchState();
        state.SetMatches(["alpha", "beta", "gamma"]);
        state.MoveNext();
        state.MoveNext();

        // Act
        state.SetMatches(["alpha"]);

        // Assert
        Assert.AreEqual("alpha", state.PendingScrollResultId);
        Assert.AreEqual("alpha", state.PendingFocusResultId);
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

        // Act
        state.ClearPendingFocusTarget("alpha");

        // Assert
        Assert.IsNull(state.PendingFocusResultId);
        Assert.AreEqual("alpha", state.PendingScrollResultId);
    }
}
