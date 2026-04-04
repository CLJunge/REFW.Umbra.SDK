namespace Umbra.Config.Attributes;

/// <summary>
/// Declares a regular-expression pattern that a string parameter must match.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraRegexAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UmbraRegexAttribute"/> class.
    /// </summary>
    /// <param name="pattern">The regular-expression pattern required for valid input.</param>
    /// <exception cref="ArgumentException"><paramref name="pattern"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public UmbraRegexAttribute(string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);
        Pattern = pattern;
    }

    /// <summary>
    /// Gets the regular-expression pattern required for the annotated parameter.
    /// </summary>
    /// <value>The required validation pattern.</value>
    public string Pattern { get; }

    /// <summary>
    /// Gets or sets the optional custom validation message shown when the pattern does not match.
    /// </summary>
    /// <value>The custom validation message, or <see langword="null"/> to use Umbra's default message.</value>
    public string? Message { get; set; }
}
