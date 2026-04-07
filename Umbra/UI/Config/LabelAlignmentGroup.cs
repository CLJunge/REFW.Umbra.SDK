using System.Diagnostics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Config;

/// <summary>
/// Tracks the shared label-column width for one configuration UI scope.
/// </summary>
/// <remarks>
/// Labels are registered during draw-tree construction without requiring an active ImGui context. On the first valid draw frame, <see cref="EnsureSeeded"/> measures the registered labels in one batch, commits the maximum width, and leaves the group permanently seeded. Late registrations after seeding can only widen the committed width for subsequent frames.
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class LabelAlignmentGroup
{
    private readonly List<string> _labels = [];
    private float _committedMax;
    private bool _seeded;

    /// <summary>
    /// Gets the committed maximum label width for this alignment group.
    /// </summary>
    /// <value>The shared label-column width, or <c>0f</c> before seeding has succeeded.</value>
    internal float LabelWidth => _committedMax;

    /// <summary>
    /// Gets or sets the additional horizontal margin inserted between the label column and editing widget.
    /// </summary>
    /// <value>The extra margin in pixels.</value>
    internal float Margin { get; set; } = 0f;

    /// <summary>
    /// Registers one label for later measurement by this alignment group.
    /// </summary>
    /// <param name="label">The visible label text for the parameter row.</param>
    /// <param name="hasDescription">Accepted for API compatibility with control-layout construction. Description tooltips do not change width measurement.</param>
    internal void Register(string label, bool hasDescription)
    {
        _ = hasDescription;
        if (_seeded)
        {
            var width = MeasureLabelWidth(label);
            if (width > _committedMax) _committedMax = width;
            return;
        }

        _labels.Add(label);
    }

    /// <summary>
    /// Measures all registered labels and commits the maximum width when an ImGui context is available.
    /// </summary>
    /// <remarks>
    /// After the first successful seed, subsequent calls are no-ops.
    /// </remarks>
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
    private string GetDebuggerDisplay()
    {
        var displayString = "LabelAlignmentGroup: LabelWidth=" + LabelWidth;
        if (Margin != 0f)
            displayString += ", Margin=" + Margin;
        return displayString;
    }
}
