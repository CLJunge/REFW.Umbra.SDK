using Hexa.NET.ImGui;
using Moq;

namespace Umbra.UI.Panel.UnitTests;


/// <summary>
/// Unit tests for the <see cref="PluginPanel.Draw"/> method.
/// </summary>
/// <remarks>
/// These tests inject a low-level panel renderer so <see cref="PluginPanel"/> draw behavior can
/// be verified without an active ImGui frame. The seam allows the tests to assert ID-scope
/// cleanup, tree-node behavior, separator placement, and section ordering directly.
/// </remarks>
[TestClass]
public sealed class PluginPanelTests_Draw
{
    /// <summary>
    /// Tests that calling <see cref="PluginPanel.Draw"/> on a disposed panel does not invoke any
    /// renderer operations.
    /// </summary>
    [TestMethod]
    public void Draw_WhenDisposed_DoesNotInvokeRenderer()
    {
        // Arrange
        var renderer = new TestPluginPanelRenderer();
        using var panel = new PluginPanel("DisposedScope", null, false, true, renderer);
        panel.Dispose();

        // Act
        panel.Draw();

        // Assert
        Assert.IsEmpty(renderer.PushIds);
        Assert.AreEqual(0, renderer.PopIdCount);
        Assert.IsEmpty(renderer.TreeNodes);
        Assert.AreEqual(0, renderer.TreePopCount);
        Assert.AreEqual(0, renderer.SeparatorCount);
    }

    /// <summary>
    /// Tests that a flat panel pushes and pops its ID scope and draws the trailing separator.
    /// </summary>
    [TestMethod]
    public void Draw_WithNoRootNodeAndNoSections_PushesScopeDrawsSeparatorAndPopsScope()
    {
        // Arrange
        var renderer = new TestPluginPanelRenderer();
        using var panel = new PluginPanel("FlatScope", rootNodeLabel: null, rootNodeDefaultOpen: false, drawSeparator: true, renderer);

        // Act
        panel.Draw();

        // Assert
        Assert.HasCount(1, renderer.PushIds);
        Assert.AreEqual("FlatScope", renderer.PushIds[0]);
        Assert.AreEqual(1, renderer.PopIdCount);
        Assert.IsEmpty(renderer.TreeNodes);
        Assert.AreEqual(0, renderer.TreePopCount);
        Assert.AreEqual(1, renderer.SeparatorCount);
    }

    /// <summary>
    /// Tests that a closed root tree node prevents section drawing and skips the separator.
    /// </summary>
    [TestMethod]
    public void Draw_WithClosedRootNode_SkipsSectionsAndSeparator()
    {
        // Arrange
        var renderer = new TestPluginPanelRenderer();
        renderer.TreeNodeResults.Enqueue(false);
        var sectionDrawn = false;
        using var panel = new PluginPanel("RootScope", rootNodeLabel: "Config", rootNodeDefaultOpen: false, drawSeparator: true, renderer);
        panel.Add(new CallbackPanelSection("SectionA", 0, null, false, () => sectionDrawn = true));

        // Act
        panel.Draw();

        // Assert
        Assert.HasCount(1, renderer.PushIds);
        Assert.AreEqual("RootScope", renderer.PushIds[0]);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Config", ImGuiTreeNodeFlags.None), renderer.TreeNodes[0]);
        Assert.IsFalse(sectionDrawn);
        Assert.AreEqual(0, renderer.TreePopCount);
        Assert.AreEqual(0, renderer.SeparatorCount);
        Assert.AreEqual(1, renderer.PopIdCount);
    }

    private static readonly int[] _expectedCalls = [1, 2];

    /// <summary>
    /// Tests that an open root tree node draws sections in sorted order, draws the separator, and
    /// pops the root node.
    /// </summary>
    [TestMethod]
    public void Draw_WithOpenRootNode_DrawsSectionsInOrderAndPopsRootNode()
    {
        // Arrange
        var renderer = new TestPluginPanelRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        var calls = new List<int>();
        using var panel = new PluginPanel("OpenRootScope", rootNodeLabel: "Config", rootNodeDefaultOpen: true, drawSeparator: true, renderer);
        panel.Add(new CallbackPanelSection("Section2", 2, null, false, () => calls.Add(2)));
        panel.Add(new CallbackPanelSection("Section1", 1, null, false, () => calls.Add(1)));

        // Act
        panel.Draw();

        // Assert
        CollectionAssert.AreEqual(_expectedCalls, calls);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Config", ImGuiTreeNodeFlags.DefaultOpen), renderer.TreeNodes[0]);
        Assert.AreEqual(1, renderer.TreePopCount);
        Assert.AreEqual(1, renderer.SeparatorCount);
        Assert.AreEqual(1, renderer.PopIdCount);
    }

    /// <summary>
    /// Tests that the panel sanitizes any caller-supplied ImGui ID suffix from the root node label
    /// before rendering it.
    /// </summary>
    [TestMethod]
    public void Draw_WithRootNodeLabelContainingSeparator_SanitizesRootLabel()
    {
        // Arrange
        var renderer = new TestPluginPanelRenderer();
        renderer.TreeNodeResults.Enqueue(false);
        using var panel = new PluginPanel("RootScope", rootNodeLabel: "Config##IgnoredSuffix", rootNodeDefaultOpen: false, drawSeparator: true, renderer);

        // Act
        panel.Draw();

        // Assert
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Config", ImGuiTreeNodeFlags.None), renderer.TreeNodes[0]);
        Assert.AreEqual(0, renderer.TreePopCount);
        Assert.AreEqual(1, renderer.PopIdCount);
    }

