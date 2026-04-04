namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for <see cref="RootTreeNode"/>.
/// </summary>
[TestClass]
public sealed class RootTreeNodeTests
{
    private static readonly int[] _expectedOneElement = [1];
    private static readonly int[] _expectedThreeElements = [1, 2, 3];

    /// <summary>
    /// Tests that a closed root tree node does not draw children or pop the tree node.
    /// </summary>
    [TestMethod]
    public void Draw_ClosedTree_DoesNotDrawChildren()
    {
        // Arrange
        var renderer = new TestRootTreeNodeRenderer();
        renderer.TreeNodeResults.Enqueue(false);
        var child = new CallbackNode(() => Assert.Fail("Child should not be drawn when tree is closed."));
        var node = new RootTreeNode("Test Node", defaultOpen: true, [child], branchId: null, renderer);

        // Act
        node.Draw();

        // Assert
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Test Node", true, false), renderer.TreeNodes[0]);
        Assert.AreEqual(0, renderer.TreePopCount);
    }

    /// <summary>
    /// Tests that an open root tree node invokes Draw on all child nodes in declaration order and
    /// pops the tree node afterward.
    /// </summary>
    [TestMethod]
    public void Draw_WithMultipleChildren_InvokesDrawOnAllChildrenInOrder()
    {
        // Arrange
        var renderer = new TestRootTreeNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var calls = new List<int>();
        var children = new List<IDrawNode>
        {
            new CallbackNode(() => calls.Add(1)),
            new CallbackNode(() => calls.Add(2)),
            new CallbackNode(() => calls.Add(3)),
        };
        var node = new RootTreeNode("Parent Node", defaultOpen: false, children, branchId: null, renderer);

        // Act
        node.Draw();

        // Assert
        CollectionAssert.AreEqual(_expectedThreeElements, calls);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Parent Node", false, false), renderer.TreeNodes[0]);
        Assert.AreEqual(1, renderer.TreePopCount);
    }

    /// <summary>
    /// Tests that an open root tree node with no children still pops the tree node exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_OpenTreeWithNoChildren_PopsTreeNode()
    {
        // Arrange
        var renderer = new TestRootTreeNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var node = new RootTreeNode("Parent Node", defaultOpen: false, [], branchId: null, renderer);

        // Act
        node.Draw();

        // Assert
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(1, renderer.TreePopCount);
    }

    /// <summary>
    /// Tests that the tree pop occurs even when a child throws an exception.
    /// </summary>
    [TestMethod]
    public void Draw_ChildThrowsException_PopsTreeAndRethrows()
    {
        // Arrange
        var renderer = new TestRootTreeNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var calls = new List<int>();
        var node = new RootTreeNode(
            "Parent Node",
            defaultOpen: true,
            [
                new CallbackNode(() => calls.Add(1)),
                new CallbackNode(() => throw new InvalidOperationException("boom")),
            ],
            branchId: null,
            renderer);

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
        CollectionAssert.AreEqual(_expectedOneElement, calls);
        Assert.AreEqual(1, renderer.TreePopCount);
    }

    /// <summary>
    /// Tests that repeated draw calls repeat the same rendering behavior each time.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_RedrawsChildrenEachTime()
    {
        // Arrange
        var renderer = new TestRootTreeNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        renderer.TreeNodeResults.Enqueue(true);
        renderer.TreeNodeResults.Enqueue(true);
        var drawCount = 0;
        var node = new RootTreeNode(
            "scope",
            defaultOpen: true,
            [new CallbackNode(() => drawCount++)],
            branchId: null,
            renderer);

        // Act
        node.Draw();
        node.Draw();
        node.Draw();

        // Assert
        Assert.AreEqual(3, drawCount);
        Assert.HasCount(3, renderer.TreeNodes);
        Assert.AreEqual(3, renderer.TreePopCount);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null child list.
    /// </summary>
    [TestMethod]
    public void Constructor_NullChildren_ThrowsArgumentNullException()
    {
        var renderer = new TestRootTreeNodeRenderer();

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new RootTreeNode("Label", true, null!, branchId: null, renderer));

        Assert.AreEqual("children", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new RootTreeNode("Label", true, [], branchId: null, null!));

        Assert.AreEqual("renderer", exception.ParamName);
    }

    /// <summary>
    /// Tests that a searchable root tree node force-opens while active search has visible descendants.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenDescendantMatches_ForceOpensRootTree()
    {
        // Arrange
        var renderer = new TestRootTreeNodeRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var node = new RootTreeNode("Root", defaultOpen: false, [new SearchableCallbackNode(visible: true)], branchId: "root:test", renderer);
        var renderState = CreateRenderState("root:test");

        // Act
        var visible = ((IConfigSearchNode)node).ApplySearch(renderState);
        node.Draw();

        // Assert
        Assert.IsTrue(visible);
        Assert.AreEqual(("Root", false, true), renderer.TreeNodes[0]);
    }

    /// <summary>
    /// Tests that a searchable root tree node hides itself when no searchable descendants remain visible.
    /// </summary>
    [TestMethod]
    public void ApplySearch_WhenNoDescendantsMatch_HidesRootTree()
    {
        // Arrange
        var renderer = new TestRootTreeNodeRenderer();
        var node = new RootTreeNode("Root", defaultOpen: false, [new SearchableCallbackNode(visible: false)], branchId: "root:test", renderer);
        var renderState = CreateRenderState("root:test");

        // Act
        var visible = ((IConfigSearchNode)node).ApplySearch(renderState);
        node.Draw();

        // Assert
        Assert.IsFalse(visible);
        Assert.AreEqual(0, renderer.TreeNodes.Count);
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
