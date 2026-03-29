namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Contains focused unit tests for <see cref="IdScopeNode"/>.
/// </summary>
[TestClass]
public sealed class IdScopeNodeTests
{
    private static readonly int[] expectedSingleElement = new[] { 1 };
    private static readonly int[] expectedThreeElements = new[] { 1, 2, 3 };

    /// <summary>
    /// Verifies that drawing a node with multiple children draws each child once in declaration order.
    /// </summary>
    [TestMethod]
    public void Draw_WithMultipleChildren_DrawsEachChildInOrder()
    {
        var calls = new List<int>();
        var node = new IdScopeNode(
            "scope",
            [
                new CallbackNode(() => calls.Add(1)),
                new CallbackNode(() => calls.Add(2)),
                new CallbackNode(() => calls.Add(3)),
            ]);

        node.Draw();

        CollectionAssert.AreEqual(expectedThreeElements, calls);
    }

    /// <summary>
    /// Verifies that drawing a node with no children completes successfully.
    /// </summary>
    [TestMethod]
    public void Draw_WithNoChildren_DoesNotThrow()
    {
        var node = new IdScopeNode("scope", []);

        node.Draw();
    }

    /// <summary>
    /// Verifies that a child exception is propagated and later children are not drawn.
    /// </summary>
    [TestMethod]
    public void Draw_WhenChildThrows_StopsAtThrowingChild()
    {
        var calls = new List<int>();
        var node = new IdScopeNode(
            "scope",
            [
                new CallbackNode(() => calls.Add(1)),
                new CallbackNode(() => throw new InvalidOperationException("boom")),
                new CallbackNode(() => calls.Add(3)),
            ]);

        try
        {
            node.Draw();
            Assert.Fail("Expected InvalidOperationException.");
        }
        catch (InvalidOperationException)
        {
        }

        CollectionAssert.AreEqual(expectedSingleElement, calls);
    }

    /// <summary>
    /// Verifies that repeated draw calls redraw the same children each time.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_RedrawsChildrenEachTime()
    {
        var drawCount = 0;
        var node = new IdScopeNode(
            "scope",
            [
                new CallbackNode(() => drawCount++),
            ]);

        node.Draw();
        node.Draw();
        node.Draw();

        Assert.AreEqual(3, drawCount);
    }

    /// <summary>
    /// Simple draw node used to observe draw behavior.
    /// </summary>
    private sealed class CallbackNode(Action callback) : IDrawNode
    {
        public void Draw() => callback();
    }
}
