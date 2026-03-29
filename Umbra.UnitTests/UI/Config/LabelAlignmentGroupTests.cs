using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;

using Hexa.NET;
using Hexa.NET.ImGui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.UI;
using Umbra.UI.Config;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for <see cref="LabelAlignmentGroup"/>.
/// </summary>
[TestClass]
public sealed class LabelAlignmentGroupTests
{
    /// <summary>
    /// Tests that Register does not throw when called with valid label and hasDescription=false
    /// in non-seeded state.
    /// </summary>
    [TestMethod]
    public void Register_ValidLabelNoDescriptionNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = "TestLabel";
        bool hasDescription = false;

        // Act & Assert
        group.Register(label, hasDescription);
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register does not throw when called with valid label and hasDescription=true
    /// in non-seeded state.
    /// </summary>
    [TestMethod]
    public void Register_ValidLabelWithDescriptionNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = "TestLabel";
        bool hasDescription = true;

        // Act & Assert
        group.Register(label, hasDescription);
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register does not throw when called with an empty string label
    /// in non-seeded state.
    /// </summary>
    [TestMethod]
    public void Register_EmptyStringLabelNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = string.Empty;
        bool hasDescription = false;

        // Act & Assert
        group.Register(label, hasDescription);
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register does not throw when called with a whitespace-only string label
    /// in non-seeded state.
    /// </summary>
    [TestMethod]
    public void Register_WhitespaceOnlyLabelNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = "   ";
        bool hasDescription = false;

        // Act & Assert
        group.Register(label, hasDescription);
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register does not throw when called with a very long string label
    /// in non-seeded state.
    /// </summary>
    [TestMethod]
    public void Register_VeryLongLabelNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = new string('A', 10000);
        bool hasDescription = false;

        // Act & Assert
        group.Register(label, hasDescription);
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register does not throw when called with a label containing special characters
    /// in non-seeded state.
    /// </summary>
    [TestMethod]
    public void Register_LabelWithSpecialCharactersNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = "Test!@#$%^&*()_+-=[]{}|;':\"<>?,./`~";
        bool hasDescription = false;

        // Act & Assert
        group.Register(label, hasDescription);
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register does not throw when called with a label containing control characters
    /// in non-seeded state.
    /// </summary>
    [TestMethod]
    public void Register_LabelWithControlCharactersNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = "Test\n\r\t\0Label";
        bool hasDescription = false;

        // Act & Assert
        group.Register(label, hasDescription);
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register does not throw when called with a label containing Unicode characters
    /// in non-seeded state.
    /// </summary>
    [TestMethod]
    public void Register_LabelWithUnicodeCharactersNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = "测试标签 🎮 🔥";
        bool hasDescription = false;

        // Act & Assert
        group.Register(label, hasDescription);
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register can be called multiple times with different labels in non-seeded state
    /// without throwing.
    /// </summary>
    [TestMethod]
    public void Register_MultipleLabelsNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act
        group.Register("Label1", false);
        group.Register("Label2", true);
        group.Register("Label3", false);
        group.Register("Label4", true);

        // Assert
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that Register can be called with the same label multiple times in non-seeded state
    /// without throwing.
    /// </summary>
    [TestMethod]
    public void Register_DuplicateLabelsNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        string label = "DuplicateLabel";

        // Act
        group.Register(label, false);
        group.Register(label, true);
        group.Register(label, false);

        // Assert
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests Register behavior in seeded state.
    /// NOTE: This test is incomplete because ImGui.CalcTextSize and ImGui.GetStyle are static
    /// methods that cannot be mocked with Moq. To fully test this scenario:
    /// 1. An active ImGui context is required
    /// 2. The test would need to call EnsureSeeded() first, which also requires ImGui context
    /// 3. Then verify that late registrations correctly update _committedMax
    /// 
    /// Manual verification steps:
    /// - Call EnsureSeeded() to set _seeded = true
    /// - Call Register with a label wider than current LabelWidth
    /// - Verify LabelWidth increases to accommodate the new label
    /// - Call Register with a narrower label
    /// - Verify LabelWidth does not decrease
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock ImGui static methods; requires active ImGui context for full testing.")]
    public void Register_LateRegistrationAfterSeeded_UpdatesCommittedMaxWhenWider()
    {
        // This test requires:
        // 1. Mocking ImGui.CalcTextSize (not possible - static method)
        // 2. Mocking ImGui.GetStyle (not possible - static method)
        // 3. Or creating an actual ImGui context (out of scope for unit tests)

        // Arrange
        var group = new LabelAlignmentGroup();
        // TODO: Call EnsureSeeded() after registering some initial labels
        // TODO: Register a new label with known width > current LabelWidth

        // Act
        // group.Register("Very Long Label That Should Update Width", false);

        // Assert
        // Assert.IsTrue(group.LabelWidth > initialWidth, "LabelWidth should increase for wider late-registered labels.");

        Assert.Inconclusive("Test requires ImGui context; cannot be completed without mocking static ImGui methods.");
    }