    /// <summary>
    /// Tests that a section tree node uses the sanitized label and section ID suffix and skips the
    /// section body when the tree is closed.
    /// </summary>
    [TestMethod]
    public void Draw_WithClosedSectionTreeNode_SanitizesLabelAndSkipsSectionBody()
    {
        // Arrange
        var renderer = new TestPluginPanelRenderer();
        renderer.TreeNodeResults.Enqueue(false);
        var drawCount = 0;
        using var panel = new PluginPanel("SectionTreeScope", rootNodeLabel: null, rootNodeDefaultOpen: false, drawSeparator: false, renderer);
        panel.Add(new CallbackPanelSection("MySection", 0, "General##IgnoredSuffix", true, () => drawCount++));

        // Act
        panel.Draw();

        // Assert
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("General##MySection", ImGuiTreeNodeFlags.DefaultOpen), renderer.TreeNodes[0]);
        Assert.AreEqual(0, drawCount);
        Assert.AreEqual(0, renderer.TreePopCount);
        Assert.AreEqual(0, renderer.SeparatorCount);
        Assert.AreEqual(1, renderer.PopIdCount);
    }

    /// <summary>
    /// Tests that tree and scope cleanup still occur when a section throws during drawing.
    /// </summary>
    [TestMethod]
    public void Draw_WhenSectionThrows_PopsTreeNodeAndScopeAndRethrows()
    {
        // Arrange
        var renderer = new TestPluginPanelRenderer();
        renderer.TreeNodeResults.Enqueue(true);
        using var panel = new PluginPanel("ThrowScope", rootNodeLabel: null, rootNodeDefaultOpen: false, drawSeparator: false, renderer);
        panel.Add(new CallbackPanelSection("ThrowingSection", 0, "Boom", false, () => throw new InvalidOperationException("boom")));

        InvalidOperationException? exception = null;

        // Act
        try
        {
            panel.Draw();
        }
        catch (InvalidOperationException ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNotNull(exception);
        Assert.AreEqual("boom", exception.Message);
        Assert.HasCount(1, renderer.TreeNodes);
        Assert.AreEqual(("Boom##ThrowingSection", ImGuiTreeNodeFlags.None), renderer.TreeNodes[0]);
        Assert.AreEqual(1, renderer.TreePopCount);
        Assert.AreEqual(1, renderer.PopIdCount);
        Assert.AreEqual(0, renderer.SeparatorCount);
    }

    /// <summary>
    /// Minimal panel section used to observe <see cref="PluginPanel"/> draw behavior.
    /// </summary>
    private sealed class CallbackPanelSection(
        string sectionId,
        int order,
        string? sectionLabel,
        bool expandedByDefault,
        Action callback) : IPanelSection
    {
        public int Order => order;

        public string? SectionLabel => sectionLabel;

        public bool ExpandedByDefault => expandedByDefault;

        public string SectionId => sectionId;

        public void Draw() => callback();

        public void Dispose()
        {
        }
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
    /// Tests that Add accepts section with null SectionLabel without throwing.
    /// </summary>
    [TestMethod]
    public void Add_SectionWithNullSectionLabel_DoesNotThrow()
    {
        // Arrange
        var panel = new PluginPanel("TestPanel");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.SectionLabel).Returns((string?)null);
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
    [DataRow("Config")]
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
    [DataRow("Plugin1", "Config", true, true)]
    [DataRow("Plugin2", "Settings", true, false)]
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

    /// <summary>
    /// Tests that Add rejects a null section.
    /// </summary>
    [TestMethod]
    public void Add_NullSection_ThrowsArgumentNullException()
    {
        using var panel = new PluginPanel($"NullSection_{Guid.NewGuid()}");

        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => panel.Add(null!));

        Assert.AreEqual("section", exception.ParamName);
    }

    /// <summary>
    /// Tests that Add rejects new sections after the panel has been disposed.
    /// </summary>
    [TestMethod]
    public void Add_WhenDisposed_ThrowsObjectDisposedException()
    {
        var panel = new PluginPanel($"DisposedPanel_{Guid.NewGuid()}");
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.Order).Returns(0);
        mockSection.Setup(s => s.SectionId).Returns("Section");
        panel.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => panel.Add(mockSection.Object));
    }

    /// <summary>
    /// Tests that disposing a panel releases its ID scope for later reuse.
    /// </summary>
    [TestMethod]
    public void Dispose_ReleasesRegisteredScope_AllowsNewPanelWithSameScope()
    {
        var idScope = $"ReusablePanel_{Guid.NewGuid()}";

        using (var first = new PluginPanel(idScope))
        {
            Assert.IsNotNull(first);
        }

        using var second = new PluginPanel(idScope);

        Assert.IsNotNull(second);
    }

    private static readonly string[] _expectedSections = new[] { "FirstSection", "SecondSection" };

    /// <summary>
    /// Tests that disposing a panel continues disposing later sections even when an earlier section throws.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenSectionDisposeThrows_DisposesRemainingSections()
    {
        // Arrange
        var disposedSections = new List<string>();
        var panel = new PluginPanel($"DisposeSections_{Guid.NewGuid()}");
        panel.Add(new DisposableTrackingPanelSection(
            "FirstSection",
            0,
            () =>
            {
                disposedSections.Add("FirstSection");
                throw new InvalidOperationException("boom");
            }));
        panel.Add(new DisposableTrackingPanelSection(
            "SecondSection",
            1,
            () => disposedSections.Add("SecondSection")));

        // Act
        panel.Dispose();

        // Assert
        CollectionAssert.AreEqual(_expectedSections, disposedSections);
    }

    private sealed class DisposableTrackingPanelSection(
        string sectionId,
        int order,
        Action disposeCallback) : IPanelSection
    {
        public int Order => order;

        public string SectionId => sectionId;

        public void Draw()
        {
        }

        public void Dispose() => disposeCallback();
    }
}
