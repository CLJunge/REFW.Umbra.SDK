namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Records <see cref="ConfigDrawer{TConfig}"/> scope operations for unit tests.
/// </summary>
internal sealed class TestConfigDrawerScope : IConfigDrawerRenderer
{
    public List<string> PushedIds { get; } = [];
    public List<string> InputTextLabels { get; } = [];
    public List<string> ButtonLabels { get; } = [];
    public List<string> ButtonWidthRequests { get; } = [];
    public List<float> NextItemWidths { get; } = [];
    public int SameLineCount { get; private set; }
    public int PopCount { get; private set; }
    public int ScrollHereCount { get; private set; }

    public bool NextInputTextResult { get; set; }
    public string? NextInputTextValue { get; set; }
    public float AvailableWidth { get; set; } = 300f;
    public float ItemSpacingX { get; set; } = 8f;
    public Dictionary<string, float> ButtonWidths { get; } = [];
    public Queue<bool> ButtonResults { get; } = new();

    public void PushId(string idScope) => PushedIds.Add(idScope);

    public void PopId() => PopCount++;

    public float GetAvailableWidth() => AvailableWidth;

    public float GetItemSpacingX() => ItemSpacingX;

    public float GetButtonWidth(string label)
    {
        ButtonWidthRequests.Add(label);
        return ButtonWidths.TryGetValue(label, out var width) ? width : 40f;
    }

    public void SetNextItemWidth(float width) => NextItemWidths.Add(width);

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
}
