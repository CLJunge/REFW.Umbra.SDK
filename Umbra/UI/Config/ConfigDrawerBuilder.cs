using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Builds the top-level draw-tree result consumed by <see cref="ConfigDrawer{TConfig}.Draw"/>.
/// </summary>
/// <remarks>
/// <para>
/// The builder treats every nested settings object as its own layout scope. Each scope owns a
/// local category map, so category names are only unique within the group that declares them.
/// This allows arbitrarily deep nested settings groups without category collisions between sibling
/// or cousin branches of the configuration tree.
/// </para>
/// <para>
/// Recursive traversal of the config object graph is delegated to
/// <see cref="ConfigDrawTreeCollector"/>. This type remains responsible for top-level scope setup,
/// collecting the final node and disposable lists, tracking category nodes for later stable sorting,
/// and applying the final parameter-order pass.
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

        ConfigDrawTreeCollector.CollectInto(scope, obj, type, RegisterCategoryNode, Disposables, SortNodesInPlace);

        foreach (var node in scope.Nodes)
            Nodes.Add(node);
    }

    /// <summary>
    /// Registers a category node created by a child <see cref="ConfigDrawScope"/> so it can be
    /// included in the final per-category stable sort pass.
    /// </summary>
    /// <param name="node">The category node to track.</param>
    private void RegisterCategoryNode(CategoryNode node) => _allCategoryNodes.Add(node);

    /// <summary>
    /// Applies a stable sort to parameter nodes within each tracked rendered scope, ordering them
    /// by their <see cref="ParameterNode.Order"/> value ascending.
    /// </summary>
    /// <remarks>
    /// Call this once after <see cref="Collect"/> has finished walking the entire config tree.
    /// Nodes without an explicit
    /// <see cref="Umbra.Config.Attributes.UmbraParameterOrderAttribute"/> (<c>[UmbraParameterOrder]</c>)
    /// receive an implicit key of <see cref="int.MaxValue"/>, placing them after all explicitly
    /// ordered entries while
    /// preserving original declaration order among equals. The root <see cref="Nodes"/> list and
    /// every local <see cref="CategoryNode.Children"/> list are sorted independently. Uncategorized
    /// nested-group scope roots are sorted during collection before they are wrapped in an
    /// <see cref="IdScopeNode"/>, so ordering remains local to each rendered scope.
    /// </remarks>
    internal void SortAll()
    {
        foreach (var cat in _allCategoryNodes)
            SortNodesInPlace(cat.Children);

        SortNodesInPlace(Nodes);
    }

    /// <summary>
    /// Applies the local stable parameter-order sort to one rendered node list.
    /// </summary>
    /// <param name="nodes">The node list to sort in place.</param>
    private static void SortNodesInPlace(List<IDrawNode> nodes)
        => nodes.SortBy(static n => n is ParameterNode p ? p.Order : int.MaxValue);
}
