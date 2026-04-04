namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the minimum character count required for a string parameter.
/// </summary>
/// <param name="length">The inclusive minimum number of characters required.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraMinLengthAttribute(uint length) : Attribute
{
    /// <summary>
    /// Gets the declared inclusive minimum character count.
    /// </summary>
    /// <value>The minimum number of characters accepted for the annotated string parameter.</value>
    public uint Length { get; } = length;
}
