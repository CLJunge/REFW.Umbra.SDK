namespace Umbra.SDK.Config.Attributes;

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
