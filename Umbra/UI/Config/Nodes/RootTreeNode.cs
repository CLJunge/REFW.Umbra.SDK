using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Draw node that wraps all child nodes inside a single collapsible tree node.
/// Emitted by <see cref="ConfigDrawer{TConfig}"/> when
/// <see cref="Umbra.Config.Attributes.UmbraRootNodeAttribute"/> is present on the root
/// config class; the entire settings panel lives inside this one node.
/// </summary>
/// <remarks>
/// The default constructor renders through the shared active ImGui context. Unit tests can replace
/// the low-level renderer through the internal constructor so tree-open behavior and cleanup can be
/// verified without requiring an active ImGui frame.
/// </remarks>
internal sealed class RootTreeNode : IDrawNode
{
    private readonly string _label;
    private readonly bool _defaultOpen;
    private readonly List<IDrawNode> _children;
    private readonly IRootTreeNodeRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="RootTreeNode"/> that renders through the shared active ImGui context.
    /// </summary>
    /// <param name="label">The label displayed on the tree node header.</param>
    /// <param name="defaultOpen">When <see langword="true"/>, the tree node starts expanded on first render.</param>
    /// <param name="children">The ordered list of child draw nodes to render when the node is open.</param>
    internal RootTreeNode(string label, bool defaultOpen, List<IDrawNode> children)
        : this(label, defaultOpen, children, ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="RootTreeNode"/> with the specified low-level renderer.
    /// </summary>
    /// <param name="label">The label displayed on the tree node header.</param>
    /// <param name="defaultOpen">When <see langword="true"/>, the tree node starts expanded on first render.</param>
    /// <param name="children">The ordered list of child draw nodes to render when the node is open.</param>
    /// <param name="renderer">The renderer used for tree-node operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="children"/> or <paramref name="renderer"/> is <see langword="null"/>.</exception>
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
