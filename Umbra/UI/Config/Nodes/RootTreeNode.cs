using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Wraps the root configuration draw tree in one collapsible tree node.
/// </summary>
/// <remarks>
/// <see cref="ConfigDrawer{TConfig}"/> emits this node when the root configuration type carries <see cref="Umbra.Config.Attributes.UmbraRootNodeAttribute"/> and root-node suppression is not requested.
/// </remarks>
internal sealed class RootTreeNode : IDrawNode
{
    private readonly string _label;
    private readonly bool _defaultOpen;
    private readonly List<IDrawNode> _children;
    private readonly IRootTreeNodeRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="RootTreeNode"/> that renders through the shared ImGui render context.
    /// </summary>
    /// <param name="label">The visible label shown on the root tree node.</param>
    /// <param name="defaultOpen"><see langword="true"/> to start the root tree node expanded; otherwise, <see langword="false"/>.</param>
    /// <param name="children">The ordered child nodes rendered while the root tree node is open.</param>
    internal RootTreeNode(string label, bool defaultOpen, List<IDrawNode> children)
        : this(label, defaultOpen, children, ImGuiConfigRenderContext.Instance)
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
    internal RootTreeNode(string label, bool defaultOpen, List<IDrawNode> children, IRootTreeNodeRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(children);
        ArgumentNullException.ThrowIfNull(renderer);
        _label = label;
        _defaultOpen = defaultOpen;
        _children = children;
        _renderer = renderer;
    }

    /// <inheritdoc/>
    public void Draw()
    {
        if (!_renderer.TreeNode(_label, _defaultOpen)) return;
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
}
