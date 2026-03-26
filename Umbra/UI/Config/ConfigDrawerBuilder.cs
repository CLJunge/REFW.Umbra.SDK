using System.Collections.Concurrent;
using System.Linq.Expressions;
using Hexa.NET.ImGui;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Drawers;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Walks a configuration object tree once at construction time and produces the ordered list of
/// top-level <see cref="IDrawNode"/> instances consumed by <see cref="ConfigDrawer{TConfig}.Draw"/>.
/// </summary>
/// <remarks>
/// <para>
/// The builder now treats every nested settings object as its own layout scope. Each scope owns a
/// local category map, so category names are only unique within the group that declares them.
/// This allows arbitrarily deep nested settings groups without category collisions between sibling
/// or cousin branches of the configuration tree.
/// </para>
/// <para>
/// When a nested group does not declare its own <see cref="CategoryAttribute"/>, its uncategorized
/// direct children are injected into the parent scope's current category context. When the nested
/// group does declare its own category, that category is rendered as a real container node whose
/// children are the nested group's uncategorized direct controls plus any additional locally scoped
/// categories declared within that group. This avoids creating a redundant nested category header
/// with the same label as the container.
/// </para>
/// <para>
/// Every nested-group subtree is additionally wrapped in a stable ImGui ID scope derived from the
/// group's structural settings path. This keeps custom nested-group drawers and repeated local UI
/// labels isolated even when sibling branches reuse the same category names and widget labels.
/// </para>
/// </remarks>
internal sealed class ConfigDrawerBuilder
{
    private static readonly ConcurrentDictionary<NestedGroupDrawerFactoryKey, NestedGroupDrawerFactory> s_nestedGroupDrawerFactories = new();
    private readonly List<CategoryNode> _allCategoryNodes = [];

    /// <summary>The ordered list of draw nodes assembled during the <see cref="Collect"/> pass.</summary>
    internal readonly List<IDrawNode> Nodes = [];

    /// <summary>
    /// Disposable resources (e.g. stateful custom drawers) collected during the
    /// <see cref="Collect"/> pass. <see cref="ConfigDrawer{TConfig}"/> disposes these
    /// on unload to release any captured input state.
    /// </summary>
    internal readonly List<IDisposable> Disposables = [];

    /// <summary>
    /// Mutable build state for one configuration-group layout scope.
    /// Each scope owns its own category deduplication map, alignment state, and structural path
    /// used for descendant nested-group ImGui ID scopes.
    /// </summary>
    private sealed class ScopeState(
        string groupPath,
        string? defaultCategory,
        CollapseAsTreeAttribute? collapseAttr,
        IndentAttribute? categoryIndentAttr,
        LabelMarginAttribute? labelMarginAttr,
        LabelAlignmentGroup? alignmentGroup = null)
    {
        internal string GroupPath { get; } = groupPath;
        internal string? DefaultCategory { get; } = defaultCategory;
        internal CollapseAsTreeAttribute? CollapseAttr { get; } = collapseAttr;
        internal IndentAttribute? CategoryIndentAttr { get; } = categoryIndentAttr;
        internal LabelMarginAttribute? LabelMarginAttr { get; } = labelMarginAttr;
        internal List<IDrawNode> Nodes { get; } = [];
        internal Dictionary<string, CategoryNode> NamedCategories { get; } = [];
        internal LabelAlignmentGroup AlignmentGroup { get; } = alignmentGroup ?? new();
        internal CategoryNode? CurrentCategoryNode { get; set; }
        internal string? LastCategory { get; set; }
    }

    /// <summary>
    /// Cache key for one nested-group drawer binding shape.
    /// </summary>
    /// <param name="DrawerType">The concrete nested-group drawer type being instantiated.</param>
    /// <param name="GroupType">The runtime nested settings-group type exposed by the property.</param>
    private readonly record struct NestedGroupDrawerFactoryKey(Type DrawerType, Type GroupType);

