namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Records <see cref="ParameterNode"/> layout operations for unit tests.
/// </summary>
internal sealed class TestParameterNodeRenderer : IParameterNodeRenderer
{
    public List<(Hexa.NET.ImGui.ImGuiCol Color, System.Numerics.Vector4 Value)> PushedStyleColors { get; } = [];
    public int SpacingCount { get; private set; }
    public int IndentCount { get; private set; }
    public int UnindentCount { get; private set; }
    public int BeginDisabledCount { get; private set; }
    public int EndDisabledCount { get; private set; }
    public bool? LastBeginDisabledValue { get; private set; }
    public int PushStyleColorCount { get; private set; }
    public int PopStyleColorCount { get; private set; }
    public int ScrollHereCount { get; private set; }
    public int KeyboardFocusCount { get; private set; }
    public float? LastIndentAmount { get; private set; }
    public float? LastUnindentAmount { get; private set; }

    public void Spacing() => SpacingCount++;

    public void Indent(float amount)
    {
        IndentCount++;
        LastIndentAmount = amount;
    }

    public void Unindent(float amount)
    {
        UnindentCount++;
        LastUnindentAmount = amount;
    }

    public void BeginDisabled(bool disabled)
    {
        BeginDisabledCount++;
        LastBeginDisabledValue = disabled;
    }

    public void EndDisabled() => EndDisabledCount++;

    public void PushStyleColor(Hexa.NET.ImGui.ImGuiCol color, System.Numerics.Vector4 value)
    {
        PushStyleColorCount++;
        PushedStyleColors.Add((color, value));
    }

    public void PopStyleColor(int count)
        => PopStyleColorCount += count;

    public void SetScrollHereY(float centerYRatio)
        => ScrollHereCount++;

    public void SetKeyboardFocusHere()
        => KeyboardFocusCount++;
}
