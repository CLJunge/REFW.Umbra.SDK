using Umbra.Config.Attributes;


namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Unit tests for the <see cref="CategoryNode"/> class.
/// </summary>
[TestClass]
public sealed class CategoryNodeTests
{
    /// <summary>
    /// Tests that Draw completes without exception when both indentAttr and collapseAttr are null.
    /// This should result in DrawAsHeader being called without any indentation.
    /// </summary>
    [TestMethod]
    public void Draw_WithNullIndentAndNullCollapse_CompletesWithoutException()
    {
        // Arrange
        var node = new CategoryNode("Test Category", collapseAttr: null, indentAttr: null);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw completes without exception when indentAttr is null and collapseAttr is provided.
    /// This should result in DrawAsTree being called without indentation.
    /// </summary>
    [TestMethod]
    public void Draw_WithNullIndentAndNonNullCollapse_CompletesWithoutException()
    {
        // Arrange
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: false);
        var node = new CategoryNode("Test Category", collapseAttr: collapseAttr, indentAttr: null);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw completes without exception when indentAttr is provided and collapseAttr is null.
    /// This should result in Indent being called, then DrawAsHeader, then Unindent.
    /// </summary>
    [TestMethod]
    public void Draw_WithNonNullIndentAndNullCollapse_CompletesWithoutException()
    {
        // Arrange
        var indentAttr = new UmbraIndentAttribute(20f);
        var node = new CategoryNode("Test Category", collapseAttr: null, indentAttr: indentAttr);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw completes without exception when both indentAttr and collapseAttr are provided.
    /// This should result in Indent being called, then DrawAsTree, then Unindent.
    /// </summary>
    [TestMethod]
    public void Draw_WithNonNullIndentAndNonNullCollapse_CompletesWithoutException()
    {
        // Arrange
        var indentAttr = new UmbraIndentAttribute(20f);
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: true);
        var node = new CategoryNode("Test Category", collapseAttr: collapseAttr, indentAttr: indentAttr);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw handles multiple consecutive calls without issues.
    /// Verifies the method is idempotent and can be called multiple times safely.
    /// </summary>
    [TestMethod]
    public void Draw_CalledMultipleTimes_CompletesWithoutException()
    {
        // Arrange
        var indentAttr = new UmbraIndentAttribute(15f);
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: true);
        var node = new CategoryNode("Test Category", collapseAttr: collapseAttr, indentAttr: indentAttr);

        // Act & Assert - all calls should complete without throwing
        node.Draw();
        node.Draw();
        node.Draw();
    }
}
