namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Records <see cref="ConfigDrawer{TConfig}"/> scope operations for unit tests.
/// </summary>
internal sealed class TestConfigDrawerScope : IConfigDrawerRenderer
{
    public List<string> PushedIds { get; } = [];
    public List<string> RenderedTexts { get; } = [];
    public List<string> InputTextLabels { get; } = [];
    public List<string> ButtonLabels { get; } = [];
    public List<bool> DisabledStack { get; } = [];
    public int EndDisabledCount { get; private set; }
    public List<string> TextWidthRequests { get; } = [];
    public List<string> ButtonWidthRequests { get; } = [];
    public List<float> NextItemWidths { get; } = [];
    public int SameLineCount { get; private set; }
    public int PopCount { get; private set; }
    public int ScrollHereCount { get; private set; }

    public bool NextInputTextResult { get; set; }
    public string? NextInputTextValue { get; set; }
    public float AvailableWidth { get; set; } = 300f;
    public float ItemSpacingX { get; set; } = 8f;
    public Dictionary<string, float> TextWidths { get; } = [];
    public Dictionary<string, float> ButtonWidths { get; } = [];
    public Queue<bool> ButtonResults { get; } = new();

    private readonly Stack<bool> _disabledScopeStack = new();
    private int _disabledCount;

    public void PushId(string idScope) => PushedIds.Add(idScope);

    public void PopId() => PopCount++;

    public float GetAvailableWidth() => AvailableWidth;

    public float GetItemSpacingX() => ItemSpacingX;

    public float GetTextWidth(string text)
    {
        TextWidthRequests.Add(text);
        return TextWidths.TryGetValue(text, out var width) ? width : 48f;
    }

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
        var result = ButtonResults.Count != 0 && ButtonResults.Dequeue();
        if (_disabledCount > 0)
            return false;

        return result;
    }

    public void Text(string text) => RenderedTexts.Add(text);

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

    public void BeginDisabled(bool disabled)
    {
        DisabledStack.Add(disabled);
        _disabledScopeStack.Push(disabled);
        if (disabled)
            _disabledCount++;
    }

    public void EndDisabled()
    {
        if (_disabledScopeStack.Count == 0)
            throw new InvalidOperationException("EndDisabled called without a matching BeginDisabled.");

        if (_disabledScopeStack.Pop())
            _disabledCount--;

        EndDisabledCount++;
    }
}
