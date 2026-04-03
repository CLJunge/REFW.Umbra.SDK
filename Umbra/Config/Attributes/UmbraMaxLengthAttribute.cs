namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the maximum character count accepted by a string parameter's text input control.
/// </summary>
/// <param name="length">The maximum character count accepted by the input control.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraMaxLengthAttribute(uint length) : Attribute
{
    /// <summary>
    /// Gets the declared maximum character count.
    /// </summary>
    /// <value>The maximum number of characters accepted by the input control.</value>
    public uint Length { get; } = length;
}
