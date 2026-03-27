namespace Umbra.Config.Attributes;

/// <summary>
/// Provides descriptive text for a settings parameter member.
/// The description is typically shown as a tooltip or help text in the UI.
/// </summary>
/// <remarks>
/// In the built-in settings pipeline, descriptions are read from public instance properties
/// discovered by <see cref="SettingsRegistrar"/>.
/// </remarks>
/// <param name="text">The descriptive text to display.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DescriptionAttribute(string text) : Attribute
{
    /// <summary>Gets the description text for the parameter.</summary>
    public string Text { get; } = text;
}
