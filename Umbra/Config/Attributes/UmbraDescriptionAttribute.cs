namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the descriptive help text associated with a parameter in the configuration UI.
/// </summary>
/// <param name="text">The descriptive text to expose through the parameter metadata.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraDescriptionAttribute(string text) : Attribute
{
    /// <summary>
    /// Gets the descriptive text declared for the annotated member.
    /// </summary>
    /// <value>The help text associated with the parameter.</value>
    public string Text { get; } = text;
}
