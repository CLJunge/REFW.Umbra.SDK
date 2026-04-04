using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Creates the draw nodes used to render nested configuration groups.
/// </summary>
/// <remarks>
/// This helper isolates wrapper-node composition, nested-drawer node creation, and ImGui subtree scoping from <see cref="ConfigDrawTreeCollector"/>.
/// </remarks>
internal static class NestedNodeComposer
{
    /// <summary>
    /// Wraps an already-built nested-group subtree in property-level visibility, spacing, and ordering behavior.
    /// </summary>
    internal static ParameterNode CreateWrappedNode(
        List<IDrawNode> nodes,
        object owner,
        IHideIfAttribute? propHideIf,
        int order,
        int spacingBefore,
        int spacingAfter)
    {
        var isVisible = propHideIf is not null
            ? VisibilityPredicateResolver.Build(propHideIf, owner)
            : static () => true;

        return new ParameterNode(
            isVisible,
            () =>
            {
                foreach (var node in nodes)
                    node.Draw();
            },
            order,
            spacingBefore,
            spacingAfter,
            children: nodes);
    }

    /// <summary>
    /// Creates the node used for a nested-group custom drawer, including any required local category and subtree ID scope.
    /// </summary>
    internal static IDrawNode? CreateNestedDrawerNode(
        Action<CategoryNode> registerCategoryNode,
        UmbraLabelMarginAttribute? inheritedLabelMargin,
        string groupScopePath,
        TypeDrawMetadata.PropertyDrawMetadata propMeta,
        Type propType,
        INestedDrawerAttribute nestedDrawerAttr,
        object nested,
        object owner,
        string? localCategory,
        UmbraCollapseAsTreeAttribute? collapseAttr,
        UmbraIndentAttribute? indentAttr,
        out IDisposable? disposable)
    {
        disposable = null;

        try
        {
            var drawAction = NestedDrawerBinder.BuildDrawAction(nestedDrawerAttr, propType, nested, out disposable);
            if (drawAction is null)
                return null;

            if (localCategory is not null)
            {
                var localScope = new ConfigDrawScope(
                    groupScopePath,
                    localCategory,
                    collapseAttr,
                    indentAttr,
                    inheritedLabelMargin,
                    registerCategoryNode);
                localScope.AddNode(
                    localCategory,
                    new ParameterNode(
                        VisibilityPredicateResolver.Build(propMeta.HideIf, owner),
                        drawAction,
                        order: propMeta.Order,
                        spacingBefore: propMeta.SpacingBefore,
                        spacingAfter: propMeta.SpacingAfter));

                return CreateIdScopedSubtree(groupScopePath, localScope.Nodes);
            }

            var drawerNode = new ParameterNode(
                VisibilityPredicateResolver.Build(propMeta.HideIf, owner),
                drawAction,
                order: propMeta.Order,
                spacingBefore: propMeta.SpacingBefore,
                spacingAfter: propMeta.SpacingAfter);

            return CreateIdScopedSubtree(groupScopePath, [drawerNode]);
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigDrawer: failed to instantiate nested group drawer '{nestedDrawerAttr.DrawerType.Name}'.");
            disposable = null;
            return null;
        }
    }

    /// <summary>
    /// Wraps a nested-group subtree in a stable ImGui ID scope derived from its structural settings path.
    /// </summary>
    internal static IdScopeNode CreateIdScopedSubtree(string scopePath, List<IDrawNode> nodes)
        => new(scopePath, nodes);
}
