using Hexa.NET.ImGui;

namespace Umbra.SDK.UI;

/// <summary>
/// Provides reusable ImGui widget helpers for use inside plugin settings panels and live
/// section drawers.
/// </summary>
/// <remarks>
/// All methods in this class are stateless and safe to call from any ImGui draw callback.
/// They must be called from within an active ImGui window or child window.
/// </remarks>
public static class ImGuiWidgets
{
    /// <summary>
    /// Renders an inline <c>(?)</c> marker that shows a tooltip containing
    /// <paramref name="description"/> when hovered.
    /// Call this after <c>ImGui.SameLine()</c> so it appears on the same row as its control.
    /// </summary>
    /// <param name="description">The tooltip text to display on hover.</param>
    public static void DrawHelpMarker(string description)
    {
        ImGui.TextDisabled("(?)");
        if (!ImGui.IsItemHovered()) return;
        ImGui.BeginTooltip();
        ImGui.PushTextWrapPos(ImGui.GetFontSize() * 24f);
        ImGui.TextUnformatted(description);
        ImGui.PopTextWrapPos();
        ImGui.EndTooltip();
    }
}
