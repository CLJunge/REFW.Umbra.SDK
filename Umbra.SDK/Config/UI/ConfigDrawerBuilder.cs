using System.Linq.Expressions;
using System.Reflection;
using Hexa.NET.ImGui;
using Umbra.SDK.Config.Attributes;
using Umbra.SDK.Config.UI.Nodes;
using Umbra.SDK.Config.UI.ParameterDrawers;
using Umbra.SDK.Logging;

namespace Umbra.SDK.Config.UI;

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
    /// <remarks>
    /// Returns immediately without emitting any nodes when <paramref name="type"/> is decorated
    /// with <see cref="INestedGroupDrawerAttribute"/>. Such types are rendered entirely by their
    /// custom drawer; expanding their parameters here would duplicate what the drawer manages.
    /// </remarks>
    internal void Collect(object obj, Type type, IndentAttribute? propertyIndentOverride = null)
    {
        // A type decorated with [NestedGroupDrawer<TDrawer>] is rendered entirely by its
        // custom drawer. Expanding its parameters here would duplicate what that drawer
        // manages, so bail out immediately regardless of how Collect was reached.
        var typeMeta = TypeDrawMetadata.For(type);
        if (typeMeta.NestedGroupDrawer is not null)
            return;

        var classCategory  = typeMeta.Category;
        var classIndent    = typeMeta.Indent;
        var classCollapseAttr = typeMeta.CollapseAttr;
        var classLabelMargin  = typeMeta.LabelMargin;

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
                float? indentAmount = meta.Indent ?? classIndent?.Amount;
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
                // Class-level [NestedGroupDrawer<TDrawer>] — skip recursion and render the
                // group instance directly via the custom drawer.
                var nestedDrawerAttr = propTypeMeta.NestedGroupDrawer;
                if (nestedDrawerAttr is not null)
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
                            continue;
                        }

                        if (drawerInstance is IDisposable disposable)
                            Disposables.Add(disposable);

                        var drawMethod = genericIface.GetMethod("Draw")!;
                        var callExpr = Expression.Call(
                            Expression.Convert(Expression.Constant(drawerInstance), genericIface),
                            drawMethod,
                            Expression.Convert(Expression.Constant(nested), groupType));
                        var drawAction = Expression.Lambda<Action>(callExpr).Compile();

                        // Category: property override → nested class attribute → parent class attribute.
                        var cat = prop.GetCustomAttribute<CategoryAttribute>()?.Name
                                 ?? propTypeMeta.Category
                                 ?? classCategory;

                        EmitCategoryHeader(
                            cat,
                            propTypeMeta.CollapseAttr,
                            prop.GetCustomAttribute<IndentAttribute>());

                        var targetList = cat is not null ? _currentCategoryNode!.Children : Nodes;

                        IHideIfAttribute? propHideIf = null;
                        foreach (var a in prop.GetCustomAttributes(false))
                        {
                            if (a is IHideIfAttribute h) { propHideIf = h; break; }
                        }

                        targetList.Add(new ParameterNode(
                            VisibilityPredicateResolver.Build(propHideIf, obj),
                            drawAction,
                            order: prop.GetCustomAttribute<ParameterOrderAttribute>()?.Order ?? 0,
                            spacingBefore: prop.GetCustomAttribute<SpacingBeforeAttribute>()?.Count ?? 0,
                            spacingAfter: prop.GetCustomAttribute<SpacingAfterAttribute>()?.Count ?? 0));
                    }
                    catch (Exception ex)
                    {
                        Logger.Exception(ex, $"ConfigDrawer: failed to instantiate nested group drawer '{nestedDrawerAttr.DrawerType.Name}'.");
                    }
                }
                else
                {
                    Collect(nested, propType, prop.GetCustomAttribute<IndentAttribute>());
                }
            }
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
        static int NodeOrder(IDrawNode n)
        {
            return n is ParameterNode p ? p.Order : int.MaxValue;
        }

        foreach (var cat in _allCategoryNodes)
            cat.Children.StableSortBy(NodeOrder);

        Nodes.StableSortBy(NodeOrder);
    }
}
