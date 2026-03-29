using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Renders <see cref="ButtonDrawer"/> widgets through the active ImGui frame.
/// </summary>
/// <remarks>
/// This is the production renderer used inside the game process. Unit tests replace it with an
/// in-memory renderer so drawer behavior can be asserted without touching native ImGui entry
/// points.
/// </remarks>
internal sealed class ImGuiButtonDrawerRenderer : IButtonDrawerRenderer
{
    /// <inheritdoc/>
    public void TextDisabled(string text)
    {
        ImGui.TextDisabled(text);
    }

    /// <inheritdoc/>
    public bool PushButtonColors(ButtonStyle style)
    {
        return ButtonStyleColors.Push(style);
    }

    /// <inheritdoc/>
    public bool PushButtonColors(Vector4 normal, Vector4 hovered, Vector4 active)
    {
        return ButtonStyleColors.Push(normal, hovered, active);
    }

    /// <inheritdoc/>
    public void PopButtonColors()
    {
        ButtonStyleColors.Pop();
    }

    /// <inheritdoc/>
    public bool Button(string label, Vector2 size)
    {
        return ImGui.Button(label, size);
    }

    /// <inheritdoc/>
    public void SameLine()
    {
        ImGui.SameLine();
    }

    /// <inheritdoc/>
    public void DrawHelpMarker(string description)
    {
        ImGuiWidgets.DrawHelpMarker(description);
    }
}
