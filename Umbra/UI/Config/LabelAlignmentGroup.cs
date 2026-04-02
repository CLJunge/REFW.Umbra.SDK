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
/// maximum to <see cref="LabelWidth"/>, and marks the group as permanently seeded. If no current
/// ImGui context exists yet, <see cref="EnsureSeeded"/> returns immediately and leaves the group
/// unseeded so a later call during a valid frame can perform the measurement safely. After a
/// successful seed, subsequent calls are no-ops: the committed width is never recomputed and
/// never decreases, so hiding parameters via
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
    private readonly List<string> _labels = [];
    private float _committedMax;
    private bool _seeded;

    /// <summary>
    /// The maximum label-column width committed on the first draw frame by
    /// <see cref="EnsureSeeded"/>. Zero until that call; stable and non-decreasing thereafter.
    /// </summary>
    internal float LabelWidth => _committedMax;

    /// <summary>
    /// Extra pixels inserted between the end of the label column and the start of the
    /// editing control, on top of the standard <see cref="ImGui.GetStyle()"/> <c>.ItemSpacing.X</c>
    /// gap.
    /// Defaults to <c>0f</c> (no additional margin). Configured at draw-tree build time by
    /// <see cref="ConfigDrawerBuilder"/> from
    /// <see cref="Umbra.Config.Attributes.UmbraLabelMarginAttribute"/> (<c>[UmbraLabelMargin]</c>);
    /// never written at draw time.
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
    /// <see langword="true"/> when the parameter carries
    /// <see cref="Umbra.Config.Attributes.UmbraDescriptionAttribute"/> (<c>[UmbraDescription]</c>).
    /// Descriptions render as hover tooltips on the label itself, so this flag is accepted for API
    /// compatibility but does not change width measurement.
    /// </param>
    internal void Register(string label, bool hasDescription)
    {
        _ = hasDescription;
        if (_seeded)
        {
            // Late registration after seeding: measure immediately so the column still widens
            // to accommodate the new label rather than silently using a stale narrower width.
            var width = MeasureLabelWidth(label);
            if (width > _committedMax) _committedMax = width;
            return;
        }

        _labels.Add(label);
    }

    /// <summary>
    /// Measures all registered labels in a single batch and commits the maximum width to
    /// <see cref="LabelWidth"/> when an ImGui context is available.
    /// Called by <see cref="ControlLayout.Pre"/> on the first draw frame; all subsequent
    /// calls are immediate no-ops after a successful seed.
    /// </summary>
    internal void EnsureSeeded()
    {
        if (_seeded) return;
        if (ImGui.GetCurrentContext().IsNull) return;

        CommitMeasuredMax(_labels);

        _labels.Clear();
        _seeded = true;
    }

    /// <summary>
    /// Measures each pending label in <paramref name="labels"/> and folds the maximum width into
    /// <see cref="LabelWidth"/>.
    /// </summary>
    /// <param name="labels">The pending labels to measure.</param>
    private void CommitMeasuredMax(List<string> labels)
    {
        for (var i = 0; i < labels.Count; i++)
        {
            var width = ImGui.CalcTextSize(labels[i]).X;
            if (width > _committedMax)
                _committedMax = width;
        }
    }

    /// <summary>
    /// Measures one label using the current ImGui font.
    /// </summary>
    /// <param name="label">The label text to measure.</param>
    /// <returns>The measured width used for label-column alignment.</returns>
    private static float MeasureLabelWidth(string label) => ImGui.CalcTextSize(label).X;

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
