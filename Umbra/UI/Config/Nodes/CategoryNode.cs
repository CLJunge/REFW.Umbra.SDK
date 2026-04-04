using System.Diagnostics;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Renders one configuration category together with its child nodes.
/// </summary>
/// <remarks>
/// A category can render either as a flat separator block or as a collapsible tree scope depending on whether <see cref="UmbraCollapseAsTreeAttribute"/> metadata is supplied. The default constructor uses the shared ImGui render context; tests can supply a renderer seam to verify category behavior without an active ImGui frame.
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
internal sealed class CategoryNode : IDrawNode, IConfigSearchNode
{
    private readonly string _label;
    private readonly string? _branchId;
    private readonly UmbraCollapseAsTreeAttribute? _collapseAttr;
    private readonly UmbraIndentAttribute? _indentAttr;
    private readonly ICategoryNodeRenderer _renderer;
    private bool _searchVisible = true;
    private bool _forceOpen;

    /// <summary>
    /// Initializes a new <see cref="CategoryNode"/> that renders through the shared ImGui render context.
    /// </summary>
    /// <param name="label">The visible category label.</param>
    /// <param name="collapseAttr">Optional collapse metadata that switches the category to tree-node rendering.</param>
    /// <param name="indentAttr">Optional indentation metadata applied around the category header and its children.</param>
    internal CategoryNode(
        string label,
        string? branchId = null,
        UmbraCollapseAsTreeAttribute? collapseAttr = null,
        UmbraIndentAttribute? indentAttr = null)
        : this(label, branchId, collapseAttr, indentAttr, ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="CategoryNode"/> with the specified renderer seam.
    /// </summary>
    /// <param name="label">The visible category label.</param>
    /// <param name="collapseAttr">Optional collapse metadata that switches the category to tree-node rendering.</param>
    /// <param name="indentAttr">Optional indentation metadata applied around the category header and its children.</param>
    /// <param name="renderer">The renderer used for category-node UI operations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal CategoryNode(
        string label,
        string? branchId,
        UmbraCollapseAsTreeAttribute? collapseAttr,
        UmbraIndentAttribute? indentAttr,
        ICategoryNodeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _label = label;
        _branchId = branchId;
        _collapseAttr = collapseAttr;
        _indentAttr = indentAttr;
        _renderer = renderer;
    }

    /// <summary>
    /// Gets the label-alignment group shared by the parameter rows rendered inside this category.
    /// </summary>
    internal LabelAlignmentGroup AlignmentGroup { get; } = new();

    /// <summary>
    /// Gets the ordered child nodes rendered inside this category.
    /// </summary>
    internal readonly List<IDrawNode> Children = [];

    /// <inheritdoc/>
    public void Draw()
    {
        if (!_searchVisible)
            return;

        var hasIndent = _indentAttr != null;
        if (hasIndent) _renderer.Indent(_indentAttr!.Amount);
        try
        {
            if (_collapseAttr is not null) DrawAsTree();
            else DrawAsHeader();
        }
        finally
        {
            if (hasIndent) _renderer.Unindent(_indentAttr!.Amount);
        }
    }

    /// <summary>
    /// Renders the category as a non-collapsible separator header followed by all child nodes.
    /// </summary>
    private void DrawAsHeader()
    {
        _renderer.SeparatorText(_label);
        foreach (var child in Children)
            child.Draw();
    }

    /// <summary>
    /// Renders the category as a collapsible tree node and draws its children only while the tree is open.
    /// </summary>
    private void DrawAsTree()
    {
        var open = _renderer.TreeNode(_label, _collapseAttr!.DefaultOpen, _forceOpen);
        if (!open) return;

        try
        {
            foreach (var child in Children)
                child.Draw();
        }
        finally
        {
            _renderer.TreePop();
        }
    }

    private string GetDebuggerDisplay()
    {
        var displayString = $"Category: {_label}";

        if (Children.Count > 0)
            displayString += $" ({Children.Count} child node{(Children.Count > 1 ? "s" : "")})";

        return displayString;
    }

    bool IConfigSearchNode.ApplySearch(ConfigSearchRenderState? searchState)
    {
        if (searchState is null || !searchState.HasActiveQuery)
        {
            _searchVisible = true;
            _forceOpen = false;
            ApplySearchToChildren(null);
            return true;
        }

        var hasVisibleChild = ApplySearchToChildren(searchState);
        _searchVisible = hasVisibleChild;
        _forceOpen = hasVisibleChild || searchState.IsBranchForcedOpen(_branchId);
        return hasVisibleChild;
    }

    private bool ApplySearchToChildren(ConfigSearchRenderState? searchState)
    {
        var hasVisibleChild = false;
        foreach (var child in Children)
        {
            if (child is IConfigSearchNode searchNode)
            {
                if (searchNode.ApplySearch(searchState))
                    hasVisibleChild = true;
                continue;
            }

            hasVisibleChild = true;
        }

        return hasVisibleChild;
    }
}
