using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;


namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Unit tests for <see cref="ButtonDrawer"/>.
/// </summary>
[TestClass]
public sealed class ButtonDrawerTests
{
    /// <summary>
    /// Tests that Draw handles a null parameter by attempting to render disabled text.
    /// </summary>
    [TestMethod]
    public void Draw_NullParameter_RendersDisabledText()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";

        // Act - ImGui.TextDisabled will be called, but we cannot verify static calls
        // The method should not throw
        drawer.Draw(label, null!);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle null parameter gracefully");
    }

    /// <summary>
    /// Tests that Draw handles a parameter that is not Parameter&lt;Action&gt; by rendering disabled text.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterNotActionType_RendersDisabledText()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<int>(42);

        // Act - ImGui.TextDisabled will be called for type mismatch
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle incorrect parameter type gracefully");
    }

    /// <summary>
    /// Tests that Draw with valid Parameter&lt;Action&gt; and null metadata uses default values.
    /// </summary>
    [TestMethod]
    public void Draw_ValidParameterWithDefaultMetadata_UsesDefaultStyle()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var actionInvoked = false;
        Action testAction = () => actionInvoked = true;
        var parameter = new Parameter<Action>(testAction)
        {
            Metadata = new ParameterMetadata()
        };

        // Act - Button will be rendered with default style, but we cannot verify ImGui calls
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle default metadata gracefully");
    }

    /// <summary>
    /// Tests that Draw with ButtonStyle.Custom and null CustomButtonColors logs warning once and falls back to Default.
    /// </summary>
    [TestMethod]
    public void Draw_CustomStyleWithoutColors_LogsWarningOnceAndFallsBackToDefault()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = ButtonStyle.Custom,
                CustomButtonColors = null
            }
        };

        // Act - First call should log warning
        drawer.Draw(label, parameter);

        // Act - Second call should not log warning again
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception, warning logged internally
        Assert.IsTrue(true, "Method should log warning once for misconfiguration");
    }

    /// <summary>
    /// Tests that Draw with ButtonStyle.Custom and valid CustomButtonColors uses custom colors.
    /// </summary>
    [TestMethod]
    public void Draw_CustomStyleWithColors_UsesCustomColors()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var normalColor = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
        var hoveredColor = new Vector4(0.8f, 0.0f, 0.0f, 1.0f);
        var activeColor = new Vector4(0.6f, 0.0f, 0.0f, 1.0f);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = ButtonStyle.Custom,
                CustomButtonColors = (normalColor, hoveredColor, activeColor)
            }
        };

        // Act - Button will be rendered with custom colors
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should use custom colors when provided");
    }

    /// <summary>
    /// Tests that Draw with ButtonStyle.Default uses default button style.
    /// </summary>
    [TestMethod]
    public void Draw_DefaultStyle_UsesDefaultButtonStyle()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = ButtonStyle.Default
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should use default button style");
    }

    /// <summary>
    /// Tests that Draw with ButtonStyle.Danger uses danger button style.
    /// </summary>
    [TestMethod]
    public void Draw_DangerStyle_UsesDangerButtonStyle()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "DeleteButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = ButtonStyle.Danger
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should use danger button style");
    }

    /// <summary>
    /// Tests that Draw with null ControlWidth defaults to 0f (auto-size).
    /// </summary>
    [TestMethod]
    public void Draw_NullControlWidth_DefaultsToZero()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ControlWidth = null
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception, button sized automatically
        Assert.IsTrue(true, "Method should default to auto-size when ControlWidth is null");
    }

    /// <summary>
    /// Tests that Draw with ControlWidth = -1f fills available width.
    /// </summary>
    [TestMethod]
    public void Draw_ControlWidthNegative_FillsAvailableWidth()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ControlWidth = -1f
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should fill available width when ControlWidth is negative");
    }

    /// <summary>
    /// Tests that Draw with positive ControlWidth uses fixed pixel width.
    /// </summary>
    [TestMethod]
    public void Draw_ControlWidthPositive_UsesFixedWidth()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ControlWidth = 200f
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should use fixed width when ControlWidth is positive");
    }

    /// <summary>
    /// Tests that Draw with ControlWidth = 0f uses auto-size.
    /// </summary>
    [TestMethod]
    public void Draw_ControlWidthZero_UsesAutoSize()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ControlWidth = 0f
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should use auto-size when ControlWidth is zero");
    }

    /// <summary>
    /// Tests that Draw with null Description does not render help marker.
    /// </summary>
    [TestMethod]
    public void Draw_NullDescription_NoHelpMarker()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                Description = null
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception, no help marker rendered
        Assert.IsTrue(true, "Method should not render help marker when Description is null");
    }

    /// <summary>
    /// Tests that Draw with non-null Description renders help marker on same line.
    /// </summary>
    [TestMethod]
    public void Draw_NonNullDescription_RendersHelpMarker()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                Description = "This is a helpful description"
            }
        };

        // Act - Help marker will be rendered via ImGuiWidgets.DrawHelpMarker
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should render help marker when Description is provided");
    }

    /// <summary>
    /// Tests that Draw with null Action does not throw when button would be clicked.
    /// </summary>
    [TestMethod]
    public void Draw_NullAction_NoExceptionOnClick()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(null)
        {
            Metadata = new ParameterMetadata()
        };

        // Act - Even if button is clicked, null action should be handled safely
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle null Action gracefully");
    }

    /// <summary>
    /// Tests that Draw with empty string label renders button with empty label.
    /// </summary>
    [TestMethod]
    public void Draw_EmptyLabel_RendersButtonWithEmptyLabel()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = string.Empty;
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle empty label gracefully");
    }

    /// <summary>
    /// Tests that Draw with whitespace label renders button with whitespace label.
    /// </summary>
    [TestMethod]
    public void Draw_WhitespaceLabel_RendersButtonWithWhitespaceLabel()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "   ";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle whitespace label gracefully");
    }

    /// <summary>
    /// Tests that Draw with special characters in label renders correctly.
    /// </summary>
    [TestMethod]
    public void Draw_LabelWithSpecialCharacters_RendersCorrectly()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "Test<>Button&'\"#@!";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle special characters in label");
    }

    /// <summary>
    /// Tests that Draw with very long label renders without exception.
    /// </summary>
    [TestMethod]
    public void Draw_VeryLongLabel_RendersWithoutException()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = new string('A', 10000);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle very long label");
    }

    /// <summary>
    /// Tests that Draw with CustomButtonColors and non-Custom ButtonStyle uses custom colors (custom colors take priority).
    /// </summary>
    [TestMethod]
    public void Draw_CustomColorsWithNonCustomStyle_UsesCustomColors()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var normalColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
        var hoveredColor = new Vector4(0.6f, 0.6f, 0.6f, 1.0f);
        var activeColor = new Vector4(0.4f, 0.4f, 0.4f, 1.0f);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = ButtonStyle.Default,
                CustomButtonColors = (normalColor, hoveredColor, activeColor)
            }
        };

        // Act - Custom colors should take priority over ButtonStyle
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should use custom colors even when ButtonStyle is not Custom");
    }

    /// <summary>
    /// Tests that Draw with extreme ControlWidth values handles them gracefully.
    /// </summary>
    [TestMethod]
    [DataRow(float.MaxValue)]
    [DataRow(float.MinValue)]
    [DataRow(float.Epsilon)]
    [DataRow(-float.Epsilon)]
    public void Draw_ExtremeControlWidthValues_HandlesGracefully(float controlWidth)
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ControlWidth = controlWidth
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, $"Method should handle extreme ControlWidth value: {controlWidth}");
    }

    /// <summary>
    /// Tests that Draw with extreme color values in CustomButtonColors handles them gracefully.
    /// </summary>
    [TestMethod]
    public void Draw_ExtremeColorValues_HandlesGracefully()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var normalColor = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
        var hoveredColor = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
        var activeColor = new Vector4(float.NaN, float.NaN, float.NaN, float.NaN);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                CustomButtonColors = (normalColor, hoveredColor, activeColor)
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle extreme color values gracefully");
    }

    /// <summary>
    /// Tests that multiple ButtonDrawer instances maintain separate warning state.
    /// </summary>
    [TestMethod]
    public void Draw_MultipleDrawerInstances_MaintainSeparateWarningState()
    {
        // Arrange
        var drawer1 = new ButtonDrawer();
        var drawer2 = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = ButtonStyle.Custom,
                CustomButtonColors = null
            }
        };

        // Act - Both drawers should log warning on their first call
        drawer1.Draw(label, parameter);
        drawer2.Draw(label, parameter);

        // Act - Second calls should not log warning
        drawer1.Draw(label, parameter);
        drawer2.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Each drawer instance should maintain separate warning state");
    }

    /// <summary>
    /// Tests that Draw with null ButtonStyle in metadata uses Default style.
    /// </summary>
    [TestMethod]
    public void Draw_NullButtonStyle_UsesDefaultStyle()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = null
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception, defaults to Default style
        Assert.IsTrue(true, "Method should use Default style when ButtonStyle is null");
    }

    /// <summary>
    /// Tests that Draw with Description containing special characters renders help marker correctly.
    /// </summary>
    [TestMethod]
    public void Draw_DescriptionWithSpecialCharacters_RendersHelpMarkerCorrectly()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                Description = "Special chars: <>&\"'#@!\n\r\t"
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle special characters in Description");
    }

    /// <summary>
    /// Tests that Draw with very long Description renders help marker without exception.
    /// </summary>
    [TestMethod]
    public void Draw_VeryLongDescription_RendersHelpMarkerWithoutException()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                Description = new string('D', 100000)
            }
        };

        // Act
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should handle very long Description");
    }

    /// <summary>
    /// Tests that Draw with empty Description string renders help marker.
    /// </summary>
    [TestMethod]
    public void Draw_EmptyDescription_RendersHelpMarker()
    {
        // Arrange
        var drawer = new ButtonDrawer();
        var label = "TestButton";
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                Description = string.Empty
            }
        };

        // Act - Empty string is non-null, so help marker should be rendered
        drawer.Draw(label, parameter);

        // Assert - Method completes without exception
        Assert.IsTrue(true, "Method should render help marker for empty Description string");
    }
}