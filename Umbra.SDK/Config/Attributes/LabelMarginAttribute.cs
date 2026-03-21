namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Adds extra pixels of space between the label column and the editing control for all
/// parameters in the decorated settings class.
/// </summary>
/// <remarks>
/// <para>
/// The margin is applied on top of the standard <c>ImGui.GetStyle().ItemSpacing.X</c> gap
/// that already separates labels from controls. Use it to widen the column gap when labels
/// and controls feel too close together for a specific category or root settings group.
/// </para>
/// <para>
/// The attribute is class-scoped: it affects the <see cref="Umbra.SDK.Config.UI.LabelAlignmentGroup"/>
/// shared by all parameters that belong to the same category or root scope as the decorated class.
/// When multiple classes contribute to the same alignment group with different margin values,
/// the last class processed determines the final margin.
/// </para>
/// </remarks>
/// <param name="pixels">Extra pixels to insert between the label column and the editing widget. Must be ≥ 0.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LabelMarginAttribute(float pixels) : Attribute
{
    /// <summary>Gets the extra pixel gap inserted between the label column and the editing widget.</summary>
    public float Pixels { get; } = pixels;
}
