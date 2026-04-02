using System.Collections.Concurrent;
using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Stores cached config-draw metadata for one reflected type.
/// </summary>
/// <remarks>
/// This type now owns the immutable metadata shape and the shared per-type cache consulted by
/// <see cref="ConfigDrawerBuilder.Collect"/>. Reflection scanning and property-getter construction are
/// delegated to <see cref="TypeDrawMetadataFactory"/> and <see cref="PropertyGetterFactory"/>.
/// </remarks>
internal sealed class TypeDrawMetadata
{
    private static readonly ConcurrentDictionary<Type, TypeDrawMetadata> s_cache = new();

    /// <summary>
    /// Cached UI metadata for one public instance property of a config type.
    /// </summary>
    /// <param name="property">The reflected property.</param>
    /// <param name="propertyType">The property's type.</param>
    /// <param name="getValue">The cached accessor that reads the property's current value from a config instance.</param>
    /// <param name="isParameter">Whether the property is a <see cref="Umbra.Config.Parameter{T}"/>.</param>
    /// <param name="category">The property's <see cref="UmbraCategoryAttribute"/> value, or <see langword="null"/> if not specified.</param>
    /// <param name="indentAttr">The property's <see cref="UmbraIndentAttribute"/>, if any.</param>
    /// <param name="collapseAttr">The property's <see cref="UmbraCollapseAsTreeAttribute"/>, if any.</param>
    /// <param name="labelMarginAttr">The property's <see cref="UmbraLabelMarginAttribute"/>, if any.</param>
    /// <param name="nestedDrawerAttr">The property's <see cref="INestedDrawerAttribute"/>, if any.</param>
    /// <param name="hideIf">The property's <see cref="IHideIfAttribute"/>, if any.</param>
    /// <param name="order">The property's <see cref="UmbraParameterOrderAttribute.Order"/> value, or <see cref="int.MaxValue"/> if not specified.</param>
    /// <param name="spacingBefore">The property's <see cref="UmbraSpacingBeforeAttribute.Count"/> value, or 0 if not specified.</param>
    /// <param name="spacingAfter">The property's <see cref="UmbraSpacingAfterAttribute.Count"/> value, or 0 if not specified.</param>
    /// <param name="settingsPrefix">The property's <see cref="UmbraPrefixAttribute.Prefix"/> value, or <see langword="null"/> if not specified.</param>
    /// <param name="settingsParameterKeyOverride">The property's <see cref="UmbraParameterAttribute.KeyOverride"/> value, or <see langword="null"/> if not specified.</param>
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
        int order,
        int spacingBefore,
        int spacingAfter,
        string? settingsPrefix,
        string? settingsParameterKeyOverride)
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
        internal int Order { get; } = order;
        internal int SpacingBefore { get; } = spacingBefore;
        internal int SpacingAfter { get; } = spacingAfter;
        internal string? SettingsPrefix { get; } = settingsPrefix;
        internal string? SettingsParameterKeyOverride { get; } = settingsParameterKeyOverride;

        internal bool HasWrapperMetadata => HideIf is not null
            || Order != int.MaxValue
            || SpacingBefore != 0
            || SpacingAfter != 0;
    }

    internal string? Category { get; }
    internal string? SettingsPrefix { get; }
    internal UmbraIndentAttribute? IndentAttr { get; }
    internal UmbraCollapseAsTreeAttribute? CollapseAttr { get; }
    internal UmbraLabelMarginAttribute? LabelMarginAttr { get; }
    internal INestedDrawerAttribute? NestedDrawerAttr { get; }
    internal bool IsAutoRegisterSettings { get; }
    internal PropertyDrawMetadata[] Properties { get; }

    internal TypeDrawMetadata(
        string? category,
        string? settingsPrefix,
        UmbraIndentAttribute? indentAttr,
        UmbraCollapseAsTreeAttribute? collapseAttr,
        UmbraLabelMarginAttribute? labelMarginAttr,
        INestedDrawerAttribute? nestedDrawerAttr,
        bool isAutoRegisterSettings,
        PropertyDrawMetadata[] properties)
    {
        Category = category;
        SettingsPrefix = settingsPrefix;
        IndentAttr = indentAttr;
        CollapseAttr = collapseAttr;
        LabelMarginAttr = labelMarginAttr;
        NestedDrawerAttr = nestedDrawerAttr;
        IsAutoRegisterSettings = isAutoRegisterSettings;
        Properties = properties;
    }

    /// <summary>
    /// Returns the cached metadata snapshot for <paramref name="type"/>, building it once on first use.
    /// </summary>
    /// <param name="type">The reflected config type whose draw metadata should be retrieved.</param>
    /// <returns>The cached metadata snapshot for <paramref name="type"/>.</returns>
    internal static TypeDrawMetadata For(Type type) => s_cache.GetOrAdd(type, TypeDrawMetadataFactory.Build);
}
