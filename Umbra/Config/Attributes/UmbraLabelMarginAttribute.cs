namespace Umbra.Config.Attributes;

/// <summary>
/// Adds extra pixels of space between the label column and the editing control for all
/// parameters in the decorated settings class, or in the nested settings group exposed by
/// the decorated property.
/// </summary>
/// <param name="pixels">Extra pixels to insert between the label column and the editing widget.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraLabelMarginAttribute(float pixels) : Attribute
{
    /// <summary>Gets the extra pixel gap inserted between the label column and the editing widget.</summary>
    public float Pixels { get; } = pixels;
}