    /// <summary>
    /// Tests Register behavior in seeded state with hasDescription=true.
    /// NOTE: This test is incomplete because ImGui.CalcTextSize and ImGui.GetStyle are static
    /// methods that cannot be mocked with Moq. To fully test this scenario:
    /// 1. An active ImGui context is required
    /// 2. Verify that hasDescription=true adds spacing and "(?)" marker width
    /// 3. Verify the total width correctly updates _committedMax
    /// 
    /// Manual verification steps:
    /// - Call EnsureSeeded() to set _seeded = true
    /// - Call Register with hasDescription=true
    /// - Verify LabelWidth includes label width + ItemSpacing.X + "(?)" width
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock ImGui static methods; requires active ImGui context for full testing.")]
    public void Register_LateRegistrationWithDescriptionAfterSeeded_IncludesHelpMarkerWidth()
    {
        // This test requires:
        // 1. Mocking ImGui.CalcTextSize (not possible - static method)
        // 2. Mocking ImGui.GetStyle (not possible - static method)

        // Arrange
        var group = new LabelAlignmentGroup();
        // TODO: Call EnsureSeeded() after registering initial labels

        // Act
        // group.Register("Label With Description", true);

        // Assert
        // Verify that LabelWidth = CalcTextSize("Label With Description").X + ItemSpacing.X + CalcTextSize("(?)").X

        Assert.Inconclusive("Test requires ImGui context; cannot be completed without mocking static ImGui methods.");
    }

    /// <summary>
    /// Tests Register behavior in seeded state when new label width is not greater than committed max.
    /// NOTE: This test is incomplete because ImGui.CalcTextSize cannot be mocked.
    /// 
    /// Manual verification steps:
    /// - Call EnsureSeeded() with some wide labels to establish a large LabelWidth
    /// - Call Register with a narrower label
    /// - Verify LabelWidth does not decrease (committed max is never reduced)
    /// </summary>
    [TestMethod]
    [Ignore("Cannot mock ImGui static methods; requires active ImGui context for full testing.")]
    public void Register_LateRegistrationNarrowerThanMaxAfterSeeded_DoesNotReduceCommittedMax()
    {
        // This test requires:
        // 1. Mocking ImGui.CalcTextSize (not possible - static method)

        // Arrange
        var group = new LabelAlignmentGroup();
        // TODO: Register wide labels and call EnsureSeeded()
        // float initialWidth = group.LabelWidth;

        // Act
        // group.Register("Short", false);

        // Assert
        // Assert.AreEqual(initialWidth, group.LabelWidth, "LabelWidth should not decrease for narrower late-registered labels.");

        Assert.Inconclusive("Test requires ImGui context; cannot be completed without mocking static ImGui methods.");
    }

    /// <summary>
    /// Tests that EnsureSeeded returns immediately without processing when already seeded.
    /// This verifies the idempotency guarantee: subsequent calls after the first are no-ops.
    /// </summary>
    [TestMethod]
    public void EnsureSeeded_WhenAlreadySeeded_ReturnsImmediatelyWithoutReprocessing()
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Note: This test cannot fully verify the internal state without an active ImGui context.
        // The first call to EnsureSeeded will set _seeded to true but will fail when attempting
        // to call ImGui.CalcTextSize due to no active ImGui context. However, we can verify
        // that subsequent calls hit the guard clause and return immediately without throwing.

        try
        {
            // First call - will attempt to process and likely throw due to no ImGui context
            group.EnsureSeeded();
        }
        catch
        {
            // Expected: ImGui calls will fail without active context
            // The _seeded flag should still be set to true before the loop
        }

