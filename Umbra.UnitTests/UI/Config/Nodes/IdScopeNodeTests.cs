using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.UI.Config.Nodes;


namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for <see cref="IdScopeNode"/>.
/// </summary>
[TestClass]
public sealed class IdScopeNodeTests
{
    /// <summary>
    /// Tests that Draw calls Draw on all children in the correct order when given a valid scopeId and non-empty children list.
    /// Input: Valid scopeId string and list containing three mock IDrawNode children.
    /// Expected: Each child's Draw method is called exactly once in the order they appear in the list.
    /// </summary>
    [TestMethod]
    public void Draw_WithMultipleChildren_CallsDrawOnAllChildrenInOrder()
    {
        // Arrange
        var callOrder = new List<int>();
        var child1 = new Mock<IDrawNode>();
        child1.Setup(c => c.Draw()).Callback(() => callOrder.Add(1));
        var child2 = new Mock<IDrawNode>();
        child2.Setup(c => c.Draw()).Callback(() => callOrder.Add(2));
        var child3 = new Mock<IDrawNode>();
        child3.Setup(c => c.Draw()).Callback(() => callOrder.Add(3));
        var children = new List<IDrawNode> { child1.Object, child2.Object, child3.Object };
        var node = new IdScopeNode("testScope", children);

        // Act
        node.Draw();

        // Assert
        child1.Verify(c => c.Draw(), Times.Once);
        child2.Verify(c => c.Draw(), Times.Once);
        child3.Verify(c => c.Draw(), Times.Once);
        CollectionAssert.AreEqual(new List<int> { 1, 2, 3 }, callOrder);
    }

    /// <summary>
    /// Tests that Draw does not throw when the children list is empty.
    /// Input: Valid scopeId string and empty children list.
    /// Expected: No exception is thrown and method completes successfully.
    /// </summary>
    [TestMethod]
    public void Draw_WithEmptyChildrenList_DoesNotThrow()
    {
        // Arrange
        var children = new List<IDrawNode>();
        var node = new IdScopeNode("testScope", children);

        // Act & Assert
        node.Draw(); // Should not throw
    }

    /// <summary>
    /// Tests that Draw works correctly with an empty string as scopeId.
    /// Input: Empty string as scopeId and list with one mock child.
    /// Expected: Child's Draw method is called exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WithEmptyScopeId_CallsDrawOnChildren()
    {
        // Arrange
        var child = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child.Object };
        var node = new IdScopeNode("", children);

        // Act
        node.Draw();

        // Assert
        child.Verify(c => c.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that Draw works correctly with whitespace-only scopeId.
    /// Input: Whitespace-only string as scopeId and list with one mock child.
    /// Expected: Child's Draw method is called exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WithWhitespaceScopeId_CallsDrawOnChildren()
    {
        // Arrange
        var child = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child.Object };
        var node = new IdScopeNode("   ", children);

        // Act
        node.Draw();

        // Assert
        child.Verify(c => c.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that Draw works correctly with special characters in scopeId.
    /// Input: ScopeId containing dots, slashes, and special characters, and list with one mock child.
    /// Expected: Child's Draw method is called exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WithSpecialCharactersInScopeId_CallsDrawOnChildren()
    {
        // Arrange
        var child = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child.Object };
        var node = new IdScopeNode("scope.with.dots/and\\slashes#special@!$%", children);

        // Act
        node.Draw();

        // Assert
        child.Verify(c => c.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that Draw works correctly with unicode characters in scopeId.
    /// Input: ScopeId containing unicode characters (Chinese, emoji, Cyrillic) and list with one mock child.
    /// Expected: Child's Draw method is called exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WithUnicodeScopeId_CallsDrawOnChildren()
    {
        // Arrange
        var child = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child.Object };
        var node = new IdScopeNode("测试🎮Тест", children);

        // Act
        node.Draw();

        // Assert
        child.Verify(c => c.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that Draw works correctly with a very long scopeId string.
    /// Input: ScopeId of 10000 characters and list with one mock child.
    /// Expected: Child's Draw method is called exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WithVeryLongScopeId_CallsDrawOnChildren()
    {
        // Arrange
        var child = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child.Object };
        var longScopeId = new string('a', 10000);
        var node = new IdScopeNode(longScopeId, children);

        // Act
        node.Draw();

        // Assert
        child.Verify(c => c.Draw(), Times.Once);
    }

    /// <summary>
    /// Tests that when the first child throws an exception, subsequent children are not called.
    /// Input: List with first child that throws and second child that should not be called.
    /// Expected: First child's Draw is called once, second child's Draw is never called, and exception is thrown.
    /// </summary>
    [TestMethod]
    public void Draw_WhenFirstChildThrows_DoesNotCallSubsequentChildren()
    {
        // Arrange
        var child1 = new Mock<IDrawNode>();
        child1.Setup(c => c.Draw()).Throws<InvalidOperationException>();
        var child2 = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child1.Object, child2.Object };
        var node = new IdScopeNode("testScope", children);

        // Act
        try
        {
            node.Draw();
        }
        catch (InvalidOperationException)
        {
            // Expected exception
        }

        // Assert
        child1.Verify(c => c.Draw(), Times.Once);
        child2.Verify(c => c.Draw(), Times.Never);
    }

    /// <summary>
    /// Tests that when a middle child throws an exception, children before it are called but children after are not.
    /// Input: List with three children where the second throws an exception.
    /// Expected: First child is called once, second child is called once and throws, third child is never called.
    /// </summary>
    [TestMethod]
    public void Draw_WhenMiddleChildThrows_CallsPreviousChildrenOnly()
    {
        // Arrange
        var child1 = new Mock<IDrawNode>();
        var child2 = new Mock<IDrawNode>();
        child2.Setup(c => c.Draw()).Throws<ArgumentException>();
        var child3 = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child1.Object, child2.Object, child3.Object };
        var node = new IdScopeNode("testScope", children);

        // Act
        try
        {
            node.Draw();
        }
        catch (ArgumentException)
        {
            // Expected exception
        }

        // Assert
        child1.Verify(c => c.Draw(), Times.Once);
        child2.Verify(c => c.Draw(), Times.Once);
        child3.Verify(c => c.Draw(), Times.Never);
    }

    /// <summary>
    /// Tests that Draw can be called multiple times successfully on the same node instance.
    /// Input: Node with single child, Draw called three times consecutively.
    /// Expected: Child's Draw method is called exactly three times total.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_CallsChildDrawEachTime()
    {
        // Arrange
        var child = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child.Object };
        var node = new IdScopeNode("testScope", children);

        // Act
        node.Draw();
        node.Draw();
        node.Draw();

        // Assert
        child.Verify(c => c.Draw(), Times.Exactly(3));
    }

    /// <summary>
    /// Tests that Draw works correctly with a single child in the list.
    /// Input: Valid scopeId and list containing exactly one mock child.
    /// Expected: Child's Draw method is called exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WithSingleChild_CallsDrawOnce()
    {
        // Arrange
        var child = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child.Object };
        var node = new IdScopeNode("testScope", children);

        // Act
        node.Draw();

        // Assert
        child.Verify(c => c.Draw(), Times.Once);
    }

}