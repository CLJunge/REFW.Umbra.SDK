using System;
using System.Numerics;

using Hexa.NET.ImGui;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra.Config.Attributes;
using Umbra.UI.Config;

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
    public void Pop_WhenCalled_DoesNotThrow()
    {
        // Arrange - no setup needed for this static method

        // Act & Assert - verify the method executes without throwing
        try
        {
            ButtonStyleColors.Pop();
            // If we reach here without exception, the test passes
            // Note: Without ImGui context initialization, PopStyleColor may be a no-op
            // or may operate on an uninitialized stack, but it should not throw
        }
        catch (Exception ex)
        {
            Assert.Fail($"Pop() should not throw an exception. Exception: {ex.GetType().Name}: {ex.Message}");
        }
    }

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
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Push with zero vectors (transparent black) returns true.
    /// Verifies the method handles edge case of all-zero color components.
    /// </summary>
    [TestMethod]
    public void Push_ZeroVectors_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = Vector4.Zero;
        Vector4 hovered = Vector4.Zero;
        Vector4 active = Vector4.Zero;

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Push with maximum valid color values (opaque white) returns true.
    /// All components set to 1.0f representing full intensity.
    /// </summary>
    [TestMethod]
    public void Push_MaximumValidVectors_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = Vector4.One;
        Vector4 hovered = Vector4.One;
        Vector4 active = Vector4.One;

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Push handles negative component values without throwing.
    /// Negative values are invalid for colors but technically valid for Vector4.
    /// Verifies the method doesn't crash on invalid color data.
    /// </summary>
    [TestMethod]
    public void Push_NegativeComponents_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(-0.5f, -0.3f, -0.8f, -1.0f);
        Vector4 hovered = new(-1.0f, -1.0f, -1.0f, -1.0f);
        Vector4 active = new(-0.1f, -0.2f, -0.3f, -0.4f);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Push handles component values above 1.0 without throwing.
    /// Values above 1.0 are outside typical color range but valid for Vector4.
    /// </summary>
    [TestMethod]
    public void Push_ComponentsAboveOne_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(1.5f, 2.0f, 3.0f, 10.0f);
        Vector4 hovered = new(100.0f, 200.0f, 300.0f, 400.0f);
        Vector4 active = new(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

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
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Push handles positive infinity components without throwing.
    /// Verifies resilience against extreme floating-point values.
    /// </summary>
    [TestMethod]
    public void Push_PositiveInfinityComponents_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector4 hovered = new(float.PositiveInfinity, 0.5f, 0.5f, 1.0f);
        Vector4 active = new(0.0f, float.PositiveInfinity, 0.0f, 1.0f);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests that Push handles negative infinity components without throwing.
    /// Verifies resilience against extreme negative floating-point values.
    /// </summary>
    [TestMethod]
    public void Push_NegativeInfinityComponents_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        Vector4 hovered = new(float.NegativeInfinity, 0.5f, 0.5f, 1.0f);
        Vector4 active = new(0.0f, float.NegativeInfinity, 0.0f, 1.0f);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests Push with mixed edge cases across the three parameters.
    /// Each parameter uses a different edge case to verify comprehensive handling.
    /// </summary>
    /// <param name="normalX">X component of normal vector.</param>
    /// <param name="normalY">Y component of normal vector.</param>
    /// <param name="normalZ">Z component of normal vector.</param>
    /// <param name="normalW">W component of normal vector.</param>
    /// <param name="hoveredX">X component of hovered vector.</param>
    /// <param name="hoveredY">Y component of hovered vector.</param>
    /// <param name="hoveredZ">Z component of hovered vector.</param>
    /// <param name="hoveredW">W component of hovered vector.</param>
    /// <param name="activeX">X component of active vector.</param>
    /// <param name="activeY">Y component of active vector.</param>
    /// <param name="activeZ">Z component of active vector.</param>
    /// <param name="activeW">W component of active vector.</param>
    [TestMethod]
    [DataRow(0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.5f, 0.5f, 0.5f, 0.5f, DisplayName = "Zero, Max, Mid")]
    [DataRow(-1.0f, -1.0f, -1.0f, -1.0f, 0.5f, 0.5f, 0.5f, 1.0f, 2.0f, 2.0f, 2.0f, 2.0f, DisplayName = "Negative, Valid, AboveOne")]
    [DataRow(0.2f, 0.4f, 0.8f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, DisplayName = "Valid, Zero, Max")]
    [DataRow(float.MinValue, float.MinValue, float.MinValue, float.MinValue, float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue, 0.5f, 0.5f, 0.5f, 1.0f, DisplayName = "Min, Max, Valid")]
    public void Push_MixedEdgeCaseVectors_ReturnsTrue(
        float normalX, float normalY, float normalZ, float normalW,
        float hoveredX, float hoveredY, float hoveredZ, float hoveredW,
        float activeX, float activeY, float activeZ, float activeW)
    {
        // Arrange
        Vector4 normal = new(normalX, normalY, normalZ, normalW);
        Vector4 hovered = new(hoveredX, hoveredY, hoveredZ, hoveredW);
        Vector4 active = new(activeX, activeY, activeZ, activeW);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests Push with minimum float value components.
    /// Verifies handling of extreme boundary values.
    /// </summary>
    [TestMethod]
    public void Push_MinValueComponents_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
        Vector4 hovered = new(float.MinValue, 0.0f, 0.0f, 1.0f);
        Vector4 active = new(0.5f, float.MinValue, 0.5f, 1.0f);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests Push with maximum float value components.
    /// Verifies handling of extreme boundary values.
    /// </summary>
    [TestMethod]
    public void Push_MaxValueComponents_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
        Vector4 hovered = new(float.MaxValue, 0.0f, 0.0f, 1.0f);
        Vector4 active = new(0.5f, float.MaxValue, 0.5f, 1.0f);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests Push with typical semi-transparent color values.
    /// Verifies handling of common use case with alpha channel less than 1.0.
    /// </summary>
    [TestMethod]
    public void Push_SemiTransparentColors_ReturnsTrue()
    {
        // Arrange
        Vector4 normal = new(0.5f, 0.5f, 0.5f, 0.5f);
        Vector4 hovered = new(0.6f, 0.6f, 0.6f, 0.7f);
        Vector4 active = new(0.4f, 0.4f, 0.4f, 0.3f);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    /// Tests Push with grayscale color variations.
    /// Verifies handling of equal RGB components with varying alpha.
    /// </summary>
    [TestMethod]
    [DataRow(0.0f, 1.0f, DisplayName = "Black")]
    [DataRow(0.25f, 1.0f, DisplayName = "DarkGray")]
    [DataRow(0.5f, 1.0f, DisplayName = "MidGray")]
    [DataRow(0.75f, 1.0f, DisplayName = "LightGray")]
    [DataRow(1.0f, 1.0f, DisplayName = "White")]
    [DataRow(0.5f, 0.0f, DisplayName = "TransparentGray")]
    public void Push_GrayscaleColors_ReturnsTrue(float intensity, float alpha)
    {
        // Arrange
        Vector4 normal = new(intensity, intensity, intensity, alpha);
        Vector4 hovered = new(intensity + 0.1f, intensity + 0.1f, intensity + 0.1f, alpha);
        Vector4 active = new(intensity - 0.1f, intensity - 0.1f, intensity - 0.1f, alpha);

        // Act
        bool result = ButtonStyleColors.Push(normal, hovered, active);

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
        ButtonStyle style = ButtonStyle.Primary;

        // Act
        // NOTE: This will attempt to call ImGui.PushStyleColor (static, cannot mock).
        // If ImGui is not initialized in the test environment, this may throw or fail.
        bool result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsTrue(result, "Push should return true for Primary style.");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>true</c>
    /// when invoked with <see cref="ButtonStyle.Success"/>, indicating that style colors
    /// were pushed to the ImGui color stack.
    /// </summary>
    /// <remarks>
    /// NOTE: This test cannot verify that ImGui.PushStyleColor was actually called
    /// because it is a static method that cannot be mocked with Moq. This test only
    /// verifies the return value.
    /// </remarks>
    [TestMethod]
    public void Push_Success_ReturnsTrue()
    {
        // Arrange
        ButtonStyle style = ButtonStyle.Success;

        // Act
        bool result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsTrue(result, "Push should return true for Success style.");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>true</c>
    /// when invoked with <see cref="ButtonStyle.Warning"/>, indicating that style colors
    /// were pushed to the ImGui color stack.
    /// </summary>
    /// <remarks>
    /// NOTE: This test cannot verify that ImGui.PushStyleColor was actually called
    /// because it is a static method that cannot be mocked with Moq. This test only
    /// verifies the return value.
    /// </remarks>
    [TestMethod]
    public void Push_Warning_ReturnsTrue()
    {
        // Arrange
        ButtonStyle style = ButtonStyle.Warning;

        // Act
        bool result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsTrue(result, "Push should return true for Warning style.");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>true</c>
    /// when invoked with <see cref="ButtonStyle.Danger"/>, indicating that style colors
    /// were pushed to the ImGui color stack.
    /// </summary>
    /// <remarks>
    /// NOTE: This test cannot verify that ImGui.PushStyleColor was actually called
    /// because it is a static method that cannot be mocked with Moq. This test only
    /// verifies the return value.
    /// </remarks>
    [TestMethod]
    public void Push_Danger_ReturnsTrue()
    {
        // Arrange
        ButtonStyle style = ButtonStyle.Danger;

        // Act
        bool result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsTrue(result, "Push should return true for Danger style.");
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
        ButtonStyle style = ButtonStyle.Default;

        // Act
        bool result = ButtonStyleColors.Push(style);

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
        ButtonStyle style = ButtonStyle.Custom;

        // Act
        bool result = ButtonStyleColors.Push(style);

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
        ButtonStyle style = (ButtonStyle)999;

        // Act
        bool result = ButtonStyleColors.Push(style);

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
        ButtonStyle style = (ButtonStyle)(-1);

        // Act
        bool result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsFalse(result, "Push should return false for negative enum value.");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>false</c>
    /// when invoked with <see cref="int.MaxValue"/> cast to <see cref="ButtonStyle"/>.
    /// This verifies graceful handling of extreme out-of-range enum values.
    /// </summary>
    [TestMethod]
    public void Push_MaxIntEnumValue_ReturnsFalse()
    {
        // Arrange
        ButtonStyle style = (ButtonStyle)int.MaxValue;

        // Act
        bool result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsFalse(result, "Push should return false for int.MaxValue cast to ButtonStyle.");
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <c>false</c>
    /// when invoked with <see cref="int.MinValue"/> cast to <see cref="ButtonStyle"/>.
    /// This verifies graceful handling of extreme out-of-range enum values.
    /// </summary>
    [TestMethod]
    public void Push_MinIntEnumValue_ReturnsFalse()
    {
        // Arrange
        ButtonStyle style = (ButtonStyle)int.MinValue;

        // Act
        bool result = ButtonStyleColors.Push(style);

        // Assert
        Assert.IsFalse(result, "Push should return false for int.MinValue cast to ButtonStyle.");
    }
}