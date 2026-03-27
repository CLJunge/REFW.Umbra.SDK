namespace Umbra.Config.Attributes;

/// <summary>
/// Specifies a human-readable display name for a settings parameter member.
/// </summary>
/// <param name="name">The display name to show in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraDisplayNameAttribute(string name) : Attribute
{
    /// <summary>Gets the display name of the parameter.</summary>
    public string Name { get; } = name;
}
