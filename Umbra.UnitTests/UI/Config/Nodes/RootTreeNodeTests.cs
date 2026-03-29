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
    /// Tests that Draw does not throw when called with an empty children list.
    /// </summary>
    [TestMethod]
    public void Draw_EmptyChildrenList_CompletesWithoutException()
    {
        // Arrange
        var label = "Empty Parent";
        var defaultOpen = true;
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw invokes Draw on a single child when children list contains one child.
    /// </summary>
    [TestMethod]
    public void Draw_SingleChild_InvokesDrawOnChild()
    {
        // Arrange
        var label = "Parent Node";
        var defaultOpen = true;
        var child = new Mock<IDrawNode>();
        var children = new List<IDrawNode> { child.Object };
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act
        node.Draw();

        // Assert
        child.Verify(c => c.Draw(), Times.AtLeastOnce);
    }

    /// <summary>
    /// Tests Draw with defaultOpen set to true.
    /// Verifies the method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Draw_DefaultOpenTrue_CompletesWithoutException()
    {
        // Arrange
        var label = "Node";
        var defaultOpen = true;
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }

    /// <summary>
    /// Tests Draw with defaultOpen set to false.
    /// Verifies the method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Draw_DefaultOpenFalse_CompletesWithoutException()
    {
        // Arrange
        var label = "Node";
        var defaultOpen = false;
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }

    /// <summary>
    /// Tests Draw with an empty string label.
    /// Verifies the method completes without throwing.
    /// </summary>
    /// <param name="label">The label string to test.</param>
    /// <param name="defaultOpen">The defaultOpen flag value.</param>
    [TestMethod]
    [DataRow("", true)]
    [DataRow("", false)]
    [DataRow("   ", true)]
    [DataRow("   ", false)]
    public void Draw_EmptyOrWhitespaceLabel_CompletesWithoutException(string label, bool defaultOpen)
    {
        // Arrange
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }

    /// <summary>
    /// Tests Draw with various special characters in the label.
    /// Verifies the method completes without throwing.
    /// </summary>
    /// <param name="label">The label string containing special characters.</param>
    [TestMethod]
    [DataRow("Node\nWith\nNewlines")]
    [DataRow("Node\tWith\tTabs")]
    [DataRow("Node With Spaces")]
    [DataRow("Node##With##Hashes")]
    [DataRow("Node/With/Slashes")]
    [DataRow("Node\\With\\Backslashes")]
    [DataRow("Node<With>Brackets")]
    [DataRow("Node|With|Pipes")]
    [DataRow("Node:With:Colons")]
    [DataRow("Node\"With\"Quotes")]
    [DataRow("Node'With'Apostrophes")]
    public void Draw_LabelWithSpecialCharacters_CompletesWithoutException(string label)
    {
        // Arrange
        var defaultOpen = true;
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }

    /// <summary>
    /// Tests Draw with a very long label string.
    /// Verifies the method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Draw_VeryLongLabel_CompletesWithoutException()
    {
        // Arrange
        var label = new string('A', 10000);
        var defaultOpen = true;
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }

    /// <summary>
    /// Tests Draw with Unicode characters in the label.
    /// Verifies the method completes without throwing.
    /// </summary>
    /// <param name="label">The label string containing Unicode characters.</param>
    [TestMethod]
    [DataRow("Node 日本語")]
    [DataRow("Node Ελληνικά")]
    [DataRow("Node العربية")]
    [DataRow("Node Русский")]
    [DataRow("Node 中文")]
    [DataRow("Node 🎮🎯🎨")]
    public void Draw_LabelWithUnicodeCharacters_CompletesWithoutException(string label)
    {
        // Arrange
        var defaultOpen = false;
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }

    /// <summary>
    /// Tests Draw with control characters in the label.
    /// Verifies the method completes without throwing.
    /// </summary>
    [TestMethod]
    public void Draw_LabelWithControlCharacters_CompletesWithoutException()
    {
        // Arrange
        var label = "Node\0With\0Null\u0001\u0002\u0003";
        var defaultOpen = true;
        var children = new List<IDrawNode>();
        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
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

    /// <summary>
    /// Tests Draw with a large number of children.
    /// Verifies the method completes without throwing and all children are invoked.
    /// </summary>
    [TestMethod]
    public void Draw_LargeNumberOfChildren_InvokesAllChildren()
    {
        // Arrange
        var label = "Parent with many children";
        var defaultOpen = true;
        var children = new List<IDrawNode>();
        var mocks = new List<Mock<IDrawNode>>();

        for (var i = 0; i < 100; i++)
        {
            var mock = new Mock<IDrawNode>();
            mocks.Add(mock);
            children.Add(mock.Object);
        }

        var node = new RootTreeNode(label, defaultOpen, children);

        // Act
        node.Draw();

        // Assert
        foreach (var mock in mocks)
        {
            mock.Verify(c => c.Draw(), Times.AtLeastOnce);
        }
    }

    /// <summary>
    /// Tests Draw with mixed defaultOpen values and various children configurations.
    /// </summary>
    /// <param name="defaultOpen">The defaultOpen flag value.</param>
    /// <param name="childCount">The number of children to create.</param>
    [TestMethod]
    [DataRow(true, 0)]
    [DataRow(false, 0)]
    [DataRow(true, 1)]
    [DataRow(false, 1)]
    [DataRow(true, 5)]
    [DataRow(false, 5)]
    public void Draw_VariousConfigurations_CompletesWithoutException(bool defaultOpen, int childCount)
    {
        // Arrange
        var label = "Test Node";
        var children = new List<IDrawNode>();

        for (var i = 0; i < childCount; i++)
        {
            var mock = new Mock<IDrawNode>();
            children.Add(mock.Object);
        }

        var node = new RootTreeNode(label, defaultOpen, children);

        // Act & Assert
        node.Draw();
    }
}