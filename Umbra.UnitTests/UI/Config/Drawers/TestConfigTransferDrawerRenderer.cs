using System.Numerics;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Records <see cref="ConfigTransferDrawer"/> rendering operations in memory for unit tests.
/// </summary>
internal sealed class TestConfigTransferDrawerRenderer : IConfigTransferDrawerRenderer
{
    public List<string> Texts { get; } = [];

    public List<string> DisabledTexts { get; } = [];

    public List<(Vector4 Color, string Text)> ColoredTexts { get; } = [];

    public List<string> HelpMarkers { get; } = [];

    public List<string> Buttons { get; } = [];

    public List<(string Label, string Value, uint MaxLength)> Inputs { get; } = [];

    public List<float> Widths { get; } = [];

    public int SameLineCount { get; private set; }

    public Queue<bool> ButtonResults { get; } = new();

    public Queue<(bool Changed, string Value)> InputResults { get; } = new();

    public void Text(string text) => Texts.Add(text);

    public void TextDisabled(string text) => DisabledTexts.Add(text);

    public void TextColored(Vector4 color, string text) => ColoredTexts.Add((color, text));

    public void SameLine() => SameLineCount++;

    public void DrawHelpMarker(string description) => HelpMarkers.Add(description);

    public bool Button(string label)
    {
        Buttons.Add(label);
        if (ButtonResults.Count == 0)
            return false;

        return ButtonResults.Dequeue();
    }

    public void SetNextItemWidth(float width) => Widths.Add(width);

    public bool InputText(string label, ref string value, uint maxLength)
    {
        Inputs.Add((label, value, maxLength));
        if (InputResults.Count == 0)
            return false;

        var next = InputResults.Dequeue();
        value = next.Value;
        return next.Changed;
    }
}
