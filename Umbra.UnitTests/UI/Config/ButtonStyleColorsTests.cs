using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for the <see cref="ButtonStyleColors"/> class.
/// </summary>
[TestClass]
public sealed class ButtonStyleColorsTests
{
    private TestButtonStyleColorSink _colorSink = null!;

    /// <summary>
    /// Installs a recording color sink before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _colorSink = new TestButtonStyleColorSink();
        ButtonStyleColors.SetColorSink(_colorSink);
    }

    /// <summary>
    /// Restores the default ImGui-backed color sink after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup() => ButtonStyleColors.ResetColorSink();

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Pop"/> pops three colors from the active sink.
    /// </summary>
    [TestMethod]
    public void Pop_WhenCalled_PopsThreeColors()
    {
        // Act
        ButtonStyleColors.Pop();

        // Assert
        Assert.HasCount(1, _colorSink.PopCounts);
        Assert.AreEqual(3, _colorSink.PopCounts[0]);
    }

    /// <summary>
    /// Tests that pushing fully custom colors returns <see langword="true"/> and forwards all
    /// three colors to the active sink.
    /// </summary>
    [TestMethod]
    public void Push_ValidColorVectors_ReturnsTrueAndPushesAllColors()
    {
        // Arrange
        Vector4 normal = new(0.20f, 0.45f, 0.80f, 1.0f);
        Vector4 hovered = new(0.30f, 0.55f, 0.90f, 1.0f);
        Vector4 active = new(0.15f, 0.38f, 0.72f, 1.0f);

        // Act
        var result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
        Assert.HasCount(3, _colorSink.PushedColors);
        Assert.AreEqual((ImGuiCol.Button, normal), _colorSink.PushedColors[0]);
        Assert.AreEqual((ImGuiCol.ButtonHovered, hovered), _colorSink.PushedColors[1]);
        Assert.AreEqual((ImGuiCol.ButtonActive, active), _colorSink.PushedColors[2]);
    }

    /// <summary>
    /// Tests that custom color vectors with NaN components are still forwarded to the active sink.
    /// </summary>
    [TestMethod]
    public void Push_NaNComponents_ReturnsTrueAndPushesAllColors()
    {
        // Arrange
        Vector4 normal = new(float.NaN, float.NaN, float.NaN, float.NaN);
        Vector4 hovered = new(0.5f, float.NaN, 0.5f, 1.0f);
        Vector4 active = new(float.NaN, 0.0f, 0.0f, 1.0f);

        // Act
        var result = ButtonStyleColors.Push(normal, hovered, active);

        // Assert
        Assert.IsTrue(result);
        Assert.HasCount(3, _colorSink.PushedColors);
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <see langword="true"/>
    /// for <see cref="ButtonStyle.Primary"/> and pushes three preset colors.
    /// </summary>
    [TestMethod]
    public void Push_Primary_ReturnsTrueAndPushesPresetColors()
    {
        // Act
        var result = ButtonStyleColors.Push(ButtonStyle.Primary);

        // Assert
        Assert.IsTrue(result);
        Assert.HasCount(3, _colorSink.PushedColors);
        Assert.AreEqual(ImGuiCol.Button, _colorSink.PushedColors[0].Color);
        Assert.AreEqual(ImGuiCol.ButtonHovered, _colorSink.PushedColors[1].Color);
        Assert.AreEqual(ImGuiCol.ButtonActive, _colorSink.PushedColors[2].Color);
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <see langword="false"/>
    /// for <see cref="ButtonStyle.Default"/> and does not push any colors.
    /// </summary>
    [TestMethod]
    public void Push_Default_ReturnsFalse()
    {
        // Act
        var result = ButtonStyleColors.Push(ButtonStyle.Default);

        // Assert
        Assert.IsFalse(result);
        Assert.IsEmpty(_colorSink.PushedColors);
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <see langword="false"/>
    /// for <see cref="ButtonStyle.Custom"/> and does not push any colors.
    /// </summary>
    [TestMethod]
    public void Push_Custom_ReturnsFalse()
    {
        // Act
        var result = ButtonStyleColors.Push(ButtonStyle.Custom);

        // Assert
        Assert.IsFalse(result);
        Assert.IsEmpty(_colorSink.PushedColors);
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <see langword="false"/>
    /// for an undefined enum value.
    /// </summary>
    [TestMethod]
    public void Push_UndefinedEnumValue_ReturnsFalse()
    {
        // Act
        var result = ButtonStyleColors.Push((ButtonStyle)999);

        // Assert
        Assert.IsFalse(result);
        Assert.IsEmpty(_colorSink.PushedColors);
    }

    /// <summary>
    /// Tests that <see cref="ButtonStyleColors.Push(ButtonStyle)"/> returns <see langword="false"/>
    /// for a negative enum value.
    /// </summary>
    [TestMethod]
    public void Push_NegativeEnumValue_ReturnsFalse()
    {
        // Act
        var result = ButtonStyleColors.Push((ButtonStyle)(-1));

        // Assert
        Assert.IsFalse(result);
        Assert.IsEmpty(_colorSink.PushedColors);
    }
}
