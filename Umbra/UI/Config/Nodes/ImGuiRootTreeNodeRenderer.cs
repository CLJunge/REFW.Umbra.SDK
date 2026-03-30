using Hexa.NET.ImGui;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Renders <see cref="RootTreeNode"/> output through the active ImGui frame.
/// </summary>
internal sealed class ImGuiRootTreeNodeRenderer : IRootTreeNodeRenderer
{
    /// <inheritdoc/>
    public bool TreeNode(string label, bool defaultOpen)
    {
        var flags = defaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
        return ImGui.TreeNodeEx(label, flags);
    }

    /// <inheritdoc/>
    public void TreePop() => ImGui.TreePop();
}
