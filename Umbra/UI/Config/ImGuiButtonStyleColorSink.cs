using System.Numerics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Config;

/// <summary>
/// Pushes and pops button style colors through the active ImGui frame.
/// </summary>
internal sealed class ImGuiButtonStyleColorSink : IButtonStyleColorSink
{
    /// <inheritdoc/>
    public void PushStyleColor(ImGuiCol color, Vector4 value) => ImGui.PushStyleColor(color, value);

    /// <inheritdoc/>
    public void PopStyleColor(int count) => ImGui.PopStyleColor(count);
}
