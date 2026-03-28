using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config;

/// <summary>
/// Represents one local layout scope while a configuration draw tree is being assembled.
/// </summary>
/// <remarks>
/// A scope owns the node list, category map, and label-alignment state for one configuration
/// group. Category creation and routing are handled here so <see cref="ConfigDrawerBuilder"/>
/// can stay focused on traversal and composition.
/// </remarks>
internal sealed class ConfigDrawScope(
    string groupPath,
    string? defaultCategory,
    UmbraCollapseAsTreeAttribute? collapseAttr,
    UmbraIndentAttribute? categoryIndentAttr,
    UmbraLabelMarginAttribute? labelMarginAttr,
    Action<CategoryNode> registerCategory,
    LabelAlignmentGroup? alignmentGroup = null)
{
    private readonly Dictionary<string, CategoryNode> _namedCategories = [];
    private CategoryNode? _currentCategoryNode;
    private string? _lastCategory;

    internal string GroupPath { get; } = groupPath;
    internal string? DefaultCategory { get; } = defaultCategory;
    internal UmbraCollapseAsTreeAttribute? CollapseAttr { get; } = collapseAttr;
    internal UmbraIndentAttribute? CategoryIndentAttr { get; } = categoryIndentAttr;
    internal UmbraLabelMarginAttribute? LabelMarginAttr { get; } = labelMarginAttr;
    internal List<IDrawNode> Nodes { get; } = [];
    internal LabelAlignmentGroup AlignmentGroup { get; } = alignmentGroup ?? new();

    /// <summary>
    /// Routes one node into the specified category bucket of this scope, or the scope root when
    /// <paramref name="category"/> is <see langword="null"/>.
    /// </summary>
    /// <param name="category">The local category bucket to route into, or <see langword="null"/> for the root list.</param>
    /// <param name="node">The node to append.</param>
    internal void AddNode(string? category, IDrawNode node)
    {
        var targetList = GetTargetList(category);
        targetList.Add(node);
    }

    /// <summary>
    /// Returns the active <see cref="LabelAlignmentGroup"/> for <paramref name="category"/>,
    /// creating the category header on demand when required.
    /// </summary>
    /// <param name="category">The local category name, or <see langword="null"/> for the scope-wide alignment group.</param>
    internal LabelAlignmentGroup GetAlignmentGroup(string? category)
    {
        if (category is null)
            return AlignmentGroup;

        EnsureCategory(category);
        return _currentCategoryNode!.AlignmentGroup;
    }

    /// <summary>
    /// Materializes this collected scope into a visible container category.
    /// </summary>
    /// <param name="category">The category label for the container.</param>
    /// <returns>
    /// A <see cref="CategoryNode"/> whose children are the current top-level nodes of this scope.
    /// </returns>
    internal CategoryNode CreateContainerNode(string category)
    {
        var node = new CategoryNode(category, CollapseAttr, CategoryIndentAttr);
        foreach (var child in Nodes)
            node.Children.Add(child);

        registerCategory(node);
        return node;
    }

    /// <summary>
    /// Returns the target node list for <paramref name="category"/>, creating the category header
    /// on demand when necessary.
    /// </summary>
    /// <param name="category">The local category bucket to route into, or <see langword="null"/> for the root list.</param>
    private List<IDrawNode> GetTargetList(string? category)
    {
        if (category is null)
            return Nodes;

        EnsureCategory(category);
        return _currentCategoryNode!.Children;
    }

    /// <summary>
    /// Ensures that a <see cref="CategoryNode"/> exists for <paramref name="category"/> within this scope.
    /// </summary>
    /// <param name="category">The category name to resolve.</param>
    private void EnsureCategory(string category)
    {
        if (category == _lastCategory)
            return;

        if (_namedCategories.TryGetValue(category, out var existing))
        {
            _currentCategoryNode = existing;
            _lastCategory = category;
            return;
        }

        var node = new CategoryNode(category, CollapseAttr, CategoryIndentAttr);
        Nodes.Add(node);
        _namedCategories[category] = node;
        _currentCategoryNode = node;
        _lastCategory = category;
        registerCategory(node);
    }
}
