using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Walks a configuration object tree once at construction time and produces the ordered list of
/// top-level <see cref="IDrawNode"/> instances consumed by <see cref="ConfigDrawer{TConfig}.Draw"/>.
/// </summary>
/// <remarks>
/// <para>
/// The builder treats every nested settings object as its own layout scope. Each scope owns a
/// local category map, so category names are only unique within the group that declares them.
/// This allows arbitrarily deep nested settings groups without category collisions between sibling
/// or cousin branches of the configuration tree.
/// </para>
/// <para>
/// When a nested group does not declare its own <see cref="UmbraCategoryAttribute"/>, its uncategorized
/// direct children are injected into the parent scope's current category context. When the nested
/// group does declare its own category, that category is rendered as a real container node whose
/// children are the nested group's uncategorized direct controls plus any additional locally scoped
/// categories declared within that group. This avoids creating a redundant nested category header
/// with the same label as the container.
/// </para>
/// <para>
/// Every nested-group subtree is additionally wrapped in a stable ImGui ID scope derived from the
/// group's structural settings path. Nested-group custom drawer binding is delegated to
/// <see cref="NestedGroupDrawerBinder"/>, structural path derivation is delegated to
/// <see cref="NestedGroupScopePathResolver"/>, scope-local category routing is delegated to
/// <see cref="ConfigDrawScope"/>, nested-group node composition is delegated to
/// <see cref="NestedGroupNodeComposer"/>, and leaf parameter node composition is delegated to
/// <see cref="ParameterNodeComposer"/> so this type remains focused on tree assembly.
/// </para>
/// </remarks>
internal sealed class ConfigDrawerBuilder
{
    private readonly List<CategoryNode> _allCategoryNodes = [];

    /// <summary>The ordered list of draw nodes assembled during the <see cref="Collect"/> pass.</summary>
    internal readonly List<IDrawNode> Nodes = [];

    /// <summary>
    /// Disposable resources (e.g. stateful custom drawers) collected during the
    /// <see cref="Collect"/> pass. <see cref="ConfigDrawer{TConfig}"/> disposes these
    /// on unload to release any captured input state.
    /// </summary>
    internal readonly List<IDisposable> Disposables = [];

    /// <summary>Walks <paramref name="obj"/> recursively and populates <see cref="Nodes"/>.</summary>
    /// <param name="obj">The configuration object instance to inspect.</param>
    /// <param name="type">
    /// The <see cref="Type"/> of <paramref name="obj"/> to reflect over.
    /// Passed explicitly so that the correct compile-time type is used rather than the runtime type,
    /// which matters for nested groups accessed through a base-typed property.
    /// </param>
    /// <param name="propertyIndentOverride">
    /// An <see cref="UmbraIndentAttribute"/> read from the parent's property declaration for this
    /// nested group. When non-<see langword="null"/>, it is applied to category nodes created in
    /// this scope so the entire section header and its child controls indent together.
    /// </param>
    /// <param name="categoryOverride">
    /// The explicit category assigned to this group by its parent property, or <see langword="null"/>
    /// when the group should use its own type-level category or remain uncategorized locally.
    /// </param>
    /// <param name="collapseOverride">
    /// The property-level <see cref="UmbraCollapseAsTreeAttribute"/> selected for this nested group,
    /// or <see langword="null"/> when the type-level attribute should be used as fallback.
    /// </param>
    /// <param name="labelMarginOverride">
    /// The property-level <see cref="UmbraLabelMarginAttribute"/> selected for this nested group,
    /// or <see langword="null"/> when the type-level attribute should be used as fallback.
    /// </param>
    /// <remarks>
    /// Returns immediately without emitting any nodes when <paramref name="type"/> is decorated
    /// with <see cref="INestedGroupDrawerAttribute"/>. Such types are rendered entirely by their
    /// custom drawer; expanding their parameters here would duplicate what the drawer manages.
    /// Nested child groups receive their own stable ImGui ID scopes derived from the root config's
    /// settings prefix and the nested-group property path.
    /// </remarks>
    internal void Collect(
        object obj,
        Type type,
        UmbraIndentAttribute? propertyIndentOverride = null,
        string? categoryOverride = null,
        UmbraCollapseAsTreeAttribute? collapseOverride = null,
        UmbraLabelMarginAttribute? labelMarginOverride = null)
    {
        Nodes.Clear();
        Disposables.Clear();
        _allCategoryNodes.Clear();

        var typeMeta = TypeDrawMetadata.For(type);
        var rootGroupPath = typeMeta.SettingsPrefix ?? string.Empty;
        var scope = new ConfigDrawScope(
            rootGroupPath,
            categoryOverride ?? typeMeta.Category,
            collapseOverride ?? typeMeta.CollapseAttr,
            propertyIndentOverride,
            labelMarginOverride ?? typeMeta.LabelMarginAttr,
            RegisterCategoryNode);

        CollectInto(scope, obj, type);

        foreach (var node in scope.Nodes)
            Nodes.Add(node);
    }

