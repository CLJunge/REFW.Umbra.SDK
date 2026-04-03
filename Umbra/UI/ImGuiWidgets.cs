using Hexa.NET.ImGui;

namespace Umbra.UI;

/// <summary>
/// Provides reusable ImGui helper widgets used across Umbra UI components.
/// </summary>
/// <remarks>
/// These helpers are stateless and are intended to be called from within an active ImGui window or child window during the current frame.
/// </remarks>
public static class ImGuiWidgets
{
    private const string _helpMarkerText = "(?)";

    /// <summary>
    /// Renders a tooltip containing <paramref name="description"/> when the previously submitted ImGui item is hovered.
    /// </summary>
    /// <param name="description">The tooltip text to display while the previous item is hovered.</param>
    public static void DrawHoverTooltip(string description)
    {
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 24f);
        ImGui.TextUnformatted(description);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }

    /// <summary>
    /// Renders an inline help marker that shows <paramref name="description"/> in a tooltip when hovered.
    /// </summary>
    /// <param name="description">The tooltip text displayed by the help marker.</param>
    /// <remarks>
    /// Call this after <see cref="ImGui.SameLine()"/> when the marker should appear on the same row as its associated control.
    /// </remarks>
    public static void DrawHelpMarker(string description)
    {
        ImGui.TextDisabled(_helpMarkerText);
        DrawHoverTooltip(description);
    }
}
