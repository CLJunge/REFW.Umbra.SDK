using Moq;


namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for <see cref="RootTreeNode"/>.
/// </summary>
[TestClass]
public sealed class RootTreeNodeTests
{
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
        var node = new RootTreeNode("Test Node", defaultOpen: true, [child], renderer);

        // Act
        node.Draw();

        // Assert
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Test Node", true), renderer.TreeNodes[0]);
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
        var node = new RootTreeNode("Parent Node", defaultOpen: false, children, renderer);

        // Act
        node.Draw();

        // Assert
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, calls);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Parent Node", false), renderer.TreeNodes[0]);
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
        CollectionAssert.AreEqual(new[] { 1 }, calls);
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
    /// Simple draw node used to observe draw behavior.
    /// </summary>
    private sealed class CallbackNode(Action callback) : IDrawNode
    {
        public void Draw() => callback();
    }
}
