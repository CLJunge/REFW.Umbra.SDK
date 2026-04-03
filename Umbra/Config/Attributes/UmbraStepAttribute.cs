namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the numeric step value used for unconstrained numeric controls.
/// </summary>
/// <remarks>
/// Umbra also uses this value when inferring fallback float-format precision if <see cref="UmbraFormatAttribute"/> is absent.
/// </remarks>
/// <param name="step">The declared numeric step value.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraStepAttribute(double step) : Attribute
{
    /// <summary>
    /// Gets the declared numeric step value.
    /// </summary>
    /// <value>The step value used for unconstrained numeric controls.</value>
    public double Step { get; } = step;
}
