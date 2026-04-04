namespace Umbra.UI.Config.Search.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigSearchIndex"/>.
/// </summary>
[TestClass]
public sealed class ConfigSearchIndexTests
{
    /// <summary>
    /// Tests that search matching includes labels, descriptions, and categories.
    /// </summary>
    [TestMethod]
    public void FindMatches_SearchesLabelDescriptionAndCategory()
    {
        // Arrange
        var index = new ConfigSearchIndex();
        index.AddParameterResult("alpha", "Master Volume", "Adjusts global audio level.", "Audio", "settings.audio");
        index.AddParameterResult("beta", "Gamma", "Adjusts display brightness.", "Graphics", "settings.graphics");

        // Act
        var labelMatches = index.FindMatches("MASTER");
        var descriptionMatches = index.FindMatches("BRIGHTNESS");
        var categoryMatches = index.FindMatches("AUDIO");

        // Assert
        CollectionAssert.AreEqual(new List<string> { "alpha" }, labelMatches);
        CollectionAssert.AreEqual(new List<string> { "beta" }, descriptionMatches);
        CollectionAssert.AreEqual(new List<string> { "alpha" }, categoryMatches);
    }

    /// <summary>
    /// Tests that empty queries return no matches.
    /// </summary>
    [TestMethod]
    public void FindMatches_EmptyQuery_ReturnsNoMatches()
    {
        // Arrange
        var index = new ConfigSearchIndex();
        index.AddParameterResult("alpha", "Master Volume", null, "Audio", "settings.audio");

        // Act
        var matches = index.FindMatches(string.Empty);

        // Assert
        Assert.IsEmpty(matches);
    }

    /// <summary>
    /// Tests that group and category branch identities are registered for parameter results.
    /// </summary>
    [TestMethod]
    public void AddParameterResult_RegistersAncestorBranches()
    {
        // Arrange
        var index = new ConfigSearchIndex();

        // Act
        index.AddParameterResult("alpha", "Master Volume", null, "Audio", "settings.audio");

        // Assert
        Assert.HasCount(3, index.Branches);
        Assert.AreEqual("group:settings", index.Branches[0].BranchId);
        Assert.AreEqual("group:settings.audio", index.Branches[1].BranchId);
        Assert.AreEqual("category:settings.audio|Audio", index.Branches[2].BranchId);
        Assert.AreEqual("group:settings.audio", index.Branches[2].ParentBranchId);
        Assert.HasCount(3, index.Entries[0].AncestorBranchIds);
    }

    /// <summary>
    /// Tests that prepending a root branch updates all existing search entries consistently.
    /// </summary>
    [TestMethod]
    public void PrependRootBranch_AddsRootBranchToExistingEntries()
    {
        // Arrange
        var index = new ConfigSearchIndex();
        index.AddParameterResult("alpha", "Master Volume", null, "Audio", "settings.audio");

        // Act
        index.PrependRootBranch("root:test-scope");

        // Assert
        Assert.AreEqual("root:test-scope", index.Branches[^1].BranchId);
        Assert.AreEqual("root:test-scope", index.Entries[0].AncestorBranchIds[0]);
        Assert.HasCount(4, index.Entries[0].AncestorBranchIds);
    }
}
