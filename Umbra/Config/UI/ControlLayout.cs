using Hexa.NET.ImGui;
using Umbra.UI;

namespace Umbra.Config.UI;

/// <summary>
/// Holds the pre-computed layout state for a single parameter row in the two-column
/// settings UI. Replaces the heap-allocated <c>Action pre</c> delegate and closure
/// previously returned by <c>ControlFactory.GetControlLayout</c>.
/// </summary>
/// <remarks>
/// <see cref="Pre"/> inlines the layout logic that was previously in the <c>void pre()</c>
/// local function inside <c>GetControlLayout</c>. Storing this state as a value struct
/// eliminates one closure-object allocation and one delegate allocation per parameter
/// per <see cref="ConfigDrawer{TConfig}"/> construction.
/// </remarks>
internal readonly struct ControlLayout
{
    private readonly string _label;
    private readonly string? _desc;
    private readonly LabelAlignmentGroup _alignGroup;
    private readonly float _controlWidth;

    /// <summary>
    /// The hidden ImGui control label (<c>"##" + parameter.Key</c>) pre-computed during
    /// <c>SettingsStore.Load()</c> and stored in <see cref="ParameterMetadata.HiddenLabel"/>.
    /// </summary>
    internal readonly string HiddenLabel;

    internal ControlLayout(
        string label,
        string? desc,
        LabelAlignmentGroup alignGroup,
        float controlWidth,
        string hiddenLabel)
    {
        _label = label;
        _desc = desc;
        _alignGroup = alignGroup;
        _controlWidth = controlWidth;
        HiddenLabel = hiddenLabel;
    }

    /// <summary>
    /// Performs the standard two-column pre-draw step: observes the label width for the
    /// shared alignment group, renders the label text (and optional help marker), advances
    /// the cursor to the shared column x position, and sets the next item width.
    /// Must be called immediately before the ImGui widget call in each per-frame draw action.
    /// </summary>
    internal void Pre()
    {
        _alignGroup.Observe(_label, _desc is not null);
        var startX = ImGui.GetCursorPosX();
        ImGui.Text(_label);
        if (_desc is not null)
        {
            ImGui.SameLine();
            ImGuiWidgets.DrawHelpMarker(_desc);
        }
        ImGui.SameLine();
        // Advance to the shared column position (plus optional per-group margin); never
        // move backward so that on frame 1 (before the committed max is available) labels
        // wider than the current max still place their control immediately to the right
        // rather than overlapping.
        var columnX = startX + _alignGroup.LabelWidth + _alignGroup.Margin + ImGui.GetStyle().ItemSpacing.X;
        if (ImGui.GetCursorPosX() < columnX)
            ImGui.SetCursorPosX(columnX);
        ImGui.SetNextItemWidth(_controlWidth);
    }
}
