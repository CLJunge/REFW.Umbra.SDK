using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.UI.Panel;

namespace Umbra.UI.Panel.UnitTests;


/// <summary>
/// Contains unit tests for the <see cref="PluginPanelTreeNodeLabels"/> class.
/// </summary>
[TestClass]
public class PluginPanelTreeNodeLabelsTests
{
    /// <summary>
    /// Tests that Sanitize removes the ImGui separator and everything after it when the separator is in the middle of the label.
    /// Input: "Label##ID"
    /// Expected: "Label"
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithSeparatorInMiddle_RemovesSeparatorAndSuffix()
    {
        // Arrange
        string input = "Label##ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

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
        string input = "NoSeparator";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

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
        string input = "##ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that Sanitize removes the separator and everything after when the separator is at the end of the label.
    /// Input: "Label##"
    /// Expected: "Label"
    /// </summary>
    [TestMethod]
    public void Sanitize_SeparatorAtEnd_RemovesSeparator()
    {
        // Arrange
        string input = "Label##";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Label", result);
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
        string input = "First##Second##Third";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

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
        string input = "Label#ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

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
        string input = string.Empty;

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that Sanitize preserves whitespace in the label before the separator.
    /// Input: " Label With Spaces ##ID"
    /// Expected: " Label With Spaces "
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithWhitespace_PreservesWhitespaceBeforeSeparator()
    {
        // Arrange
        string input = " Label With Spaces ##ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(" Label With Spaces ", result);
    }

    /// <summary>
    /// Tests that Sanitize preserves special characters in the label before the separator.
    /// Input: "Special!@#$%^&*()##ID"
    /// Expected: "Special!@#$%^&*()"
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithSpecialCharacters_PreservesSpecialCharactersBeforeSeparator()
    {
        // Arrange
        string input = "Special!@#$%^&*()##ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Special!@#$%^&*()", result);
    }

    /// <summary>
    /// Tests that Sanitize preserves Unicode characters in the label before the separator.
    /// Input: "Label日本語##ID"
    /// Expected: "Label日本語"
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithUnicodeCharacters_PreservesUnicodeBeforeSeparator()
    {
        // Arrange
        string input = "Label日本語##ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Label日本語", result);
    }

    /// <summary>
    /// Tests that Sanitize handles a very long string without separator correctly.
    /// Input: A 10,000 character string without "##"
    /// Expected: The same 10,000 character string
    /// </summary>
    [TestMethod]
    public void Sanitize_VeryLongStringWithoutSeparator_ReturnsOriginalString()
    {
        // Arrange
        string input = new string('A', 10000);

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(input, result);
    }

    /// <summary>
    /// Tests that Sanitize handles a very long string with separator correctly.
    /// Input: A 10,000 character string followed by "##ID"
    /// Expected: The 10,000 character string without "##ID"
    /// </summary>
    [TestMethod]
    public void Sanitize_VeryLongStringWithSeparator_RemovesSeparatorAndSuffix()
    {
        // Arrange
        string longPrefix = new string('A', 10000);
        string input = longPrefix + "##ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(longPrefix, result);
    }

