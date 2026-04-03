using System.Numerics;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.Logging.UnitTests;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Unit tests for <see cref="ButtonDrawer"/>.
/// </summary>
[TestClass]
public sealed class ButtonDrawerTests
{
    private TestButtonDrawerRenderer _renderer = null!;
    private TestLogSink _logSink = null!;
    private bool _originalLoggingEnabled;

    /// <summary>
    /// Installs a recording renderer and logger sink before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _renderer = new TestButtonDrawerRenderer();
        _logSink = new TestLogSink();
        _originalLoggingEnabled = Logger.Enabled;
        Logger.SetLogSink(_logSink);
        Logger.EnableAll();
    }

    /// <summary>
    /// Restores the default logger state after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        Logger.ResetLogSink();
        Logger.Enabled = _originalLoggingEnabled;
    }

    /// <summary>
    /// Tests that Draw handles a null parameter by rendering disabled text.
    /// </summary>
    [TestMethod]
    public void Draw_NullParameter_RendersDisabledText()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);

        // Act
        drawer.Draw("TestButton", null!);

        // Assert
        Assert.HasCount(1, _renderer.DisabledTexts);
        Assert.AreEqual("TestButton: (ButtonDrawer requires Parameter<Action>)", _renderer.DisabledTexts[0]);
        Assert.IsEmpty(_renderer.Buttons);
    }

    /// <summary>
    /// Tests that Draw handles a parameter that is not <see cref="Parameter{T}"/> of
    /// <see cref="Action"/> by rendering disabled text.
    /// </summary>
    [TestMethod]
    public void Draw_ParameterNotActionType_RendersDisabledText()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);
        var parameter = new Parameter<int>(42);

        // Act
        drawer.Draw("TestButton", parameter);

        // Assert
        Assert.HasCount(1, _renderer.DisabledTexts);
        Assert.AreEqual("TestButton: (ButtonDrawer requires Parameter<Action>)", _renderer.DisabledTexts[0]);
        Assert.IsEmpty(_renderer.Buttons);
    }

    /// <summary>
    /// Tests that Draw with valid <see cref="Parameter{T}"/> metadata uses the default button
    /// style and configured width.
    /// </summary>
    [TestMethod]
    public void Draw_ValidParameterWithDefaultMetadata_UsesDefaultStyle()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ControlWidth = 125f
            }
        };

        // Act
        drawer.Draw("TestButton", parameter);

        // Assert
        Assert.HasCount(1, _renderer.PushedStyles);
        Assert.AreEqual(ButtonStyle.Default, _renderer.PushedStyles[0]);
        Assert.HasCount(1, _renderer.Buttons);
        Assert.AreEqual("TestButton", _renderer.Buttons[0].Label);
        Assert.AreEqual(new Vector2(125f, 0f), _renderer.Buttons[0].Size);
        Assert.AreEqual(0, _renderer.PopCount);
        Assert.IsEmpty(_logSink.WarningMessages);
    }

    /// <summary>
    /// Tests that Draw with <see cref="ButtonStyle.Custom"/> and no custom colors logs a warning
    /// once per drawer instance and falls back to the default style.
    /// </summary>
    [TestMethod]
    public void Draw_CustomStyleWithoutColors_LogsWarningOnceAndFallsBackToDefault()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = ButtonStyle.Custom,
                CustomButtonColors = null
            }
        };

        // Act
        drawer.Draw("TestButton", parameter);
        drawer.Draw("TestButton", parameter);

        // Assert
        Assert.HasCount(2, _renderer.PushedStyles);
        Assert.AreEqual(ButtonStyle.Default, _renderer.PushedStyles[0]);
        Assert.AreEqual(ButtonStyle.Default, _renderer.PushedStyles[1]);
        Assert.HasCount(1, _logSink.WarningMessages);
        Assert.Contains("ButtonStyle.Custom", _logSink.WarningMessages[0]);
    }

    /// <summary>
    /// Tests that Draw with <see cref="ButtonStyle.Custom"/> and valid custom colors uses those
    /// colors instead of the preset style table.
    /// </summary>
    [TestMethod]
    public void Draw_CustomStyleWithColors_UsesCustomColors()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);
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

        // Act
        drawer.Draw("TestButton", parameter);

        // Assert
        Assert.IsEmpty(_renderer.PushedStyles);
        Assert.HasCount(1, _renderer.PushedCustomColors);
        Assert.AreEqual((normalColor, hoveredColor, activeColor), _renderer.PushedCustomColors[0]);
        Assert.AreEqual(1, _renderer.PopCount);
    }

    /// <summary>
    /// Tests that Draw with a clicked button safely ignores a null action value.
    /// </summary>
    [TestMethod]
    public void Draw_NullAction_NoExceptionOnClick()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);
        _renderer.NextButtonResult = true;
        var parameter = new Parameter<Action>(null)
        {
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("TestButton", parameter);

        // Assert
        Assert.HasCount(1, _renderer.Buttons);
        Assert.IsEmpty(_logSink.ErrorMessages);
    }

    /// <summary>
    /// Tests that custom colors take priority even when a non-custom preset style is also set.
    /// </summary>
    [TestMethod]
    public void Draw_CustomColorsWithNonCustomStyle_UsesCustomColors()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);
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

        // Act
        drawer.Draw("TestButton", parameter);

        // Assert
        Assert.IsEmpty(_renderer.PushedStyles);
        Assert.HasCount(1, _renderer.PushedCustomColors);
        Assert.AreEqual((normalColor, hoveredColor, activeColor), _renderer.PushedCustomColors[0]);
    }

    /// <summary>
    /// Tests that multiple <see cref="ButtonDrawer"/> instances maintain separate warning state.
    /// </summary>
    [TestMethod]
    public void Draw_MultipleDrawerInstances_MaintainSeparateWarningState()
    {
        // Arrange
        var renderer1 = new TestButtonDrawerRenderer();
        var renderer2 = new TestButtonDrawerRenderer();
        var drawer1 = new ButtonDrawer(renderer1);
        var drawer2 = new ButtonDrawer(renderer2);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                ButtonStyle = ButtonStyle.Custom,
                CustomButtonColors = null
            }
        };

        // Act
        drawer1.Draw("TestButton", parameter);
        drawer2.Draw("TestButton", parameter);
        drawer1.Draw("TestButton", parameter);
        drawer2.Draw("TestButton", parameter);

        // Assert
        Assert.HasCount(2, _logSink.WarningMessages);
        Assert.HasCount(2, renderer1.PushedStyles);
        Assert.HasCount(2, renderer2.PushedStyles);
    }

    /// <summary>
    /// Tests that clicking the button invokes the stored action.
    /// </summary>
    [TestMethod]
    public void Draw_ClickedButton_InvokesAction()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);
        _renderer.NextButtonResult = true;
        var invoked = 0;
        var parameter = new Parameter<Action>(() => invoked++)
        {
            Metadata = new ParameterMetadata()
        };

        // Act
        drawer.Draw("TestButton", parameter);

        // Assert
        Assert.AreEqual(1, invoked);
    }

    /// <summary>
    /// Tests that a description renders an inline help marker.
    /// </summary>
    [TestMethod]
    public void Draw_WithDescription_RendersHelpMarker()
    {
        // Arrange
        var drawer = new ButtonDrawer(_renderer);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                Description = "Helpful description"
            }
        };

        // Act
        drawer.Draw("TestButton", parameter);

        // Assert
        Assert.AreEqual(1, _renderer.SameLineCount);
        Assert.HasCount(1, _renderer.HelpMarkers);
        Assert.AreEqual("Helpful description", _renderer.HelpMarkers[0]);
    }

    /// <summary>
    /// Tests that a missing description does not render a help marker or same-line call.
    /// </summary>
    [TestMethod]
    public void Draw_WithoutDescription_DoesNotRenderHelpMarker()
    {
        var drawer = new ButtonDrawer(_renderer);
        var parameter = new Parameter<Action>(() => { })
        {
            Metadata = new ParameterMetadata
            {
                Description = null
            }
        };

        drawer.Draw("TestButton", parameter);

        Assert.IsEmpty(_renderer.HelpMarkers);
        Assert.AreEqual(0, _renderer.SameLineCount);
    }

    /// <summary>
    /// Tests that the constructor rejects a null renderer.
    /// </summary>
    [TestMethod]
    public void Constructor_NullRenderer_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new ButtonDrawer(null!));

        Assert.AreEqual("renderer", exception.ParamName);
    }
}
