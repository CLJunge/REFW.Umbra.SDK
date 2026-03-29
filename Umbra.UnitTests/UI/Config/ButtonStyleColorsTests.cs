using System.Numerics;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.UnitTests;


/// <summary>
/// Unit tests for the <see cref="ButtonStyleColors"/> class.
/// </summary>
[TestClass]
public class ButtonStyleColorsTests
{
    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Pop"/> executes without throwing an exception.
    /// Note: This test has limited coverage because Pop() delegates to the static ImGui.PopStyleColor method,
    /// which cannot be mocked with Moq. The test verifies the method can be invoked, but cannot verify
    /// the internal ImGui state changes. In a real scenario, Pop should only be called after a successful
    /// Push operation that returned true, which would have pushed 3 colors onto the ImGui style stack.
    /// </summary>
    [TestMethod]
    public void Pop_WhenCalled_DoesNotThrow() =>
        // Arrange - no setup needed for this static method

        // Act - method should execute without throwing
        ButtonStyleColors.Pop();// Assert - if we reach here without exception, the test passes// Note: Without ImGui context initialization, PopStyleColor may be a no-op// or may operate on an uninitialized stack, but it should not throw

    /// <summary>
    /// Tests that Push with typical valid RGBA color values returns true.
    /// Note: Cannot verify ImGui.PushStyleColor calls due to static method mocking limitations with Moq.
    /// Full verification requires integration testing or runtime inspection.
    /// </summary>
    [TestMethod]
    public void Push_ValidColorVectors_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(0.20f, 0.45f, 0.80f, 1.0f);
        Vector4 hovered = new(0.30f, 0.55f, 0.90f, 1.0f);
        Vector4 active = new(0.15f, 0.38f, 0.72f, 1.0f);

        // Act
        var result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Push handles NaN (Not-a-Number) components without throwing.
    /// Verifies resilience against special floating-point values.
    /// </summary>
    [TestMethod]
    public void Push_NaNComponents_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(float.NaN, float.NaN, float.NaN, float.NaN);
        Vector4 hovered = new(0.5f, float.NaN, 0.5f, 1.0f);
        Vector4 active = new(float.NaN, 0.0f, 0.0f, 1.0f);

        // Act
        var result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>true</c>
    /// when invoked with <see cref="ButtonStyle.Primary"/>, indicating that style colors
    /// were pushed to the ImGui color stack.
    /// </summary>
    /// <remarks>
    /// NOTE: This test cannot verify that ImGui.PushStyleColor was actually called
    /// because it is a static method that cannot be mocked with Moq. This test only
    /// verifies the return value. In a production environment, ImGui must be initialized
    /// for the actual PushStyleColor calls to succeed.
    /// </remarks>
    [TestMethod]
    public void Push_Primary_ReturnsTrue()
    {
        // Arrange
        var style = ButtonStyle.Primary;

        // Act
        // NOTE: This will attempt to call ImGui.PushStyleColor (static, cannot mock).
        // If ImGui is not initialized in the test environment, this may throw or fail.
        var result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsTrue(result, "Push should return true for Primary style.");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>false</c>
    /// when invoked with <see cref="ButtonStyle.Default"/>, which is not present in the
    /// internal color dictionary. This ensures the method correctly handles styles that
    /// should use the default ImGui theme colors.
    /// </summary>
    [TestMethod]
    public void Push_Default_ReturnsFalse()
    {
        // Arrange
        var style = ButtonStyle.Default;

        // Act
        var result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsFalse(result, "Push should return false for Default style (not in color dictionary).");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>false</c>
    /// when invoked with <see cref="ButtonStyle.Custom"/>, which is not present in the
    /// internal color dictionary. Custom styles are handled separately via custom color attributes.
    /// </summary>
    [TestMethod]
    public void Push_Custom_ReturnsFalse()
    {
        // Arrange
        var style = ButtonStyle.Custom;

        // Act
        var result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsFalse(result, "Push should return false for Custom style (not in color dictionary).");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>false</c>
    /// when invoked with an undefined enum value outside the declared range.
    /// This verifies graceful handling of invalid enum values.
    /// </summary>
    [TestMethod]
    public void Push_UndefinedEnumValue_ReturnsFalse()
    {
        // Arrange
        var style = (ButtonStyle)999;

        // Act
        var result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsFalse(result, "Push should return false for undefined enum value.");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>false</c>
    /// when invoked with a negative enum value cast from -1.
    /// This verifies graceful handling of invalid negative enum values.
    /// </summary>
    [TestMethod]
    public void Push_NegativeEnumValue_ReturnsFalse()
    {
        // Arrange
        var style = (ButtonStyle)(-1);

        // Act
        var result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsFalse(result, "Push should return false for negative enum value.");
    }

}
