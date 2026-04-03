namespace Umbra.Config.Attributes;

/// <summary>
/// Declares extra horizontal space between the label column and editing widget for an annotated configuration scope.
/// </summary>
/// <remarks>
/// Applied to a configuration type, this attribute affects all controls in that scope. Applied to a nested-group property, it affects the controls rendered for that nested scope.
/// </remarks>
/// <param name="pixels">The additional pixel gap inserted between the label column and editing widget.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraLabelMarginAttribute(float pixels) : Attribute
{
    /// <summary>
    /// Gets the additional label margin in pixels.
    /// </summary>
    /// <value>The extra horizontal gap inserted between the label column and editing widget.</value>
    public float Pixels { get; } = pixels;
}
