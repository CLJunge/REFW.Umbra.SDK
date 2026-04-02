using Umbra.Config;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Walks one configuration object graph into nested <see cref="ConfigDrawScope"/> instances.
/// </summary>
/// <remarks>
/// This type isolates recursive config-tree traversal from <see cref="ConfigDrawerBuilder"/>,
/// leaving the builder responsible for top-level orchestration and final ordering.
/// </remarks>
internal static class ConfigDrawTreeCollector
{
    /// <summary>
    /// Walks one configuration-group object into the specified local layout <paramref name="scope"/>.
    /// </summary>
    /// <param name="scope">The local category and alignment scope to populate.</param>
    /// <param name="obj">The group instance to reflect over.</param>
    /// <param name="type">The compile-time type of <paramref name="obj"/>.</param>
    /// <param name="registerCategoryNode">Tracks category nodes for the caller's later sort pass.</param>
    /// <param name="disposables">Collects stateful resources created during traversal.</param>
    /// <param name="sortNodesInPlace">Applies the caller's local stable ordering policy.</param>
    /// <remarks>
    /// Property values are read through the cached delegates stored in <see cref="TypeDrawMetadata"/>
    /// so repeated drawer construction walks the object graph without invoking
    /// <see cref="System.Reflection.PropertyInfo.GetValue(object?)"/> for every property.
    /// </remarks>
    internal static void CollectInto(
        ConfigDrawScope scope,
        object obj,
        Type type,
        Action<CategoryNode> registerCategoryNode,
        List<IDisposable> disposables,
        Action<List<IDrawNode>> sortNodesInPlace)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(registerCategoryNode);
        ArgumentNullException.ThrowIfNull(disposables);
        ArgumentNullException.ThrowIfNull(sortNodesInPlace);

        var typeMeta = TypeDrawMetadata.For(type);
        if (typeMeta.NestedDrawerAttr is not null)
            return;

        var classIndent = typeMeta.IndentAttr;
        var classLabelMargin = scope.LabelMarginAttr;

        foreach (var propMeta in typeMeta.Properties)
        {
            var value = propMeta.GetValue(obj);
            var propType = propMeta.PropertyType;

            if (propMeta.IsParameter)
            {
                if (value is not IParameter parameter)
                    continue;

                var category = propMeta.Category ?? scope.DefaultCategory;
                var alignmentGroup = scope.GetAlignmentGroup(category);
                var (node, resource) = ParameterNodeComposer.Create(
                    parameter,
                    obj,
                    alignmentGroup,
                    classIndent?.Amount,
                    classLabelMargin?.Pixels);
                if (resource is not null)
                    disposables.Add(resource);

                scope.AddNode(category, node);
                continue;
            }

            var propTypeMeta = TypeDrawMetadata.For(propType);
            if (!propTypeMeta.IsAutoRegisterSettings || value is not { } nested)
                continue;

            var nestedDrawerAttr = propMeta.NestedDrawerAttr ?? propTypeMeta.NestedDrawerAttr;
            var nestedLocalCategory = propMeta.Category ?? propTypeMeta.Category;
            var nestedCollapseAttr = propMeta.CollapseAttr ?? propTypeMeta.CollapseAttr;
            var nestedLabelMargin = propMeta.LabelMarginAttr
                ?? propTypeMeta.LabelMarginAttr
                ?? scope.LabelMarginAttr;
            var propertyIndent = propMeta.IndentAttr;
            var nestedGroupPath = NestedScopePathResolver.Resolve(scope.GroupPath, propMeta, propTypeMeta);

            if (nestedDrawerAttr is not null)
            {
                var drawerNode = NestedNodeComposer.CreateNestedDrawerNode(
                    registerCategoryNode,
                    scope.LabelMarginAttr,
                    nestedGroupPath,
                    propMeta,
                    propType,
                    nestedDrawerAttr,
                    nested,
                    obj,
                    nestedLocalCategory,
                    nestedCollapseAttr,
                    propertyIndent,
                    out var disposable);
                if (drawerNode is null)
                    continue;

                if (disposable is not null)
                    disposables.Add(disposable);

                var targetCategory = nestedLocalCategory is null ? scope.DefaultCategory : null;
                scope.AddNode(targetCategory, drawerNode);
                continue;
            }

            var ambientCategory = nestedLocalCategory is null ? scope.DefaultCategory : null;
            LabelAlignmentGroup? childAlignmentGroup = null;
            if (nestedLocalCategory is null)
                childAlignmentGroup = scope.GetAlignmentGroup(ambientCategory);

            var childScope = new ConfigDrawScope(
                nestedGroupPath,
                null,
                nestedCollapseAttr,
                propertyIndent,
                nestedLabelMargin,
                registerCategoryNode,
                childAlignmentGroup);

            CollectInto(childScope, nested, propType, registerCategoryNode, disposables, sortNodesInPlace);

            if (nestedLocalCategory is null)
                sortNodesInPlace(childScope.Nodes);

            if (nestedLocalCategory is not null)
            {
                var childContainer = childScope.CreateContainerNode(nestedLocalCategory);
                var scopedChildNode = NestedNodeComposer.CreateIdScopedSubtree(nestedGroupPath, [childContainer]);

                if (propMeta.HasWrapperMetadata)
                {
                    scope.AddNode(
                        null,
                        NestedNodeComposer.CreateWrappedNode(
                            [scopedChildNode],
                            obj,
                            propMeta.HideIf,
                            propMeta.Order,
                            propMeta.SpacingBefore,
                            propMeta.SpacingAfter));
                }
                else
                {
                    scope.AddNode(null, scopedChildNode);
                }

                continue;
            }

            var scopedSubtreeNode = NestedNodeComposer.CreateIdScopedSubtree(nestedGroupPath, childScope.Nodes);

            if (propMeta.HasWrapperMetadata)
            {
                scope.AddNode(
                    ambientCategory,
                    NestedNodeComposer.CreateWrappedNode(
                        [scopedSubtreeNode],
                        obj,
                        propMeta.HideIf,
                        propMeta.Order,
                        propMeta.SpacingBefore,
                        propMeta.SpacingAfter));
            }
            else
            {
                scope.AddNode(ambientCategory, scopedSubtreeNode);
            }
        }
    }
}
