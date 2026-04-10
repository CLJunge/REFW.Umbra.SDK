using Umbra.UI.Config.Rendering;
using Umbra.UI.Config.Search;

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
    private bool _isOpen;
    private bool _searchStateCaptured;
    private bool _capturedOpenState;

    /// <summary>
    /// Initializes a new <see cref="RootTreeNode"/> that renders through the shared ImGui render context.
    /// </summary>
    /// <param name="label">The visible label shown on the root tree node.</param>
    /// <param name="defaultOpen"><see langword="true"/> to start the root tree node expanded; otherwise, <see langword="false"/>.</param>
    /// <param name="children">The ordered child nodes rendered while the root tree node is open.</param>
    /// <param name="branchId">The stable search branch identifier for this root wrapper, or <see langword="null"/> when no search-driven branch state is associated with it.</param>
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
    /// <param name="branchId">The stable search branch identifier for this root wrapper, or <see langword="null"/> when no search-driven branch state is associated with it.</param>
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
        _isOpen = defaultOpen;
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (!_searchVisible) return;
        var open = _renderer.TreeNode(_label, _defaultOpen, _isOpen, _forceOpen);
        _isOpen = open;
        if (!open) return;
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
            RestoreTreeStateAfterSearch();
            _searchVisible = true;
            _forceOpen = false;
            ApplySearchToChildren(null);
            return true;
        }

        CaptureTreeStateForSearch();
        var hasVisibleChild = ApplySearchToChildren(searchState);
        _searchVisible = hasVisibleChild;
        _forceOpen = hasVisibleChild || searchState.IsBranchForcedOpen(_branchId);
        return hasVisibleChild;
    }

    private void CaptureTreeStateForSearch()
    {
        if (_searchStateCaptured)
            return;

        _capturedOpenState = _isOpen;
        _searchStateCaptured = true;
    }

    private void RestoreTreeStateAfterSearch()
    {
        if (!_searchStateCaptured)
            return;

        _isOpen = _capturedOpenState;
        _searchStateCaptured = false;
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
