using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbra.Config.Attributes;

namespace Umbra.Config.Attributes.UnitTests;


/// <summary>
/// Tests for <see cref="UmbraCustomButtonColorsAttribute"/> constructor with 12 explicit RGBA parameters.
/// </summary>
[TestClass]
public sealed class UmbraCustomButtonColorsAttributeTests
{
    /// <summary>
    /// Tests that the 12-parameter constructor correctly assigns typical valid RGBA values to all properties.
    /// Input: Standard RGBA values in the 0-1 range for all three button states.
    /// Expected: All properties reflect the exact input values.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidRgbaValues_AssignsAllPropertiesCorrectly()
    {
        // Arrange
        float normalR = 0.5f, normalG = 0.6f, normalB = 0.7f, normalA = 1.0f;
        float hoveredR = 0.6f, hoveredG = 0.7f, hoveredB = 0.8f, hoveredA = 1.0f;
        float activeR = 0.4f, activeG = 0.5f, activeB = 0.6f, activeA = 1.0f;

        // Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            normalR, normalG, normalB, normalA,
            hoveredR, hoveredG, hoveredB, hoveredA,
            activeR, activeG, activeB, activeA);

        // Assert
        Assert.AreEqual(normalR, attribute.NormalR);
        Assert.AreEqual(normalG, attribute.NormalG);
        Assert.AreEqual(normalB, attribute.NormalB);
        Assert.AreEqual(normalA, attribute.NormalA);
        Assert.AreEqual(hoveredR, attribute.HoveredR);
        Assert.AreEqual(hoveredG, attribute.HoveredG);
        Assert.AreEqual(hoveredB, attribute.HoveredB);
        Assert.AreEqual(hoveredA, attribute.HoveredA);
        Assert.AreEqual(activeR, attribute.ActiveR);
        Assert.AreEqual(activeG, attribute.ActiveG);
        Assert.AreEqual(activeB, attribute.ActiveB);
        Assert.AreEqual(activeA, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor accepts and stores zero values for all color channels.
    /// Input: All parameters set to 0.0f.
    /// Expected: All properties are 0.0f.
    /// </summary>
    [TestMethod]
    public void Constructor_ZeroValues_AssignsAllPropertiesCorrectly()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            0f, 0f, 0f, 0f,
            0f, 0f, 0f, 0f,
            0f, 0f, 0f, 0f);

