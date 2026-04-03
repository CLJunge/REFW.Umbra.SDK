namespace Umbra.Config.Attributes;

/// <summary>
/// Marks a member as a settings parameter declaration for Umbra's registration pipeline.
/// </summary>
/// <remarks>
/// <see cref="SettingsStore{TConfig}.Load()"/> currently discovers only public instance
/// properties marked with this attribute. Field targets remain allowed for attribute-shape
/// consistency with related metadata attributes and for potential reflective tooling, but fields
/// are ignored by <see cref="SettingsRegistrar"/> during normal settings registration.
/// </remarks>
/// <param name="keyOverride">
/// An optional explicit key used to store and retrieve this parameter. When supplied, the override
/// must be non-empty.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraParameterAttribute(string? keyOverride = null) : Attribute
{
    /// <summary>
    /// Gets the explicit persisted-key override for this parameter declaration.
    /// </summary>
    public string? KeyOverride { get; } = keyOverride;
}
