using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves stable structural ImGui ID paths for nested configuration groups.
/// </summary>
/// <remarks>
/// This type isolates nested-group path derivation from <see cref="ConfigDrawerBuilder"/> so the
/// builder remains focused on tree traversal and node composition.
/// </remarks>
internal static class NestedGroupScopePathResolver
{
    /// <summary>
    /// Resolves the stable structural ImGui ID path for a nested-group property.
    /// Property-level <see cref="UmbraSettingsPrefixAttribute"/> wins, followed by the nested type's
    /// type-level prefix, then <see cref="UmbraSettingsParameterAttribute.KeyOverride"/>, and finally
    /// the camel-cased property name.
    /// </summary>
    /// <param name="parentPath">The dot-separated structural path of the parent group.</param>
    /// <param name="propMeta">The cached metadata for the nested-group property being inspected.</param>
    /// <param name="propTypeMeta">The cached metadata for the nested-group type exposed by the property.</param>
    /// <returns>The fully combined dot-separated path used for the nested group's ImGui ID scope.</returns>
    internal static string Resolve(
        string parentPath,
        TypeDrawMetadata.PropertyDrawMetadata propMeta,
        TypeDrawMetadata propTypeMeta)
    {
        var segment = propMeta.SettingsPrefix
            ?? propTypeMeta.SettingsPrefix
            ?? propMeta.SettingsParameterKeyOverride
            ?? propMeta.Property.Name.ToCamelCase()
            ?? propMeta.Property.Name;

        return Combine(parentPath, segment);
    }

    /// <summary>
    /// Combines two dot-separated structural path segments into a single stable path, omitting the
    /// separator when either segment is empty.
    /// </summary>
    /// <param name="left">The parent path segment.</param>
    /// <param name="right">The child path segment.</param>
    /// <returns>
    /// <paramref name="right"/> when <paramref name="left"/> is empty;
    /// <paramref name="left"/> when <paramref name="right"/> is empty;
    /// otherwise <c>"left.right"</c>.
    /// </returns>
    private static string Combine(string left, string right)
    {
        if (string.IsNullOrEmpty(left)) return right;
        if (string.IsNullOrEmpty(right)) return left;
        return $"{left}.{right}";
    }
}