    /// <summary>
    /// Walks one configuration-group object into the specified local layout <paramref name="scope"/>.
    /// </summary>
    /// <param name="scope">The local category and alignment scope to populate.</param>
    /// <param name="obj">The group instance to reflect over.</param>
    /// <param name="type">The compile-time type of <paramref name="obj"/>.</param>
    private void CollectInto(ConfigDrawScope scope, object obj, Type type)
    {
        var typeMeta = TypeDrawMetadata.For(type);
        if (typeMeta.NestedGroupDrawerAttr is not null)
            return;

        var classIndent = typeMeta.IndentAttr;
        var classLabelMargin = scope.LabelMarginAttr;

        foreach (var propMeta in typeMeta.Properties)
        {
            var prop = propMeta.Property;
            var propType = propMeta.PropertyType;

            if (propMeta.IsParameter)
            {
                if (prop.GetValue(obj) is not IParameter parameter)
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
                    Disposables.Add(resource);

                scope.AddNode(category, node);
                continue;
            }

            var propTypeMeta = TypeDrawMetadata.For(propType);
            if (!propTypeMeta.IsAutoRegisterSettings || prop.GetValue(obj) is not { } nested)
                continue;

            var nestedDrawerAttr = propMeta.NestedGroupDrawerAttr ?? propTypeMeta.NestedGroupDrawerAttr;
            var nestedLocalCategory = propMeta.Category ?? propTypeMeta.Category;
            var nestedCollapseAttr = propMeta.CollapseAttr ?? propTypeMeta.CollapseAttr;
            var nestedLabelMargin = propMeta.LabelMarginAttr
                ?? propTypeMeta.LabelMarginAttr
                ?? scope.LabelMarginAttr;
            var propertyIndent = propMeta.IndentAttr;
            var nestedGroupPath = NestedGroupScopePathResolver.Resolve(scope.GroupPath, propMeta, propTypeMeta);

            if (nestedDrawerAttr is not null)
            {
                var drawerNode = NestedGroupNodeComposer.CreateNestedDrawerNode(
                    RegisterCategoryNode,
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
                    Disposables.Add(disposable);

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
                RegisterCategoryNode,
                childAlignmentGroup);

            CollectInto(childScope, nested, propType);

            if (nestedLocalCategory is not null)
            {
                var childContainer = childScope.CreateContainerNode(nestedLocalCategory);
                var scopedChildNode = NestedGroupNodeComposer.CreateIdScopedSubtree(nestedGroupPath, [childContainer]);

                if (propMeta.HasWrapperMetadata)
                {
                    scope.AddNode(
                        null,
                        NestedGroupNodeComposer.CreateWrappedNode(
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

            var scopedSubtreeNode = NestedGroupNodeComposer.CreateIdScopedSubtree(nestedGroupPath, childScope.Nodes);

            if (propMeta.HasWrapperMetadata)
            {
                scope.AddNode(
                    ambientCategory,
                    NestedGroupNodeComposer.CreateWrappedNode(
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

    /// <summary>
    /// Registers a category node created by a child <see cref="ConfigDrawScope"/> so it can be
    /// included in the final per-category stable sort pass.
    /// </summary>
    /// <param name="node">The category node to track.</param>
    private void RegisterCategoryNode(CategoryNode node) => _allCategoryNodes.Add(node);

    /// <summary>
    /// Applies a stable sort to parameter nodes within every category context, ordering them
    /// by their <see cref="ParameterNode.Order"/> value ascending.
    /// </summary>
    /// <remarks>
    /// Call this once after <see cref="Collect"/> has finished walking the entire config tree.
    /// Nodes without an explicit <c>[ParameterOrder]</c> attribute receive an implicit key of
    /// <see cref="int.MaxValue"/>, placing them after all explicitly ordered entries while
    /// preserving original declaration order among equals. The root <see cref="Nodes"/> list and
    /// every local <see cref="CategoryNode.Children"/> list are sorted independently so ordering
    /// remains local to each group scope.
    /// </remarks>
    internal void SortAll()
    {
        static int NodeOrder(IDrawNode n) => n is ParameterNode p ? p.Order : int.MaxValue;

        foreach (var cat in _allCategoryNodes)
            cat.Children.SortBy(NodeOrder);

        Nodes.SortBy(NodeOrder);
    }
}
