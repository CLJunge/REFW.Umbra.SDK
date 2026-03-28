namespace Umbra.Config.Attributes;

/// <summary>
/// Specifies a key prefix that is prepended to every parameter key within
/// the decorated settings class or struct, or within the nested settings group
/// exposed by the decorated property.
/// </summary>
/// <remarks>
/// Changing the prefix changes the fully-qualified persisted keys produced for every parameter in
/// that scope. This is a supported way to rename or regroup settings keys, but it does not migrate
/// existing JSON automatically: previously saved values under the old prefix will no longer match
/// during load unless the file is updated to the new key names.
/// </remarks>
/// <param name="prefix">
/// The prefix string to prepend to each parameter key.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraSettingsPrefixAttribute(string prefix) : Attribute
{
    /// <summary>
    /// Gets the prefix string applied to parameter keys in the decorated scope.
    /// </summary>
    public string Prefix { get; } = prefix;
}
