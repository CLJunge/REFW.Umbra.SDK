namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Marks a class or struct so that its settings parameters are automatically
/// discovered and registered when <see cref="SettingsStore{TConfig}.Load()"/>
/// is called on a <c>SettingsStore</c> that wraps the decorated type.
/// Apply this attribute to any settings group that should participate in
/// auto-registration without requiring manual registration calls.
/// </summary>
/// <remarks>
/// If this attribute is absent from the root config type passed to
/// <see cref="SettingsStore{TConfig}.Load()"/>, no parameters are discovered
/// and the returned instance will hold only its property default values.
/// Nested group types exposed via <c>SettingsParameterAttribute</c> properties must also
/// carry this attribute to be traversed.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class AutoRegisterSettingsAttribute : Attribute { }


/// <summary>
/// Specifies a key prefix that is prepended to every parameter key within
/// the decorated settings class or struct.
/// Use this to namespace settings and avoid key collisions across multiple
/// settings groups registered with <c>SettingsStore</c>.
/// </summary>
/// <param name="prefix">
/// The prefix string to prepend to each parameter key, e.g. <c>"Camera"</c>
/// results in keys such as <c>"Camera.FieldOfView"</c>.
/// </param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class SettingsPrefixAttribute(string prefix) : Attribute
{
    /// <summary>Gets the prefix string applied to all parameter keys in the decorated type.</summary>
    public string Prefix { get; } = prefix;
}

/// <summary>
/// Marks a property or field as a settings parameter so that
/// <c>SettingsStore</c> discovers and registers it during initialization.
/// Optionally allows overriding the storage key used to persist the value,
/// instead of deriving it from the member name (and any applicable prefix).
/// </summary>
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
