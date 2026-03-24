using System.Linq.Expressions;
using System.Reflection;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;
using Umbra.Config.UI.Nodes;
using Umbra.Config.UI.ParameterDrawers;
using Umbra.Logging;

namespace Umbra.Config.UI;

/// <summary>
/// Walks a configuration object tree once at construction time and produces the ordered list of
/// top-level <see cref="IDrawNode"/> instances consumed by <see cref="ConfigDrawer{TConfig}.Draw"/>.
/// Each distinct category name maps to exactly one <see cref="CategoryNode"/>; if the same name
/// is encountered again after nested-group recursion causes a category break, the existing node
/// is reused instead of creating a duplicate. Tree-node categories manage their
/// <c>ImGui.TreeNode</c> scope internally.
/// </summary>
internal sealed class ConfigDrawerBuilder
{
    private string? _lastCategory;
    private CategoryNode? _currentCategoryNode;
    private readonly List<CategoryNode> _allCategoryNodes = [];
    private readonly Dictionary<string, CategoryNode> _namedCategories = [];
    private readonly LabelAlignmentGroup _rootAlignmentGroup = new();

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
    /// An <see cref="IndentAttribute"/> read from the parent's property declaration for this
    /// nested group. When non-<see langword="null"/>, it is forwarded to
    /// <see cref="EmitCategoryHeader"/> and stored on the resulting <see cref="CategoryNode"/>,
    /// which wraps the entire category block — section header and all child controls — inside
    /// a matching <c>ImGui.Indent</c>/<c>ImGui.Unindent</c> scope.
    /// This is distinct from the class-level <see cref="IndentAttribute"/> on
    /// <paramref name="type"/>, which only affects individual parameter controls as a fallback.
    /// </param>
    /// <param name="categoryOverride">
    /// The effective category inherited from the parent nested-group property for this branch,
    /// or <see langword="null"/> when the current type should rely on its own type-level
    /// <see cref="CategoryAttribute"/> fallback.
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
    /// When a plain nested settings-group property carries wrapper-style property metadata such as
    /// <see cref="HideIfAttribute{T}"/>, <see cref="SpacingBeforeAttribute"/>,
    /// <see cref="SpacingAfterAttribute"/>, or <see cref="ParameterOrderAttribute"/>, the nested
    /// group's parameter nodes are collected via <see cref="CollectFlatParameterNodes"/> using
    /// the parent's shared category and alignment state, then wrapped in a single conditional
    /// <see cref="ParameterNode"/> that participates in the correct category scope's sort order.
    /// </remarks>
    internal void Collect(
        object obj,
        Type type,
        IndentAttribute? propertyIndentOverride = null,
        string? categoryOverride = null,
        CollapseAsTreeAttribute? collapseOverride = null,
        LabelMarginAttribute? labelMarginOverride = null)
    {
        // A type decorated with [NestedGroupDrawer<TDrawer>] is rendered entirely by its
        // custom drawer. Expanding its parameters here would duplicate what that drawer
        // manages, so bail out immediately regardless of how Collect was reached.
        var typeMeta = TypeDrawMetadata.For(type);
        if (typeMeta.NestedGroupDrawerAttr is not null)
            return;

        var classCategory = categoryOverride ?? typeMeta.Category;
        var classIndent = typeMeta.IndentAttr;
        var classCollapseAttr = collapseOverride ?? typeMeta.CollapseAttr;
        var classLabelMargin = labelMarginOverride ?? typeMeta.LabelMarginAttr;

        foreach (var prop in typeMeta.Properties)
        {
            var propType = prop.PropertyType;

            // ── Leaf: Parameter<T> ────────────────────────────────────────
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Parameter<>))
            {
                if (prop.GetValue(obj) is not IParameter parameter) continue;

                var meta = parameter.Metadata;
                var label = meta.ResolvedLabel;
                // meta.Category is already populated by ParameterMetadataReader.ReadFrom during
                // SettingsStore.Load(), so the per-property GetCustomAttribute fallback is redundant.
                var cat = meta.Category ?? classCategory;

                // propertyIndentOverride wraps the entire category block (header + children).
                EmitCategoryHeader(cat, classCollapseAttr, propertyIndentOverride);

                // Route child nodes into the active category scope, or top-level if uncategorized.
                var targetList = cat is not null ? _currentCategoryNode!.Children : Nodes;

                var group = cat is not null ? _currentCategoryNode!.AlignmentGroup : _rootAlignmentGroup;
                if (classLabelMargin is not null) group.Margin = classLabelMargin.Pixels;
                var (draw, resource) = ControlFactory.BuildDrawAction(parameter, label, group);
                if (resource is not null) Disposables.Add(resource);

                // Property-level Indent (pre-populated in metadata by ParameterMetadataReader) takes
                // priority; falls back to the class-level IndentAttribute read once per Collect call.
                var indentAmount = meta.Indent ?? classIndent?.Amount;
                if (indentAmount.HasValue)
                {
                    var amount = indentAmount.Value; // capture before closure
                    var inner = draw;
                    draw = () => { ImGui.Indent(amount); inner(); ImGui.Unindent(amount); };
                }

                var spacingCount = meta.SpacingBefore;
                var spacingAfterCount = meta.SpacingAfter;
                var order = parameter.Metadata.Order ?? int.MaxValue;
                // Fast path: most parameters carry no HideIf condition; return the cached static
                // always-true delegate directly to avoid the function-call overhead of Build.
                var isVisible = meta.HideIf is not null
                    ? VisibilityPredicateResolver.Build(meta.HideIf, obj)
                    : static () => true;
                targetList.Add(new ParameterNode(isVisible, draw, order, spacingCount, spacingAfterCount));
                continue;
            }

