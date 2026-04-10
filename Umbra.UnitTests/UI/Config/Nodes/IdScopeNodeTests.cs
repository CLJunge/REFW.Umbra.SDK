namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Contains focused unit tests for <see cref="IdScopeNode"/>.
/// </summary>
[TestClass]
public sealed class IdScopeNodeTests
{
    private static readonly int[] _expectedSingleElement = [1];
    private static readonly int[] _expectedThreeElements = [1, 2, 3];

    /// <summary>
    /// Verifies that drawing a node with multiple children draws each child once in declaration
    /// order inside one pushed ID scope.
    /// </summary>
    [TestMethod]
    public void Draw_WithMultipleChildren_DrawsEachChildInOrder()
    {
        var calls = new List<int>();
        var renderer = new TestIdScopeNodeRenderer();
        var node = new IdScopeNode(
            "scope",
            [
                new CallbackNode(() => calls.Add(1)),
                new CallbackNode(() => calls.Add(2)),
                new CallbackNode(() => calls.Add(3)),
            ],
            renderer);

        node.Draw();

        CollectionAssert.AreEqual(_expectedThreeElements, calls);
        Assert.HasCount(1, renderer.PushedIds);
        Assert.AreEqual("scope", renderer.PushedIds[0]);
        Assert.AreEqual(1, renderer.PopCount);
    }

    /// <summary>
    /// Verifies that drawing a node with no children still balances the pushed ID scope.
    /// </summary>
    [TestMethod]
    public void Draw_WithNoChildren_PushesAndPopsScope()
    {
        var renderer = new TestIdScopeNodeRenderer();
        var node = new IdScopeNode("scope", [], renderer);

        node.Draw();

        Assert.HasCount(1, renderer.PushedIds);
        Assert.AreEqual("scope", renderer.PushedIds[0]);
        Assert.AreEqual(1, renderer.PopCount);
    }

    /// <summary>
    /// Verifies that a child exception is propagated, later children are not drawn, and the scope
    /// is still popped.
    /// </summary>
    [TestMethod]
    public void Draw_WhenChildThrows_StopsAtThrowingChildAndPopsScope()
    {
        var calls = new List<int>();
        var renderer = new TestIdScopeNodeRenderer();
        var node = new IdScopeNode(
            "scope",
            [
                new CallbackNode(() => calls.Add(1)),
                new CallbackNode(() => throw new InvalidOperationException("boom")),
                new CallbackNode(() => calls.Add(3)),
            ],
            renderer);

        InvalidOperationException? exception = null;
        try
        {
            node.Draw();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        Assert.IsNotNull(exception);
        Assert.AreEqual("boom", exception.Message);
        CollectionAssert.AreEqual(_expectedSingleElement, calls);
        Assert.HasCount(1, renderer.PushedIds);
        Assert.AreEqual(1, renderer.PopCount);
    }

    /// <summary>
    /// Verifies that repeated draw calls redraw the same children and rebalance the scope each time.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_RedrawsChildrenEachTime()
    {
        var drawCount = 0;
        var renderer = new TestIdScopeNodeRenderer();
        var node = new IdScopeNode(
            "scope",
            [
                new CallbackNode(() => drawCount++),
            ],
            renderer);

        node.Draw();
        node.Draw();
        node.Draw();

        Assert.AreEqual(3, drawCount);
        Assert.HasCount(3, renderer.PushedIds);
        Assert.AreEqual(3, renderer.PopCount);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null child list.
    /// </summary>
    [TestMethod]
    public void Constructor_NullChildren_ThrowsArgumentNullException()
    {
        var renderer = new TestIdScopeNodeRenderer();

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new IdScopeNode("scope", null!, renderer));

        Assert.AreEqual("children", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new IdScopeNode("scope", [], null!));

        Assert.AreEqual("renderer", exception.ParamName);
    }

    /// <summary>
    /// Simple draw node used to observe draw behavior.
    /// </summary>
    private sealed class CallbackNode(Action callback) : IDrawNode
    {
        public void Draw() => callback();
    }
}
