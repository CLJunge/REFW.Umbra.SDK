using Umbra.Config.Attributes;
using Umbra.UI.Config.Search;

namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for <see cref="CategoryNode"/>.
/// </summary>
[TestClass]
public sealed class CategoryNodeTests
{
    /// <summary>
    /// Tests that drawing a flat category without indentation renders a separator and draws all
    /// children once.
    /// </summary>
    [TestMethod]
    public void Draw_WithNullIndentAndNullCollapse_RendersHeaderAndChildren()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        var childDrawCount = 0;
        var node = new CategoryNode("Test Category", branchId: null, collapseAttr: null, indentAttr: null, renderer);
        node.Children.Add(new CallbackNode(() => childDrawCount++));

        // Act
        node.Draw();

        // Assert
        Assert.IsEmpty(renderer.Indents);
        Assert.IsEmpty(renderer.Unindents);
        Assert.HasCount(1, renderer.SeparatorLabels);
        Assert.AreEqual("Test Category", renderer.SeparatorLabels[0]);
        Assert.IsEmpty(renderer.TreeNodes);
        Assert.AreEqual(0, renderer.TreePopCount);
        Assert.AreEqual(1, childDrawCount);
    }

    /// <summary>
    /// Tests that drawing a collapsed tree category skips child rendering and does not pop a tree
    /// node when the tree is closed.
    /// </summary>
    [TestMethod]
    public void Draw_WithNullIndentAndNonNullCollapse_ClosedTreeSkipsChildren()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        renderer.TreeNodeResults.Enqueue(false);
        var childDrawCount = 0;
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: false);
        var node = new CategoryNode("Test Category", branchId: null, collapseAttr, indentAttr: null, renderer);
        node.Children.Add(new CallbackNode(() => childDrawCount++));

        // Act
        node.Draw();

        // Assert
        Assert.IsEmpty(renderer.SeparatorLabels);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Test Category", false, false), renderer.TreeNodes[0]);
        Assert.AreEqual(0, renderer.TreePopCount);
        Assert.AreEqual(0, childDrawCount);
    }

    /// <summary>
    /// Tests that drawing a flat category with indentation balances the indent scope around the
    /// header and child nodes.
    /// </summary>
    [TestMethod]
    public void Draw_WithNonNullIndentAndNullCollapse_IndentsAndUnindents()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        var indentAttr = new UmbraIndentAttribute(20f);
        var childDrawCount = 0;
        var node = new CategoryNode("Test Category", branchId: null, collapseAttr: null, indentAttr, renderer);
        node.Children.Add(new CallbackNode(() => childDrawCount++));

        // Act
        node.Draw();

        // Assert
        Assert.HasCount(1, renderer.Indents);
        Assert.AreEqual(20f, renderer.Indents[0]);
        Assert.HasCount(1, renderer.Unindents);
        Assert.AreEqual(20f, renderer.Unindents[0]);
        Assert.HasCount(1, renderer.SeparatorLabels);
        Assert.AreEqual(1, childDrawCount);
    }

    /// <summary>
    /// Tests that drawing an open tree category with indentation draws children and pops the tree
    /// node before unindenting.
    /// </summary>
    [TestMethod]
    public void Draw_WithNonNullIndentAndNonNullCollapse_OpenTreeDrawsChildrenAndPops()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var indentAttr = new UmbraIndentAttribute(20f);
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: true);
        var childDrawCount = 0;
        var node = new CategoryNode("Test Category", branchId: null, collapseAttr, indentAttr, renderer);
        node.Children.Add(new CallbackNode(() => childDrawCount++));

        // Act
        node.Draw();

        // Assert
        Assert.HasCount(1, renderer.Indents);
        Assert.HasCount(1, renderer.Unindents);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Test Category", true, false), renderer.TreeNodes[0]);
        Assert.AreEqual(1, renderer.TreePopCount);
        Assert.AreEqual(1, childDrawCount);
    }

    /// <summary>
    /// Tests that a closed tree category still balances indentation when indentation metadata is present.
    /// </summary>
    [TestMethod]
    public void Draw_WithIndentAndClosedTree_IndentsAndUnindentsWithoutDrawingChildren()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        renderer.TreeNodeResults.Enqueue(false);
        var indentAttr = new UmbraIndentAttribute(10f);
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: false);
        var childDrawCount = 0;
        var node = new CategoryNode("Test Category", branchId: null, collapseAttr, indentAttr, renderer);
        node.Children.Add(new CallbackNode(() => childDrawCount++));

        // Act
        node.Draw();

        // Assert
        Assert.HasCount(1, renderer.Indents);
        Assert.HasCount(1, renderer.Unindents);
        Assert.AreEqual(0, childDrawCount);
        Assert.AreEqual(0, renderer.TreePopCount);
    }

    /// <summary>
    /// Tests that drawing an open tree category pops the tree node even when a child throws.
    /// </summary>
    [TestMethod]
    public void Draw_WhenOpenTreeChildThrows_PopsTreeAndRethrows()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: true);
        var node = new CategoryNode("Test Category", branchId: null, collapseAttr, indentAttr: null, renderer);
        node.Children.Add(new CallbackNode(() => throw new InvalidOperationException("boom")));

        // Act
        InvalidOperationException? exception = null;
        try
        {
            node.Draw();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("boom", exception.Message);
        Assert.AreEqual(1, renderer.TreePopCount);
    }

    /// <summary>
    /// Tests that repeated draw calls repeat the same rendering behavior each time.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_RepeatsRenderingBehavior()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        renderer.TreeNodeResults.Enqueue(true);
        renderer.TreeNodeResults.Enqueue(true);
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: true);
        var node = new CategoryNode("Test Category", branchId: null, collapseAttr, indentAttr: null, renderer);

        // Act
        node.Draw();
        node.Draw();
        node.Draw();

        // Assert
        Assert.HasCount(3, renderer.TreeNodes);
        Assert.AreEqual(3, renderer.TreePopCount);
    }

    /// <summary>
    /// Tests that the constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new CategoryNode("Label", branchId: null, null, null, null!));

        Assert.AreEqual("renderer", exception.ParamName);
    }

    /// <summary>
    /// Tests that a category hides itself when none of its searchable descendants remain visible.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenNoDescendantsMatch_HidesCategory()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        var node = new CategoryNode("Test Category", branchId: "category:test", collapseAttr: null, indentAttr: null, renderer);
        node.Children.Add(new SearchableCallbackNode(visible: false));
        var renderState = CreateRenderState();

        // Act
        var visible = ((IConfigSearchNode)node).ApplySearch(renderState);
        node.Draw();

        // Assert
        Assert.IsFalse(visible);
        Assert.AreEqual(0, renderer.SeparatorLabels.Count);
        Assert.AreEqual(0, renderer.TreeNodes.Count);
    }

    /// <summary>
    /// Tests that a category force-opens its tree node when a searchable descendant is visible during active search.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenDescendantMatches_ForceOpensTreeNode()
    {
        // Arrange
        var renderer = new TestCategoryNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: false);
        var node = new CategoryNode("Test Category", branchId: "category:test", collapseAttr, indentAttr: null, renderer);
        node.Children.Add(new SearchableCallbackNode(visible: true));
        var renderState = CreateRenderState("category:test");

        // Act
        var visible = ((IConfigSearchNode)node).ApplySearch(renderState);
        node.Draw();

        // Assert
        Assert.IsTrue(visible);
        Assert.AreEqual(("Test Category", false, true), renderer.TreeNodes[0]);
    }

    private static ConfigSearchRenderState CreateRenderState(params string[] forcedOpenBranchIds)
    {
        var searchState = new ConfigDrawerSearchState();
        searchState.SetQuery("match");
        searchState.SetMatches(["result"]);
        return new ConfigSearchRenderState(
            searchState,
            new HashSet<string>(["result"], StringComparer.Ordinal),
            new HashSet<string>(forcedOpenBranchIds, StringComparer.Ordinal));
    }

    /// <summary>
    /// Simple draw node used to observe draw behavior.
    /// </summary>
    private sealed class CallbackNode(Action callback) : IDrawNode
    {
        public void Draw() => callback();
    }

    private sealed class SearchableCallbackNode(bool visible) : IDrawNode, IConfigSearchNode
    {
        public void Draw()
        {
        }

        public bool ApplySearch(ConfigSearchRenderState? searchState) => visible;
    }
}
