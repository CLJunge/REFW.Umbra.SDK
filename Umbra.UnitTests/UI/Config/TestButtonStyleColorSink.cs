using System.Numerics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Records button-style color push/pop operations for unit tests.
/// </summary>
internal sealed class TestButtonStyleColorSink : IButtonStyleColorSink
{
    public List<(ImGuiCol Color, Vector4 Value)> PushedColors { get; } = [];
    public List<int> PopCounts { get; } = [];

    public void PushStyleColor(ImGuiCol color, Vector4 value)
    {
        PushedColors.Add((color, value));
    }

    public void PopStyleColor(int count)
    {
        PopCounts.Add(count);
    }
}
