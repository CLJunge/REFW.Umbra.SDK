namespace Umbra.Config.Attributes;

/// <summary>
/// Specifies a human-readable display name for a settings parameter property or field.
/// This name is used when rendering the parameter in the UI.
/// </summary>
/// <param name="name">The display name to show in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DisplayNameAttribute(string name) : Attribute
{
    /// <summary>Gets the display name of the parameter.</summary>
    public string Name { get; } = name;
}
