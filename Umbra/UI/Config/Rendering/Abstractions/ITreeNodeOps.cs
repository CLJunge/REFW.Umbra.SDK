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
    /// <param name="openState">
    /// The explicit open state to apply for the current draw pass, or <see langword="null"/> to let the renderer
    /// use its persisted state together with <paramref name="defaultOpen"/>.
    /// </param>
    /// <param name="forceOpen"><see langword="true"/> to force the node open for the current draw pass; otherwise, <see langword="false"/>.</param>
    /// <returns><see langword="true"/> if the node is open and children should be drawn; otherwise, <see langword="false"/>.</returns>
    bool TreeNode(string label, bool defaultOpen, bool? openState = null, bool forceOpen = false);

    /// <summary>
    /// Pops the current tree node.
    /// </summary>
    void TreePop();
}
