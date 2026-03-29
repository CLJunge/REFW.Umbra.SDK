namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level tree-node operations required by <see cref="RootTreeNode"/>.
/// </summary>
/// <remarks>
/// This seam isolates root tree-node control flow from native ImGui calls so unit tests can verify
/// child ordering and tree-pop cleanup without requiring an active ImGui frame.
/// </remarks>
internal interface IRootTreeNodeRenderer
{
    /// <summary>
    /// Renders the root tree node.
    /// </summary>
    /// <param name="label">The tree label to display.</param>
    /// <param name="defaultOpen">Whether the tree node should default to its expanded state.</param>
    /// <returns><see langword="true"/> when the node is open and children should be drawn.</returns>
    bool TreeNode(string label, bool defaultOpen);

    /// <summary>
    /// Pops the current root tree node after its children have been drawn.
    /// </summary>
    void TreePop();
}
