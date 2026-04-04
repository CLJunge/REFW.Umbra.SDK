using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Nodes.UnitTests;

namespace Umbra.UI.Config.Search.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigDrawerSearchApplicator"/>.
/// </summary>
[TestClass]
public sealed class ConfigDrawerSearchApplicatorTests
{
    /// <summary>
    /// Verifies that a null search state is forwarded to searchable nodes.
    /// </summary>
    [TestMethod]
    public void Apply_WhenSearchStateIsNull_ForwardsNullRenderState()
    {
        // Arrange
        var node = new SearchAwareTestNode("alpha");
        var nodes = new List<IDrawNode> { node };
        var searchIndex = new ConfigSearchIndex();

        // Act
        ConfigDrawerSearchApplicator.Apply(nodes, searchIndex, null);

        // Assert
        Assert.AreEqual(1, node.ApplyCount);
        Assert.IsTrue(node.LastRenderStateWasNull);
    }

    /// <summary>
    /// Verifies that an active query keeps only matching nodes visible.
    /// </summary>
    [TestMethod]
    public void Apply_WhenActiveQueryMatchesSingleResult_UpdatesVisibleAndHiddenNodes()
    {
        // Arrange
        var alphaNode = new SearchAwareTestNode("alpha");
        var betaNode = new SearchAwareTestNode("beta");
        var nodes = new List<IDrawNode> { alphaNode, betaNode };
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Master Volume", "Adjusts output level.", "Audio", "settings.audio");
        searchIndex.AddParameterResult("beta", "Gamma", "Adjusts display brightness.", "Graphics", "settings.graphics");
        var searchState = new ConfigDrawerSearchState();
        searchState.SetQuery("audio");
        searchState.SetMatches(searchIndex.FindMatches(searchState.NormalizedQuery));

        // Act
        ConfigDrawerSearchApplicator.Apply(nodes, searchIndex, searchState);

        // Assert
        Assert.IsTrue(alphaNode.LastWasVisible);
        Assert.IsTrue(alphaNode.LastIsMatch);
        Assert.IsFalse(betaNode.LastWasVisible);
        Assert.IsFalse(betaNode.LastIsMatch);
    }

    /// <summary>
    /// Verifies that ancestor branch identifiers force-open the root wrapper when a matching result is present.
    /// </summary>
    [TestMethod]
    public void Apply_WhenMatchingResultHasRootAncestor_ForceOpensRootTree()
    {
        // Arrange
        var renderer = new TestRootTreeNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var child = new SearchAwareTestNode("alpha");
        var root = new RootTreeNode("Root", defaultOpen: false, new List<IDrawNode> { child }, "root:test", renderer);
        var nodes = new List<IDrawNode> { root };
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Master Volume", null, "Audio", "settings.audio");
        searchIndex.PrependRootBranch("root:test");
        var searchState = new ConfigDrawerSearchState();
        searchState.SetQuery("audio");
        searchState.SetMatches(searchIndex.FindMatches(searchState.NormalizedQuery));

        // Act
        ConfigDrawerSearchApplicator.Apply(nodes, searchIndex, searchState);
        root.Draw();

        // Assert
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Root", false, true), renderer.TreeNodes[0]);
    }

    /// <summary>
    /// Verifies that repeated apply calls preserve the current focused match semantics.
    /// </summary>
    [TestMethod]
    public void Apply_WhenRepeatedWithFocusedResult_PreservesFocusedState()
    {
        // Arrange
        var alphaNode = new SearchAwareTestNode("alpha");
        var betaNode = new SearchAwareTestNode("beta");
        var nodes = new List<IDrawNode> { alphaNode, betaNode };
        var searchIndex = new ConfigSearchIndex();
        searchIndex.AddParameterResult("alpha", "Gamma", null, "Graphics", "settings.graphics");
        searchIndex.AddParameterResult("beta", "Game Speed", null, "Gameplay", "settings.gameplay");
        var searchState = new ConfigDrawerSearchState();
        searchState.SetQuery("ga");
        searchState.SetMatches(searchIndex.FindMatches(searchState.NormalizedQuery));
        searchState.MoveNext();

        // Act
        ConfigDrawerSearchApplicator.Apply(nodes, searchIndex, searchState);
        var firstFocusedResult = alphaNode.LastIsFocused;
        ConfigDrawerSearchApplicator.Apply(nodes, searchIndex, searchState);

        // Assert
        Assert.IsTrue(firstFocusedResult);
        Assert.IsTrue(alphaNode.LastIsFocused);
        Assert.IsTrue(alphaNode.LastWasVisible);
        Assert.IsTrue(betaNode.LastWasVisible);
        Assert.IsFalse(betaNode.LastIsFocused);
    }

    private sealed class SearchAwareTestNode(string resultId) : IDrawNode, IConfigSearchNode
    {
        public int ApplyCount { get; private set; }
        public bool LastRenderStateWasNull { get; private set; }
        public bool LastIsMatch { get; private set; }
        public bool LastIsFocused { get; private set; }
        public bool LastWasVisible { get; private set; }

        public void Draw()
        {
        }

        public bool ApplySearch(ConfigSearchRenderState? searchState)
        {
            ApplyCount++;
            LastRenderStateWasNull = searchState is null;
            if (searchState is null || !searchState.HasActiveQuery)
            {
                LastIsMatch = false;
                LastIsFocused = false;
                LastWasVisible = true;
                return true;
            }

            LastIsMatch = searchState.IsMatch(resultId);
            LastIsFocused = searchState.IsFocused(resultId);
            LastWasVisible = LastIsMatch;
            return LastWasVisible;
        }
    }
}