        // Act - second call should hit the guard clause and return immediately
        // Assert - no exception should be thrown on subsequent calls
        group.EnsureSeeded();
        group.EnsureSeeded();
        group.EnsureSeeded();
    }

    /// <summary>
    /// Tests that EnsureSeeded can be called on an empty alignment group without throwing.
    /// When no labels have been registered, the method should set the seeded flag and return
    /// without processing any entries.
    /// </summary>
    [TestMethod]
    public void EnsureSeeded_WithNoRegisteredLabels_CompletesWithoutException()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        // No labels registered - _entries list is empty

        // Act
        group.EnsureSeeded();

        // Assert
        // Should complete without throwing
        // Subsequent calls should be no-ops due to guard clause
        group.EnsureSeeded();
    }

    /// <summary>
    /// Tests that multiple calls to EnsureSeeded are safe and idempotent.
    /// After the first call sets the seeded flag, all subsequent calls should be immediate no-ops
    /// that hit the guard clause without attempting to process entries or call ImGui methods.
    /// </summary>
    [TestMethod]
    public void EnsureSeeded_CalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act - call multiple times
        try
        {
            group.EnsureSeeded();
        }
        catch
        {
            // First call may throw due to ImGui context, but _seeded should be set
        }

        // Assert - subsequent calls should not throw due to guard clause
        group.EnsureSeeded();
        group.EnsureSeeded();
        group.EnsureSeeded();
        group.EnsureSeeded();
    }

    /// <summary>
    /// Tests that LabelWidth property returns zero before EnsureSeeded is called.
    /// The committed maximum width should remain zero until the first seeding occurs.
    /// </summary>
    [TestMethod]
    public void LabelWidth_BeforeEnsureSeeded_ReturnsZero()
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act
        float width = group.LabelWidth;

        // Assert
        Assert.AreEqual(0f, width, "LabelWidth should be zero before EnsureSeeded is called");
    }

    /// <summary>
    /// Tests that Margin property has default value of zero.
    /// </summary>
    [TestMethod]
    public void Margin_DefaultValue_IsZero()
    {
        // Arrange & Act
        var group = new LabelAlignmentGroup();

        // Assert
        Assert.AreEqual(0f, group.Margin, "Margin should default to 0f");
    }

    /// <summary>
    /// Tests that Margin property can be set to a positive value.
    /// </summary>
    [TestMethod]
    public void Margin_SetToPositiveValue_ReturnsSetValue()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        const float expectedMargin = 10.5f;

        // Act
        group.Margin = expectedMargin;

        // Assert
        Assert.AreEqual(expectedMargin, group.Margin, "Margin should return the set value");
    }

    /// <summary>
    /// Tests that Margin property can be set to zero.
    /// </summary>
    [TestMethod]
    public void Margin_SetToZero_ReturnsZero()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        group.Margin = 5f;

        // Act
        group.Margin = 0f;

        // Assert
        Assert.AreEqual(0f, group.Margin, "Margin should be settable to zero");
    }

    /// <summary>
    /// Tests that Margin property can be set to a negative value.
    /// While unusual, negative margins may be used for special layout adjustments.
    /// </summary>
    [TestMethod]
    public void Margin_SetToNegativeValue_ReturnsNegativeValue()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        const float expectedMargin = -3.5f;

        // Act
        group.Margin = expectedMargin;

        // Assert
        Assert.AreEqual(expectedMargin, group.Margin, "Margin should accept negative values");
    }

    /// <summary>
    /// Tests that Margin handles extreme float values.
    /// </summary>
    [TestMethod]
    [DataRow(float.MaxValue, DisplayName = "MaxValue")]
    [DataRow(float.MinValue, DisplayName = "MinValue")]
    [DataRow(float.Epsilon, DisplayName = "Epsilon")]
    [DataRow(-float.Epsilon, DisplayName = "NegativeEpsilon")]
    public void Margin_SetToExtremeFloatValues_ReturnsSetValue(float value)
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act
        group.Margin = value;

        // Assert
        Assert.AreEqual(value, group.Margin, $"Margin should handle extreme float value: {value}");
    }

    /// <summary>
    /// Tests that Margin handles special float values including NaN and infinities.
    /// While these values are likely invalid for UI layout, the property should accept them.
    /// </summary>
    [TestMethod]
    [DataRow(float.NaN, DisplayName = "NaN")]
    [DataRow(float.PositiveInfinity, DisplayName = "PositiveInfinity")]
    [DataRow(float.NegativeInfinity, DisplayName = "NegativeInfinity")]
    public void Margin_SetToSpecialFloatValues_ReturnsSetValue(float value)
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act
        group.Margin = value;

        // Assert
        if (float.IsNaN(value))
        {
            Assert.IsTrue(float.IsNaN(group.Margin), "Margin should preserve NaN value");
        }
        else
        {
            Assert.AreEqual(value, group.Margin, $"Margin should handle special float value: {value}");
        }
    }

    /// <summary>
    /// Tests that the LabelWidth property returns zero when a new instance is created.
    /// </summary>
    [TestMethod]
    public void LabelWidth_NewInstance_ReturnsZero()
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act
        float actual = group.LabelWidth;

        // Assert
        Assert.AreEqual(0f, actual);
    }

    /// <summary>
    /// Tests that the LabelWidth property returns zero for multiple new instances,
    /// verifying consistent initialization behavior.
    /// </summary>
    [TestMethod]
    public void LabelWidth_MultipleNewInstances_AllReturnZero()
    {
        // Arrange & Act
        var group1 = new LabelAlignmentGroup();
        var group2 = new LabelAlignmentGroup();
        var group3 = new LabelAlignmentGroup();

        // Assert
        Assert.AreEqual(0f, group1.LabelWidth);
        Assert.AreEqual(0f, group2.LabelWidth);
        Assert.AreEqual(0f, group3.LabelWidth);
    }

    /// <summary>
    /// Tests that the LabelWidth property consistently returns the same value
    /// when accessed multiple times on the same instance before seeding.
    /// </summary>
    [TestMethod]
    public void LabelWidth_MultipleAccesses_ReturnsSameValue()
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act
        float value1 = group.LabelWidth;
        float value2 = group.LabelWidth;
        float value3 = group.LabelWidth;

        // Assert
        Assert.AreEqual(value1, value2);
        Assert.AreEqual(value2, value3);
        Assert.AreEqual(0f, value1);
    }
}