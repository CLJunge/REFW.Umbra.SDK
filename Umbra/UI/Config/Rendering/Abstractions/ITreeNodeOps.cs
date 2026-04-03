namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines the collapsible tree-node operations used by the configuration UI pipeline.
/// </summary>
internal interface ITreeNodeOps
{
    /// <summary>
    /// Renders a tree node.
    /// </summary>
    /// <param name="label">The visible tree-node label.</param>
    /// <param name="defaultOpen"><see langword="true"/> to start the node expanded; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the node is open and children should be drawn; otherwise, <see langword="false"/>.</returns>
    bool TreeNode(string label, bool defaultOpen);

    /// <summary>
    /// Pops the current tree node.
    /// </summary>
    void TreePop();
}
