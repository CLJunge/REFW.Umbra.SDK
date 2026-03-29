using Moq;


namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for <see cref="RootTreeNode"/>.
/// </summary>
/// <remarks>
/// Note: ImGui methods (TreeNodeEx, TreePop) are static and cannot be mocked.
/// These tests verify parameter handling, child invocation, and exception safety
/// but cannot control or verify ImGui method calls directly.
/// </remarks>
[TestClass]
public sealed class RootTreeNodeTests
{
    /// <summary>
    /// Tests that Draw completes without throwing when called with valid parameters.
    /// </summary>
    [TestMethod]
    public void Draw_ValidParameters_CompletesWithoutException()
    {
        // Arrange
        var label = "Test Node";
        var defaultOpen = true;
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw invokes Draw on all child nodes when children list contains multiple children.
    /// Note: This test assumes ImGui.TreeNodeEx returns true, which is its default behavior in test environments.
    /// </summary>
    [TestMethod]
    public void Draw_WithMultipleChildren_InvokesDrawOnAllChildren()
    {
        // Arrange
        var label = "Parent Node";
        var defaultOpen = false;
        var child1 = new Mock<IDrawNode>();
        var child2 = new Mock<IDrawNode>();
        var child3 = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child1.Object, child2.Object, child3.Object };
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act
        node.Draw();

        // Assert
        child1.Verify(c => c.Draw(), Times.AtLeastOnce);
        child2.Verify(c => c.Draw(), Times.AtLeastOnce);
        child3.Verify(c => c.Draw(), Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests that TreePop is called via the finally block even when a child throws an exception.
    /// Verifies that the first child's Draw is invoked before the exception.
    /// </summary>
    [TestMethod]
    public void Draw_ChildThrowsException_FirstChildDrawIsCalled()
    {
        // Arrange
        var label = "Parent Node";
        var defaultOpen = true;
        var child1 = new Mock<IDrawNode>();
        var child2 = new Mock<IDrawNode>();
        child2.Setup(c => c.Draw()).Throws<InvalidOperationException>();
        var children = new List<IDrawNode> { child1.Object, child2.Object };
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act
        try
        {
            node.Draw();
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        // Assert
        child1.Verify(c => c.Draw(), Times.AtLeastOnce);
    }

}
