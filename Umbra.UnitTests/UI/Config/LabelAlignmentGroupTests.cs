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
        var label = "TestLabel";
        var hasDescription = false;

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
    /// Tests that Register accepts an empty label in non-seeded state without changing the committed width.
    /// </summary>
    [TestMethod]
    public void Register_EmptyLabelNotSeeded_DoesNotThrow()
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act
        group.Register(string.Empty, false);

        // Assert
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 before the first seeding pass.");
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
        var label = "DuplicateLabel";

        // Act
        group.Register(label, false);
        group.Register(label, true);
        group.Register(label, false);

        // Assert
        Assert.AreEqual(0f, group.LabelWidth, "LabelWidth should remain 0 in non-seeded state.");
    }

    /// <summary>
    /// Tests that EnsureSeeded returns immediately without processing when already seeded.
    /// This verifies the idempotency guarantee: subsequent calls after seeding are no-ops.
    /// </summary>
    [TestMethod]
    public void EnsureSeeded_WhenAlreadySeeded_ReturnsImmediatelyWithoutReprocessing()
    {
        // Arrange
        var group = new LabelAlignmentGroup();
        SetSeeded(group);

        // Act & Assert
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
    /// Tests that LabelWidth property returns zero before EnsureSeeded is called.
    /// The committed maximum width should remain zero until the first seeding occurs.
    /// </summary>
    [TestMethod]
    public void LabelWidth_BeforeEnsureSeeded_ReturnsZero()
    {
        // Arrange
        var group = new LabelAlignmentGroup();

        // Act
        var width = group.LabelWidth;

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
    /// Tests that Margin returns the most recently assigned value.
    /// </summary>
    [TestMethod]
    public void Margin_SetMultipleTimes_ReturnsLatestValue()
    {
        // Arrange
        var group = new LabelAlignmentGroup
        {
            // Act
            Margin = 4f
        };
        group.Margin = -2f;

        // Assert
        Assert.AreEqual(-2f, group.Margin, "Margin should reflect the latest assignment.");
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
        var group = new LabelAlignmentGroup
        {
            // Act
            Margin = value
        };

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
        var group = new LabelAlignmentGroup
        {
            // Act
            Margin = value
        };

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
        var actual = group.LabelWidth;

        // Assert
        Assert.AreEqual(0f, actual);
    }

    /// <summary>
    /// Marks a <see cref="LabelAlignmentGroup"/> instance as already seeded so the idempotency
    /// behavior can be exercised without requiring a live ImGui context.
    /// </summary>
    private static void SetSeeded(LabelAlignmentGroup group)
    {
        var seededField = typeof(LabelAlignmentGroup).GetField("_seeded", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.IsNotNull(seededField, "Expected the _seeded field to exist for test setup.");
        seededField.SetValue(group, true);
    }

}
