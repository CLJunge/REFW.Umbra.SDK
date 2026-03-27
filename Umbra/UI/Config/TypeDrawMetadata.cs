using System.Collections.Concurrent;
using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Caches all class-level metadata consulted by <see cref="ConfigDrawerBuilder.Collect"/>
/// in a single <see cref="MemberInfo.GetCustomAttributes(bool)"/> pass per type.
/// </summary>
internal sealed class TypeDrawMetadata
{
    private static readonly ConcurrentDictionary<Type, TypeDrawMetadata> s_cache = new();

    /// <summary>
    /// Cached UI metadata for one public instance property of a config type.
    /// </summary>
    internal sealed class PropertyDrawMetadata(
        PropertyInfo property,
        Type propertyType,
        bool isParameter,
        string? category,
        UmbraIndentAttribute? indentAttr,
        UmbraCollapseAsTreeAttribute? collapseAttr,
        UmbraLabelMarginAttribute? labelMarginAttr,
        INestedGroupDrawerAttribute? nestedGroupDrawerAttr,
        IHideIfAttribute? hideIf,
        int order,
        int spacingBefore,
        int spacingAfter,
        string? settingsPrefix,
        string? settingsParameterKeyOverride)
    {
        internal PropertyInfo Property { get; } = property;
        internal Type PropertyType { get; } = propertyType;
        internal bool IsParameter { get; } = isParameter;
        internal string? Category { get; } = category;
        internal UmbraIndentAttribute? IndentAttr { get; } = indentAttr;
        internal UmbraCollapseAsTreeAttribute? CollapseAttr { get; } = collapseAttr;
        internal UmbraLabelMarginAttribute? LabelMarginAttr { get; } = labelMarginAttr;
        internal INestedGroupDrawerAttribute? NestedGroupDrawerAttr { get; } = nestedGroupDrawerAttr;
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
    internal INestedGroupDrawerAttribute? NestedGroupDrawerAttr { get; }
    internal bool IsAutoRegisterSettings { get; }
    internal PropertyDrawMetadata[] Properties { get; }

    private TypeDrawMetadata(
        string? category,
        string? settingsPrefix,
        UmbraIndentAttribute? indentAttr,
        UmbraCollapseAsTreeAttribute? collapseAttr,
        UmbraLabelMarginAttribute? labelMarginAttr,
        INestedGroupDrawerAttribute? nestedGroupDrawerAttr,
        bool isAutoRegisterSettings,
        PropertyDrawMetadata[] properties)
    {
        Category = category;
        SettingsPrefix = settingsPrefix;
        IndentAttr = indentAttr;
        CollapseAttr = collapseAttr;
        LabelMarginAttr = labelMarginAttr;
        NestedGroupDrawerAttr = nestedGroupDrawerAttr;
        IsAutoRegisterSettings = isAutoRegisterSettings;
        Properties = properties;
    }

    internal static TypeDrawMetadata For(Type type) => s_cache.GetOrAdd(type, Build);

    private static TypeDrawMetadata Build(Type type)
    {
        string? category = null;
        string? settingsPrefix = null;
        UmbraIndentAttribute? indentAttr = null;
        UmbraCollapseAsTreeAttribute? collapseAttr = null;
        UmbraLabelMarginAttribute? labelMarginAttr = null;
        INestedGroupDrawerAttribute? nestedDrawerAttr = null;
        var isAutoRegister = false;

        foreach (var a in type.GetCustomAttributes(inherit: true))
        {
            if (a is UmbraCategoryAttribute cat) { category = cat.Name; continue; }
            if (a is UmbraSettingsPrefixAttribute prefix) { settingsPrefix = prefix.Prefix; continue; }
            if (a is UmbraIndentAttribute ind) { indentAttr = ind; continue; }
            if (a is UmbraCollapseAsTreeAttribute col) { collapseAttr = col; continue; }
            if (a is UmbraLabelMarginAttribute lm) { labelMarginAttr = lm; continue; }
            if (a is INestedGroupDrawerAttribute ngd) { nestedDrawerAttr = ngd; continue; }
            if (a is UmbraAutoRegisterSettingsAttribute) isAutoRegister = true;
        }

        var rawProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var properties = new PropertyDrawMetadata[rawProperties.Length];
        for (var i = 0; i < rawProperties.Length; i++)
            properties[i] = BuildPropertyMetadata(rawProperties[i]);

        return new TypeDrawMetadata(category, settingsPrefix, indentAttr, collapseAttr, labelMarginAttr, nestedDrawerAttr, isAutoRegister, properties);
    }

    /// <summary>
    /// Reads and caches all property-level UI metadata consulted during config drawer assembly.
    /// </summary>
    private static PropertyDrawMetadata BuildPropertyMetadata(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var isParameter = propertyType.IsGenericType
            && propertyType.GetGenericTypeDefinition() == typeof(Umbra.Config.Parameter<>);

        string? category = null;
        string? settingsPrefix = null;
        string? settingsParameterKeyOverride = null;
        UmbraIndentAttribute? indentAttr = null;
        UmbraCollapseAsTreeAttribute? collapseAttr = null;
        UmbraLabelMarginAttribute? labelMarginAttr = null;
        INestedGroupDrawerAttribute? nestedGroupDrawerAttr = null;
        IHideIfAttribute? hideIf = null;
        var order = int.MaxValue;
        var spacingBefore = 0;
        var spacingAfter = 0;

        foreach (var attribute in property.GetCustomAttributes(inherit: false))
        {
            if (attribute is UmbraCategoryAttribute cat) { category = cat.Name; continue; }
            if (attribute is UmbraSettingsPrefixAttribute prefix) { settingsPrefix = prefix.Prefix; continue; }
            if (attribute is UmbraSettingsParameterAttribute settingsParameter) { settingsParameterKeyOverride = settingsParameter.KeyOverride; continue; }
            if (attribute is UmbraIndentAttribute indent) { indentAttr = indent; continue; }
            if (attribute is UmbraCollapseAsTreeAttribute collapse) { collapseAttr = collapse; continue; }
            if (attribute is UmbraLabelMarginAttribute labelMargin) { labelMarginAttr = labelMargin; continue; }
            if (attribute is INestedGroupDrawerAttribute nestedDrawer) { nestedGroupDrawerAttr = nestedDrawer; continue; }
            if (attribute is IHideIfAttribute propertyHideIf) { hideIf = propertyHideIf; continue; }
            if (attribute is UmbraParameterOrderAttribute parameterOrder) { order = parameterOrder.Order; continue; }
            if (attribute is UmbraSpacingBeforeAttribute before) { spacingBefore = before.Count; continue; }
            if (attribute is UmbraSpacingAfterAttribute after) { spacingAfter = after.Count; }
        }

        return new PropertyDrawMetadata(
            property,
            propertyType,
            isParameter,
            category,
            indentAttr,
            collapseAttr,
            labelMarginAttr,
            nestedGroupDrawerAttr,
            hideIf,
            order,
            spacingBefore,
            spacingAfter,
            settingsPrefix,
            settingsParameterKeyOverride);
    }
}
