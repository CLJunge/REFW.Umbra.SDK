using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Resolves the stable structural ImGui ID path used for nested configuration groups.
/// </summary>
/// <remarks>
/// This helper centralizes nested-group path derivation so the collector and builder can rely on one consistent precedence order for scope segments.
/// </remarks>
internal static class NestedScopePathResolver
{
    /// <summary>
    /// Resolves the stable scope path for a nested-group property.
    /// </summary>
    /// <remarks>
    /// Segment selection prefers the property-level <see cref="UmbraPrefixAttribute"/>, then the nested type's prefix, then <see cref="UmbraParameterAttribute.KeyOverride"/>, and finally the camel-cased property name. The selected segment must be non-empty.
    /// </remarks>
    internal static string Resolve(
        string parentPath,
        TypeDrawMetadata.PropertyDrawMetadata propMeta,
        TypeDrawMetadata propTypeMeta)
    {
        var segment = propMeta.ConfigPrefix;
        segment ??= propTypeMeta.ConfigPrefix;
        segment ??= propMeta.ConfigParameterKeyOverride;
        segment ??= propMeta.Property.Name.ToCamelCase() ?? propMeta.Property.Name;

        if (string.IsNullOrEmpty(segment))
        {
            throw new InvalidOperationException(
                $"Nested group property '{propMeta.Property.DeclaringType?.FullName ?? propMeta.Property.ReflectedType?.FullName ?? "<unknown>"}.{propMeta.Property.Name}' resolves to an empty scope segment. Nested group identifiers must be non-empty.");
        }

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
