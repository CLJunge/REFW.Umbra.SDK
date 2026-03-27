namespace Umbra.Config.Attributes;

/// <summary>
/// Specifies a human-readable display name for a settings parameter member.
/// This name is used when rendering the parameter in the UI.
/// </summary>
/// <remarks>
/// In the built-in settings pipeline, display names are read from public instance properties
/// discovered by <see cref="SettingsRegistrar"/>.
/// </remarks>
/// <param name="name">The display name to show in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[Obsolete("Use UmbraDisplayNameAttribute instead for the collision-safe Umbra-prefixed name.")]
public class DisplayNameAttribute(string name) : Attribute
{
    /// <summary>Gets the display name of the parameter.</summary>
    public string Name { get; } = name;
}
