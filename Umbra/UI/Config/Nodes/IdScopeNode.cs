namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Draw node that wraps a subtree in a stable ImGui ID scope.
/// </summary>
/// <remarks>
/// The scope ID is typically a dot-separated structural path derived from the owning nested
/// settings-group property and its configured prefix. This keeps repeated local widget labels and
/// custom nested-group drawer IDs isolated across sibling branches of the configuration tree.
/// The default constructor renders through the active ImGui frame. Unit tests can replace the
/// low-level renderer through the internal constructor so scope cleanup can be verified without a
/// live ImGui context. The pop operation is guaranteed to run even if a child node throws while
/// drawing.
/// </remarks>
/// <param name="scopeId">The stable ImGui ID pushed before drawing the subtree.</param>
/// <param name="children">The child nodes that should render inside the pushed ID scope.</param>
internal sealed class IdScopeNode : IDrawNode
{
    private readonly string _scopeId;
    private readonly List<IDrawNode> _children;
    private readonly IIdScopeNodeRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="IdScopeNode"/> that renders through the active ImGui frame.
    /// </summary>
    /// <param name="scopeId">The stable ImGui ID pushed before drawing the subtree.</param>
    /// <param name="children">The child nodes that should render inside the pushed ID scope.</param>
    internal IdScopeNode(string scopeId, List<IDrawNode> children)
        : this(scopeId, children, new ImGuiIdScopeNodeRenderer())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="IdScopeNode"/> with the specified low-level renderer.
    /// </summary>
    /// <param name="scopeId">The stable ImGui ID pushed before drawing the subtree.</param>
    /// <param name="children">The child nodes that should render inside the pushed ID scope.</param>
    /// <param name="renderer">The renderer used for ID-scope push/pop operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="children"/> or <paramref name="renderer"/> is <see langword="null"/>.</exception>
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
}
