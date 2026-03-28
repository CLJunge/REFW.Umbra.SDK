namespace Umbra.Config.Attributes;

/// <summary>
/// Sets the maximum character length for a <c>string</c> settings parameter's input field.
/// </summary>
/// <param name="length">The maximum number of characters the input field will accept.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraMaxLengthAttribute(uint length) : Attribute
{
    /// <summary>Gets the maximum number of characters allowed in the input field.</summary>
    public uint Length { get; } = length;
}
