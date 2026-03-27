using System.Collections.Concurrent;
using System.Reflection;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Caches all class-level metadata attributes consulted by <see cref="ConfigDrawerBuilder.Collect"/>
/// in a single <see cref="MemberInfo.GetCustomAttributes(bool)"/> pass per type, eliminating the six
/// repeated per-attribute <c>GetCustomAttribute</c> calls that would otherwise be made on the same
/// <see cref="Type"/> object at the top of every <c>Collect</c> invocation.
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
    /// <remarks>
    /// This collapses the repeated per-property <c>GetCustomAttribute</c> and
    /// <c>GetCustomAttributes</c> calls previously performed inside
    /// <see cref="ConfigDrawerBuilder.CollectInto"/> into a single attribute scan per property per
    /// <see cref="AppDomain"/> lifetime.
    /// </remarks>
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

        /// <summary>
        /// Gets whether this property carries any wrapper-style metadata that should apply to the
        /// whole nested-group subtree rather than to an individual leaf parameter.
        /// </summary>
        internal bool HasWrapperMetadata => HideIf is not null
            || Order != int.MaxValue
            || SpacingBefore != 0
            || SpacingAfter != 0;
    }

    /// <summary>
    /// Category name from <see cref="CategoryAttribute"/>, or <see langword="null"/> when absent.
    /// Used as the fallback category for all leaf parameters that declare no category of their own.
    /// </summary>
    internal string? Category { get; }

    /// <summary>
    /// Settings prefix from <see cref="SettingsPrefixAttribute"/>, or <see langword="null"/> when absent.
    /// Used to seed the root config ImGui scope path and as a fallback for nested-group scope paths.
    /// </summary>
    internal string? SettingsPrefix { get; }

    /// <summary>
    /// Class-level <see cref="IndentAttribute"/>, or <see langword="null"/> when absent.
    /// Applied as a fallback indent to every parameter control whose own <see cref="Umbra.Config.ParameterMetadata"/>
    /// carries no property-level <see cref="IndentAttribute"/>.
    /// </summary>
    internal IndentAttribute? IndentAttr { get; }

    /// <summary>
    /// <see cref="CollapseAsTreeAttribute"/>, or <see langword="null"/> when absent.
    /// Passed directly to <c>EmitCategoryHeader</c> to control whether the category block renders
    /// as a collapsible <see cref="ImGui.TreeNode(string)"/> or a flat <see cref="ImGui.SeparatorText(string)"/>.
    /// </summary>
    internal CollapseAsTreeAttribute? CollapseAttr { get; }

    /// <summary>
    /// <see cref="LabelMarginAttribute"/>, or <see langword="null"/> when absent.
    /// When present, its <see cref="LabelMarginAttribute.Pixels"/> value overrides the
    /// default label-column width for all parameters in this type.
    /// </summary>
    internal LabelMarginAttribute? LabelMarginAttr { get; }

    /// <summary>
    /// Class-level <see cref="INestedGroupDrawerAttribute"/>, or <see langword="null"/> when absent.
    /// When non-<see langword="null"/>, <c>Collect</c> skips parameter expansion for this type and
    /// delegates rendering entirely to the associated custom drawer.
    /// </summary>
    internal INestedGroupDrawerAttribute? NestedGroupDrawerAttr { get; }

    /// <summary>
    /// Whether the type carries <see cref="AutoRegisterSettingsAttribute"/>.
    /// Used to detect direct (non-<see cref="Umbra.Config.Parameter{T}"/>) nested settings-group properties
    /// during the <c>Collect</c> property loop.
    /// </summary>
    internal bool IsAutoRegisterSettings { get; }

    /// <summary>
    /// The public instance properties of this type together with all property-level UI metadata
    /// consulted by <see cref="ConfigDrawerBuilder.CollectInto"/>.
    /// Cached from a single <c>Type.GetProperties(BindingFlags.Public | BindingFlags.Instance)</c>
    /// call and one attribute scan per property performed once in <see cref="Build"/>.
    /// </summary>
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

    /// <summary>
    /// Returns the cached <see cref="TypeDrawMetadata"/> for <paramref name="type"/>,
    /// building and caching it on first access via a single attribute scan.
    /// </summary>
    /// <param name="type">The type to read metadata for.</param>
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
            if (a is CategoryAttribute cat) { category = cat.Name; continue; }
            if (a is SettingsPrefixAttribute prefix) { settingsPrefix = prefix.Prefix; continue; }
            if (a is IndentAttribute ind) { indentAttr = ind; continue; }
            if (a is CollapseAsTreeAttribute col) { collapseAttr = col; continue; }
            if (a is LabelMarginAttribute lm) { labelMarginAttr = lm; continue; }
            if (a is INestedGroupDrawerAttribute ngd) { nestedDrawerAttr = ngd; continue; }
            if (a is AutoRegisterSettingsAttribute) isAutoRegister = true;
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
    /// <param name="property">The reflected property whose metadata should be cached.</param>
    /// <returns>The cached property metadata consumed by <see cref="ConfigDrawerBuilder"/>.</returns>
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
            if (attribute is CategoryAttribute cat) { category = cat.Name; continue; }
            if (attribute is SettingsPrefixAttribute prefix) { settingsPrefix = prefix.Prefix; continue; }
            if (attribute is SettingsParameterAttribute settingsParameter) { settingsParameterKeyOverride = settingsParameter.KeyOverride; continue; }
            if (attribute is IndentAttribute indent) { indentAttr = indent; continue; }
            if (attribute is CollapseAsTreeAttribute collapse) { collapseAttr = collapse; continue; }
            if (attribute is LabelMarginAttribute labelMargin) { labelMarginAttr = labelMargin; continue; }
            if (attribute is INestedGroupDrawerAttribute nestedDrawer) { nestedGroupDrawerAttr = nestedDrawer; continue; }
            if (attribute is IHideIfAttribute propertyHideIf) { hideIf = propertyHideIf; continue; }
            if (attribute is ParameterOrderAttribute parameterOrder) { order = parameterOrder.Order; continue; }
            if (attribute is SpacingBeforeAttribute before) { spacingBefore = before.Count; continue; }
            if (attribute is SpacingAfterAttribute after) { spacingAfter = after.Count; }
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
