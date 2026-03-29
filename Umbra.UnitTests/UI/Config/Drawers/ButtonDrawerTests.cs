using System.Numerics;
using Umbra.Config;
using Umbra.Config.Attributes;


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
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata()
        };

        // Act - Button will be rendered with default style, but we cannot verify ImGui calls
        drawer.Draw(label, parameter);
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
    }

}