            // ── Branch: nested settings group ─────────────────────────────
            var propTypeMeta = TypeDrawMetadata.For(propType);
            if (propTypeMeta.IsAutoRegisterSettings
                && prop.GetValue(obj) is { } nested)
            {
                // Property-level [NestedGroupDrawer<TDrawer>] takes priority; the nested type's
                // drawer declaration remains a backward-compatible fallback.
                var nestedDrawerAttr = GetNestedGroupDrawerAttribute(prop, propTypeMeta);
                var nestedCategory = prop.GetCustomAttribute<CategoryAttribute>()?.Name
                    ?? propTypeMeta.Category
                    ?? classCategory;
                var nestedCollapseAttr = prop.GetCustomAttribute<CollapseAsTreeAttribute>()
                    ?? propTypeMeta.CollapseAttr;
                var nestedLabelMargin = prop.GetCustomAttribute<LabelMarginAttribute>()
                    ?? propTypeMeta.LabelMarginAttr
                    ?? labelMarginOverride;
                if (nestedDrawerAttr is not null)
                    EmitNestedGroupDrawerNode(prop, propType, nestedDrawerAttr, nested, obj, nestedCategory, nestedCollapseAttr);
                else
                {
                    var propertyIndent = prop.GetCustomAttribute<IndentAttribute>();

                    if (TryGetNestedGroupWrapperMetadata(prop, out var propHideIf, out var order, out var spacingBefore, out var spacingAfter))
                    {
                        EmitNestedGroupNode(
                            propType,
                            nested,
                            obj,
                            propertyIndent,
                            nestedCategory,
                            nestedCollapseAttr,
                            nestedLabelMargin,
                            propHideIf,
                            order,
                            spacingBefore,
                            spacingAfter);
                    }
                    else
                    {
                        Collect(nested, propType, propertyIndent, nestedCategory, nestedCollapseAttr, nestedLabelMargin);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Emits the category header for a wrapped nested group on <c>this</c> builder (sharing the
    /// parent's category map and alignment state), collects the group's parameter nodes into a
    /// temporary flat list, and wraps that list in a single conditional <see cref="ParameterNode"/>
    /// so property-level visibility, spacing, and ordering apply to the entire section.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unlike an approach that uses a separate <see cref="ConfigDrawerBuilder"/> instance, this
    /// method emits the category header via the parent's <see cref="EmitCategoryHeader"/> call,
    /// ensuring that the category node and label-alignment group are shared with the rest of the
    /// build pass. This prevents duplicate category headers when the same category name is
    /// encountered elsewhere in the config and keeps label-column alignment consistent across the
    /// entire category scope.
    /// </para>
    /// <para>
    /// The wrapper <see cref="ParameterNode"/> is routed into the same target list that other
    /// nodes in the resolved category use — <see cref="CategoryNode.Children"/> when a category
    /// is resolved, or the top-level <see cref="Nodes"/> list when the group is uncategorized.
    /// This ensures that <see cref="ParameterOrderAttribute"/> sorting is scoped to the category
    /// rather than mixing the wrapper into the top-level ordering pass.
    /// </para>
    /// </remarks>
    /// <param name="propType">The compile-time type of the nested settings group.</param>
    /// <param name="nested">The live nested settings group instance.</param>
    /// <param name="owner">The parent configuration object that owns the nested-group property.</param>
    /// <param name="propertyIndent">
    /// The property-level <see cref="IndentAttribute"/> declared on the parent property, forwarded
    /// to <see cref="EmitCategoryHeader"/> so the category block is indented correctly.
    /// </param>
    /// <param name="categoryOverride">
    /// The effective category for the nested group, resolved from the parent property first and
    /// the nested type second. When non-<see langword="null"/>, the category header is emitted on
    /// <c>this</c> builder and the wrapper node is added to <see cref="CategoryNode.Children"/>.
    /// </param>
    /// <param name="collapseOverride">
    /// The effective collapse behaviour for the category header emitted by this group.
    /// </param>
    /// <param name="labelMarginOverride">
    /// The effective label-column margin applied to controls collected from the nested group.
    /// </param>
    /// <param name="propHideIf">
    /// Optional property-level <see cref="HideIfAttribute{T}"/> that determines whether the entire
    /// nested section should be rendered.
    /// </param>
    /// <param name="order">The property-level sort key for the wrapped section.</param>
    /// <param name="spacingBefore">The property-level vertical spacing emitted above the wrapped section.</param>
    /// <param name="spacingAfter">The property-level vertical spacing emitted below the wrapped section.</param>
    private void EmitNestedGroupNode(
        Type propType,
        object nested,
        object owner,
        IndentAttribute? propertyIndent,
        string? categoryOverride,
        CollapseAsTreeAttribute? collapseOverride,
        LabelMarginAttribute? labelMarginOverride,
        IHideIfAttribute? propHideIf,
        int order,
        int spacingBefore,
        int spacingAfter)
    {
        // Emit the category header on this builder so the CategoryNode and its
        // LabelAlignmentGroup are shared with the rest of the build pass.
        // This prevents duplicate headers and aligns the wrapped group's label
        // column with all other parameters in the same category.
        EmitCategoryHeader(categoryOverride, collapseOverride, propertyIndent);

        // Route the wrapper node into the same list as other nodes in the resolved
        // category scope, so ParameterOrder sorting is scoped correctly.
        var targetList = categoryOverride is not null ? _currentCategoryNode!.Children : Nodes;
        var alignmentGroup = categoryOverride is not null
            ? _currentCategoryNode!.AlignmentGroup
            : _rootAlignmentGroup;

        var tempNodes = new List<IDrawNode>();
        CollectFlatParameterNodes(tempNodes, alignmentGroup, nested, propType, labelMarginOverride);
        tempNodes.StableSortBy(static n => n is ParameterNode p ? p.Order : int.MaxValue);

        var isVisible = propHideIf is not null
            ? VisibilityPredicateResolver.Build(propHideIf, owner)
            : static () => true;

        targetList.Add(new ParameterNode(
            isVisible,
            () =>
            {
                foreach (var node in tempNodes)
                    node.Draw();
            },
            order,
            spacingBefore,
            spacingAfter));
    }

    /// <summary>
    /// Collects <see cref="ParameterNode"/> entries from a nested settings group directly into a
    /// caller-provided list, using a specified <see cref="LabelAlignmentGroup"/>, without emitting
    /// or interacting with category headers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is the flat-collection counterpart to <see cref="Collect"/>. It is called when
    /// building a wrapped nested group via <see cref="EmitNestedGroupNode"/>: the category header
    /// has already been emitted on the parent builder, so only the parameter leaf nodes need to be
    /// gathered into a temporary list that the wrapper <see cref="ParameterNode"/> will iterate.
    /// </para>
    /// <para>
    /// Sub-nested-group properties that carry a <see cref="INestedGroupDrawerAttribute"/> are
    /// instantiated and their draw action appended to <paramref name="target"/> directly via
    /// <see cref="CollectFlatNestedDrawerNode"/>. Plain sub-nested-groups (without a custom
    /// drawer) are recursed into flatly, sharing the same <paramref name="alignmentGroup"/>.
    /// Property-level wrapper attributes (<see cref="HideIfAttribute{T}"/>, spacing, ordering) on
    /// sub-nested-group properties within a flat section are not processed; they apply only at the
    /// outermost wrapped-group level.
    /// </para>
    /// </remarks>
    /// <param name="target">The list to append collected <see cref="ParameterNode"/> entries to.</param>
    /// <param name="alignmentGroup">
    /// The <see cref="LabelAlignmentGroup"/> shared with the parent category scope. All controls
    /// collected here observe their label widths into this group so column alignment is consistent
    /// with other parameters rendered in the same category.
    /// </param>
    /// <param name="obj">The nested settings object instance to reflect over.</param>
    /// <param name="type">The compile-time type of <paramref name="obj"/>.</param>
    /// <param name="labelMarginOverride">
    /// Effective label-margin attribute inherited from the enclosing scope. Applies to this group's
    /// leaf parameters and propagates as a fallback into any sub-nested groups that do not declare
    /// their own <see cref="LabelMarginAttribute"/>, so a property-level override applies to the
    /// entire nested-group subtree.
    /// </param>
    private void CollectFlatParameterNodes(
        List<IDrawNode> target,
        LabelAlignmentGroup alignmentGroup,
        object obj,
        Type type,
        LabelMarginAttribute? labelMarginOverride)
    {
        var typeMeta = TypeDrawMetadata.For(type);
        var classIndent = typeMeta.IndentAttr;
        var classLabelMargin = labelMarginOverride ?? typeMeta.LabelMarginAttr;

        foreach (var prop in typeMeta.Properties)
        {
            var propType = prop.PropertyType;

            // ── Leaf: Parameter<T> ────────────────────────────────────────
            if (propType.IsGenericType && propType.GetGenericTypeDefinition() == typeof(Parameter<>))
            {
                if (prop.GetValue(obj) is not IParameter parameter) continue;

                var meta = parameter.Metadata;
                var label = meta.ResolvedLabel;
                var group = alignmentGroup;
                if (classLabelMargin is not null) group.Margin = classLabelMargin.Pixels;
                var (draw, resource) = ControlFactory.BuildDrawAction(parameter, label, group);
                if (resource is not null) Disposables.Add(resource);

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
                target.Add(new ParameterNode(isVisible, draw, meta.Order ?? int.MaxValue, meta.SpacingBefore, meta.SpacingAfter));
                continue;
            }

            // ── Branch: nested settings group ─────────────────────────────
            var propTypeMeta = TypeDrawMetadata.For(propType);
            if (!propTypeMeta.IsAutoRegisterSettings || prop.GetValue(obj) is not { } nested)
                continue;

            var nestedDrawerAttr = GetNestedGroupDrawerAttribute(prop, propTypeMeta);
            var nestedLabelMargin = prop.GetCustomAttribute<LabelMarginAttribute>()
                ?? propTypeMeta.LabelMarginAttr
                ?? labelMarginOverride;

            if (nestedDrawerAttr is not null)
                CollectFlatNestedDrawerNode(target, prop, propType, nestedDrawerAttr, nested, obj);
            else
                CollectFlatParameterNodes(target, alignmentGroup, nested, propType, nestedLabelMargin);
        }
    }

    /// <summary>
    /// Instantiates the resolved <see cref="INestedGroupDrawer{T}"/>, compiles a zero-reflection
    /// draw delegate, and appends a <see cref="ParameterNode"/> directly to the provided
    /// <paramref name="target"/> list. Used when a custom-drawer nested group is encountered
    /// during flat collection inside a wrapped section.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="EmitNestedGroupDrawerNode"/>, this method does not interact with the
    /// parent builder's category state: no category header is emitted, and the node is added to
    /// <paramref name="target"/> rather than being routed through <see cref="CategoryNode.Children"/>.
    /// </remarks>
    /// <param name="target">The list to append the resulting <see cref="ParameterNode"/> to.</param>
    /// <param name="prop">The property on the parent config that holds the nested group.</param>
    /// <param name="propType">The runtime type of the nested group.</param>
    /// <param name="nestedDrawerAttr">The resolved nested-group drawer attribute.</param>
    /// <param name="nested">The live nested group instance retrieved from <paramref name="prop"/>.</param>
    /// <param name="owner">The parent config instance that owns <paramref name="prop"/>.</param>
    private void CollectFlatNestedDrawerNode(
        List<IDrawNode> target,
        PropertyInfo prop,
        Type propType,
        INestedGroupDrawerAttribute nestedDrawerAttr,
        object nested,
        object owner)
    {
        try
        {
            var drawerInstance = Activator.CreateInstance(nestedDrawerAttr.DrawerType)!;

            Type? genericIface = null;
            Type? groupType = null;
            foreach (var iface in nestedDrawerAttr.DrawerType.GetInterfaces())
            {
                if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(INestedGroupDrawer<>))
                    continue;

                var candidateGroupType = iface.GetGenericArguments()[0];
                if (!candidateGroupType.IsAssignableFrom(propType))
                    continue;

                genericIface = iface;
                groupType = candidateGroupType;
                break;
            }

            if (genericIface is null || groupType is null)
            {
                Logger.Error(
                    $"ConfigDrawer: nested group drawer '{nestedDrawerAttr.DrawerType.Name}' does not support group type '{propType.FullName}'.");
                return;
            }

            if (drawerInstance is IDisposable disposable)
                Disposables.Add(disposable);

            var drawMethod = genericIface.GetMethod("Draw")!;
            var callExpr = Expression.Call(
                Expression.Convert(Expression.Constant(drawerInstance), genericIface),
                drawMethod,
                Expression.Convert(Expression.Constant(nested), groupType));
            var drawAction = Expression.Lambda<Action>(callExpr).Compile();

            var propHideIf = GetHideIfAttribute(prop);
            target.Add(new ParameterNode(
                VisibilityPredicateResolver.Build(propHideIf, owner),
                drawAction,
                order: prop.GetCustomAttribute<ParameterOrderAttribute>()?.Order ?? int.MaxValue,
                spacingBefore: prop.GetCustomAttribute<SpacingBeforeAttribute>()?.Count ?? 0,
                spacingAfter: prop.GetCustomAttribute<SpacingAfterAttribute>()?.Count ?? 0));
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigDrawer: failed to instantiate nested group drawer '{nestedDrawerAttr.DrawerType.Name}'.");
        }
    }

    /// <summary>
    /// Emits a <see cref="CategoryNode"/> for <paramref name="category"/> if it differs from the
    /// last emitted category, then updates the tracked category, active category node, and parameter list.
    /// </summary>
    /// <param name="category">The category name to emit, or <see langword="null"/> to skip.</param>
    /// <param name="collapseAttr">
    /// When non-<see langword="null"/>, the emitted node renders as a collapsible <c>ImGui.TreeNode</c>
    /// scope; otherwise it renders as a flat <c>ImGui.SeparatorText</c> header.
    /// </param>
    /// <param name="indentAttr">
    /// The property-level <see cref="IndentAttribute"/> from the parent's property declaration,
    /// or <see langword="null"/> when no indent was requested. When non-<see langword="null"/>,
    /// the emitted <see cref="CategoryNode"/> wraps its entire output — section header and all
    /// child controls — inside a matching <c>ImGui.Indent</c>/<c>ImGui.Unindent</c> scope.
    /// </param>
    private void EmitCategoryHeader(string? category, CollapseAsTreeAttribute? collapseAttr, IndentAttribute? indentAttr)
    {
        if (category is null || category == _lastCategory) return;

        // If this category was already emitted earlier — e.g. after Collect() returned from a
        // nested group and _lastCategory changed — resume routing into the existing node instead
        // of creating a duplicate header.
        if (_namedCategories.TryGetValue(category, out var existing))
        {
            _currentCategoryNode = existing;
            _lastCategory = category;
            return;
        }

        var node = new CategoryNode(category, collapseAttr, indentAttr);
        Nodes.Add(node);
        _allCategoryNodes.Add(node);
        _namedCategories[category] = node;
        _currentCategoryNode = node;
        _lastCategory = category;
    }

    /// <summary>
    /// Instantiates the resolved <see cref="INestedGroupDrawer{T}"/>, compiles a zero-reflection
    /// draw delegate, routes the resulting <see cref="ParameterNode"/> into the correct category
    /// bucket, and registers the drawer as a disposable resource if it implements <see cref="IDisposable"/>.
    /// </summary>
    /// <remarks>
    /// Extracted from <see cref="Collect"/> to keep tree-walking logic separate from the
    /// one-time drawer instantiation and expression-compilation concern.
    /// </remarks>
    /// <param name="prop">The property on the parent config that holds the nested group.</param>
    /// <param name="propType">The runtime type of the nested group.</param>
    /// <param name="nestedDrawerAttr">
    /// The resolved nested-group drawer attribute selected from the property first and the nested
    /// group type second.
    /// </param>
    /// <param name="nested">The live nested group instance retrieved from <paramref name="prop"/>.</param>
    /// <param name="owner">The parent config instance that owns <paramref name="prop"/>.</param>
    /// <param name="category">The effective category resolved for the nested group.</param>
    /// <param name="collapseAttr">The effective collapse behaviour resolved for the nested group.</param>
    private void EmitNestedGroupDrawerNode(
        PropertyInfo prop,
        Type propType,
        INestedGroupDrawerAttribute nestedDrawerAttr,
        object nested,
        object owner,
        string? category,
        CollapseAsTreeAttribute? collapseAttr)
    {
        try
        {
            var drawerInstance = Activator.CreateInstance(nestedDrawerAttr.DrawerType)!;

            Type? genericIface = null;
            Type? groupType = null;
            foreach (var iface in nestedDrawerAttr.DrawerType.GetInterfaces())
            {
                if (!iface.IsGenericType)
                    continue;

                if (iface.GetGenericTypeDefinition() != typeof(INestedGroupDrawer<>))
                    continue;

                var candidateGroupType = iface.GetGenericArguments()[0];
                // Ensure the drawer's group type is compatible with the nested group's actual type.
                if (!candidateGroupType.IsAssignableFrom(propType))
                    continue;

                genericIface = iface;
                groupType = candidateGroupType;
                break;
            }

            if (genericIface is null || groupType is null)
            {
                Logger.Error(
                    $"ConfigDrawer: nested group drawer '{nestedDrawerAttr.DrawerType.Name}' does not support group type '{propType.FullName}'.");
                return;
            }

            if (drawerInstance is IDisposable disposable)
                Disposables.Add(disposable);

            var drawMethod = genericIface.GetMethod("Draw")!;
            var callExpr = Expression.Call(
                Expression.Convert(Expression.Constant(drawerInstance), genericIface),
                drawMethod,
                Expression.Convert(Expression.Constant(nested), groupType));
            var drawAction = Expression.Lambda<Action>(callExpr).Compile();

            EmitCategoryHeader(
                category,
                collapseAttr,
                prop.GetCustomAttribute<IndentAttribute>());

            var targetList = category is not null ? _currentCategoryNode!.Children : Nodes;

            var propHideIf = GetHideIfAttribute(prop);

            targetList.Add(new ParameterNode(
                VisibilityPredicateResolver.Build(propHideIf, owner),
                drawAction,
                order: prop.GetCustomAttribute<ParameterOrderAttribute>()?.Order ?? int.MaxValue,
                spacingBefore: prop.GetCustomAttribute<SpacingBeforeAttribute>()?.Count ?? 0,
                spacingAfter: prop.GetCustomAttribute<SpacingAfterAttribute>()?.Count ?? 0));
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, $"ConfigDrawer: failed to instantiate nested group drawer '{nestedDrawerAttr.DrawerType.Name}'.");
        }
    }

    /// <summary>
    /// Reads property-level metadata that wraps an entire plain nested settings group rather than
    /// an individual leaf parameter.
    /// </summary>
    /// <param name="prop">The parent property that holds the nested settings group.</param>
    /// <param name="hideIf">Receives the property's optional <see cref="HideIfAttribute{T}"/>.</param>
    /// <param name="order">Receives the property's optional explicit sort order.</param>
    /// <param name="spacingBefore">Receives the property's optional spacing inserted above the group.</param>
    /// <param name="spacingAfter">Receives the property's optional spacing inserted below the group.</param>
    /// <returns>
    /// <see langword="true"/> when at least one wrapper-style property attribute is present and the
    /// nested group should be emitted as a single wrapped section; otherwise <see langword="false"/>.
    /// </returns>
    private static bool TryGetNestedGroupWrapperMetadata(
        PropertyInfo prop,
        out IHideIfAttribute? hideIf,
        out int order,
        out int spacingBefore,
        out int spacingAfter)
    {
        hideIf = GetHideIfAttribute(prop);
        order = prop.GetCustomAttribute<ParameterOrderAttribute>()?.Order ?? int.MaxValue;
        spacingBefore = prop.GetCustomAttribute<SpacingBeforeAttribute>()?.Count ?? 0;
        spacingAfter = prop.GetCustomAttribute<SpacingAfterAttribute>()?.Count ?? 0;

        return hideIf is not null
            || order != int.MaxValue
            || spacingBefore != 0
            || spacingAfter != 0;
    }

    /// <summary>
    /// Returns the first property-level hide-condition attribute declared on <paramref name="prop"/>,
    /// or <see langword="null"/> when none is present.
    /// </summary>
    /// <param name="prop">The reflected property to inspect.</param>
    private static IHideIfAttribute? GetHideIfAttribute(PropertyInfo prop)
    {
        foreach (var attribute in prop.GetCustomAttributes(false))
        {
            if (attribute is IHideIfAttribute hideIf)
                return hideIf;
        }

        return null;
    }

    /// <summary>
    /// Returns the nested-group drawer attribute declared on <paramref name="prop"/>, or the
    /// nested type's cached fallback declaration when the property defines no drawer of its own.
    /// </summary>
    /// <param name="prop">The nested-group property being inspected.</param>
    /// <param name="propTypeMeta">The cached type-level metadata for the nested group type.</param>
    private static INestedGroupDrawerAttribute? GetNestedGroupDrawerAttribute(PropertyInfo prop, TypeDrawMetadata propTypeMeta)
    {
        foreach (var attribute in prop.GetCustomAttributes(false))
        {
            if (attribute is INestedGroupDrawerAttribute nestedDrawerAttr)
                return nestedDrawerAttr;
        }

        return propTypeMeta.NestedGroupDrawerAttr;
    }

    /// <summary>
    /// Applies a stable sort to parameter nodes within every category context, ordering them
    /// by their <see cref="ParameterNode.Order"/> value ascending.
    /// </summary>
    /// <remarks>
    /// Call this once after <see cref="Collect"/> has finished walking the entire config tree.
    /// Nodes without an explicit <c>[ParameterOrder]</c> attribute receive an implicit key of
    /// <see cref="int.MaxValue"/>, placing them after all explicitly ordered entries while
    /// preserving original declaration order among equals. Both per-category children and the
    /// flat top-level <see cref="Nodes"/> list are sorted in place via
    /// <see cref="ListExtensions.StableSortBy{T}"/>. <see cref="CategoryNode"/> entries use
    /// <see cref="int.MaxValue"/> as their sort key and therefore keep their insertion order
    /// relative to each other and to unordered parameters.
    /// </remarks>
    internal void SortAll()
    {
        static int NodeOrder(IDrawNode n) => n is ParameterNode p ? p.Order : int.MaxValue;

        foreach (var cat in _allCategoryNodes)
            cat.Children.StableSortBy(NodeOrder);

        Nodes.StableSortBy(NodeOrder);
    }
}
