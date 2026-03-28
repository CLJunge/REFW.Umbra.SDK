using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Creates draw nodes used to render nested configuration groups.
/// </summary>
/// <remarks>
/// This type isolates wrapper-node composition and nested-group custom-drawer node creation from
/// <see cref="ConfigDrawerBuilder"/> so the builder remains focused on traversing the config tree.
/// </remarks>
internal static class NestedGroupNodeComposer
{
    /// <summary>
    /// Creates a wrapper node that applies property-level visibility, spacing, and ordering to an
    /// already-built nested-group subtree.
    /// </summary>
    /// <param name="nodes">The already-built child nodes to render inside the wrapper.</param>
    /// <param name="owner">The parent object that owns the nested-group property.</param>
    /// <param name="propHideIf">Optional property-level visibility condition for the whole group.</param>
    /// <param name="order">The property-level sort key for the wrapped section.</param>
    /// <param name="spacingBefore">The property-level vertical spacing emitted above the wrapped section.</param>
    /// <param name="spacingAfter">The property-level vertical spacing emitted below the wrapped section.</param>
    /// <returns>A <see cref="ParameterNode"/> that renders the supplied subtree under the wrapper metadata.</returns>
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
            spacingAfter);
    }

    /// <summary>
    /// Creates the draw node used for a nested-group custom drawer, including any required local
    /// category scope and stable ImGui ID scoping.
    /// </summary>
    /// <param name="registerCategoryNode">Callback used to track any category nodes created during composition.</param>
    /// <param name="inheritedLabelMargin">The label-margin setting inherited from the parent scope.</param>
    /// <param name="groupScopePath">The stable structural path used for the nested group's ImGui ID scope.</param>
    /// <param name="propMeta">The cached property metadata for the nested-group property.</param>
    /// <param name="propType">The runtime type of the nested group.</param>
    /// <param name="nestedDrawerAttr">The resolved nested-group drawer attribute.</param>
    /// <param name="nested">The live nested group instance that will be passed to the drawer.</param>
    /// <param name="owner">The parent config instance that owns the nested-group property.</param>
    /// <param name="localCategory">The explicit category declared on the nested-group property or its type, if any.</param>
    /// <param name="collapseAttr">The collapse behavior for a local category emitted specifically for this nested drawer.</param>
    /// <param name="indentAttr">The property-level indent for a local category emitted specifically for this nested drawer.</param>
    /// <param name="disposable">Receives the created drawer instance when it implements <see cref="IDisposable"/>.</param>
    /// <returns>
    /// A composed draw node ready to be routed into the parent scope, or <see langword="null"/> when
    /// the drawer cannot be bound to <paramref name="propType"/>.
    /// </returns>
    internal static IDrawNode? CreateNestedDrawerNode(
        Action<CategoryNode> registerCategoryNode,
        UmbraLabelMarginAttribute? inheritedLabelMargin,
        string groupScopePath,
        TypeDrawMetadata.PropertyDrawMetadata propMeta,
        Type propType,
        INestedGroupDrawerAttribute nestedDrawerAttr,
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
            var drawAction = NestedGroupDrawerBinder.BuildDrawAction(nestedDrawerAttr, propType, nested, out disposable);
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
    /// Wraps a nested-group subtree in a stable ImGui ID scope derived from the group's structural
    /// settings path.
    /// </summary>
    /// <param name="scopePath">The stable dot-separated group path used for the ImGui ID scope.</param>
    /// <param name="nodes">The already-built nodes belonging to the nested-group subtree.</param>
    /// <returns>
    /// An <see cref="IdScopeNode"/> that pushes <paramref name="scopePath"/> before drawing the
    /// subtree and pops it afterward.
    /// </returns>
    internal static IdScopeNode CreateIdScopedSubtree(string scopePath, List<IDrawNode> nodes)
        => new(scopePath, nodes);
}

