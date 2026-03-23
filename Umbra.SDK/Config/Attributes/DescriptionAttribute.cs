namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Provides a description for a settings parameter property or field.
/// The description is typically shown as a tooltip or help text in the UI.
/// </summary>
/// <param name="text">The descriptive text to display.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DescriptionAttribute(string text) : Attribute
{
    /// <summary>Gets the description text for the parameter.</summary>
    public string Text { get; } = text;
}