    /// <summary>
    /// Tests that Sanitize preserves newline characters before the separator.
    /// Input: "Line1\nLine2##ID"
    /// Expected: "Line1\nLine2"
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithNewline_PreservesNewlineBeforeSeparator()
    {
        // Arrange
        string input = "Line1\nLine2##ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Line1\nLine2", result);
    }

    /// <summary>
    /// Tests that Sanitize preserves tab characters before the separator.
    /// Input: "Label\tWithTab##ID"
    /// Expected: "Label\tWithTab"
    /// </summary>
    [TestMethod]
    public void Sanitize_LabelWithTab_PreservesTabBeforeSeparator()
    {
        // Arrange
        string input = "Label\tWithTab##ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Label\tWithTab", result);
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
        string input = "##";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// Tests that Sanitize preserves three consecutive hashes when they don't form a separator pair at the start.
    /// Input: "Label###ID"
    /// Expected: "Label#"
    /// </summary>
    [TestMethod]
    public void Sanitize_ThreeConsecutiveHashes_RemovesFromFirstSeparator()
    {
        // Arrange
        string input = "Label###ID";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("Label#", result);
    }

    /// <summary>
    /// Tests that Sanitize handles whitespace-only labels without separator correctly.
    /// Input: "   "
    /// Expected: "   "
    /// </summary>
    [TestMethod]
    public void Sanitize_WhitespaceOnly_ReturnsWhitespace()
    {
        // Arrange
        string input = "   ";

        // Act
        string result = PluginPanelTreeNodeLabels.Sanitize(input);

        // Assert
        Assert.AreEqual("   ", result);
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
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel is an empty string.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_EmptyTreeNodeLabel_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns(string.Empty);
        mockSection.Setup(s => s.SectionId).Returns("TestSection_EmptyLabel");

        // Act & Assert
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel is whitespace only.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_WhitespaceOnlyTreeNodeLabel_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("   ");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_WhitespaceLabel");

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
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel contains a single '#' but not '##'.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_TreeNodeLabelWithSingleHash_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Label#Value");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_SingleHash");

        // Act & Assert
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel contains the separator at the start.
    /// Expected: A warning should be logged (cannot verify directly due to static Logger).
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_SeparatorAtStart_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("##Label");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_SeparatorAtStart");

        // Act & Assert
        // Note: Cannot verify Logger.Warning call directly as it's a static method.
        // This test verifies the method executes without throwing.
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
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel contains the separator at the end.
    /// Expected: A warning should be logged (cannot verify directly due to static Logger).
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_SeparatorAtEnd_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Label##");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_SeparatorAtEnd");

        // Act & Assert
        // Note: Cannot verify Logger.Warning call directly as it's a static method.
        // This test verifies the method executes without throwing.
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when the section's TreeNodeLabel contains multiple separators.
    /// Expected: A warning should be logged (cannot verify directly due to static Logger).
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_MultipleSeparators_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Label##ID##Other");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_MultipleSeparators");

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
    /// Tests that WarnIfInvalid does not throw when called with different sections having different labels with separators.
    /// Expected: Each unique (SectionId, TreeNodeLabel) combination should log a warning.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_DifferentSectionsWithSeparators_DoesNotThrow()
    {
        // Arrange
        var mockSection1 = new Mock<IPanelSection>();
        mockSection1.Setup(s => s.TreeNodeLabel).Returns("Label1##ID1");
        mockSection1.Setup(s => s.SectionId).Returns("TestSection_Different1");

        var mockSection2 = new Mock<IPanelSection>();
        mockSection2.Setup(s => s.TreeNodeLabel).Returns("Label2##ID2");
        mockSection2.Setup(s => s.SectionId).Returns("TestSection_Different2");

        var mockSection3 = new Mock<IPanelSection>();
        mockSection3.Setup(s => s.TreeNodeLabel).Returns("Label3##ID3");
        mockSection3.Setup(s => s.SectionId).Returns("TestSection_Different3");

        // Act & Assert
        // Note: Cannot verify individual Logger.Warning calls due to static Logger.
        // This test verifies the method executes without throwing for multiple different sections.
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection1.Object);
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection2.Object);
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection3.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when called with the same SectionId but different TreeNodeLabels.
    /// Expected: Each unique (SectionId, TreeNodeLabel) combination should log a warning.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_SameSectionIdDifferentLabels_DoesNotThrow()
    {
        // Arrange
        var mockSection1 = new Mock<IPanelSection>();
        mockSection1.Setup(s => s.TreeNodeLabel).Returns("LabelA##ID");
        mockSection1.Setup(s => s.SectionId).Returns("TestSection_SameId");

        var mockSection2 = new Mock<IPanelSection>();
        mockSection2.Setup(s => s.TreeNodeLabel).Returns("LabelB##ID");
        mockSection2.Setup(s => s.SectionId).Returns("TestSection_SameId");

        // Act & Assert
        // Note: Cannot verify individual Logger.Warning calls due to static Logger.
        // This test verifies the method executes without throwing for different labels on same section ID.
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection1.Object);
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection2.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when called with very long TreeNodeLabel containing separator.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_VeryLongLabelWithSeparator_DoesNotThrow()
    {
        // Arrange
        var longLabel = new string('A', 1000) + "##" + new string('B', 1000);
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns(longLabel);
        mockSection.Setup(s => s.SectionId).Returns("TestSection_VeryLongLabel");

        // Act & Assert
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when called with special characters in TreeNodeLabel containing separator.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_SpecialCharactersWithSeparator_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("Label\t\n\r##ID");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_SpecialChars");

        // Act & Assert
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
        for (int t = 0; t < threadCount; t++)
        {
            int threadIndex = t;
            tasks[t] = Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < callsPerThread; i++)
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
            });
        }

        // Act
        Task.WaitAll(tasks);

        // Assert
        Assert.AreEqual(0, exceptions.Count, $"Expected no exceptions, but got {exceptions.Count}. First: {(exceptions.Count > 0 ? exceptions[0].ToString() : "N/A")}");
    }

    /// <summary>
    /// Tests that WarnIfInvalid is thread-safe when multiple threads call with the same section simultaneously.
    /// Expected: No exceptions thrown, warning should be logged exactly once.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_ConcurrentCallsSameSection_ThreadSafe()
    {
        // Arrange
        const int threadCount = 50;
        var exceptions = new List<Exception>();
        var exceptionLock = new object();
        var barrier = new Barrier(threadCount);

        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("ConcurrentLabel##Shared");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_ConcurrentShared");

        var tasks = new Task[threadCount];
        for (int t = 0; t < threadCount; t++)
        {
            tasks[t] = Task.Run(() =>
            {
                try
                {
                    barrier.SignalAndWait();
                    PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
                }
                catch (Exception ex)
                {
                    lock (exceptionLock)
                    {
                        exceptions.Add(ex);
                    }
                }
            });
        }

        // Act
        Task.WaitAll(tasks);

        // Assert
        Assert.AreEqual(0, exceptions.Count, $"Expected no exceptions, but got {exceptions.Count}. First: {(exceptions.Count > 0 ? exceptions[0].ToString() : "N/A")}");
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when TreeNodeLabel contains Unicode characters with separator.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_UnicodeCharactersWithSeparator_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("日本語ラベル##IDユニコード");
        mockSection.Setup(s => s.SectionId).Returns("TestSection_Unicode");

        // Act & Assert
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }

    /// <summary>
    /// Tests that WarnIfInvalid does not throw when SectionId contains separator-like patterns.
    /// The separator should only be detected in TreeNodeLabel, not SectionId.
    /// </summary>
    [TestMethod]
    public void WarnIfInvalid_SectionIdWithSeparatorPattern_DoesNotThrow()
    {
        // Arrange
        var mockSection = new Mock<IPanelSection>();
        mockSection.Setup(s => s.TreeNodeLabel).Returns("ValidLabel##ID");
        mockSection.Setup(s => s.SectionId).Returns("TestSection##WithSeparator");

        // Act & Assert
        // Note: The separator in SectionId doesn't affect the warning logic,
        // which only checks TreeNodeLabel.
        PluginPanelTreeNodeLabels.WarnIfInvalid(mockSection.Object);
    }
}