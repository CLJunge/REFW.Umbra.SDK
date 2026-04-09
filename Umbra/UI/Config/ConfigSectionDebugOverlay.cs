#if DEBUG
using System.Numerics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Config;

/// <summary>
/// Draws a compact debug overlay listing the enabled/disabled status of every optional
/// <see cref="ConfigSection{TConfig}"/> feature.
/// </summary>
/// <remarks>
/// This type exists only in <c>DEBUG</c> builds and is called at the top of
/// <see cref="ConfigSection{TConfig}.Draw"/> to give developers a quick visual summary
/// of which optional features are active for the current section.
/// </remarks>
internal static class ConfigSectionDebugOverlay
{
    private static readonly Vector4 _enabledColor = new(0.4f, 1f, 0.4f, 1f);
    private static readonly Vector4 _disabledColor = new(1f, 0.4f, 0.4f, 1f);
    private static readonly string[] _featureLabels = ["Search", "Transfer", "Undo", "Presets", "Save Controller"];
    private static float _labelColumnWidth = 0f;

    /// <summary>
    /// Draws the optional-feature status block and a trailing separator.
    /// </summary>
    internal static void Draw(bool search, bool transfer, bool undo, bool presets, bool saveController)
    {
        // Calculate label width on first draw (ImGui must be initialized)
        if (_labelColumnWidth == 0f)
            CalculateLabelColumnWidth();

        ImGui.PushID("DebugOverlay");

        try
        {
            if (ImGui.TreeNode("Optional Features"))
            {
                try
                {
                    DrawStatus("Search", search);
                    DrawStatus("Transfer", transfer);
                    DrawStatus("Undo", undo);
                    DrawStatus("Presets", presets);
                    DrawStatus("Save Controller", saveController);
                }
                finally { ImGui.TreePop(); }
            }
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
        finally { ImGui.PopID(); }
    }

    private static void CalculateLabelColumnWidth()
    {
        // Find the longest label and calculate its width with padding
        const float extraPadding = 75f;
        var maxWidth = 0f;

        foreach (var label in _featureLabels)
        {
            var size = ImGui.CalcTextSize($"{label}:");
            maxWidth = Math.Max(maxWidth, size.X);
        }

        _labelColumnWidth = maxWidth + extraPadding;
    }

    private static void DrawStatus(string label, bool enabled)
    {
        ImGui.TextDisabled($"{label}:");
        ImGui.SameLine(_labelColumnWidth);

        if (enabled)
            ImGui.TextColored(_enabledColor, "Enabled");
        else
            ImGui.TextColored(_disabledColor, "Disabled");
    }
}
#endif
