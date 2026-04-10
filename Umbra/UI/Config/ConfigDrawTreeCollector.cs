using Umbra.Config;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Search;

namespace Umbra.UI.Config;

/// <summary>
/// Walks a configuration object graph into nested <see cref="ConfigDrawScope"/> instances.
/// </summary>
/// <remarks>
/// This collector isolates recursive config-tree traversal from <see cref="ConfigDrawerBuilder"/>. It uses cached metadata and property getters so draw-tree construction avoids repeated reflection-heavy member access.
/// </remarks>
internal static class ConfigDrawTreeCollector
{
    /// <summary>
    /// Populates <paramref name="scope"/> from the supplied configuration-group instance.
    /// </summary>
    /// <param name="scope">The local category and alignment scope to populate.</param>
    /// <param name="obj">The configuration-group instance to traverse.</param>
    /// <param name="type">The reflected type used for cached metadata lookup.</param>
    /// <param name="registerCategoryNode">Tracks materialized category nodes for later sorting.</param>
    /// <param name="disposables">Collects disposable resources created while resolving nodes and drawers.</param>
    /// <param name="sortNodesInPlace">Applies the caller's stable local ordering policy.</param>
    /// <param name="searchIndex">Collects the flat search index built alongside the rendered nodes.</param>
    /// <param name="numericEditSink">The optional numeric edit sink forwarded to built-in numeric controls, or <see langword="null"/> when numeric edit tracking is disabled.</param>
    /// <param name="textEditSink">The optional text edit sink forwarded to built-in text controls, or <see langword="null"/> when text edit tracking is disabled.</param>
    /// <param name="inheritedVisibility">The effective runtime visibility inherited from ancestor wrappers, or <see langword="null"/> when no ancestor visibility filter applies.</param>
    /// <param name="inheritedDisabled">The effective disabled state inherited from ancestor wrappers, or <see langword="null"/> when no ancestor disabled condition applies.</param>
    internal static void CollectInto(
        ConfigDrawScope scope,
        object obj,
        Type type,
        Action<CategoryNode> registerCategoryNode,
        List<IDisposable> disposables,
        Action<List<IDrawNode>> sortNodesInPlace,
        ConfigSearchIndex searchIndex,
        INumericEditSink? numericEditSink = null,
        ITextEditSink? textEditSink = null,
        Func<bool>? inheritedVisibility = null,
        Func<bool>? inheritedDisabled = null)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(obj);
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(registerCategoryNode);
        ArgumentNullException.ThrowIfNull(disposables);
        ArgumentNullException.ThrowIfNull(sortNodesInPlace);
        ArgumentNullException.ThrowIfNull(searchIndex);

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
                var parameterDisableIf = parameter.Metadata.DisableIf ?? propMeta.DisableIf;
                var parameterHideIf = parameter.Metadata.HideIf ?? propMeta.HideIf;
                var parameterDisabled = ComposeDisabled(inheritedDisabled, parameterDisableIf, obj);
                var (node, resource) = ParameterNodeComposer.Create(
                    parameter,
                    obj,
                    alignmentGroup,
                    classIndent?.Amount,
                    classLabelMargin?.Pixels,
                    parameterDisabled,
                    numericEditSink,
                    textEditSink);
                if (resource is not null)
                    disposables.Add(resource);

                searchIndex.AddParameterResult(
                    parameter.Key,
                    parameter.Metadata.ResolvedLabel,
                    parameter.Metadata.Description,
                    category,
                    scope.GroupPath,
                    ComposeVisibility(inheritedVisibility, parameterHideIf, obj));

                scope.AddNode(category, node);
                continue;
            }

            var propTypeMeta = TypeDrawMetadata.For(propType);
            if (!propTypeMeta.IsAutoRegisterConfig || value is not { } nested)
                continue;

            var nestedDrawerAttr = propMeta.NestedDrawerAttr ?? propTypeMeta.NestedDrawerAttr;
            var nestedLocalCategory = propMeta.Category ?? propTypeMeta.Category;
            var nestedCollapseAttr = propMeta.CollapseAttr ?? propTypeMeta.CollapseAttr;
            var nestedLabelMargin = propMeta.LabelMarginAttr
                ?? propTypeMeta.LabelMarginAttr
                ?? scope.LabelMarginAttr;
            var propertyIndent = propMeta.IndentAttr;
            var nestedGroupPath = NestedScopePathResolver.Resolve(scope.GroupPath, propMeta, propTypeMeta);
            var nestedVisibility = ComposeVisibility(inheritedVisibility, propMeta.HideIf, obj);
            var nestedDisabled = ComposeDisabled(inheritedDisabled, propMeta.DisableIf, obj);

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
                    nestedDisabled,
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

            CollectInto(childScope, nested, propType, registerCategoryNode, disposables, sortNodesInPlace, searchIndex, numericEditSink, textEditSink, nestedVisibility, nestedDisabled);

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

    private static Func<bool>? ComposeVisibility(
        Func<bool>? inheritedVisibility,
        Umbra.Config.Attributes.IHideIfAttribute? hideIf,
        object owner)
    {
        var localVisibility = hideIf is null
            ? null
            : VisibilityPredicateResolver.Build(hideIf, owner);

        return ComposeVisibility(inheritedVisibility, localVisibility);
    }

    private static Func<bool>? ComposeVisibility(Func<bool>? inheritedVisibility, Func<bool>? localVisibility)
    {
        if (inheritedVisibility is null)
            return localVisibility;

        if (localVisibility is null)
            return inheritedVisibility;

        return () => inheritedVisibility() && localVisibility();
    }

    private static Func<bool>? ComposeDisabled(
        Func<bool>? inheritedDisabled,
        Umbra.Config.Attributes.IDisableIfAttribute? disableIf,
        object owner)
    {
        var localDisabled = disableIf is null
            ? null
            : DisablePredicateResolver.Build(disableIf, owner);

        return ComposeDisabled(inheritedDisabled, localDisabled);
    }

    private static Func<bool>? ComposeDisabled(Func<bool>? inheritedDisabled, Func<bool>? localDisabled)
    {
        if (inheritedDisabled is null)
            return localDisabled;

        if (localDisabled is null)
            return inheritedDisabled;

        return () => inheritedDisabled() || localDisabled();
    }
}