        // Assert
        Assert.AreEqual(0f, attribute.NormalR);
        Assert.AreEqual(0f, attribute.NormalG);
        Assert.AreEqual(0f, attribute.NormalB);
        Assert.AreEqual(0f, attribute.NormalA);
        Assert.AreEqual(0f, attribute.HoveredR);
        Assert.AreEqual(0f, attribute.HoveredG);
        Assert.AreEqual(0f, attribute.HoveredB);
        Assert.AreEqual(0f, attribute.HoveredA);
        Assert.AreEqual(0f, attribute.ActiveR);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0f, attribute.ActiveB);
        Assert.AreEqual(0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor accepts and stores negative values for all color channels.
    /// Input: All parameters set to negative values.
    /// Expected: All properties store the negative values without clamping.
    /// </summary>
    [TestMethod]
    public void Constructor_NegativeValues_AssignsAllPropertiesCorrectly()
    {
        // Arrange
        float normalR = -0.5f, normalG = -0.6f, normalB = -0.7f, normalA = -1.0f;
        float hoveredR = -0.2f, hoveredG = -0.3f, hoveredB = -0.4f, hoveredA = -0.5f;
        float activeR = -1.5f, activeG = -2.0f, activeB = -3.0f, activeA = -0.1f;

        // Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            normalR, normalG, normalB, normalA,
            hoveredR, hoveredG, hoveredB, hoveredA,
            activeR, activeG, activeB, activeA);

        // Assert
        Assert.AreEqual(normalR, attribute.NormalR);
        Assert.AreEqual(normalG, attribute.NormalG);
        Assert.AreEqual(normalB, attribute.NormalB);
        Assert.AreEqual(normalA, attribute.NormalA);
        Assert.AreEqual(hoveredR, attribute.HoveredR);
        Assert.AreEqual(hoveredG, attribute.HoveredG);
        Assert.AreEqual(hoveredB, attribute.HoveredB);
        Assert.AreEqual(hoveredA, attribute.HoveredA);
        Assert.AreEqual(activeR, attribute.ActiveR);
        Assert.AreEqual(activeG, attribute.ActiveG);
        Assert.AreEqual(activeB, attribute.ActiveB);
        Assert.AreEqual(activeA, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor accepts and stores values greater than 1 for all color channels.
    /// Input: All parameters set to values greater than 1.
    /// Expected: All properties store values greater than 1 without clamping.
    /// </summary>
    [TestMethod]
    public void Constructor_ValuesGreaterThanOne_AssignsAllPropertiesCorrectly()
    {
        // Arrange
        float normalR = 1.5f, normalG = 2.0f, normalB = 3.5f, normalA = 10.0f;
        float hoveredR = 1.1f, hoveredG = 1.2f, hoveredB = 1.3f, hoveredA = 1.4f;
        float activeR = 5.0f, activeG = 6.0f, activeB = 7.0f, activeA = 8.0f;

        // Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            normalR, normalG, normalB, normalA,
            hoveredR, hoveredG, hoveredB, hoveredA,
            activeR, activeG, activeB, activeA);

        // Assert
        Assert.AreEqual(normalR, attribute.NormalR);
        Assert.AreEqual(normalG, attribute.NormalG);
        Assert.AreEqual(normalB, attribute.NormalB);
        Assert.AreEqual(normalA, attribute.NormalA);
        Assert.AreEqual(hoveredR, attribute.HoveredR);
        Assert.AreEqual(hoveredG, attribute.HoveredG);
        Assert.AreEqual(hoveredB, attribute.HoveredB);
        Assert.AreEqual(hoveredA, attribute.HoveredA);
        Assert.AreEqual(activeR, attribute.ActiveR);
        Assert.AreEqual(activeG, attribute.ActiveG);
        Assert.AreEqual(activeB, attribute.ActiveB);
        Assert.AreEqual(activeA, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor accepts and stores float.MinValue and float.MaxValue.
    /// Input: Alternating float.MinValue and float.MaxValue across parameters.
    /// Expected: All properties store the extreme values correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_FloatMinMaxValues_AssignsAllPropertiesCorrectly()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            float.MinValue, float.MaxValue, float.MinValue, float.MaxValue,
            float.MinValue, float.MaxValue, float.MinValue, float.MaxValue,
            float.MinValue, float.MaxValue, float.MinValue, float.MaxValue);

        // Assert
        Assert.AreEqual(float.MinValue, attribute.NormalR);
        Assert.AreEqual(float.MaxValue, attribute.NormalG);
        Assert.AreEqual(float.MinValue, attribute.NormalB);
        Assert.AreEqual(float.MaxValue, attribute.NormalA);
        Assert.AreEqual(float.MinValue, attribute.HoveredR);
        Assert.AreEqual(float.MaxValue, attribute.HoveredG);
        Assert.AreEqual(float.MinValue, attribute.HoveredB);
        Assert.AreEqual(float.MaxValue, attribute.HoveredA);
        Assert.AreEqual(float.MinValue, attribute.ActiveR);
        Assert.AreEqual(float.MaxValue, attribute.ActiveG);
        Assert.AreEqual(float.MinValue, attribute.ActiveB);
        Assert.AreEqual(float.MaxValue, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor accepts and stores float.NaN values.
    /// Input: All parameters set to float.NaN.
    /// Expected: All properties store NaN (verified using float.IsNaN).
    /// </summary>
    [TestMethod]
    public void Constructor_NaNValues_AssignsAllPropertiesCorrectly()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            float.NaN, float.NaN, float.NaN, float.NaN,
            float.NaN, float.NaN, float.NaN, float.NaN,
            float.NaN, float.NaN, float.NaN, float.NaN);

        // Assert
        Assert.IsTrue(float.IsNaN(attribute.NormalR));
        Assert.IsTrue(float.IsNaN(attribute.NormalG));
        Assert.IsTrue(float.IsNaN(attribute.NormalB));
        Assert.IsTrue(float.IsNaN(attribute.NormalA));
        Assert.IsTrue(float.IsNaN(attribute.HoveredR));
        Assert.IsTrue(float.IsNaN(attribute.HoveredG));
        Assert.IsTrue(float.IsNaN(attribute.HoveredB));
        Assert.IsTrue(float.IsNaN(attribute.HoveredA));
        Assert.IsTrue(float.IsNaN(attribute.ActiveR));
        Assert.IsTrue(float.IsNaN(attribute.ActiveG));
        Assert.IsTrue(float.IsNaN(attribute.ActiveB));
        Assert.IsTrue(float.IsNaN(attribute.ActiveA));
    }

    /// <summary>
    /// Tests that the constructor accepts and stores positive and negative infinity values.
    /// Input: Alternating float.PositiveInfinity and float.NegativeInfinity.
    /// Expected: All properties store the infinity values correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_InfinityValues_AssignsAllPropertiesCorrectly()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity,
            float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity,
            float.PositiveInfinity, float.NegativeInfinity, float.PositiveInfinity, float.NegativeInfinity);

        // Assert
        Assert.AreEqual(float.PositiveInfinity, attribute.NormalR);
        Assert.AreEqual(float.NegativeInfinity, attribute.NormalG);
        Assert.AreEqual(float.PositiveInfinity, attribute.NormalB);
        Assert.AreEqual(float.NegativeInfinity, attribute.NormalA);
        Assert.AreEqual(float.PositiveInfinity, attribute.HoveredR);
        Assert.AreEqual(float.NegativeInfinity, attribute.HoveredG);
        Assert.AreEqual(float.PositiveInfinity, attribute.HoveredB);
        Assert.AreEqual(float.NegativeInfinity, attribute.HoveredA);
        Assert.AreEqual(float.PositiveInfinity, attribute.ActiveR);
        Assert.AreEqual(float.NegativeInfinity, attribute.ActiveG);
        Assert.AreEqual(float.PositiveInfinity, attribute.ActiveB);
        Assert.AreEqual(float.NegativeInfinity, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor correctly handles a mixed combination of edge case values.
    /// Input: Mix of valid, zero, negative, NaN, and infinity values across different states.
    /// Expected: Each property stores its corresponding input value without interference.
    /// </summary>
    [TestMethod]
    public void Constructor_MixedEdgeCaseValues_AssignsAllPropertiesCorrectly()
    {
        // Arrange
        float normalR = 0.5f, normalG = 0f, normalB = -1.0f, normalA = 2.0f;
        float hoveredR = float.NaN, hoveredG = float.PositiveInfinity, hoveredB = float.NegativeInfinity, hoveredA = float.MinValue;
        float activeR = float.MaxValue, activeG = -0.5f, activeB = 1.5f, activeA = 0.0f;

        // Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            normalR, normalG, normalB, normalA,
            hoveredR, hoveredG, hoveredB, hoveredA,
            activeR, activeG, activeB, activeA);

        // Assert
        Assert.AreEqual(normalR, attribute.NormalR);
        Assert.AreEqual(normalG, attribute.NormalG);
        Assert.AreEqual(normalB, attribute.NormalB);
        Assert.AreEqual(normalA, attribute.NormalA);
        Assert.IsTrue(float.IsNaN(attribute.HoveredR));
        Assert.AreEqual(float.PositiveInfinity, attribute.HoveredG);
        Assert.AreEqual(float.NegativeInfinity, attribute.HoveredB);
        Assert.AreEqual(float.MinValue, attribute.HoveredA);
        Assert.AreEqual(float.MaxValue, attribute.ActiveR);
        Assert.AreEqual(activeG, attribute.ActiveG);
        Assert.AreEqual(activeB, attribute.ActiveB);
        Assert.AreEqual(activeA, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor correctly assigns boundary values at exactly 0 and 1.
    /// Input: All parameters at exact 0.0f or 1.0f boundaries.
    /// Expected: Properties store the exact boundary values.
    /// </summary>
    [TestMethod]
    public void Constructor_BoundaryValues_AssignsAllPropertiesCorrectly()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(
            0f, 1f, 0f, 1f,
            1f, 0f, 1f, 0f,
            0.5f, 0.5f, 0.5f, 0.5f);

        // Assert
        Assert.AreEqual(0f, attribute.NormalR);
        Assert.AreEqual(1f, attribute.NormalG);
        Assert.AreEqual(0f, attribute.NormalB);
        Assert.AreEqual(1f, attribute.NormalA);
        Assert.AreEqual(1f, attribute.HoveredR);
        Assert.AreEqual(0f, attribute.HoveredG);
        Assert.AreEqual(1f, attribute.HoveredB);
        Assert.AreEqual(0f, attribute.HoveredA);
        Assert.AreEqual(0.5f, attribute.ActiveR);
        Assert.AreEqual(0.5f, attribute.ActiveG);
        Assert.AreEqual(0.5f, attribute.ActiveB);
        Assert.AreEqual(0.5f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with RGB parameters correctly sets all properties
    /// with typical values in the standard [0, 1] range.
    /// </summary>
    [TestMethod]
    public void Constructor_WithTypicalValues_SetsAllPropertiesCorrectly()
    {
        // Arrange
        float r = 0.5f;
        float g = 0.5f;
        float b = 0.5f;

        // Act
        var attribute = new UmbraCustomButtonColorsAttribute(r, g, b);

        // Assert
        Assert.AreEqual(0.5f, attribute.NormalR);
        Assert.AreEqual(0.5f, attribute.NormalG);
        Assert.AreEqual(0.5f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(0.6f, attribute.HoveredR, 0.0001f);
        Assert.AreEqual(0.6f, attribute.HoveredG, 0.0001f);
        Assert.AreEqual(0.6f, attribute.HoveredB, 0.0001f);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(0.42f, attribute.ActiveR, 0.0001f);
        Assert.AreEqual(0.42f, attribute.ActiveG, 0.0001f);
        Assert.AreEqual(0.42f, attribute.ActiveB, 0.0001f);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with zero RGB values sets Normal to zero,
    /// Hovered to 0.1, and Active to zero (clamped from -0.08).
    /// </summary>
    [TestMethod]
    public void Constructor_WithZeroValues_SetsNormalToZeroAndDerivedValuesCorrectly()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(0f, 0f, 0f);

        // Assert
        Assert.AreEqual(0f, attribute.NormalR);
        Assert.AreEqual(0f, attribute.NormalG);
        Assert.AreEqual(0f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(0.1f, attribute.HoveredR, 0.0001f);
        Assert.AreEqual(0.1f, attribute.HoveredG, 0.0001f);
        Assert.AreEqual(0.1f, attribute.HoveredB, 0.0001f);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(0f, attribute.ActiveR);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with maximum RGB values (1.0) sets Normal to one,
    /// Hovered to one (clamped from 1.1), and Active to 0.92.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMaxValues_SetsNormalToOneAndClampsHovered()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(1f, 1f, 1f);

        // Assert
        Assert.AreEqual(1f, attribute.NormalR);
        Assert.AreEqual(1f, attribute.NormalG);
        Assert.AreEqual(1f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(1f, attribute.HoveredR);
        Assert.AreEqual(1f, attribute.HoveredG);
        Assert.AreEqual(1f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(0.92f, attribute.ActiveR, 0.0001f);
        Assert.AreEqual(0.92f, attribute.ActiveG, 0.0001f);
        Assert.AreEqual(0.92f, attribute.ActiveB, 0.0001f);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with negative RGB values stores them in Normal
    /// and clamps Hovered and Active to zero.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNegativeValues_SetsNormalAndClampsHoveredAndActive()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(-0.5f, -0.5f, -0.5f);

        // Assert
        Assert.AreEqual(-0.5f, attribute.NormalR);
        Assert.AreEqual(-0.5f, attribute.NormalG);
        Assert.AreEqual(-0.5f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(0f, attribute.HoveredR);
        Assert.AreEqual(0f, attribute.HoveredG);
        Assert.AreEqual(0f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(0f, attribute.ActiveR);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with RGB values above one stores them in Normal
    /// and clamps Hovered to one and Active to one.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValuesAboveOne_SetsNormalAndClampsHoveredAndActive()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(1.5f, 1.5f, 1.5f);

        // Assert
        Assert.AreEqual(1.5f, attribute.NormalR);
        Assert.AreEqual(1.5f, attribute.NormalG);
        Assert.AreEqual(1.5f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(1f, attribute.HoveredR);
        Assert.AreEqual(1f, attribute.HoveredG);
        Assert.AreEqual(1f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(1f, attribute.ActiveR);
        Assert.AreEqual(1f, attribute.ActiveG);
        Assert.AreEqual(1f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with high values near one (0.95) correctly
    /// clamps Hovered to one when adding 0.10 exceeds the upper bound.
    /// </summary>
    [TestMethod]
    public void Constructor_WithHighValuesNearOne_ClampsHoveredToOne()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(0.95f, 0.95f, 0.95f);

        // Assert
        Assert.AreEqual(0.95f, attribute.NormalR);
        Assert.AreEqual(0.95f, attribute.NormalG);
        Assert.AreEqual(0.95f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(1f, attribute.HoveredR);
        Assert.AreEqual(1f, attribute.HoveredG);
        Assert.AreEqual(1f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(0.87f, attribute.ActiveR, 0.0001f);
        Assert.AreEqual(0.87f, attribute.ActiveG, 0.0001f);
        Assert.AreEqual(0.87f, attribute.ActiveB, 0.0001f);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with low values near zero (0.05) correctly
    /// clamps Active to zero when subtracting 0.08 goes below the lower bound.
    /// </summary>
    [TestMethod]
    public void Constructor_WithLowValuesNearZero_ClampsActiveToZero()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(0.05f, 0.05f, 0.05f);

        // Assert
        Assert.AreEqual(0.05f, attribute.NormalR);
        Assert.AreEqual(0.05f, attribute.NormalG);
        Assert.AreEqual(0.05f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(0.15f, attribute.HoveredR, 0.0001f);
        Assert.AreEqual(0.15f, attribute.HoveredG, 0.0001f);
        Assert.AreEqual(0.15f, attribute.HoveredB, 0.0001f);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(0f, attribute.ActiveR);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with NaN values propagates NaN through
    /// Normal, Hovered, and Active channel calculations.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNaN_PropagatesNaNThroughAllStates()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(float.NaN, float.NaN, float.NaN);

        // Assert
        Assert.IsTrue(float.IsNaN(attribute.NormalR));
        Assert.IsTrue(float.IsNaN(attribute.NormalG));
        Assert.IsTrue(float.IsNaN(attribute.NormalB));
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.IsTrue(float.IsNaN(attribute.HoveredR));
        Assert.IsTrue(float.IsNaN(attribute.HoveredG));
        Assert.IsTrue(float.IsNaN(attribute.HoveredB));
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.IsTrue(float.IsNaN(attribute.ActiveR));
        Assert.IsTrue(float.IsNaN(attribute.ActiveG));
        Assert.IsTrue(float.IsNaN(attribute.ActiveB));
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with positive infinity values stores infinity in Normal
    /// and clamps Hovered and Active to one.
    /// </summary>
    [TestMethod]
    public void Constructor_WithPositiveInfinity_ClampsHoveredAndActiveToOne()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

        // Assert
        Assert.AreEqual(float.PositiveInfinity, attribute.NormalR);
        Assert.AreEqual(float.PositiveInfinity, attribute.NormalG);
        Assert.AreEqual(float.PositiveInfinity, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(1f, attribute.HoveredR);
        Assert.AreEqual(1f, attribute.HoveredG);
        Assert.AreEqual(1f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(1f, attribute.ActiveR);
        Assert.AreEqual(1f, attribute.ActiveG);
        Assert.AreEqual(1f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor with negative infinity values stores negative infinity in Normal
    /// and clamps Hovered and Active to zero.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNegativeInfinity_ClampsHoveredAndActiveToZero()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        // Assert
        Assert.AreEqual(float.NegativeInfinity, attribute.NormalR);
        Assert.AreEqual(float.NegativeInfinity, attribute.NormalG);
        Assert.AreEqual(float.NegativeInfinity, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        Assert.AreEqual(0f, attribute.HoveredR);
        Assert.AreEqual(0f, attribute.HoveredG);
        Assert.AreEqual(0f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(0f, attribute.ActiveR);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests that the constructor handles different edge case values independently
    /// for each RGB channel, demonstrating correct per-channel clamping behavior.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMixedEdgeCases_HandlesEachChannelIndependently()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(0.95f, 0.05f, -0.5f);

        // Assert - Normal state stores input as-is
        Assert.AreEqual(0.95f, attribute.NormalR);
        Assert.AreEqual(0.05f, attribute.NormalG);
        Assert.AreEqual(-0.5f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        // Assert - Hovered: +0.10 with clamping
        Assert.AreEqual(1f, attribute.HoveredR); // 0.95 + 0.10 = 1.05 -> clamped to 1
        Assert.AreEqual(0.15f, attribute.HoveredG, 0.0001f); // 0.05 + 0.10 = 0.15
        Assert.AreEqual(0f, attribute.HoveredB); // -0.5 + 0.10 = -0.4 -> clamped to 0
        Assert.AreEqual(1.0f, attribute.HoveredA);

        // Assert - Active: -0.08 with clamping
        Assert.AreEqual(0.87f, attribute.ActiveR, 0.0001f); // 0.95 - 0.08 = 0.87
        Assert.AreEqual(0f, attribute.ActiveG); // 0.05 - 0.08 = -0.03 -> clamped to 0
        Assert.AreEqual(0f, attribute.ActiveB); // -0.5 - 0.08 = -0.58 -> clamped to 0
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests the constructor with exact boundary values to verify precise clamping behavior
    /// at the [0, 1] range boundaries.
    /// </summary>
    [TestMethod]
    public void Constructor_WithExactBoundaryValues_HandlesBoundariesCorrectly()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(1.0f, 0.0f, 0.5f);

        // Assert - Normal state
        Assert.AreEqual(1.0f, attribute.NormalR);
        Assert.AreEqual(0.0f, attribute.NormalG);
        Assert.AreEqual(0.5f, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        // Assert - Hovered state
        Assert.AreEqual(1.0f, attribute.HoveredR); // 1.0 + 0.10 = 1.1 -> clamped to 1.0
        Assert.AreEqual(0.1f, attribute.HoveredG, 0.0001f); // 0.0 + 0.10 = 0.1
        Assert.AreEqual(0.6f, attribute.HoveredB, 0.0001f); // 0.5 + 0.10 = 0.6
        Assert.AreEqual(1.0f, attribute.HoveredA);

        // Assert - Active state
        Assert.AreEqual(0.92f, attribute.ActiveR, 0.0001f); // 1.0 - 0.08 = 0.92
        Assert.AreEqual(0.0f, attribute.ActiveG); // 0.0 - 0.08 = -0.08 -> clamped to 0
        Assert.AreEqual(0.42f, attribute.ActiveB, 0.0001f); // 0.5 - 0.08 = 0.42
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests the constructor with the minimum float value to verify behavior
    /// with extreme negative values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMinValue_ClampsHoveredAndActiveToZero()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(float.MinValue, float.MinValue, float.MinValue);

        // Assert - Normal state stores min value
        Assert.AreEqual(float.MinValue, attribute.NormalR);
        Assert.AreEqual(float.MinValue, attribute.NormalG);
        Assert.AreEqual(float.MinValue, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        // Assert - Hovered and Active clamp to 0
        Assert.AreEqual(0f, attribute.HoveredR);
        Assert.AreEqual(0f, attribute.HoveredG);
        Assert.AreEqual(0f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(0f, attribute.ActiveR);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests the constructor with the maximum float value to verify behavior
    /// with extreme positive values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMaxValue_ClampsHoveredAndActiveToOne()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(float.MaxValue, float.MaxValue, float.MaxValue);

        // Assert - Normal state stores max value
        Assert.AreEqual(float.MaxValue, attribute.NormalR);
        Assert.AreEqual(float.MaxValue, attribute.NormalG);
        Assert.AreEqual(float.MaxValue, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        // Assert - Hovered and Active clamp to 1
        Assert.AreEqual(1f, attribute.HoveredR);
        Assert.AreEqual(1f, attribute.HoveredG);
        Assert.AreEqual(1f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        Assert.AreEqual(1f, attribute.ActiveR);
        Assert.AreEqual(1f, attribute.ActiveG);
        Assert.AreEqual(1f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }

    /// <summary>
    /// Tests the constructor with small epsilon values near zero to verify
    /// precise floating-point behavior and clamping.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEpsilonValues_HandlesSmallFloatsPrecisely()
    {
        // Arrange
        float epsilon = float.Epsilon;

        // Act
        var attribute = new UmbraCustomButtonColorsAttribute(epsilon, epsilon, epsilon);

        // Assert - Normal state
        Assert.AreEqual(epsilon, attribute.NormalR);
        Assert.AreEqual(epsilon, attribute.NormalG);
        Assert.AreEqual(epsilon, attribute.NormalB);
        Assert.AreEqual(1.0f, attribute.NormalA);

        // Assert - Hovered state: epsilon + 0.10
        Assert.AreEqual(0.1f, attribute.HoveredR, 0.0001f);
        Assert.AreEqual(0.1f, attribute.HoveredG, 0.0001f);
        Assert.AreEqual(0.1f, attribute.HoveredB, 0.0001f);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        // Assert - Active state: epsilon - 0.08 -> clamped to 0
        Assert.AreEqual(0f, attribute.ActiveR);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0f, attribute.ActiveB);
        Assert.AreEqual(1.0f, attribute.ActiveA);
    }
}