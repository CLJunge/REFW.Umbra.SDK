using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Holds the pre-computed layout state for a single parameter row in the two-column
/// settings UI.
/// </summary>
/// <remarks>
/// <para>
/// Constructed at draw-tree build time by <see cref="ControlFactory"/>. The constructor
/// immediately calls <see cref="LabelAlignmentGroup.Register"/> so the label is enrolled in
/// the group's build-time batch before any ImGui frame is active; no font measurement happens
/// at this point.
/// </para>
/// <para>
/// On the first draw frame, <see cref="Pre"/> calls <see cref="LabelAlignmentGroup.EnsureSeeded"/>,
/// which measures every registered label in one <see cref="ImGui.CalcTextSize(string)"/> pass,
/// commits the group maximum, and marks the group as permanently seeded. Subsequent
/// <see cref="Pre"/> calls skip seeding entirely. Storing this state as a value struct
/// eliminates one closure-object allocation and one delegate allocation per parameter
/// per <see cref="ConfigDrawer{TConfig}"/> construction.
/// </para>
/// </remarks>
internal readonly struct ControlLayout
{
    private readonly string _label;
    private readonly string? _desc;
    private readonly LabelAlignmentGroup _alignGroup;
    private readonly float _controlWidth;

    /// <summary>
    /// The hidden ImGui control label (<c>"##" + parameter.Key</c>) pre-computed during
    /// <see cref="SettingsStore{TConfig}.Load()"/> and stored in <see cref="ParameterMetadata.HiddenLabel"/>.
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
        alignGroup.Register(label, desc is not null);
    }

    /// <summary>
    /// Performs the standard two-column pre-draw step: seeds the shared alignment group on
    /// the first call, renders the label text (and optional help marker), advances the cursor
    /// to the shared column x position, and sets the next item width.
    /// Must be called immediately before the ImGui widget call in each per-frame draw action.
    /// </summary>
    /// <remarks>
    /// The first call triggers <see cref="LabelAlignmentGroup.EnsureSeeded"/>, which measures
    /// all registered labels in a single batch and permanently commits the group maximum.
    /// Every subsequent call is a no-op for seeding and simply uses the already-committed
    /// <see cref="LabelAlignmentGroup.LabelWidth"/> to position the cursor.
    /// </remarks>
    internal void Pre()
    {
        _alignGroup.EnsureSeeded();
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
