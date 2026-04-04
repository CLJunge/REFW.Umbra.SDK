namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Records <see cref="ConfigDrawer{TConfig}"/> scope operations for unit tests.
/// </summary>
internal sealed class TestConfigDrawerScope : IConfigDrawerRenderer
{
    public List<string> PushedIds { get; } = [];
    public List<string> InputTextLabels { get; } = [];
    public List<string> ButtonLabels { get; } = [];
    public int SameLineCount { get; private set; }
    public int PopCount { get; private set; }
    public int ScrollHereCount { get; private set; }

    public bool NextInputTextResult { get; set; }
    public string? NextInputTextValue { get; set; }
    public Queue<bool> ButtonResults { get; } = new();

    public void PushId(string idScope) => PushedIds.Add(idScope);

    public void PopId() => PopCount++;

    public bool InputText(string label, ref string value, uint maxLength)
    {
        InputTextLabels.Add(label);
        _ = maxLength;

        var result = NextInputTextResult;
        NextInputTextResult = false;
        if (result && NextInputTextValue is not null)
        {
            value = NextInputTextValue;
            NextInputTextValue = null;
        }

        return result;
    }

    public bool Button(string label)
    {
        ButtonLabels.Add(label);
        if (ButtonResults.Count == 0)
            return false;

        return ButtonResults.Dequeue();
    }

    public void Text(string text)
    {
    }

    public void TextDisabled(string text)
    {
    }

    public void TextColored(System.Numerics.Vector4 color, string text)
    {
    }

    public void SameLine() => SameLineCount++;

    public void DrawHelpMarker(string description)
    {
    }

    public void PushStyleColor(Hexa.NET.ImGui.ImGuiCol color, System.Numerics.Vector4 value)
    {
    }

    public void PopStyleColor(int count)
    {
    }

    public void SetScrollHereY(float centerYRatio)
        => ScrollHereCount++;
}
