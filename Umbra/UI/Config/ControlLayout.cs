using Hexa.NET.ImGui;

namespace Umbra.UI.Config;

/// <summary>
/// Stores the precomputed two-column layout state for one parameter row.
/// </summary>
/// <remarks>
/// <see cref="ControlLayoutFactory"/> creates this value during draw-tree construction after the label has already been registered with its owning <see cref="LabelAlignmentGroup"/>. On the first draw frame, <see cref="Pre"/> ensures that the shared label-width seed has been committed before positioning the editing widget.
/// </remarks>
internal readonly struct ControlLayout
{
    private readonly string _label;
    private readonly string? _desc;
    private readonly LabelAlignmentGroup _alignGroup;
    private readonly float _controlWidth;

    /// <summary>
    /// Gets the hidden ImGui widget label associated with the parameter row.
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
    /// Performs the standard pre-widget layout step for one two-column parameter row.
    /// </summary>
    /// <remarks>
    /// This method ensures the shared alignment group is seeded, renders the visible label, shows the optional description tooltip, advances to the shared control column, and sets the next item width.
    /// </remarks>
    internal void Pre()
    {
        _alignGroup.EnsureSeeded();
        var startX = ImGui.GetCursorPosX();
        ImGui.Text(_label);
        if (_desc is not null) ImGuiWidgets.DrawHoverTooltip(_desc);
        ImGui.SameLine();
        var columnX = startX + _alignGroup.LabelWidth + _alignGroup.Margin + ImGui.GetStyle().ItemSpacing.X;
        if (ImGui.GetCursorPosX() < columnX)
            ImGui.SetCursorPosX(columnX);
        ImGui.SetNextItemWidth(_controlWidth);
    }
}
