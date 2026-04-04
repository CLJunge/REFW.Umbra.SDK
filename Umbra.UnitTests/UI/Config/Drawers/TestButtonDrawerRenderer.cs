using System.Numerics;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Records button-drawer rendering operations in memory for unit tests.
/// </summary>
internal sealed class TestButtonDrawerRenderer : IButtonDrawerRenderer
{
    public List<string> Texts { get; } = [];
    public List<(Vector4 Color, string Text)> ColoredTexts { get; } = [];
    public List<string> DisabledTexts { get; } = [];
    public List<ButtonStyle> PushedStyles { get; } = [];
    public List<(Vector4 Normal, Vector4 Hovered, Vector4 Active)> PushedCustomColors { get; } = [];
    public List<(string Label, Vector2 Size)> Buttons { get; } = [];
    public List<string> HelpMarkers { get; } = [];
    public int PopCount { get; private set; }
    public int SameLineCount { get; private set; }
    public bool NextButtonResult { get; set; }

    public void TextDisabled(string text) => DisabledTexts.Add(text);

    public bool PushButtonColors(ButtonStyle style)
    {
        PushedStyles.Add(style);
        return style != ButtonStyle.Default;
    }

    public bool PushButtonColors(Vector4 normal, Vector4 hovered, Vector4 active)
    {
        PushedCustomColors.Add((normal, hovered, active));
        return true;
    }

    public void PopButtonColors() => PopCount++;

    public bool Button(string label, Vector2 size)
    {
        Buttons.Add((label, size));
        var result = NextButtonResult;
        NextButtonResult = false;
        return result;
    }

    public void SameLine() => SameLineCount++;

    public void DrawHelpMarker(string description) => HelpMarkers.Add(description);
    public void TextColored(Vector4 color, string text) => ColoredTexts.Add((color, text));
    public void Text(string text) => Texts.Add(text);
}
