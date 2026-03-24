namespace Umbra.Config.Attributes;

/// <summary>
/// Adds extra pixels of space between the label column and the editing control for all
/// parameters in the decorated settings class, or in the nested settings group exposed by
/// the decorated property.
/// </summary>
/// <remarks>
/// <para>
/// The margin is applied on top of the standard <c>ImGui.GetStyle().ItemSpacing.X</c> gap
/// that already separates labels from controls. Use it to widen the column gap when labels
/// and controls feel too close together for a specific category or root settings group.
/// </para>
/// <para>
/// The attribute affects the <see cref="Umbra.Config.UI.LabelAlignmentGroup"/> shared by all
/// parameters that belong to the same category or root scope as the decorated property or type.
/// For nested settings groups, prefer applying it to the parent property so the full group
/// presentation stays co-located with the property declaration. When multiple sources contribute
/// to the same alignment group with different margin values, the last one processed determines the
/// final margin.
/// </para>
/// </remarks>
/// <param name="pixels">Extra pixels to insert between the label column and the editing widget. Must be ≥ 0.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class LabelMarginAttribute(float pixels) : Attribute
{
    /// <summary>Gets the extra pixel gap inserted between the label column and the editing widget.</summary>
    public float Pixels { get; } = pixels;
}
