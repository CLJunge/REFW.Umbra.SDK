namespace Umbra.Config.Attributes;

/// <summary>
/// Marks a public instance property as a settings parameter so that
/// <see cref="SettingsStore{TConfig}.Load()"/> discovers and registers it during initialization.
/// Optionally allows overriding the storage key used to persist the value,
/// instead of deriving it from the member name (and any applicable prefix).
/// </summary>
/// <remarks>
/// The built-in registration pipeline reflects only public instance properties.
/// Although this attribute can be applied to fields, field-backed parameters are ignored by
/// <see cref="SettingsRegistrar"/> and <see cref="UI.Config.ConfigDrawer{TConfig}"/> unless a custom
/// reflection pipeline handles them explicitly.
/// </remarks>
/// <param name="keyOverride">
/// An optional explicit key used to store and retrieve this parameter.
/// When <see langword="null"/>, the key is derived from the member name
/// combined with the prefix defined by <see cref="SettingsPrefixAttribute"/>, if present.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SettingsParameterAttribute(string? keyOverride = null) : Attribute
{
    /// <summary>
    /// Gets the explicit key override for this parameter, or <see langword="null"/>
    /// if the key should be derived automatically from the member name.
    /// </summary>
    public string? KeyOverride { get; } = keyOverride;
}
