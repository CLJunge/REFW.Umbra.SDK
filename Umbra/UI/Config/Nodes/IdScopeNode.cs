using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Wraps a subtree of configuration draw nodes in a stable ImGui ID scope.
/// </summary>
/// <remarks>
/// The scope ID is typically derived from the structural settings path of a nested group so repeated local widget labels remain isolated across sibling branches of the configuration tree. The pop operation always runs even if a child node throws while drawing.
/// </remarks>
internal sealed class IdScopeNode : IDrawNode, IConfigSearchNode
{
    private readonly string _scopeId;
    private readonly List<IDrawNode> _children;
    private readonly IIdScopeNodeRenderer _renderer;
    private bool _searchVisible = true;

    /// <summary>
    /// Initializes a new <see cref="IdScopeNode"/> that renders through the shared ImGui render context.
    /// </summary>
    /// <param name="scopeId">The stable ImGui ID pushed before drawing the subtree.</param>
    /// <param name="children">The child nodes rendered inside the pushed scope.</param>
    internal IdScopeNode(string scopeId, List<IDrawNode> children)
        : this(scopeId, children, ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="IdScopeNode"/> with the specified renderer seam.
    /// </summary>
    /// <param name="scopeId">The stable ImGui ID pushed before drawing the subtree.</param>
    /// <param name="children">The child nodes rendered inside the pushed scope.</param>
    /// <param name="renderer">The renderer used for ID-scope push and pop operations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="children"/> or <paramref name="renderer"/> is <see langword="null"/>.</exception>
    internal IdScopeNode(string scopeId, List<IDrawNode> children, IIdScopeNodeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(children);
        ArgumentNullException.ThrowIfNull(renderer);
        _scopeId = scopeId;
        _children = children;
        _renderer = renderer;
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (!_searchVisible)
            return;

        _renderer.PushId(_scopeId);
        try
        {
            foreach (var child in _children)
                child.Draw();
        }
        finally
        {
            _renderer.PopId();
        }
    }

    bool IConfigSearchNode.ApplySearch(ConfigSearchRenderState? searchState)
    {
        if (searchState is null || !searchState.HasActiveQuery)
        {
            _searchVisible = true;
            ApplySearchToChildren(null);
            return true;
        }

        _searchVisible = ApplySearchToChildren(searchState);
        return _searchVisible;
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
