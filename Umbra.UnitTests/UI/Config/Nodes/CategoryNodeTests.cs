using Umbra.Config.Attributes;

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
        var node = new CategoryNode("Test Category", collapseAttr: null, indentAttr: null, renderer);
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
        var node = new CategoryNode("Test Category", collapseAttr, indentAttr: null, renderer);
        node.Children.Add(new CallbackNode(() => childDrawCount++));

        // Act
        node.Draw();

        // Assert
        Assert.IsEmpty(renderer.SeparatorLabels);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Test Category", false), renderer.TreeNodes[0]);
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
        var node = new CategoryNode("Test Category", collapseAttr: null, indentAttr, renderer);
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
        var node = new CategoryNode("Test Category", collapseAttr, indentAttr, renderer);
        node.Children.Add(new CallbackNode(() => childDrawCount++));

        // Act
        node.Draw();

        // Assert
        Assert.HasCount(1, renderer.Indents);
        Assert.HasCount(1, renderer.Unindents);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Test Category", true), renderer.TreeNodes[0]);
        Assert.AreEqual(1, renderer.TreePopCount);
        Assert.AreEqual(1, childDrawCount);
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
        var node = new CategoryNode("Test Category", collapseAttr, indentAttr: null, renderer);
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
        var node = new CategoryNode("Test Category", collapseAttr, indentAttr: null, renderer);

        // Act
        node.Draw();
        node.Draw();
        node.Draw();

        // Assert
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
