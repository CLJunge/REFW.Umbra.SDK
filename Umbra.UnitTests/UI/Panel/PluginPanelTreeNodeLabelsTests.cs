using Moq;

namespace Umbra.UI.Panel.UnitTests;


/// <summary>
/// Contains unit tests for the <see cref="PluginPanelTreeNodeLabels"/> class.
/// </summary>
[TestClass]
public class PluginPanelTreeNodeLabelsTests
{
    /// <summary>
    /// Gets or sets the test context which provides information about and functionality for the current test run.
    /// </summary>
    public TestContext? TestContext { get; set; }

    /// <summary>
    /// Tests that Sanitize removes the ImGui separator and everything after it when the separator is in the middle of the label.
    /// Input: "Label##ID"
    /// Expected: "Label"
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithSeparatorInMiddle_RemovesSeparatorAndSuffix()
    {
        // Arrange
        var input = "Label##ID";

        // Act
        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Label", result);
    }

    /// <summary>
    /// Tests that Sanitize returns the original label when no separator is present.
    /// Input: "NoSeparator"
    /// Expected: "NoSeparator"
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithoutSeparator_ReturnsOriginalLabel()
    {
        // Arrange
        var input = "NoSeparator";

        // Act
        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("NoSeparator", result);
    }

    /// <summary>
    /// Tests that Sanitize returns an empty string when the separator is at the start of the label.
    /// Input: "##ID"
    /// Expected: ""
    /// </summary>
    [TestMethod]
    public void Sanitize_SeparatorAtStart_ReturnsEmptyString()
    {
        // Arrange
        var input = "##ID";

        // Act
        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that Sanitize removes only the first separator and everything after when multiple separators are present.
    /// Input: "First##Second##Third"
    /// Expected: "First"
    /// </summary>
    [TestMethod]
    public void Sanitize_MultipleSeparators_RemovesFromFirstSeparator()
    {
        // Arrange
        var input = "First##Second##Third";

        // Act
        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("First", result);
    }

    /// <summary>
    /// Tests that Sanitize returns the original label when only a single hash is present (not the ImGui separator).
    /// Input: "Label#ID"
    /// Expected: "Label#ID"
    /// </summary>
    [TestMethod]
    public void Sanitize_SingleHash_ReturnsOriginalLabel()
    {
        // Arrange
        var input = "Label#ID";

        // Act
        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Label#ID", result);
    }

    /// <summary>
    /// Tests that Sanitize returns an empty string when given an empty string input.
    /// Input: ""
    /// Expected: ""
    /// </summary>
    [TestMethod]
    public void Sanitize_EmptyString_ReturnsEmptyString()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that Sanitize returns an empty string when the input is only the separator.
    /// Input: "##"
    /// Expected: ""
    /// </summary>
    [TestMethod]
    public void Sanitize_OnlySeparator_ReturnsEmptyString()
    {
        // Arrange
        var input = "##";

        // Act
        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that Sanitize removes everything from the first ## separator onwards.
    /// Input: "Label###ID"
    /// Expected: "Label"
    /// </summary>
    [TestMethod]
    public void Sanitize_ThreeConsecutiveHashes_RemovesFromFirstSeparator()
    {
        // Arrange
        var input = "Label###ID";

        // Act
        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Label", result);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel is null.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_NullTreeNodeLabel_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns((string?)null);
        mockSection.Setup(s => s.SectionId).Returns("TestSection_NullLabel");

        // Act & Assert
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel does not contain the separator.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_TreeNodeLabelWithoutSeparator_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("ValidLabel");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_ValidLabel");

        // Act & Assert
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel contains the separator in the middle.
    /// Expected: A warning should be logged (cannot verify directly due to static Logger).
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_SeparatorInMiddle_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Label##ID");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_SeparatorInMiddle");

        // Act & Assert
        // Note: Cannot verify Logger.Warning call directly as it's a static method.
        // This test verifies the method executes without throwing.
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when called multiple times with the same section.
    /// Expected: Warning logged only on first call due to HashSet tracking.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_SameSectionCalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Label##DuplicateTest");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_DuplicateCall");

        // Act & Assert
        // Note: Cannot verify that warning is logged only once due to static Logger.
        // This test verifies the method executes without throwing on multiple calls.
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid is thread-safe when called concurrently from multiple threads.
    /// Expected: No exceptions thrown, HashSet tracking should handle concurrent access correctly.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_ConcurrentCalls_ThreadSafe()
    {
        // Arrange
        const int threadCount = 10;
        const int callsPerThread = 100;
        var exceptions = new List<Exception>();
        var exceptionLock = new object();

        var tasks = new Task[threadCount];
        for (var t = 0; t < threadCount; t++)
        {
            var threadIndex = t;
            tasks[t] = Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < callsPerThread; i++)
                    {
                        var mockSection = new Mock<IPanelSection>();
                        mockSection.Setup(s => s.TreeNodeLabel).Returns($"Thread{threadIndex}Label##ID{i}");
                        mockSection.Setup(s => s.SectionId).Returns($"TestSection_Thread{threadIndex}_{i}");

                        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            }, TestContext?.CancellationToken ?? default);
        }

        // Act
        Task.WaitAll(tasks, TestContext?.CancellationToken ?? default);

        // Assert
        Assert.IsEmpty(exceptions, $"Expected no exceptions, but got {exceptions.Count}. First: {(exceptions.Count > 0 ? exceptions[0].ToString() : "N/A")}");
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when multiple threads call with the same section simultaneously.
    /// Expected: No exceptions thrown while the same section-id/label pair is hit concurrently.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_ConcurrentCallsSameSection_DoesNotThrow()
    {
        // Arrange
        const int threadCount = 20;
        var exceptions = new List<Exception>();
        var exceptionLock = new object();
        var barrier = new Barrier(threadCount);

        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("ConcurrentLabel##Shared");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_ConcurrentShared");

        var tasks = new Task[threadCount];
        for (var t = 0; t < threadCount; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                try
                {
                    barrier.SignalAndWait(TestContext?.CancellationToken ?? default);
                    PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            }, TestContext?.CancellationToken ?? default);
        }

        // Act
        Task.WaitAll(tasks, TestContext?.CancellationToken ?? default);

        // Assert
        Assert.IsEmpty(exceptions, $"Expected no exceptions, but got {exceptions.Count}. First: {(exceptions.Count > 0 ? exceptions[0].ToString() : "N/A")}");
    }

    /// <summary>
    /// Tests that labels differing only by an appended panel suffix are sanitized to the same visible text.
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithAppendedSectionSuffix_ReturnsVisiblePrefixOnly()
    {
        var input = "General Settings##PluginConfig";

        var result = PluginPanelTreeNodeLabels.Sanitize(input);

        Assert.AreEqual("General Settings", result);
    }

    /// <summary>
    /// Tests that invalid labels on different sections are handled independently without throwing.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_SameLabelDifferentSectionIds_DoesNotThrow()
    {
        var firstSection = new Mock<IPanelSection>();
        firstSection.Setup(s => s.TreeNodeLabel).Returns("Shared##Label");
        firstSection.Setup(s => s.SectionId).Returns("SectionA");

        var secondSection = new Mock<IPanelSection>();
        secondSection.Setup(s => s.TreeNodeLabel).Returns("Shared##Label");
        secondSection.Setup(s => s.SectionId).Returns("SectionB");

        PluginPanelTreeNodeLabels.WarnIfInvalid(firstSection.Object);
        PluginPanelTreeNodeLabels.WarnIfInvalid(secondSection.Object);
    }

}
