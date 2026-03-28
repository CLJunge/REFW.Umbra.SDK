namespace Umbra.Config.Attributes;

/// <summary>
/// Provides descriptive text for a settings parameter member.
/// </summary>
/// <param name="text">The descriptive text to display.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraDescriptionAttribute(string text) : Attribute
{
    /// <summary>Gets the description text for the parameter.</summary>
    public string Text { get; } = text;
}
