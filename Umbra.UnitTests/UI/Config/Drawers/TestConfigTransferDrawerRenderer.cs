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

    public List<(string Label, Vector2 Size)> SizedButtons { get; } = [];

    public List<string> OpenedPopups { get; } = [];

    public List<string> BegunPopups { get; } = [];

    public List<string> Selectables { get; } = [];

    public int SeparatorCount { get; private set; }

    public int SameLineCount { get; private set; }

    public float AvailableWidth { get; set; } = 600f;

    public float ItemSpacingX { get; set; } = 8f;

    public Queue<bool> ButtonResults { get; } = new();

    public Queue<bool> SizedButtonResults { get; } = new();

    public Queue<(bool Changed, string Value)> InputResults { get; } = new();

    public Queue<bool> BeginPopupResults { get; } = new();

    public Queue<bool> SelectableResults { get; } = new();

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

    public bool Button(string label, Vector2 size)
    {
        SizedButtons.Add((label, size));
        if (SizedButtonResults.Count == 0)
            return false;

        return SizedButtonResults.Dequeue();
    }

    public float GetAvailableWidth() => AvailableWidth;

    public float GetItemSpacingX() => ItemSpacingX;

    public float GetTextWidth(string text) => text.Length * 8f;

    public float GetButtonWidth(string label)
    {
        var hiddenIdIndex = label.IndexOf("##", StringComparison.Ordinal);
        var visibleLabel = hiddenIdIndex >= 0 ? label[..hiddenIdIndex] : label;
        return (visibleLabel.Length * 8f) + 16f;
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

    public void OpenPopup(string id) => OpenedPopups.Add(id);

    public bool BeginPopup(string id)
    {
        BegunPopups.Add(id);
        if (BeginPopupResults.Count == 0)
            return false;

        return BeginPopupResults.Dequeue();
    }

    public void EndPopup()
    {
    }

    public bool Selectable(string label)
    {
        Selectables.Add(label);
        if (SelectableResults.Count == 0)
            return false;

        return SelectableResults.Dequeue();
    }

    public void Separator() => SeparatorCount++;
}
