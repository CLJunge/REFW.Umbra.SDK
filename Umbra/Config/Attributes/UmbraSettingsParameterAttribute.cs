namespace Umbra.Config.Attributes;

/// <summary>
/// Marks a public instance property as a settings parameter so that
/// <see cref="SettingsStore{TConfig}.Load()"/> discovers and registers it during initialization.
/// </summary>
/// <param name="keyOverride">
/// An optional explicit key used to store and retrieve this parameter.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraSettingsParameterAttribute(string? keyOverride = null) : Attribute
{
    /// <summary>Gets the explicit key override for this parameter.</summary>
    public string? KeyOverride { get; } = keyOverride;
}
