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
    /// Tests that the constructor with RGB parameters correctly sets all properties
    /// with typical values in the standard [0, 1] range.
    /// </summary>
    [TestMethod]
    public void Constructor_WithTypicalValues_SetsAllPropertiesCorrectly()
    {
        // Arrange
        var r = 0.5f;
        var g = 0.5f;
        var b = 0.5f;

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
        Assert.AreEqual(1f, attribute.HoveredR);
        Assert.AreEqual(0.15f, attribute.HoveredG, 0.0001f);
        Assert.AreEqual(0f, attribute.HoveredB);
        Assert.AreEqual(1.0f, attribute.HoveredA);

        // Assert - Active: -0.08 with clamping
        Assert.AreEqual(0.87f, attribute.ActiveR, 0.0001f);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0f, attribute.ActiveB);
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
        var epsilon = float.Epsilon;

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

    /// <summary>
    /// Tests that the RGB constructor clamps infinity-derived hovered and active values per channel.
    /// </summary>
    [TestMethod]
    public void Constructor_WithInfinityRgb_ClampsDerivedStatesPerChannel()
    {
        // Arrange & Act
        var attribute = new UmbraCustomButtonColorsAttribute(float.PositiveInfinity, float.NegativeInfinity, 0.5f);

        // Assert - normal state stores the original values as-is
        Assert.AreEqual(float.PositiveInfinity, attribute.NormalR);
        Assert.AreEqual(float.NegativeInfinity, attribute.NormalG);
        Assert.AreEqual(0.5f, attribute.NormalB);

        // Assert - hovered state clamps to [0, 1]
        Assert.AreEqual(1f, attribute.HoveredR);
        Assert.AreEqual(0f, attribute.HoveredG);
        Assert.AreEqual(0.6f, attribute.HoveredB, 0.0001f);

        // Assert - active state clamps to [0, 1]
        Assert.AreEqual(1f, attribute.ActiveR);
        Assert.AreEqual(0f, attribute.ActiveG);
        Assert.AreEqual(0.42f, attribute.ActiveB, 0.0001f);
    }
}
