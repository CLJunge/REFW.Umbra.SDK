using System.Reflection;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config;

/// <summary>
/// Builds uncached <see cref="TypeDrawMetadata"/> snapshots from reflected configuration types.
/// </summary>
/// <remarks>
/// This factory isolates attribute scanning and property metadata assembly from the cache owned by <see cref="TypeDrawMetadata"/>.
/// </remarks>
internal static class TypeDrawMetadataFactory
{
    /// <summary>
    /// Builds the uncached metadata snapshot for <paramref name="type"/>.
    /// </summary>
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
    /// Builds the cached property metadata consulted during configuration drawer construction.
    /// </summary>
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