    /// <summary>
    /// Cached result of resolving and compiling the draw invoker for one drawer/group type pair.
    /// </summary>
    /// <remarks>
    /// The expensive interface scan and expression compilation happen once per unique pair and are
    /// reused by all subsequent <see cref="ConfigDrawer{TConfig}"/> builds for the same shape.
    /// Per-node work is then reduced to creating the drawer instance and binding the cached invoker
    /// to that instance and nested group object.
    /// </remarks>
    private sealed class NestedGroupDrawerFactory(
        bool isSupported,
        Type? supportedGroupType,
        Action<object, object>? invoker)
    {
        internal bool IsSupported { get; } = isSupported;
        internal Type? SupportedGroupType { get; } = supportedGroupType;

        internal Action Bind(object drawerInstance, object nested)
        {
            if (invoker is null)
                throw new InvalidOperationException("Cannot bind an unsupported nested-group drawer factory.");

            return () => invoker(drawerInstance, nested);
        }
    }

    /// <summary>Walks <paramref name="obj"/> recursively and populates <see cref="Nodes"/>.</summary>
    /// <param name="obj">The configuration object instance to inspect.</param>
    /// <param name="type">
    /// The <see cref="Type"/> of <paramref name="obj"/> to reflect over.
    /// Passed explicitly so that the correct compile-time type is used rather than the runtime type,
    /// which matters for nested groups accessed through a base-typed property.
    /// </param>
    /// <param name="propertyIndentOverride">
    /// An <see cref="IndentAttribute"/> read from the parent's property declaration for this
    /// nested group. When non-<see langword="null"/>, it is applied to category nodes created in
    /// this scope so the entire section header and its child controls indent together.
    /// </param>
    /// <param name="categoryOverride">
    /// The explicit category assigned to this group by its parent property, or <see langword="null"/>
    /// when the group should use its own type-level category or remain uncategorized locally.
    /// </param>
    /// <param name="collapseOverride">
    /// The property-level <see cref="CollapseAsTreeAttribute"/> selected for this nested group,
    /// or <see langword="null"/> when the type-level attribute should be used as fallback.
    /// </param>
    /// <param name="labelMarginOverride">
    /// The property-level <see cref="LabelMarginAttribute"/> selected for this nested group,
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
        IndentAttribute? propertyIndentOverride = null,
        string? categoryOverride = null,
        CollapseAsTreeAttribute? collapseOverride = null,
        LabelMarginAttribute? labelMarginOverride = null)
    {
        Nodes.Clear();

        var typeMeta = TypeDrawMetadata.For(type);
        var rootGroupPath = typeMeta.SettingsPrefix ?? string.Empty;
        var scope = new ScopeState(
            rootGroupPath,
            categoryOverride ?? typeMeta.Category,
            collapseOverride ?? typeMeta.CollapseAttr,
            propertyIndentOverride,
            labelMarginOverride ?? typeMeta.LabelMarginAttr);

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
    private void CollectInto(ScopeState scope, object obj, Type type)
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

                var meta = parameter.Metadata;
                var label = meta.ResolvedLabel;
                var category = propMeta.Category ?? scope.DefaultCategory;
                var alignmentGroup = GetAlignmentGroup(scope, category);
                if (classLabelMargin is not null)
                    alignmentGroup.Margin = classLabelMargin.Pixels;

                var (draw, resource) = ControlFactory.BuildDrawAction(parameter, label, alignmentGroup);
                if (resource is not null)
                    Disposables.Add(resource);

                var indentAmount = meta.Indent ?? classIndent?.Amount;
                if (indentAmount.HasValue)
                {
                    var amount = indentAmount.Value;
                    var inner = draw;
                    draw = () =>
                    {
                        ImGui.Indent(amount);
                        try
                        {
                            inner();
                        }
                        finally
                        {
                            ImGui.Unindent(amount);
                        }
                    };
                }

                var isVisible = meta.HideIf is not null
                    ? VisibilityPredicateResolver.Build(meta.HideIf, obj)
                    : static () => true;

                AddNode(
                    scope,
                    category,
                    new ParameterNode(isVisible, draw, meta.Order ?? int.MaxValue, meta.SpacingBefore, meta.SpacingAfter));
                continue;
            }

            var propTypeMeta = TypeDrawMetadata.For(propType);
            if (!propTypeMeta.IsAutoRegisterSettings || prop.GetValue(obj) is not { } nested)
                continue;

            var nestedDrawerAttr = propMeta.NestedGroupDrawerAttr ?? propTypeMeta.NestedGroupDrawerAttr;
            var nestedLocalCategory = propMeta.Category ?? propTypeMeta.Category;
            var nestedCollapseAttr = propMeta.CollapseAttr
                ?? propTypeMeta.CollapseAttr;
            var nestedLabelMargin = propMeta.LabelMarginAttr
                ?? propTypeMeta.LabelMarginAttr
                ?? scope.LabelMarginAttr;
            var propertyIndent = propMeta.IndentAttr;
            var nestedGroupPath = ResolveNestedGroupScopePath(scope.GroupPath, propMeta, propTypeMeta);

            if (nestedDrawerAttr is not null)
            {
                EmitNestedGroupDrawerNode(
                    scope,
                    nestedGroupPath,
                    propMeta,
                    propType,
                    nestedDrawerAttr,
                    nested,
                    obj,
                    nestedLocalCategory,
                    nestedCollapseAttr,
                    propertyIndent);
                continue;
            }

            var ambientCategory = nestedLocalCategory is null ? scope.DefaultCategory : null;
            var childDefaultCategory = nestedLocalCategory is null ? ambientCategory : null;
            LabelAlignmentGroup? childAlignmentGroup = null;
            if (nestedLocalCategory is null)
                childAlignmentGroup = GetAlignmentGroup(scope, ambientCategory);

            var childScope = new ScopeState(
                nestedGroupPath,
                childDefaultCategory,
                nestedCollapseAttr,
                propertyIndent,
                nestedLabelMargin,
                childAlignmentGroup);

            CollectInto(childScope, nested, propType);

            if (nestedLocalCategory is not null)
            {
                var childContainer = CreateScopeContainerNode(nestedLocalCategory, childScope);
                var scopedChildNode = CreateIdScopedSubtree(nestedGroupPath, [childContainer]);

                if (propMeta.HasWrapperMetadata)
                {
                    AddWrappedScopeNode(scope, [scopedChildNode], obj, propMeta.HideIf, propMeta.Order, propMeta.SpacingBefore, propMeta.SpacingAfter, ambientCategory: null);
                }
                else
                {
                    AddNode(scope, null, scopedChildNode);
                }

                continue;
            }

            var scopedSubtreeNode = CreateIdScopedSubtree(nestedGroupPath, childScope.Nodes);

            if (propMeta.HasWrapperMetadata)
            {
                AddWrappedScopeNode(scope, [scopedSubtreeNode], obj, propMeta.HideIf, propMeta.Order, propMeta.SpacingBefore, propMeta.SpacingAfter, ambientCategory);
            }
            else
            {
                AddNode(scope, ambientCategory, scopedSubtreeNode);
            }
        }
    }

    /// <summary>
    /// Wraps an entire child scope in a single conditional <see cref="ParameterNode"/> so
    /// property-level visibility, spacing, and ordering apply to the whole nested group.
    /// </summary>
    /// <param name="scope">The parent scope that receives the wrapper node.</param>
    /// <param name="nodes">The already-built child nodes to render inside the wrapper.</param>
    /// <param name="owner">The parent object that owns the nested-group property.</param>
    /// <param name="propHideIf">Optional property-level visibility condition for the whole group.</param>
    /// <param name="order">The property-level sort key for the wrapped section.</param>
    /// <param name="spacingBefore">The property-level vertical spacing emitted above the wrapped section.</param>
    /// <param name="spacingAfter">The property-level vertical spacing emitted below the wrapped section.</param>
    /// <param name="ambientCategory">
    /// The parent-scope category bucket used when the child group did not declare its own local
    /// category and should continue rendering inside the parent's category context.
    /// </param>
    private void AddWrappedScopeNode(
        ScopeState scope,
        List<IDrawNode> nodes,
        object owner,
        IHideIfAttribute? propHideIf,
        int order,
        int spacingBefore,
        int spacingAfter,
        string? ambientCategory)
    {
        var isVisible = propHideIf is not null
            ? VisibilityPredicateResolver.Build(propHideIf, owner)
            : static () => true;

        AddNode(
            scope,
            ambientCategory,
            new ParameterNode(
                isVisible,
                () =>
                {
                    foreach (var node in nodes)
                        node.Draw();
                },
                order,
                spacingBefore,
                spacingAfter));
    }

    /// <summary>
    /// Materializes a collected child scope into a visible parent-owned <see cref="CategoryNode"/>
    /// so a nested-group property's own category becomes the single rendered container for that
    /// group's uncategorized direct controls and any additional child categories.
    /// </summary>
    /// <param name="category">The category label declared by the nested-group property or type.</param>
    /// <param name="childScope">The already-collected child scope that should render inside the container.</param>
    /// <returns>
    /// A <see cref="CategoryNode"/> whose children are the top-level nodes of
    /// <paramref name="childScope"/>.
    /// </returns>
    private CategoryNode CreateScopeContainerNode(string category, ScopeState childScope)
    {
        var node = new CategoryNode(category, childScope.CollapseAttr, childScope.CategoryIndentAttr);
        foreach (var child in childScope.Nodes)
            node.Children.Add(child);

        _allCategoryNodes.Add(node);
        return node;
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
    private static IdScopeNode CreateIdScopedSubtree(string scopePath, List<IDrawNode> nodes)
        => new(scopePath, nodes);

    /// <summary>
    /// Routes one node into the specified category bucket of <paramref name="scope"/>, or the
    /// scope root when <paramref name="category"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="scope">The receiving scope.</param>
    /// <param name="category">The local category bucket to route into, or <see langword="null"/> for the root list.</param>
    /// <param name="node">The node to append.</param>
    private void AddNode(ScopeState scope, string? category, IDrawNode node)
    {
        var targetList = GetTargetList(scope, category);
        targetList.Add(node);
    }

    /// <summary>
    /// Returns the active <see cref="LabelAlignmentGroup"/> for <paramref name="category"/> inside
    /// <paramref name="scope"/>, creating the category header on demand when required.
    /// </summary>
    /// <param name="scope">The scope whose alignment state should be consulted.</param>
    /// <param name="category">The local category name, or <see langword="null"/> for the scope-wide alignment group.</param>
    private LabelAlignmentGroup GetAlignmentGroup(ScopeState scope, string? category)
    {
        if (category is null)
            return scope.AlignmentGroup;

        EmitCategoryHeader(scope, category);
        return scope.CurrentCategoryNode!.AlignmentGroup;
    }

    /// <summary>
    /// Returns the target node list for <paramref name="category"/> inside <paramref name="scope"/>,
    /// creating the category header on demand when necessary.
    /// </summary>
    /// <param name="scope">The receiving scope.</param>
    /// <param name="category">The local category bucket to route into, or <see langword="null"/> for the root list.</param>
    private List<IDrawNode> GetTargetList(ScopeState scope, string? category)
    {
        if (category is null)
            return scope.Nodes;

        EmitCategoryHeader(scope, category);
        return scope.CurrentCategoryNode!.Children;
    }

    /// <summary>
    /// Emits a <see cref="CategoryNode"/> for <paramref name="category"/> within the local
    /// <paramref name="scope"/> if it differs from the last emitted category in that same scope.
    /// </summary>
    /// <param name="scope">The local group scope that owns the category map.</param>
    /// <param name="category">The category name to emit.</param>
    private void EmitCategoryHeader(ScopeState scope, string category)
    {
        if (category == scope.LastCategory)
            return;

        if (scope.NamedCategories.TryGetValue(category, out var existing))
        {
            scope.CurrentCategoryNode = existing;
            scope.LastCategory = category;
            return;
        }

        var node = new CategoryNode(category, scope.CollapseAttr, scope.CategoryIndentAttr);
        scope.Nodes.Add(node);
        scope.NamedCategories[category] = node;
        scope.CurrentCategoryNode = node;
        scope.LastCategory = category;
        _allCategoryNodes.Add(node);
    }

    /// <summary>
    /// Instantiates the resolved <see cref="INestedGroupDrawer{T}"/>, compiles a zero-reflection
    /// draw delegate, wraps it in the nested group's stable ImGui ID scope, and routes the
    /// resulting node into the correct scope.
    /// </summary>
    /// <param name="scope">The local scope that should receive the drawer node.</param>
    /// <param name="groupScopePath">The stable structural path used for the nested group's ImGui ID scope.</param>
    /// <param name="propMeta">The cached property metadata for the nested-group property.</param>
    /// <param name="propType">The runtime type of the nested group.</param>
    /// <param name="nestedDrawerAttr">The resolved nested-group drawer attribute.</param>
    /// <param name="nested">The live nested group instance that will be passed to the drawer.</param>
    /// <param name="owner">The parent config instance that owns the nested-group property.</param>
    /// <param name="localCategory">
    /// The explicit category declared on the nested-group property or its type, if any. When
    /// non-<see langword="null"/>, that category becomes the visible container for the drawer output.
    /// When <see langword="null"/>, the drawer inherits the parent scope category instead of creating
    /// a new local category bucket.
    /// </param>
    /// <param name="collapseAttr">
    /// The collapse behaviour for a local category emitted specifically for this nested drawer.
    /// Ignored when <paramref name="localCategory"/> is <see langword="null"/> because no new
    /// category header is created in that case.
    /// </param>
    /// <param name="indentAttr">
    /// The property-level indent for a local category emitted specifically for this nested drawer.
    /// Ignored when <paramref name="localCategory"/> is <see langword="null"/>.
    /// </param>
    private void EmitNestedGroupDrawerNode(
        ScopeState scope,
        string groupScopePath,
        TypeDrawMetadata.PropertyDrawMetadata propMeta,
        Type propType,
        INestedGroupDrawerAttribute nestedDrawerAttr,
        object nested,
        object owner,
        string? localCategory,
        CollapseAsTreeAttribute? collapseAttr,
        IndentAttribute? indentAttr)
    {
        try
        {
            var drawAction = BuildNestedGroupDrawAction(nestedDrawerAttr, propType, nested, out var disposable);
            if (drawAction is null)
                return;

            if (disposable is not null)
                Disposables.Add(disposable);

            var targetCategory = localCategory ?? scope.DefaultCategory;
            if (localCategory is not null)
            {
                var localScope = new ScopeState(groupScopePath, localCategory, collapseAttr, indentAttr, scope.LabelMarginAttr);
                AddNode(
                    localScope,
                    localCategory,
                    new ParameterNode(
                        VisibilityPredicateResolver.Build(propMeta.HideIf, owner),
                        drawAction,
                        order: propMeta.Order,
                        spacingBefore: propMeta.SpacingBefore,
                        spacingAfter: propMeta.SpacingAfter));

                AddNode(scope, null, CreateIdScopedSubtree(groupScopePath, localScope.Nodes));
                return;
            }

            var drawerNode = new ParameterNode(
                VisibilityPredicateResolver.Build(propMeta.HideIf, owner),
                drawAction,
                order: propMeta.Order,
                spacingBefore: propMeta.SpacingBefore,
                spacingAfter: propMeta.SpacingAfter);

            AddNode(
                scope,
                targetCategory,
                CreateIdScopedSubtree(groupScopePath, [drawerNode]));
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigDrawer: failed to instantiate nested group drawer '{nestedDrawerAttr.DrawerType.Name}'.");
        }
    }

    /// <summary>
    /// Creates the one-time draw delegate for a nested-group custom drawer and returns any
    /// disposable drawer instance that should be tracked by the builder.
    /// </summary>
    /// <param name="nestedDrawerAttr">The resolved nested-group drawer attribute.</param>
    /// <param name="propType">The runtime type of the nested group.</param>
    /// <param name="nested">The live nested group instance that will be passed into the drawer.</param>
    /// <param name="disposable">Receives the drawer instance when it implements <see cref="IDisposable"/>.</param>
    /// <returns>
    /// A draw delegate bound to a cached per-type invoker, or <see langword="null"/> when the
    /// drawer type does not support <paramref name="propType"/>.
    /// </returns>
    private static Action? BuildNestedGroupDrawAction(
        INestedGroupDrawerAttribute nestedDrawerAttr,
        Type propType,
        object nested,
        out IDisposable? disposable)
    {
        disposable = null;
        var drawerType = nestedDrawerAttr.DrawerType;
        var drawerInstance = Activator.CreateInstance(drawerType)!;
        var factory = s_nestedGroupDrawerFactories.GetOrAdd(
            new NestedGroupDrawerFactoryKey(drawerType, propType),
            static key => CreateNestedGroupDrawerFactory(key.DrawerType, key.GroupType));

        if (!factory.IsSupported)
        {
            Logger.Error(
                $"ConfigDrawer: nested group drawer '{drawerType.Name}' does not support group type '{propType.FullName}'.");
            return null;
        }

        if (drawerInstance is IDisposable trackedDisposable)
            disposable = trackedDisposable;

        return factory.Bind(drawerInstance, nested);
    }

    /// <summary>
    /// Resolves and compiles the cached invoker used by nested-group drawers for a specific
    /// drawer/group type pair.
    /// </summary>
    /// <param name="drawerType">The concrete drawer type to inspect.</param>
    /// <param name="propType">The runtime nested-group type exposed by the property.</param>
    /// <returns>
    /// A cached factory describing whether the drawer supports <paramref name="propType"/> and,
    /// when supported, the precompiled invoker used to bind concrete instances.
    /// </returns>
    private static NestedGroupDrawerFactory CreateNestedGroupDrawerFactory(Type drawerType, Type propType)
    {
        Type? genericIface = null;
        Type? groupType = null;
        foreach (var iface in drawerType.GetInterfaces())
        {
            if (!iface.IsGenericType)
                continue;

            if (iface.GetGenericTypeDefinition() != typeof(INestedGroupDrawer<>))
                continue;

            var candidateGroupType = iface.GetGenericArguments()[0];
            if (!candidateGroupType.IsAssignableFrom(propType))
                continue;

            genericIface = iface;
            groupType = candidateGroupType;
            break;
        }

        if (genericIface is null || groupType is null)
            return new NestedGroupDrawerFactory(false, null, null);

        var drawMethod = genericIface.GetMethod("Draw")!;
        var drawerParam = Expression.Parameter(typeof(object), "drawer");
        var groupParam = Expression.Parameter(typeof(object), "group");
        var callExpr = Expression.Call(
            Expression.Convert(drawerParam, genericIface),
            drawMethod,
            Expression.Convert(groupParam, groupType));
        var invoker = Expression.Lambda<Action<object, object>>(callExpr, drawerParam, groupParam).Compile();

        return new NestedGroupDrawerFactory(true, groupType, invoker);
    }

    /// <summary>
    /// Resolves the stable structural ImGui ID path for a nested-group property.
    /// Property-level <see cref="SettingsPrefixAttribute"/> wins, followed by the nested type's
    /// type-level prefix, then <see cref="SettingsParameterAttribute.KeyOverride"/>, and finally
    /// the camel-cased property name.
    /// </summary>
    /// <param name="parentPath">The dot-separated structural path of the parent group.</param>
    /// <param name="propMeta">The cached metadata for the nested-group property being inspected.</param>
    /// <param name="propTypeMeta">The cached metadata for the nested-group type exposed by the property.</param>
    /// <returns>The fully combined dot-separated path used for the nested group's ImGui ID scope.</returns>
    private static string ResolveNestedGroupScopePath(
        string parentPath,
        TypeDrawMetadata.PropertyDrawMetadata propMeta,
        TypeDrawMetadata propTypeMeta)
    {
        var segment = propMeta.SettingsPrefix
            ?? propTypeMeta.SettingsPrefix
            ?? propMeta.SettingsParameterKeyOverride
            ?? propMeta.Property.Name.ToCamelCase()
            ?? propMeta.Property.Name;

        return CombinePath(parentPath, segment);
    }

    /// <summary>
    /// Combines two dot-separated structural path segments into a single stable path, omitting the
    /// separator when either segment is empty.
    /// </summary>
    /// <param name="left">The parent path segment.</param>
    /// <param name="right">The child path segment.</param>
    /// <returns>
    /// <paramref name="right"/> when <paramref name="left"/> is empty;
    /// <paramref name="left"/> when <paramref name="right"/> is empty;
    /// otherwise <c>"left.right"</c>.
    /// </returns>
    private static string CombinePath(string left, string right)
    {
        if (string.IsNullOrEmpty(left)) return right;
        if (string.IsNullOrEmpty(right)) return left;
        return $"{left}.{right}";
    }


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
            cat.Children.StableSortBy(NodeOrder);

        Nodes.StableSortBy(NodeOrder);
    }
}
