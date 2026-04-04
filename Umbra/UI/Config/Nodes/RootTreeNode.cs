using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Wraps the root configuration draw tree in one collapsible tree node.
/// </summary>
/// <remarks>
/// <see cref="ConfigDrawer{TConfig}"/> emits this node when the root configuration type carries <see cref="Umbra.Config.Attributes.UmbraRootNodeAttribute"/> and root-node suppression is not requested.
/// </remarks>
internal sealed class RootTreeNode : IDrawNode, IConfigSearchNode
{
    private readonly string _label;
    private readonly bool _defaultOpen;
    private readonly string? _branchId;
    private readonly List<IDrawNode> _children;
    private readonly IRootTreeNodeRenderer _renderer;
    private bool _searchVisible = true;
    private bool _forceOpen;

    /// <summary>
    /// Initializes a new <see cref="RootTreeNode"/> that renders through the shared ImGui render context.
    /// </summary>
    /// <param name="label">The visible label shown on the root tree node.</param>
    /// <param name="defaultOpen"><see langword="true"/> to start the root tree node expanded; otherwise, <see langword="false"/>.</param>
    /// <param name="children">The ordered child nodes rendered while the root tree node is open.</param>
    internal RootTreeNode(string label, bool defaultOpen, List<IDrawNode> children, string? branchId = null)
        : this(label, defaultOpen, children, branchId, ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="RootTreeNode"/> with the specified renderer seam.
    /// </summary>
    /// <param name="label">The visible label shown on the root tree node.</param>
    /// <param name="defaultOpen"><see langword="true"/> to start the root tree node expanded; otherwise, <see langword="false"/>.</param>
    /// <param name="children">The ordered child nodes rendered while the root tree node is open.</param>
    /// <param name="renderer">The renderer used for tree-node operations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="children"/> or <paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal RootTreeNode(string label, bool defaultOpen, List<IDrawNode> children, string? branchId, IRootTreeNodeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(children);
        ArgumentNullException.ThrowIfNull(renderer);
        _label = label;
        _defaultOpen = defaultOpen;
        _branchId = branchId;
        _children = children;
        _renderer = renderer;
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (!_searchVisible) return;
        if (!_renderer.TreeNode(_label, _defaultOpen, _forceOpen)) return;
        try
        {
            foreach (var child in _children)
                child.Draw();
        }
        finally
        {
            _renderer.TreePop();
        }
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
        foreach (var child in _children)
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
