using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Builds uncached <see cref="TypeDrawMetadata"/> instances from reflected config types.
/// </summary>
/// <remarks>
/// This type isolates attribute scanning and property metadata assembly from
/// <see cref="TypeDrawMetadata"/>, leaving that type responsible only for immutable metadata shape
/// and cache access.
/// </remarks>
internal static class TypeDrawMetadataFactory
{
    /// <summary>
    /// Builds the uncached metadata snapshot for <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The config type whose draw metadata should be scanned.</param>
    /// <returns>The fully populated uncached metadata snapshot.</returns>
    internal static TypeDrawMetadata Build(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        string? category = null;
        string? settingsPrefix = null;
        UmbraIndentAttribute? indentAttr = null;
        UmbraCollapseAsTreeAttribute? collapseAttr = null;
        UmbraLabelMarginAttribute? labelMarginAttr = null;
        INestedDrawerAttribute? nestedDrawerAttr = null;
        var isAutoRegister = false;

        foreach (var attribute in type.GetCustomAttributes(inherit: true))
        {
            if (attribute is UmbraCategoryAttribute categoryAttribute) { category = categoryAttribute.Name; continue; }
            if (attribute is UmbraPrefixAttribute prefixAttribute) { settingsPrefix = prefixAttribute.Prefix; continue; }
            if (attribute is UmbraIndentAttribute indentAttribute) { indentAttr = indentAttribute; continue; }
            if (attribute is UmbraCollapseAsTreeAttribute collapseAttribute) { collapseAttr = collapseAttribute; continue; }
            if (attribute is UmbraLabelMarginAttribute marginAttribute) { labelMarginAttr = marginAttribute; continue; }
            if (attribute is INestedDrawerAttribute nestedDrawerAttribute) { nestedDrawerAttr = nestedDrawerAttribute; continue; }
            if (attribute is UmbraAutoRegisterAttribute) isAutoRegister = true;
        }

        var rawProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var properties = new TypeDrawMetadata.PropertyDrawMetadata[rawProperties.Length];
        for (var i = 0; i < rawProperties.Length; i++)
            properties[i] = BuildPropertyMetadata(rawProperties[i]);

        return new TypeDrawMetadata(
            category,
            settingsPrefix,
            indentAttr,
            collapseAttr,
            labelMarginAttr,
            nestedDrawerAttr,
            isAutoRegister,
            properties);
    }

    /// <summary>
    /// Reads and assembles the property-level metadata consulted during config drawer construction.
    /// </summary>
    /// <remarks>
    /// The returned metadata also carries a cached boxed getter delegate so
    /// <see cref="ConfigDrawTreeCollector.CollectInto(ConfigDrawScope, object, Type, Action{Nodes.CategoryNode}, List{IDisposable}, Action{List{Nodes.IDrawNode}})"/> can traverse the
    /// live config object graph without paying <see cref="PropertyInfo.GetValue(object?)"/> reflection
    /// overhead for each property on every draw-tree build.
    /// </remarks>
    /// <param name="property">The reflected property whose metadata should be scanned.</param>
    /// <returns>The assembled metadata snapshot for one public instance property.</returns>
    private static TypeDrawMetadata.PropertyDrawMetadata BuildPropertyMetadata(PropertyInfo property)
    {
        var propertyType = property.PropertyType;
        var getValue = PropertyGetterFactory.Create(property);
        var isParameter = propertyType.IsGenericType
            && propertyType.GetGenericTypeDefinition() == typeof(Umbra.Config.Parameter<>);

        string? category = null;
        string? settingsPrefix = null;
        string? settingsParameterKeyOverride = null;
        UmbraIndentAttribute? indentAttr = null;
        UmbraCollapseAsTreeAttribute? collapseAttr = null;
        UmbraLabelMarginAttribute? labelMarginAttr = null;
        INestedDrawerAttribute? nestedDrawerAttr = null;
        IHideIfAttribute? hideIf = null;
        var order = int.MaxValue;
        var spacingBefore = 0;
        var spacingAfter = 0;

        foreach (var attribute in property.GetCustomAttributes(inherit: false))
        {
            if (attribute is UmbraCategoryAttribute categoryAttribute) { category = categoryAttribute.Name; continue; }
            if (attribute is UmbraPrefixAttribute prefixAttribute) { settingsPrefix = prefixAttribute.Prefix; continue; }
            if (attribute is UmbraParameterAttribute parameterAttribute) { settingsParameterKeyOverride = parameterAttribute.KeyOverride; continue; }
            if (attribute is UmbraIndentAttribute indentAttribute) { indentAttr = indentAttribute; continue; }
            if (attribute is UmbraCollapseAsTreeAttribute collapseAttribute) { collapseAttr = collapseAttribute; continue; }
            if (attribute is UmbraLabelMarginAttribute marginAttribute) { labelMarginAttr = marginAttribute; continue; }
            if (attribute is INestedDrawerAttribute nestedDrawerAttribute) { nestedDrawerAttr = nestedDrawerAttribute; continue; }
            if (attribute is IHideIfAttribute hideIfAttribute) { hideIf = hideIfAttribute; continue; }
            if (attribute is UmbraParameterOrderAttribute orderAttribute) { order = orderAttribute.Order; continue; }
            if (attribute is UmbraSpacingBeforeAttribute beforeAttribute) { spacingBefore = beforeAttribute.Count; continue; }
            if (attribute is UmbraSpacingAfterAttribute afterAttribute) { spacingAfter = afterAttribute.Count; }
        }

        return new TypeDrawMetadata.PropertyDrawMetadata(
            property,
            propertyType,
            getValue,
            isParameter,
            category,
            indentAttr,
            collapseAttr,
            labelMarginAttr,
            nestedDrawerAttr,
            hideIf,
            order,
            spacingBefore,
            spacingAfter,
            settingsPrefix,
            settingsParameterKeyOverride);
    }
}
