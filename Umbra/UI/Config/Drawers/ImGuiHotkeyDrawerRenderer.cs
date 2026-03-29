using Hexa.NET.ImGui;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Renders <see cref="HotkeyDrawer"/> widgets through the active ImGui frame.
/// </summary>
internal sealed class ImGuiHotkeyDrawerRenderer : IHotkeyDrawerRenderer
{
    /// <inheritdoc/>
    public void TextDisabled(string text) => ImGui.TextDisabled(text);

    /// <inheritdoc/>
    public void Text(string text) => ImGui.Text(text);

    /// <inheritdoc/>
    public void SameLine() => ImGui.SameLine();

    /// <inheritdoc/>
    public bool Button(string label) => ImGui.Button(label);

    /// <inheritdoc/>
    public void DrawHelpMarker(string description) => ImGuiWidgets.DrawHelpMarker(description);
}
