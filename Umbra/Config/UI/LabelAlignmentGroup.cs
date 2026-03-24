using System.Diagnostics;
using Hexa.NET.ImGui;

namespace Umbra.Config.UI;

/// <summary>
/// Tracks the maximum label-column width observed across all parameters in a shared scope
/// (a <see cref="Nodes.CategoryNode"/> or the root parameter list). Used by
/// <see cref="ControlFactory"/> to align all editing widgets to a common x position within
/// the group.
/// </summary>
/// <remarks>
/// Width is measured at draw time via <see cref="ImGui.CalcTextSize(string)"/>, so a one-frame
/// convergence delay occurs on the very first render. From the second frame onward
/// <see cref="LabelWidth"/> is stable and all controls in the group are column-aligned.
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class LabelAlignmentGroup
{
    private float _committedMax;
    private float _runningMax;
    private int _lastFrame = -1;

    /// <summary>
    /// The maximum label-column width committed at the end of the previous frame.
    /// Stable throughout the current frame; used by <see cref="ControlFactory"/> to
    /// compute the column x position at which all controls in this group are placed.
    /// </summary>
    public float LabelWidth => _committedMax;

    /// <summary>
    /// Extra pixels inserted between the end of the label column and the start of the
    /// editing control, on top of the standard <c>ImGui.GetStyle().ItemSpacing.X</c> gap.
    /// Defaults to <c>0f</c> (no additional margin). Set this on a per-group basis to
    /// widen the gap for a specific category or the root parameter list.
    /// </summary>
    public float Margin { get; set; } = 0f;

    /// <summary>
    /// Measures <paramref name="label"/> (plus optional help-marker space when
    /// <paramref name="hasDescription"/> is <see langword="true"/>) and accumulates the
    /// running maximum for the current frame. At each frame boundary the previous
    /// frame's running maximum is committed to <see cref="LabelWidth"/>.
    /// </summary>
    /// <param name="label">The visible label text for the current parameter row.</param>
    /// <param name="hasDescription">
    /// <see langword="true"/> when the parameter carries a <c>[Description]</c> and a
    /// <c>(?)</c> help marker will be drawn inline. The marker width plus item spacing
    /// is included in the width measurement so the column accounts for the marker.
    /// </param>
    public void Observe(string label, bool hasDescription)
    {
        var frame = ImGui.GetFrameCount();
        if (frame != _lastFrame)
        {
            if (_runningMax > 0f) _committedMax = _runningMax;
            _runningMax = 0f;
            _lastFrame = frame;
        }

        var w = ImGui.CalcTextSize(label).X;
        if (hasDescription)
            w += ImGui.GetStyle().ItemSpacing.X + ImGui.CalcTextSize("(?)").X;
        if (w > _runningMax) _runningMax = w;
    }

    /// <summary>
    /// Builds a human-readable summary string for debugger visualizers.
    /// </summary>
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
