using System.Diagnostics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Config;

/// <summary>
/// Tracks the maximum label-column width for all parameters in a shared scope
/// (a <see cref="Nodes.CategoryNode"/> or the root parameter list). Used by
/// <see cref="ControlFactory"/> to align all editing widgets to a common x position within
/// the group.
/// </summary>
/// <remarks>
/// <para>
/// Labels are registered at draw-tree build time via <see cref="Register"/> without requiring
/// an active ImGui context. On the first draw frame, <see cref="EnsureSeeded"/> measures all
/// registered labels in a single <see cref="ImGui.CalcTextSize(string)"/> batch, commits the
/// maximum to <see cref="LabelWidth"/>, and marks the group as permanently seeded. All
/// subsequent calls to <see cref="EnsureSeeded"/> are no-ops: the committed width is never
/// recomputed and never decreases, so hiding parameters via
/// <see cref="Umbra.Config.Attributes.UmbraHideIfAttribute{T}"/> cannot narrow the column.
/// </para>
/// <para>
/// In normal usage, <see cref="ControlLayout.Pre"/> calls <see cref="EnsureSeeded"/> before
/// any labels are laid out on the first draw frame, so <see cref="LabelWidth"/> is populated
/// up front and alignment is correct from the first render. The only time a control may
/// briefly render against a narrower column is when its label is registered after seeding
/// (for example, dynamically created controls); such late registrations are measured
/// immediately in <see cref="Register"/> and can only widen the committed width for
/// subsequent frames.
/// </para>
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class LabelAlignmentGroup
{
    private readonly List<(string label, bool hasDescription)> _entries = [];
    private float _committedMax;
    private bool _seeded;

    /// <summary>
    /// The maximum label-column width committed on the first draw frame by
    /// <see cref="EnsureSeeded"/>. Zero until that call; stable and non-decreasing thereafter.
    /// </summary>
    internal float LabelWidth => _committedMax;

    /// <summary>
    /// Extra pixels inserted between the end of the label column and the start of the
    /// editing control, on top of the standard <c>ImGui.GetStyle().ItemSpacing.X</c> gap.
    /// Defaults to <c>0f</c> (no additional margin). Configured at draw-tree build time by
    /// <see cref="ConfigDrawerBuilder"/> from a <c>[LabelMargin]</c> attribute; never written
    /// at draw time.
    /// </summary>
    internal float Margin { get; set; } = 0f;

    /// <summary>
    /// Registers a label entry to be measured on the first draw frame.
    /// Called at draw-tree build time from <see cref="ControlLayout"/>'s constructor;
    /// no ImGui context is required.
    /// </summary>
    /// <remarks>
    /// If <see cref="EnsureSeeded"/> has already run (which does not occur in normal usage
    /// since the draw tree is built before the first draw), the label is measured immediately
    /// via <see cref="ImGui.CalcTextSize(string)"/> and folded into <see cref="LabelWidth"/>.
    /// </remarks>
    /// <param name="label">The visible label text for the parameter row.</param>
    /// <param name="hasDescription">
    /// <see langword="true"/> when the parameter carries a <c>[Description]</c> and a
    /// <c>(?)</c> help marker will be drawn inline. The marker width plus item spacing
    /// is included in the width measurement so the column accounts for the marker.
    /// </param>
    internal void Register(string label, bool hasDescription)
    {
        if (_seeded)
        {
            // Late registration after seeding: measure immediately so the column still widens
            // to accommodate the new label rather than silently using a stale narrower width.
            var w2 = ImGui.CalcTextSize(label).X;
            if (hasDescription)
                w2 += ImGui.GetStyle().ItemSpacing.X + ImGui.CalcTextSize("(?)").X;
            if (w2 > _committedMax) _committedMax = w2;
            return;
        }
        _entries.Add((label, hasDescription));
    }

    /// <summary>
    /// Measures all registered labels in a single batch and commits the maximum width to
    /// <see cref="LabelWidth"/>. Must be called from within an active ImGui frame.
    /// Called by <see cref="ControlLayout.Pre"/> on the first draw frame; all subsequent
    /// calls are immediate no-ops.
    /// </summary>
    internal void EnsureSeeded()
    {
        if (_seeded) return;
        _seeded = true;
        foreach (var (label, hasDesc) in _entries)
        {
            var w = ImGui.CalcTextSize(label).X;
            if (hasDesc)
                w += ImGui.GetStyle().ItemSpacing.X + ImGui.CalcTextSize("(?)").X;
            if (w > _committedMax) _committedMax = w;
        }
        _entries.Clear();
    }

    /// <summary>Builds a human-readable summary string for debugger visualizers.</summary>
    /// <returns>
    /// A string containing the current <see cref="LabelWidth"/> and, when non-zero,
    /// the configured <see cref="Margin"/>.
    /// </returns>
    private string GetDebuggerDisplay()
    {
        var displayString = "LabelAlignmentGroup: LabelWidth=" + LabelWidth;
        if (Margin != 0f)
            displayString += ", Margin=" + Margin;
        return displayString;
    }
}
