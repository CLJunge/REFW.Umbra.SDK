using Hexa.NET.ImGui;
using Umbra.SDK.Input;

namespace Umbra.SDK.UI;

/// <summary>
/// Provides static ImGui helper methods for rendering common UI controls used in plugin settings panels.
/// All methods must be called from within an active ImGui window or child window.
/// </summary>
public static class ImGuiControls
{
    /// <summary>
    /// Gets or sets the label for the "Change" button shown in <see cref="DrawHotKeySetting"/> when no capture is in progress.
    /// Defaults to <c>"Change"</c>.
    /// </summary>
    public static string HotkeyChangeLabel { get; set; } = "Change";

    /// <summary>
    /// Gets or sets the label for the "Cancel" button shown in <see cref="DrawHotKeySetting"/> while a key capture is active.
    /// Defaults to <c>"Cancel"</c>.
    /// </summary>
    public static string HotkeyCancelLabel { get; set; } = "Cancel";

    /// <summary>
    /// Gets or sets the prompt text displayed in <see cref="DrawHotKeySetting"/> while waiting for a key press.
    /// Defaults to <c>"Press any key..."</c>.
    /// </summary>
    public static string HotkeyCapturingPrompt { get; set; } = "Press any key...";

    /// <summary>
    /// Gets or sets the prompt text displayed in <see cref="DrawHotKeySetting"/> while waiting for a key press.
    /// The string <c>{0}</c> is replaced by the parameter label. Defaults to <c>"{0}: Press any key..."</c>.
    /// </summary>
    public static string HotkeyCapturingPromptWithLabel { get; set; } = "{0}: Press any key...";

    /// <summary>
    /// Displays and manages a hotkey setting UI element, allowing the user to view or change the assigned key.
    /// </summary>
    /// <remarks>
    /// Call this method within an ImGui frame to render the hotkey setting control. Only one hotkey
    /// change operation should be active at a time; use the <paramref name="otherWaiting"/> parameter to coordinate
    /// multiple hotkey settings.
    /// </remarks>
    /// <param name="label">The text label displayed alongside the hotkey setting.</param>
    /// <param name="id">
    /// A stable, unique identifier used as the ImGui button ID suffix (e.g. the parameter key).
    /// Must be unique within the active ImGui window to prevent button ID collisions when multiple
    /// hotkey controls share the same display label.
    /// </param>
    /// <param name="state">A reference to a boolean value indicating whether the hotkey change mode is active. Set to <see
    /// langword="true"/> to prompt the user for a new key.</param>
    /// <param name="keyCode">A reference to the integer key code representing the currently assigned hotkey. Updated when the user selects a
    /// new key.</param>
    /// <param name="otherWaiting">Indicates whether another hotkey change operation is currently in progress. If <see langword="true"/>, prevents
    /// starting a new change operation.</param>
    public static void DrawHotKeySetting(string label, string id, ref bool state, ref int keyCode, bool otherWaiting)
    {
        if (state)
        {
            ImGui.Text(string.Format(HotkeyCapturingPromptWithLabel, label));
            ImGui.SameLine();
            if (ImGui.Button($"{HotkeyCancelLabel}##{id}"))
            {
                state = false;
                return;
            }

            if (KeyboardInput.TryCaptureKeyboardKey(out var captured))
            {
                keyCode = captured;
                state = false;
            }

            return;
        }

        ImGui.Text($"{label}: {KeyboardInput.GetKeyName(keyCode)}");
        ImGui.SameLine();
        if (ImGui.Button($"{HotkeyChangeLabel}##{id}") && !otherWaiting)
            state = true;
    }

    /// <summary>
    /// Draws a slider control for editing a floating-point value within a specified range.
    /// </summary>
    /// <param name="label">The text label displayed next to the slider control.</param>
    /// <param name="value">A reference to the value to be edited by the slider. The value will be updated if the user interacts with the
    /// slider.</param>
    /// <param name="minValue">The minimum value allowed for the slider.</param>
    /// <param name="maxValue">The maximum value allowed for the slider.</param>
    /// <param name="format">The printf-style format string used to display the value on the slider. Defaults to <c>"%.1f"</c>.</param>
    public static void DrawSlider(string label, ref float value, float minValue, float maxValue, string format = "%.1f") => ImGui.SliderFloat(label, ref value, minValue, maxValue, format);

    /// <summary>
    /// Draws a slider control for editing an integer value within a specified range.
    /// </summary>
    /// <param name="label">The text label displayed next to the slider control.</param>
    /// <param name="value">A reference to the integer value to be edited by the slider.</param>
    /// <param name="minValue">The minimum value allowed for the slider.</param>
    /// <param name="maxValue">The maximum value allowed for the slider.</param>
    /// <param name="format">The printf-style format string used to display the value on the slider. Defaults to <c>"%d"</c>.</param>
    public static void DrawIntSlider(string label, ref int value, int minValue, int maxValue, string format = "%d") => ImGui.SliderInt(label, ref value, minValue, maxValue, format);

    /// <summary>
    /// Draws a checkbox control for editing a boolean value.
    /// </summary>
    /// <param name="label">The text label displayed next to the checkbox.</param>
    /// <param name="value">A reference to the boolean value to be edited.</param>
    public static void DrawCheckbox(string label, ref bool value) => ImGui.Checkbox(label, ref value);

    /// <summary>
    /// Draws a combo box allowing selection from a list of string items.
    /// </summary>
    /// <param name="label">The text label displayed next to the combo box.</param>
    /// <param name="selectedIndex">A reference to the index of the currently selected item. Updated when the user selects a different item.</param>
    /// <param name="items">The array of string items to display in the combo box.</param>
    public static void DrawComboBox(string label, ref int selectedIndex, string[] items) => ImGui.Combo(label, ref selectedIndex, items, items.Length);

    /// <summary>
    /// Draws a horizontal separator followed by a section header label.
    /// </summary>
    /// <param name="label">The text to display as the section header.</param>
    public static void DrawSectionHeader(string label) => ImGui.SeparatorText(label);

    /// <summary>
    /// Draws an inline <c>(?)</c> marker that shows a tooltip when hovered.
    /// Call this immediately after the control it describes using <see cref="ImGui.SameLine()"/>.
    /// </summary>
    /// <param name="description">The tooltip text to display when the marker is hovered.</param>
    public static void DrawHelpMarker(string description)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 24f);
            ImGui.TextUnformatted(description);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }
}
