using System;

using Hexa.NET.ImGui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.UI.Panel;

namespace Umbra.UI.Panel.UnitTests;


/// <summary>
/// Unit tests for the <see cref="PluginPanel.Draw"/> method.
/// </summary>
/// <remarks>
/// <para>
/// These tests are limited in scope due to the method's dependency on static ImGui methods
/// that cannot be mocked with Moq. The tests verify that the method completes without
/// throwing exceptions for various configurations, but cannot verify:
/// </para>
/// <list type="bullet">
/// <item><description>That ImGui.PushID and ImGui.PopID are called and balanced.</description></item>
/// <item><description>That ImGui.TreeNodeEx is called with the correct parameters.</description></item>
/// <item><description>That conditional branches based on TreeNodeEx return value are executed.</description></item>
/// <item><description>That ImGui.Separator is called when expected.</description></item>
/// <item><description>That ImGui.TreePop is called when expected.</description></item>
/// </list>
/// <para>
/// Full integration testing would require a working ImGui context and mocking framework
/// that supports static method interception.
/// </para>
/// </remarks>
[TestClass]
public sealed class PluginPanelTests_Draw
{
    /// <summary>
    /// Tests that calling Draw on a disposed panel returns immediately without throwing exceptions.
    /// </summary>
    /// <remarks>
    /// This verifies the early-return guard clause at line 151. The method should return before
    /// any ImGui calls are made. Note: We cannot verify that ImGui methods are NOT called due to
    /// the static method mocking limitation.
    /// </remarks>
    [TestMethod]
    public void Draw_WhenDisposed_ReturnsImmediatelyWithoutException()
    {
        // Arrange
        var panel = new PluginPanel("TestScope");
        panel.Dispose();

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw completes successfully when the panel has no root node label and no sections.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exercises the simplest execution path through the method: disposed check passes,
    /// ID scope is pushed, no root node path is taken (lines 169-173), DrawSections is called
    /// on an empty section list, separator is drawn if enabled, and ID scope is popped.
    /// </para>
    /// <para>
    /// Cannot verify: ImGui.PushID, ImGui.PopID, ImGui.Separator calls.
    /// </para>
    /// </remarks>
    [TestMethod]
    public void Draw_WithNoRootNodeAndNoSections_CompletesSuccessfully()
    {
        // Arrange
        var panel = new PluginPanel("TestScope", rootNodeLabel: null, drawSeparator: true);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw completes successfully when a root node label is provided with default closed state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exercises the root node path (lines 156-168) with rootNodeDefaultOpen = false,
    /// which should use ImGuiTreeNodeFlags.None. The actual tree node state (open/closed)
    /// depends on ImGui.TreeNodeEx return value, which we cannot control.
    /// </para>
    /// <para>
    /// Cannot verify: ImGui.TreeNodeEx called with correct label and flags, or conditional
    /// execution based on its return value.
    /// </para>
    /// </remarks>
    [TestMethod]
    public void Draw_WithRootNodeDefaultClosed_CompletesSuccessfully()
    {
        // Arrange
        var panel = new PluginPanel("TestScope", rootNodeLabel: "Settings", rootNodeDefaultOpen: false);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw completes successfully when a root node label is provided with default open state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exercises the root node path (lines 156-168) with rootNodeDefaultOpen = true,
    /// which should use ImGuiTreeNodeFlags.DefaultOpen. The actual tree node state depends
    /// on ImGui.TreeNodeEx return value, which we cannot control.
    /// </para>
    /// <para>
    /// Cannot verify: ImGui.TreeNodeEx called with ImGuiTreeNodeFlags.DefaultOpen.
    /// </para>
    /// </remarks>
    [TestMethod]
    public void Draw_WithRootNodeDefaultOpen_CompletesSuccessfully()
    {
        // Arrange
        var panel = new PluginPanel("TestScope", rootNodeLabel: "Settings", rootNodeDefaultOpen: true);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw completes successfully when separator drawing is enabled.
    /// </summary>
    /// <remarks>
    /// Cannot verify that ImGui.Separator is actually called due to static method mocking limitation.
    /// </remarks>
    [TestMethod]
    public void Draw_WithSeparatorEnabled_CompletesSuccessfully()
    {
        // Arrange
        var panel = new PluginPanel("TestScope", drawSeparator: true);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw completes successfully when separator drawing is disabled.
    /// </summary>
    /// <remarks>
    /// Cannot verify that ImGui.Separator is NOT called due to static method mocking limitation.
    /// </remarks>
    [TestMethod]
    public void Draw_WithSeparatorDisabled_CompletesSuccessfully()
    {
        // Arrange
        var panel = new PluginPanel("TestScope", drawSeparator: false);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests Draw with various combinations of configuration parameters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Parameterized test to verify the method completes without exceptions across different
    /// combinations of rootNodeLabel, rootNodeDefaultOpen, and drawSeparator values.
    /// </para>
    /// <para>
    /// Cannot verify actual ImGui call sequences or conditional branch execution.
    /// </para>
    /// </remarks>
    /// <param name="rootNodeLabel">The root node label to test with.</param>
    /// <param name="rootNodeDefaultOpen">Whether the root node should default to open.</param>
    /// <param name="drawSeparator">Whether separator drawing should be enabled.</param>
    [TestMethod]
    [DataRow(null, false, true, DisplayName = "No root node, separator enabled")]
    [DataRow(null, false, false, DisplayName = "No root node, separator disabled")]
    [DataRow("Settings", false, true, DisplayName = "Root node closed, separator enabled")]
    [DataRow("Settings", true, true, DisplayName = "Root node open, separator enabled")]
    [DataRow("Settings", false, false, DisplayName = "Root node closed, separator disabled")]
    [DataRow("Settings", true, false, DisplayName = "Root node open, separator disabled")]
    [DataRow("", false, true, DisplayName = "Empty root node label, separator enabled")]
    public void Draw_WithVariousConfigurations_CompletesSuccessfully(
        string? rootNodeLabel,
        bool rootNodeDefaultOpen,
        bool drawSeparator)
    {
        // Arrange
        var panel = new PluginPanel(
            "TestScope",
            rootNodeLabel: rootNodeLabel,
            rootNodeDefaultOpen: rootNodeDefaultOpen,
            drawSeparator: drawSeparator);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw calls the DrawSections private method by verifying that section Draw methods are invoked.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This test adds mock sections to the panel and verifies their Draw methods are called,
    /// which indirectly proves DrawSections was executed. However, we cannot verify:
    /// </para>
    /// <list type="bullet">
    /// <item><description>The exact number of times DrawSections is called.</description></item>
    /// <item><description>Whether sections are drawn only when tree nodes are open.</description></item>
    /// <item><description>The order in which ImGui state management methods are called.</description></item>
    /// </list>
    /// <para>
    /// Note: Section Draw() methods may themselves call ImGui methods which would fail without
    /// a valid ImGui context. For this test, we use mock sections that do nothing in Draw().
    /// </para>
    /// </remarks>
    [TestMethod]
    public void Draw_WithSections_CallsSectionDrawMethods()
    {
        // Arrange
        var mockSection1 = new Mock<IPanelSection>();
        mockSection1.SetupGet(s => s.SectionId).Returns("Section1");
        mockSection1.SetupGet(s => s.Order).Returns(0);
        mockSection1.SetupGet(s => s.TreeNodeLabel).Returns((string?)null);
        mockSection1.SetupGet(s => s.TreeNodeDefaultOpen).Returns(false);
        mockSection1.Setup(s => s.Draw()).Verifiable();

        var mockSection2 = new Mock<IPanelSection>();
        mockSection2.SetupGet(s => s.SectionId).Returns("Section2");
        mockSection2.SetupGet(s => s.Order).Returns(1);
        mockSection2.SetupGet(s => s.TreeNodeLabel).Returns((string?)null);
        mockSection2.SetupGet(s => s.TreeNodeDefaultOpen).Returns(false);
        mockSection2.Setup(s => s.Draw()).Verifiable();

        var panel = new PluginPanel("TestScope", rootNodeLabel: null);
        panel.Add(mockSection1.Object);
        panel.Add(mockSection2.Object);

        // Act
        panel.Draw();

        // Assert
        // Verify that Draw was called on each section, proving DrawSections executed
        mockSection1.Verify(s => s.Draw(), Times.AtLeastOnce());
        mockSection2.Verify(s => s.Draw(), Times.AtLeastOnce());
    }

    /// <summary>
    /// Tests that Draw with sections that have tree node labels completes successfully.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This exercises the per-section tree node rendering path in DrawSections (lines 249-258).
    /// Cannot verify:
    /// </para>
    /// <list type="bullet">
    /// <item><description>That ImGui.TreeNodeEx is called for each section with correct label.</description></item>
    /// <item><description>That section Draw is only called when the section's tree node is open.</description></item>
    /// <item><description>That PluginPanelTreeNodeLabels.Sanitize is called.</description></item>
    /// </list>
    /// </remarks>
    [TestMethod]
    public void Draw_WithSectionsHavingTreeNodeLabels_CompletesSuccessfully()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.SetupGet(s => s.SectionId).Returns("ConfigSection");
        mockSection.SetupGet(s => s.Order).Returns(0);
        mockSection.SetupGet(s => s.TreeNodeLabel).Returns("Configuration");
        mockSection.SetupGet(s => s.TreeNodeDefaultOpen).Returns(true);
        mockSection.Setup(s => s.Draw());

        var panel = new PluginPanel("TestScope", rootNodeLabel: "Root");
        panel.Add(mockSection.Object);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw can be called multiple times on the same panel without exceptions.
    /// </summary>
    /// <remarks>
    /// Simulates the typical usage pattern where Draw is called every frame from an ImGui callback.
    /// Cannot verify that ImGui state remains balanced across multiple calls.
    /// </remarks>
    [TestMethod]
    public void Draw_CalledMultipleTimes_CompletesSuccessfullyEachTime()
    {
        // Arrange
        var panel = new PluginPanel("TestScope", rootNodeLabel: "Settings");

        // Act - should not throw on multiple calls
        panel.Draw();
        panel.Draw();
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw handles the edge case of an empty root node label string.
    /// </summary>
    /// <remarks>
    /// An empty string is a valid root node label (non-null), so the root node path should
    /// be taken. Cannot verify ImGui.TreeNodeEx is called with empty label.
    /// </remarks>
    [TestMethod]
    public void Draw_WithEmptyRootNodeLabel_CompletesSuccessfully()
    {
        // Arrange
        var panel = new PluginPanel("TestScope", rootNodeLabel: string.Empty);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw handles whitespace-only root node labels.
    /// </summary>
    /// <remarks>
    /// Whitespace-only strings are valid root node labels and should be rendered as-is by ImGui.
    /// </remarks>
    [TestMethod]
    [DataRow(" ", DisplayName = "Single space")]
    [DataRow("   ", DisplayName = "Multiple spaces")]
    [DataRow("\t", DisplayName = "Tab character")]
    [DataRow("\n", DisplayName = "Newline character")]
    public void Draw_WithWhitespaceRootNodeLabel_CompletesSuccessfully(string rootNodeLabel)
    {
        // Arrange
        var panel = new PluginPanel("TestScope", rootNodeLabel: rootNodeLabel);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw completes successfully with a very long root node label.
    /// </summary>
    /// <remarks>
    /// Verifies the method handles extreme string lengths without exceptions.
    /// Cannot verify ImGui rendering behavior with long labels.
    /// </remarks>
    [TestMethod]
    public void Draw_WithVeryLongRootNodeLabel_CompletesSuccessfully()
    {
        // Arrange
        var longLabel = new string('A', 10000);
        var panel = new PluginPanel("TestScope", rootNodeLabel: longLabel);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that Draw handles special characters in root node labels.
    /// </summary>
    /// <remarks>
    /// ImGui uses ## as an ID separator, so labels with special characters should be handled.
    /// Cannot verify that PluginPanelTreeNodeLabels sanitization occurs or ImGui rendering.
    /// </remarks>
    [TestMethod]
    [DataRow("Settings##DuplicateID", DisplayName = "Label with ## separator")]
    [DataRow("Settings###Triple", DisplayName = "Label with ### separator")]
    [DataRow("Settings\0Control", DisplayName = "Label with null character")]
    [DataRow("設定", DisplayName = "Label with Unicode characters")]
    [DataRow("Settings\r\nMultiline", DisplayName = "Label with newlines")]
    public void Draw_WithSpecialCharactersInRootNodeLabel_CompletesSuccessfully(string rootNodeLabel)
    {
        // Arrange
        var panel = new PluginPanel("TestScope", rootNodeLabel: rootNodeLabel);

        // Act - should not throw
        panel.Draw();

        // Assert - implicit: no exception thrown
    }

    /// <summary>
    /// Tests that disposing a panel after calling Draw multiple times works correctly.
    /// </summary>
    /// <remarks>
    /// Verifies the disposal flow after normal usage.
    /// </remarks>
    [TestMethod]
    public void Draw_CalledBeforeDisposal_AllowsSuccessfulDisposal()
    {
        // Arrange
        var panel = new PluginPanel("TestScope");
        panel.Draw();
        panel.Draw();

        // Act - should not throw
        panel.Dispose();

        // Assert - calling Draw after disposal should be a no-op
        panel.Draw();
    }
}


/// <summary>
/// Unit tests for <see cref="PluginPanel.Add(IPanelSection)"/> method.
/// </summary>
[TestClass]
public sealed class PluginPanelTests
{
    /// <summary>
    /// Tests that Add returns the same PluginPanel instance for fluent chaining.
    /// </summary>
    [TestMethod]
    public void Add_ValidSection_ReturnsThisInstance()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.SectionId).Returns("TestSection");

        // Act
        var result = panel.Add(mockSection.Object);

        // Assert
        Assert.AreSame(panel, result);
    }

    /// <summary>
    /// Tests that Add can be called multiple times in fluent chain without throwing.
    /// </summary>
    [TestMethod]
    public void Add_FluentChaining_AllowsMultipleAdds()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection1 = new Mock<IPanelSection>();
        mockSection1.Setup(s => s.Order).Returns(1);
        mockSection1.Setup(s => s.SectionId).Returns("Section1");

        var mockSection2 = new Mock<IPanelSection>();
        mockSection2.Setup(s => s.Order).Returns(2);
        mockSection2.Setup(s => s.SectionId).Returns("Section2");

        var mockSection3 = new Mock<IPanelSection>();
        mockSection3.Setup(s => s.Order).Returns(3);
        mockSection3.Setup(s => s.SectionId).Returns("Section3");

        // Act
        var result = panel.Add(mockSection1.Object)
                          .Add(mockSection2.Object)
                          .Add(mockSection3.Object);

        // Assert
        Assert.AreSame(panel, result);
    }

    /// <summary>
    /// Tests that Add accepts sections with different Order values without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionsWithDifferentOrders_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection1 = new Mock<IPanelSection>();
        mockSection1.Setup(s => s.Order).Returns(10);
        mockSection1.Setup(s => s.SectionId).Returns("Section1");

        var mockSection2 = new Mock<IPanelSection>();
        mockSection2.Setup(s => s.Order).Returns(5);
        mockSection2.Setup(s => s.SectionId).Returns("Section2");

        var mockSection3 = new Mock<IPanelSection>();
        mockSection3.Setup(s => s.Order).Returns(15);
        mockSection3.Setup(s => s.SectionId).Returns("Section3");

        // Act & Assert - Should not throw
        panel.Add(mockSection1.Object)
             .Add(mockSection2.Object)
             .Add(mockSection3.Object);
    }

    /// <summary>
    /// Tests that Add accepts sections with same Order values without throwing (stable sort scenario).
    /// </summary>
    [TestMethod]
    public void Add_SectionsWithSameOrder_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection1 = new Mock<IPanelSection>();
        mockSection1.Setup(s => s.Order).Returns(10);
        mockSection1.Setup(s => s.SectionId).Returns("Section1");

        var mockSection2 = new Mock<IPanelSection>();
        mockSection2.Setup(s => s.Order).Returns(10);
        mockSection2.Setup(s => s.SectionId).Returns("Section2");

        var mockSection3 = new Mock<IPanelSection>();
        mockSection3.Setup(s => s.Order).Returns(10);
        mockSection3.Setup(s => s.SectionId).Returns("Section3");

        // Act & Assert - Should not throw
        panel.Add(mockSection1.Object)
             .Add(mockSection2.Object)
             .Add(mockSection3.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with int.MinValue Order without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithMinValueOrder_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(int.MinValue);
        mockSection.Setup(s => s.SectionId).Returns("MinSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with int.MaxValue Order without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithMaxValueOrder_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(int.MaxValue);
        mockSection.Setup(s => s.SectionId).Returns("MaxSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with zero Order without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithZeroOrder_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.SectionId).Returns("ZeroSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with negative Order without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithNegativeOrder_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(-100);
        mockSection.Setup(s => s.SectionId).Returns("NegativeSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with null TreeNodeLabel without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithNullTreeNodeLabel_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.TreeNodeLabel).Returns((string?)null);
        mockSection.Setup(s => s.SectionId).Returns("NullLabelSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with non-null TreeNodeLabel without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithTreeNodeLabel_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Test Node");
        mockSection.Setup(s => s.SectionId).Returns("LabeledSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with empty string TreeNodeLabel without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithEmptyTreeNodeLabel_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.TreeNodeLabel).Returns(string.Empty);
        mockSection.Setup(s => s.SectionId).Returns("EmptyLabelSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with TreeNodeLabel containing special characters without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithSpecialCharactersInTreeNodeLabel_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Test <>&\"' Node");
        mockSection.Setup(s => s.SectionId).Returns("SpecialCharsSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with TreeNodeLabel containing ImGui separator (##) without throwing,
    /// though a warning may be logged internally.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithSeparatorInTreeNodeLabel_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Test##Node");
        mockSection.Setup(s => s.SectionId).Returns("SeparatorSection");

        // Act & Assert - Should not throw (validation warning is logged, not exception)
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with null SectionId without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithNullSectionId_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.SectionId).Returns((string?)null!);

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add accepts section with empty SectionId without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithEmptySectionId_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.SectionId).Returns(string.Empty);

        // Act & Assert - Should not throw
        panel.Add(mockSection.Object);
    }

    /// <summary>
    /// Tests that Add can be called after a previous Add without throwing.
    /// </summary>
    [TestMethod]
    public void Add_CalledTwiceSequentially_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection1 = new Mock<IPanelSection>();
        mockSection1.Setup(s => s.Order).Returns(1);
        mockSection1.Setup(s => s.SectionId).Returns("Section1");

        var mockSection2 = new Mock<IPanelSection>();
        mockSection2.Setup(s => s.Order).Returns(2);
        mockSection2.Setup(s => s.SectionId).Returns("Section2");

        // Act
        panel.Add(mockSection1.Object);
        var result = panel.Add(mockSection2.Object);

        // Assert
        Assert.AreSame(panel, result);
    }

    /// <summary>
    /// Tests that Add with section having boundary Order values in mixed sequence does not throw.
    /// </summary>
    [TestMethod]
    public void Add_MixedBoundaryOrders_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection1 = new Mock<IPanelSection>();
        mockSection1.Setup(s => s.Order).Returns(int.MaxValue);
        mockSection1.Setup(s => s.SectionId).Returns("MaxSection");

        var mockSection2 = new Mock<IPanelSection>();
        mockSection2.Setup(s => s.Order).Returns(int.MinValue);
        mockSection2.Setup(s => s.SectionId).Returns("MinSection");

        var mockSection3 = new Mock<IPanelSection>();
        mockSection3.Setup(s => s.Order).Returns(0);
        mockSection3.Setup(s => s.SectionId).Returns("ZeroSection");

        // Act & Assert - Should not throw
        panel.Add(mockSection1.Object)
             .Add(mockSection2.Object)
             .Add(mockSection3.Object);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance when provided with a valid idScope and default parameters.
    /// Input conditions: valid non-null, non-whitespace idScope string.
    /// Expected result: PluginPanel instance is created without throwing.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidIdScope_CreatesInstance()
    {
        // Arrange
        const string validIdScope = "ValidPluginScope";

        // Act
        using var panel = new PluginPanel(validIdScope);

        // Assert
        Assert.IsNotNull(panel);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance with various valid idScope values.
    /// Input conditions: different valid idScope strings including alphanumeric, special characters, and mixed content.
    /// Expected result: PluginPanel instance is created without throwing for all valid inputs.
    /// </summary>
    /// <param name="validIdScope">The valid idScope string to test.</param>
    [TestMethod]
    [DataRow("MyPlugin")]
    [DataRow("Plugin123")]
    [DataRow("My.Plugin.Namespace")]
    [DataRow("Plugin_With_Underscores")]
    [DataRow("Plugin-With-Dashes")]
    [DataRow("PluginWithSpecialChars!@#")]
    [DataRow("Very Long Plugin Scope Name With Many Words")]
    [DataRow("a")]
    public void Constructor_VariousValidIdScopes_CreatesInstance(string validIdScope)
    {
        // Act
        using var panel = new PluginPanel(validIdScope);

        // Assert
        Assert.IsNotNull(panel);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance when rootNodeLabel is provided.
    /// Input conditions: valid idScope with various rootNodeLabel values (null, empty, whitespace, normal string).
    /// Expected result: PluginPanel instance is created without throwing.
    /// </summary>
    /// <param name="rootNodeLabel">The root node label to test.</param>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("Root Node")]
    [DataRow("Settings")]
    [DataRow("Configuration Panel")]
    public void Constructor_WithRootNodeLabel_CreatesInstance(string? rootNodeLabel)
    {
        // Arrange
        const string validIdScope = "TestPlugin";

        // Act
        using var panel = new PluginPanel(validIdScope, rootNodeLabel);

        // Assert
        Assert.IsNotNull(panel);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance with rootNodeDefaultOpen parameter set to true.
    /// Input conditions: valid idScope, rootNodeLabel, and rootNodeDefaultOpen = true.
    /// Expected result: PluginPanel instance is created without throwing.
    /// </summary>
    [TestMethod]
    public void Constructor_WithRootNodeDefaultOpenTrue_CreatesInstance()
    {
        // Arrange
        const string validIdScope = "TestPlugin";
        const string rootNodeLabel = "Settings";

        // Act
        using var panel = new PluginPanel(validIdScope, rootNodeLabel, rootNodeDefaultOpen: true);

        // Assert
        Assert.IsNotNull(panel);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance with rootNodeDefaultOpen parameter set to false.
    /// Input conditions: valid idScope, rootNodeLabel, and rootNodeDefaultOpen = false.
    /// Expected result: PluginPanel instance is created without throwing.
    /// </summary>
    [TestMethod]
    public void Constructor_WithRootNodeDefaultOpenFalse_CreatesInstance()
    {
        // Arrange
        const string validIdScope = "TestPlugin";
        const string rootNodeLabel = "Settings";

        // Act
        using var panel = new PluginPanel(validIdScope, rootNodeLabel, rootNodeDefaultOpen: false);

        // Assert
        Assert.IsNotNull(panel);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance with drawSeparator parameter set to true.
    /// Input conditions: valid idScope and drawSeparator = true (default).
    /// Expected result: PluginPanel instance is created without throwing.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDrawSeparatorTrue_CreatesInstance()
    {
        // Arrange
        const string validIdScope = "TestPlugin";

        // Act
        using var panel = new PluginPanel(validIdScope, drawSeparator: true);

        // Assert
        Assert.IsNotNull(panel);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance with drawSeparator parameter set to false.
    /// Input conditions: valid idScope and drawSeparator = false.
    /// Expected result: PluginPanel instance is created without throwing.
    /// </summary>
    [TestMethod]
    public void Constructor_WithDrawSeparatorFalse_CreatesInstance()
    {
        // Arrange
        const string validIdScope = "TestPlugin";

        // Act
        using var panel = new PluginPanel(validIdScope, drawSeparator: false);

        // Assert
        Assert.IsNotNull(panel);
    }

    /// <summary>
    /// Tests that the constructor successfully creates an instance when all parameters are explicitly provided.
    /// Input conditions: valid values for all constructor parameters in various combinations.
    /// Expected result: PluginPanel instance is created without throwing for all parameter combinations.
    /// </summary>
    /// <param name="idScope">The plugin ID scope.</param>
    /// <param name="rootNodeLabel">The root node label.</param>
    /// <param name="rootNodeDefaultOpen">Whether the root node is open by default.</param>
    /// <param name="drawSeparator">Whether to draw a separator.</param>
    [TestMethod]
    [DataRow("Plugin1", "Settings", true, true)]
    [DataRow("Plugin2", "Config", true, false)]
    [DataRow("Plugin3", "Panel", false, true)]
    [DataRow("Plugin4", "Options", false, false)]
    [DataRow("Plugin5", null, true, true)]
    [DataRow("Plugin6", null, false, false)]
    [DataRow("Plugin7", "", true, false)]
    [DataRow("Plugin8", " ", false, true)]
    public void Constructor_WithAllParameterCombinations_CreatesInstance(
        string idScope,
        string? rootNodeLabel,
        bool rootNodeDefaultOpen,
        bool drawSeparator)
    {
        // Act
        using var panel = new PluginPanel(idScope, rootNodeLabel, rootNodeDefaultOpen, drawSeparator);

        // Assert
        Assert.IsNotNull(panel);
    }

    /// <summary>
    /// Tests that multiple PluginPanel instances can be created with different idScopes.
    /// Input conditions: multiple valid but distinct idScope values.
    /// Expected result: All instances are created successfully without conflict.
    /// </summary>
    [TestMethod]
    public void Constructor_MultipleInstancesWithDifferentScopes_CreatesAllInstances()
    {
        // Act
        using var panel1 = new PluginPanel("Plugin1");
        using var panel2 = new PluginPanel("Plugin2");
        using var panel3 = new PluginPanel("Plugin3");

        // Assert
        Assert.IsNotNull(panel1);
        Assert.IsNotNull(panel2);
        Assert.IsNotNull(panel3);
    }

    /// <summary>
    /// Tests that the created PluginPanel instance can be disposed without throwing.
    /// Input conditions: valid constructor parameters.
    /// Expected result: Dispose completes without throwing.
    /// </summary>
    [TestMethod]
    public void Constructor_CreatedInstance_CanBeDisposed()
    {
        // Arrange
        var panel = new PluginPanel("TestPlugin");

        // Act & Assert (no exception should be thrown)
        panel.Dispose();
    }

    /// <summary>
    /// Tests that the constructor handles idScope with leading and trailing valid characters but containing spaces.
    /// Input conditions: idScope with spaces in the middle but non-whitespace at edges.
    /// Expected result: PluginPanel instance is created without throwing.
    /// </summary>
    [TestMethod]
    public void Constructor_IdScopeWithInternalSpaces_CreatesInstance()
    {
        // Arrange
        const string idScopeWithSpaces = "My Plugin Scope";

        // Act
        using var panel = new PluginPanel(idScopeWithSpaces);

        // Assert
        Assert.IsNotNull(panel);
    }
}