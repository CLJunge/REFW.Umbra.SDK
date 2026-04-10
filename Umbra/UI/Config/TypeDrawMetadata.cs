using System.Collections.Concurrent;
using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Stores the cached draw metadata for one reflected configuration type.
/// </summary>
/// <remarks>
/// This type owns the immutable metadata snapshot and the shared per-type cache consulted by configuration-drawer construction. Reflection scanning is delegated to <see cref="TypeDrawMetadataFactory"/>, and boxed property getter creation is delegated to <see cref="PropertyGetterFactory"/>.
/// </remarks>
internal sealed class TypeDrawMetadata
{
    private static readonly ConcurrentDictionary<Type, TypeDrawMetadata> s_cache = new();

    /// <summary>
    /// Stores the cached draw metadata for one public instance property of a configuration type.
    /// </summary>
    internal sealed class PropertyDrawMetadata(
        PropertyInfo property,
        Type propertyType,
        Func<object, object?> getValue,
        bool isParameter,
        string? category,
        UmbraIndentAttribute? indentAttr,
        UmbraCollapseAsTreeAttribute? collapseAttr,
        UmbraLabelMarginAttribute? labelMarginAttr,
        INestedDrawerAttribute? nestedDrawerAttr,
        IHideIfAttribute? hideIf,
        IDisableIfAttribute? disableIf,
        int order,
        int spacingBefore,
        int spacingAfter,
        string? configPrefix,
        string? configParameterKeyOverride)
    {
        internal PropertyInfo Property { get; } = property;
        internal Type PropertyType { get; } = propertyType;
        internal Func<object, object?> GetValue { get; } = getValue;
        internal bool IsParameter { get; } = isParameter;
        internal string? Category { get; } = category;
        internal UmbraIndentAttribute? IndentAttr { get; } = indentAttr;
        internal UmbraCollapseAsTreeAttribute? CollapseAttr { get; } = collapseAttr;
        internal UmbraLabelMarginAttribute? LabelMarginAttr { get; } = labelMarginAttr;
        internal INestedDrawerAttribute? NestedDrawerAttr { get; } = nestedDrawerAttr;
        internal IHideIfAttribute? HideIf { get; } = hideIf;
        internal IDisableIfAttribute? DisableIf { get; } = disableIf;
        internal int Order { get; } = order;
        internal int SpacingBefore { get; } = spacingBefore;
        internal int SpacingAfter { get; } = spacingAfter;
        internal string? ConfigPrefix { get; } = configPrefix;
        internal string? ConfigParameterKeyOverride { get; } = configParameterKeyOverride;

        internal bool HasWrapperMetadata => HideIf is not null
            || Order != int.MaxValue
            || SpacingBefore != 0
            || SpacingAfter != 0;
    }

    internal string? Category { get; }
    internal string? ConfigPrefix { get; }
    internal UmbraIndentAttribute? IndentAttr { get; }
    internal UmbraCollapseAsTreeAttribute? CollapseAttr { get; }
    internal UmbraLabelMarginAttribute? LabelMarginAttr { get; }
    internal INestedDrawerAttribute? NestedDrawerAttr { get; }
    internal bool IsAutoRegisterConfig { get; }
    internal PropertyDrawMetadata[] Properties { get; }

    internal TypeDrawMetadata(
        string? category,
        string? configPrefix,
        UmbraIndentAttribute? indentAttr,
        UmbraCollapseAsTreeAttribute? collapseAttr,
        UmbraLabelMarginAttribute? labelMarginAttr,
        INestedDrawerAttribute? nestedDrawerAttr,
        bool isAutoRegisterConfig,
        PropertyDrawMetadata[] properties)
    {
        Category = category;
        ConfigPrefix = configPrefix;
        IndentAttr = indentAttr;
        CollapseAttr = collapseAttr;
        LabelMarginAttr = labelMarginAttr;
        NestedDrawerAttr = nestedDrawerAttr;
        IsAutoRegisterConfig = isAutoRegisterConfig;
        Properties = properties;
    }

    /// <summary>
    /// Returns the cached metadata snapshot for <paramref name="type"/>, building it once on first use.
    /// </summary>
    internal static TypeDrawMetadata For(Type type) => s_cache.GetOrAdd(type, TypeDrawMetadataFactory.Build);
}
