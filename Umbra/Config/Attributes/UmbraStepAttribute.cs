namespace Umbra.Config.Attributes;

/// <summary>
/// Specifies the drag speed for an unconstrained numeric settings parameter.
/// </summary>
/// <param name="step">The drag speed for unconstrained numeric controls.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraStepAttribute(double step) : Attribute
{
    /// <summary>Gets the drag speed for the parameter.</summary>
    public double Step { get; } = step;
}
