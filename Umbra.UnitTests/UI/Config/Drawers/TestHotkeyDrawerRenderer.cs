namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Records <see cref="HotkeyDrawer"/> rendering operations in memory for unit tests.
/// </summary>
internal sealed class TestHotkeyDrawerRenderer : IHotkeyDrawerRenderer
{
    public List<string> DisabledTexts { get; } = [];
    public List<string> Texts { get; } = [];
    public List<string> Buttons { get; } = [];
    public List<string> HelpMarkers { get; } = [];
    public int SameLineCount { get; private set; }
    public Queue<bool> ButtonResults { get; } = new();

    public void TextDisabled(string text)
    {
        DisabledTexts.Add(text);
    }

    public void Text(string text)
    {
        Texts.Add(text);
    }

    public void SameLine()
    {
        SameLineCount++;
    }

    public bool Button(string label)
    {
        Buttons.Add(label);
        if (ButtonResults.Count == 0)
            return false;

        return ButtonResults.Dequeue();
    }

    public void DrawHelpMarker(string description)
    {
        HelpMarkers.Add(description);
    }
}
