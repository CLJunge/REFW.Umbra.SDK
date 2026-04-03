namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the numeric bounds applied to an annotated parameter.
/// </summary>
/// <remarks>
/// Umbra stores these values in <see cref="ParameterMetadata"/> for both validation and UI rendering decisions.
/// </remarks>
/// <param name="min">The inclusive minimum value.</param>
/// <param name="max">The inclusive maximum value.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraRangeAttribute(double min, double max) : Attribute
{
    /// <summary>
    /// Gets the inclusive minimum value.
    /// </summary>
    /// <value>The declared lower bound.</value>
    public double Min { get; } = min;

    /// <summary>
    /// Gets the inclusive maximum value.
    /// </summary>
    /// <value>The declared upper bound.</value>
    public double Max { get; } = max;
}
