namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Defines the minimum and maximum allowable values for a numeric settings parameter.
/// When present, the parameter renders as a slider (<c>SliderFloat</c> / <c>SliderInt</c>)
/// rather than an unconstrained drag control.
/// </summary>
/// <param name="min">The minimum allowable value.</param>
/// <param name="max">The maximum allowable value.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class RangeAttribute(double min, double max) : Attribute
{
    /// <summary>Gets the minimum allowable value for the parameter.</summary>
    public double Min { get; } = min;

    /// <summary>Gets the maximum allowable value for the parameter.</summary>
    public double Max { get; } = max;
}
