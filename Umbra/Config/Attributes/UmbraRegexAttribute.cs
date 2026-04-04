namespace Umbra.Config.Attributes;

/// <summary>
/// Declares a regular-expression pattern that a string parameter must match.
/// </summary>
/// <param name="pattern">The regular-expression pattern required for valid input.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraRegexAttribute(string pattern) : Attribute
{
    /// <summary>
    /// Gets the regular-expression pattern required for the annotated parameter.
    /// </summary>
    /// <value>The required validation pattern.</value>
    public string Pattern { get; } = pattern;

    /// <summary>
    /// Gets or sets the optional custom validation message shown when the pattern does not match.
    /// </summary>
    /// <value>The custom validation message, or <see langword="null"/> to use Umbra's default message.</value>
    public string? Message { get; set; }
}
