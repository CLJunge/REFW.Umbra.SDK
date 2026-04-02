namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines collapsible ImGui tree-node operations.
/// </summary>
internal interface ITreeNodeOps
{
    /// <summary>
    /// Renders a tree node.
    /// </summary>
    /// <param name="label">The tree label to display.</param>
    /// <param name="defaultOpen">Whether the tree defaults to its expanded state.</param>
    /// <returns><see langword="true"/> when the node is open and children should be drawn.</returns>
    bool TreeNode(string label, bool defaultOpen);

    /// <summary>
    /// Pops the current tree node.
    /// </summary>
    void TreePop();
}
