using Umbra.Config.Attributes;
using Umbra.UI.Config.Nodes;
using Umbra.UI.Config.Search;

namespace Umbra.UI.Config;

/// <summary>
/// Collects the cached draw nodes and disposables consumed by <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
/// <remarks>
/// This builder owns top-level scope setup, node and disposable collection, category tracking, and final stable ordering. Recursive traversal of nested configuration scopes is delegated to <see cref="ConfigDrawTreeCollector"/>.
/// </remarks>
internal sealed class ConfigDrawerBuilder(INumericEditSink? numericEditSink = null, ITextEditSink? textEditSink = null)
{
    private readonly List<CategoryNode> _allCategoryNodes = [];

    /// <summary>
    /// Gets the top-level nodes assembled during the current <see cref="Collect"/> pass.
    /// </summary>
    internal readonly List<IDrawNode> Nodes = [];

    /// <summary>
    /// Gets the disposable resources collected during the current <see cref="Collect"/> pass.
    /// </summary>
    internal readonly List<IDisposable> Disposables = [];

    /// <summary>
    /// Gets the flat search index collected during the current <see cref="Collect"/> pass.
    /// </summary>
    internal ConfigSearchIndex SearchIndex { get; } = new();

    /// <summary>
    /// Walks <paramref name="obj"/> and rebuilds the cached node and disposable lists for the supplied configuration scope.
    /// </summary>
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
        SearchIndex.Clear();

        var typeMeta = TypeDrawMetadata.For(type);
        var rootGroupPath = typeMeta.ConfigPrefix ?? string.Empty;
        var scope = new ConfigDrawScope(
            rootGroupPath,
            categoryOverride ?? typeMeta.Category,
            collapseOverride ?? typeMeta.CollapseAttr,
            propertyIndentOverride,
            labelMarginOverride ?? typeMeta.LabelMarginAttr,
            RegisterCategoryNode);

        ConfigDrawTreeCollector.CollectInto(scope, obj, type, RegisterCategoryNode, Disposables, SortNodesInPlace, SearchIndex, numericEditSink, textEditSink: textEditSink);

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
    /// Applies the final stable parameter-order sort to every tracked rendered scope.
    /// </summary>
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
