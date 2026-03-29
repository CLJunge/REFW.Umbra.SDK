using Moq;

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

}
