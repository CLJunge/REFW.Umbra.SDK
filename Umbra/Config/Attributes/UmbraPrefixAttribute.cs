namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the persisted key prefix applied to an annotated configuration scope.
/// </summary>
/// <remarks>
/// Applied to a root config type, this prefix becomes the first segment of every discovered parameter key in that scope. Applied to a nested-group property, it contributes the nested path segment used when child parameters are registered. Changing the prefix renames the persisted keys produced for that scope and does not migrate existing JSON automatically.
/// </remarks>
/// <param name="prefix">The prefix string to prepend to parameter keys in the annotated scope.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraPrefixAttribute(string prefix) : Attribute
{
    /// <summary>
    /// Gets the declared key prefix.
    /// </summary>
    /// <value>The prefix string applied to parameter keys in the annotated scope.</value>
    public string Prefix { get; } = prefix;
}
