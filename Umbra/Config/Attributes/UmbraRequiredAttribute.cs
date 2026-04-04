namespace Umbra.Config.Attributes;

/// <summary>
/// Declares that the annotated parameter must receive a non-empty value.
/// </summary>
/// <remarks>
/// For nullable value types and reference types, this attribute rejects <see langword="null"/>. For <see cref="string"/> parameters, it also rejects empty text and, by default, whitespace-only text.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraRequiredAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether whitespace-only strings satisfy this requirement.
    /// </summary>
    /// <value><see langword="true"/> to allow whitespace-only strings; otherwise, <see langword="false"/>.</value>
    public bool AllowWhitespace { get; set; }
}
