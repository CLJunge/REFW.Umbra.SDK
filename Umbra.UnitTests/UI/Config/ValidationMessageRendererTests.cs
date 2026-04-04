using System.Numerics;
using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ValidationMessageRenderer"/>.
/// </summary>
[TestClass]
public sealed class ValidationMessageRendererTests
{
    /// <summary>
    /// Tests that no validation message is rendered when the parameter has no validation error.
    /// </summary>
    [TestMethod]
    public void Draw_WhenParameterHasNoValidationError_DoesNotRenderMessage()
    {
        // Arrange
        var parameter = new Parameter<string>("valid");
        var textOps = new TestTextOps();

        // Act
        ValidationMessageRenderer.Draw(parameter, textOps);

        // Assert
        Assert.IsEmpty(textOps.TextColoredCalls);
    }

    /// <summary>
    /// Tests that the recorded validation error is rendered when present.
    /// </summary>
    [TestMethod]
    public void Draw_WhenParameterHasValidationError_RendersMessage()
    {
        // Arrange
        var parameter = new Parameter<string>("valid")
        {
            Metadata = new ParameterMetadata { Required = true },
            Value = null
        };
        var textOps = new TestTextOps();

        // Act
        ValidationMessageRenderer.Draw(parameter, textOps);

        // Assert
        Assert.HasCount(1, textOps.TextColoredCalls);
        Assert.AreEqual("Value is required.", textOps.TextColoredCalls[0].Text);
    }

    /// <summary>
    /// Tests that the renderer ignores validation-state implementations that expose a blank message.
    /// </summary>
    [TestMethod]
    public void Draw_WhenValidationMessageIsBlank_DoesNotRenderMessage()
    {
        // Arrange
        var parameter = new BlankValidationStateParameter();
        var textOps = new TestTextOps();

        // Act
        ValidationMessageRenderer.Draw(parameter, textOps);

        // Assert
        Assert.IsEmpty(textOps.TextColoredCalls);
    }

    private sealed class TestTextOps : ITextOps
    {
        internal List<(Vector4 Color, string Text)> TextColoredCalls { get; } = [];

        public void Text(string text)
        {
        }

        public void TextDisabled(string text)
        {
        }

        public void SameLine()
        {
        }

        public void DrawHelpMarker(string description)
        {
        }

        public void TextColored(Vector4 color, string text)
            => TextColoredCalls.Add((color, text));
    }

    private sealed class BlankValidationStateParameter : IParameter, IParameterValidationState
    {
        public event Action? ValueChanged
        {
            add { }
            remove { }
        }

        public string Key => "test";

        public ParameterMetadata Metadata { get; } = new();

        public Type ValueType => typeof(string);

        public bool IsModified => false;

        public bool HasValidationError => true;

        public string? ValidationError => "   ";

        public object? GetValue() => null;

        public void SetValue(object? value)
        {
        }

        public void Reset(bool raiseEvent = true)
        {
        }

        public void SetValueWithoutNotify(object? value)
        {
        }

        public void ClearValidationError()
        {
        }
    }
}
