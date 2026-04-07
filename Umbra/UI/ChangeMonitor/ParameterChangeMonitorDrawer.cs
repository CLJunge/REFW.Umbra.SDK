using Hexa.NET.ImGui;
using Umbra.Config;
using Umbra.UI.LiveState;

namespace Umbra.UI.ChangeMonitor;

/// <summary>
/// Renders the parameter change monitor as a scrollable ImGui text list showing recent
/// <see cref="ConfigChangeRecord"/> entries.
/// </summary>
/// <remarks>
/// Each entry shows the parameter label, the old value, and the new value. The list is
/// presented newest-first so the most recent change is always visible at the top.
/// </remarks>
public sealed class ParameterChangeMonitorDrawer : ILiveStateSectionDrawer<ParameterChangeMonitorState>
{
    /// <inheritdoc/>
    public void Draw(ParameterChangeMonitorState state)
    {
        var entries = state.Log.GetEntries();

        if (entries.Count == 0)
        {
            ImGui.TextDisabled("No parameter changes recorded.");
            return;
        }

        ImGui.Text($"{entries.Count} change(s) recorded");
        ImGui.Separator();

        if (ImGui.BeginChild("##ChangeMonitorScroll", new System.Numerics.Vector2(0, state.DisplayHeight), ImGuiChildFlags.Borders))
        {
            // Render newest first
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var entry = entries[i];
                var oldText = entry.OldValue?.ToString() ?? "(null)";
                var newText = entry.NewValue?.ToString() ?? "(null)";
                ImGui.Text($"{entry.DisplayLabel}: {oldText} -> {newText}");
            }
        }

        ImGui.EndChild();
    }
}
