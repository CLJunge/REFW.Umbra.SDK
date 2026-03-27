namespace Umbra.Config.Attributes;

/// <summary>
/// Sets the maximum character length for a <c>string</c> settings parameter's input field.
/// When absent, the UI defaults to <c>256</c> characters.
/// </summary>
/// <param name="length">The maximum number of characters the input field will accept.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[Obsolete("Use UmbraMaxLengthAttribute instead for the collision-safe Umbra-prefixed name.")]
public class MaxLengthAttribute(uint length) : Attribute
{
    /// <summary>Gets the maximum number of characters allowed in the input field.</summary>
    public uint Length { get; } = length;
}
