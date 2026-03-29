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
    /// Tests that Draw handles various indent amounts correctly, including edge cases.
    /// Verifies the method completes without exception for normal, zero, negative, and extreme float values.
    /// </summary>
    /// <param name="amount">The indentation amount to test.</param>
    [DataRow(0f)]
    [DataRow(10f)]
    [DataRow(50f)]
    [DataRow(100f)]
    [DataRow(-10f)]
    [DataRow(-50f)]
    [DataRow(float.MinValue)]
    [DataRow(float.MaxValue)]
    [DataRow(float.NaN)]
    [DataRow(float.PositiveInfinity)]
    [DataRow(float.NegativeInfinity)]
    [TestMethod]
    public void Draw_WithVariousIndentAmounts_CompletesWithoutException(float amount)
    {
        // Arrange
        var indentAttr = new UmbraIndentAttribute(amount);
        var node = new CategoryNode("Test Category", collapseAttr: null, indentAttr: indentAttr);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw handles both defaultOpen values for collapseAttr correctly.
    /// Verifies the method completes without exception when the tree node is set to open or closed by default.
    /// </summary>
    /// <param name="defaultOpen">Whether the tree node should be open by default.</param>
    [DataRow(true)]
    [DataRow(false)]
    [TestMethod]
    public void Draw_WithVariousCollapseDefaultOpen_CompletesWithoutException(bool defaultOpen)
    {
        // Arrange
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: defaultOpen);
        var node = new CategoryNode("Test Category", collapseAttr: collapseAttr, indentAttr: null);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw handles various label values correctly, including edge cases.
    /// Verifies the method completes without exception for empty, whitespace, special characters, and long strings.
    /// </summary>
    /// <param name="label">The label string to test.</param>
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("\t")]
    [DataRow("\n")]
    [DataRow("\r\n")]
    [DataRow("Normal Label")]
    [DataRow("Label with special chars: !@#$%^&*()")]
    [DataRow("Label with unicode: αβγδε 中文 🎉")]
    [DataRow("Very long label that exceeds typical display width and contains many characters to test rendering behavior with extended text content")]
    [TestMethod]
    public void Draw_WithVariousLabelValues_CompletesWithoutException(string label)
    {
        // Arrange
        var node = new CategoryNode(label, collapseAttr: null, indentAttr: null);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw completes when combining edge case indent amounts with collapse settings.
    /// Verifies robustness with extreme float values and various collapse configurations.
    /// </summary>
    /// <param name="amount">The indentation amount to test.</param>
    /// <param name="defaultOpen">Whether the tree node should be open by default.</param>
    [DataRow(0f, true)]
    [DataRow(0f, false)]
    [DataRow(float.NaN, true)]
    [DataRow(float.PositiveInfinity, false)]
    [DataRow(float.NegativeInfinity, true)]
    [DataRow(float.MaxValue, false)]
    [DataRow(float.MinValue, true)]
    [TestMethod]
    public void Draw_WithCombinedIndentAndCollapseEdgeCases_CompletesWithoutException(float amount, bool defaultOpen)
    {
        // Arrange
        var indentAttr = new UmbraIndentAttribute(amount);
        var collapseAttr = new UmbraCollapseAsTreeAttribute(defaultOpen: defaultOpen);
        var node = new CategoryNode("Test Category", collapseAttr: collapseAttr, indentAttr: indentAttr);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw handles the default indent amount (0f) correctly with collapse attribute.
    /// When amount is 0, ImGui should use its default indent spacing.
    /// </summary>
    [TestMethod]
    public void Draw_WithDefaultIndentAmount_CompletesWithoutException()
    {
        // Arrange
        var indentAttr = new UmbraIndentAttribute(); // default amount = 0f
        var node = new CategoryNode("Test Category", collapseAttr: null, indentAttr: indentAttr);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw handles the default collapse setting (defaultOpen = false) correctly with indent.
    /// The tree node should be collapsed by default.
    /// </summary>
    [TestMethod]
    public void Draw_WithDefaultCollapseSettings_CompletesWithoutException()
    {
        // Arrange
        var collapseAttr = new UmbraCollapseAsTreeAttribute(); // default defaultOpen = false
        var indentAttr = new UmbraIndentAttribute(10f);
        var node = new CategoryNode("Test Category", collapseAttr: collapseAttr, indentAttr: indentAttr);

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw completes when CategoryNode is constructed with all default optional parameters.
    /// Both collapseAttr and indentAttr should be null, resulting in a simple header without indentation.
    /// </summary>
    [TestMethod]
    public void Draw_WithAllDefaultParameters_CompletesWithoutException()
    {
        // Arrange
        var node = new CategoryNode("Test Category");

        // Act & Assert - should not throw
        node.Draw();
    }

    /// <summary>
    /// Tests that Draw handles a minimal label (single character) correctly.
    /// Verifies the method works with minimal valid input.
    /// </summary>
    [TestMethod]
    public void Draw_WithMinimalLabel_CompletesWithoutException()
    {
        // Arrange
        var node = new CategoryNode("X", collapseAttr: null, indentAttr: null);

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