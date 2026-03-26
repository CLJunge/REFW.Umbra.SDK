using Hexa.NET.ImGui;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Draw node that wraps all child nodes inside a single collapsible <see cref="ImGui.TreeNode(string)"/>.
/// Emitted by <see cref="ConfigDrawer{TConfig}"/> when <see cref="Umbra.Config.Attributes.ConfigRootNodeAttribute"/>
/// is present on the root config class; the entire settings panel lives inside this one node.
/// </summary>
/// <param name="label">The label displayed on the tree node header.</param>
/// <param name="defaultOpen">When <see langword="true"/>, the tree node starts expanded on first render.</param>
/// <param name="children">The ordered list of child draw nodes to render when the node is open.</param>
internal sealed class RootTreeNode(string label, bool defaultOpen, List<IDrawNode> children) : IDrawNode
{
    /// <inheritdoc/>
    public void Draw()
    {
        var flags = defaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
        if (!ImGui.TreeNodeEx(label, flags)) return;
        try
        {
            foreach (var child in children)
                child.Draw();
        }
        finally
        {
            ImGui.TreePop();
        }
    }
}
