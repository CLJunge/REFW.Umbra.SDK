using System.Collections.Concurrent;
using System.Reflection;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

#pragma warning disable CS0618 // Metadata cache must continue supporting legacy unprefixed attributes for backwards compatibility.

/// <summary>
/// Caches all class-level metadata consulted by <see cref="ConfigDrawerBuilder.Collect"/>
/// in a single <see cref="MemberInfo.GetCustomAttributes(bool)"/> pass per type, supporting
/// both legacy and Umbra-prefixed attribute names.
/// </summary>
/// <remarks>
/// Instances are keyed by <see cref="Type"/> identity in a thread-safe static cache. Types are only
/// reflected over once per <see cref="AppDomain"/> lifetime; subsequent <c>Collect</c>
/// calls for the same type return the cached result immediately.
/// </remarks>
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
        IndentAttribute? indentAttr,
        CollapseAsTreeAttribute? collapseAttr,
        LabelMarginAttribute? labelMarginAttr,
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
        internal IndentAttribute? IndentAttr { get; } = indentAttr;
        internal CollapseAsTreeAttribute? CollapseAttr { get; } = collapseAttr;
        internal LabelMarginAttribute? LabelMarginAttr { get; } = labelMarginAttr;
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
    internal IndentAttribute? IndentAttr { get; }
    internal CollapseAsTreeAttribute? CollapseAttr { get; }
    internal LabelMarginAttribute? LabelMarginAttr { get; }
    internal INestedGroupDrawerAttribute? NestedGroupDrawerAttr { get; }
    internal bool IsAutoRegisterSettings { get; }
    internal PropertyDrawMetadata[] Properties { get; }

    private TypeDrawMetadata(
        string? category,
        string? settingsPrefix,
        IndentAttribute? indentAttr,
        CollapseAsTreeAttribute? collapseAttr,
        LabelMarginAttribute? labelMarginAttr,
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
        IndentAttribute? indentAttr = null;
        CollapseAsTreeAttribute? collapseAttr = null;
        LabelMarginAttribute? labelMarginAttr = null;
        INestedGroupDrawerAttribute? nestedDrawerAttr = null;
        var isAutoRegister = false;

        foreach (var a in type.GetCustomAttributes(inherit: true))
        {
            switch (a)
            {
                case CategoryAttribute legacyCategory:
                    category = legacyCategory.Name;
                    continue;
                case UmbraCategoryAttribute prefixedCategory:
                    category = prefixedCategory.Name;
                    continue;
                case SettingsPrefixAttribute legacyPrefix:
                    settingsPrefix = legacyPrefix.Prefix;
                    continue;
                case UmbraSettingsPrefixAttribute prefixedPrefix:
                    settingsPrefix = prefixedPrefix.Prefix;
                    continue;
                case IndentAttribute legacyIndent:
                    indentAttr = legacyIndent;
                    continue;
                case UmbraIndentAttribute prefixedIndent:
                    indentAttr = new IndentAttribute(prefixedIndent.Amount);
                    continue;
                case CollapseAsTreeAttribute legacyCollapse:
                    collapseAttr = legacyCollapse;
                    continue;
                case UmbraCollapseAsTreeAttribute prefixedCollapse:
                    collapseAttr = new CollapseAsTreeAttribute(prefixedCollapse.DefaultOpen);
                    continue;
                case LabelMarginAttribute legacyLabelMargin:
                    labelMarginAttr = legacyLabelMargin;
                    continue;
                case UmbraLabelMarginAttribute prefixedLabelMargin:
                    labelMarginAttr = new LabelMarginAttribute(prefixedLabelMargin.Pixels);
                    continue;
                case INestedGroupDrawerAttribute ngd:
                    nestedDrawerAttr = ngd;
                    continue;
                case AutoRegisterSettingsAttribute:
                case UmbraAutoRegisterSettingsAttribute:
                    isAutoRegister = true;
                    continue;
            }
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
        IndentAttribute? indentAttr = null;
        CollapseAsTreeAttribute? collapseAttr = null;
        LabelMarginAttribute? labelMarginAttr = null;
        INestedGroupDrawerAttribute? nestedGroupDrawerAttr = null;
        IHideIfAttribute? hideIf = null;
        var order = int.MaxValue;
        var spacingBefore = 0;
        var spacingAfter = 0;

        foreach (var attribute in property.GetCustomAttributes(inherit: false))
        {
            switch (attribute)
            {
                case CategoryAttribute legacyCategory:
                    category = legacyCategory.Name;
                    continue;
                case UmbraCategoryAttribute prefixedCategory:
                    category = prefixedCategory.Name;
                    continue;
                case SettingsPrefixAttribute legacyPrefix:
                    settingsPrefix = legacyPrefix.Prefix;
                    continue;
                case UmbraSettingsPrefixAttribute prefixedPrefix:
                    settingsPrefix = prefixedPrefix.Prefix;
                    continue;
                case SettingsParameterAttribute legacySettingsParameter:
                    settingsParameterKeyOverride = legacySettingsParameter.KeyOverride;
                    continue;
                case UmbraSettingsParameterAttribute prefixedSettingsParameter:
                    settingsParameterKeyOverride = prefixedSettingsParameter.KeyOverride;
                    continue;
                case IndentAttribute legacyIndent:
                    indentAttr = legacyIndent;
                    continue;
                case UmbraIndentAttribute prefixedIndent:
                    indentAttr = new IndentAttribute(prefixedIndent.Amount);
                    continue;
                case CollapseAsTreeAttribute legacyCollapse:
                    collapseAttr = legacyCollapse;
                    continue;
                case UmbraCollapseAsTreeAttribute prefixedCollapse:
                    collapseAttr = new CollapseAsTreeAttribute(prefixedCollapse.DefaultOpen);
                    continue;
                case LabelMarginAttribute legacyLabelMargin:
                    labelMarginAttr = legacyLabelMargin;
                    continue;
                case UmbraLabelMarginAttribute prefixedLabelMargin:
                    labelMarginAttr = new LabelMarginAttribute(prefixedLabelMargin.Pixels);
                    continue;
                case INestedGroupDrawerAttribute nestedDrawer:
                    nestedGroupDrawerAttr = nestedDrawer;
                    continue;
                case IHideIfAttribute propertyHideIf:
                    hideIf = propertyHideIf;
                    continue;
                case ParameterOrderAttribute legacyOrder:
                    order = legacyOrder.Order;
                    continue;
                case UmbraParameterOrderAttribute prefixedOrder:
                    order = prefixedOrder.Order;
                    continue;
                case SpacingBeforeAttribute legacyBefore:
                    spacingBefore = legacyBefore.Count;
                    continue;
                case UmbraSpacingBeforeAttribute prefixedBefore:
                    spacingBefore = prefixedBefore.Count;
                    continue;
                case SpacingAfterAttribute legacyAfter:
                    spacingAfter = legacyAfter.Count;
                    continue;
                case UmbraSpacingAfterAttribute prefixedAfter:
                    spacingAfter = prefixedAfter.Count;
                    continue;
            }
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

#pragma warning restore CS0618
