namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Records <see cref="RootTreeNode"/> tree-node operations for unit tests.
/// </summary>
internal sealed class TestRootTreeNodeRenderer : IRootTreeNodeRenderer
{
    public List<(string Label, bool DefaultOpen, bool? OpenState, bool ForceOpen)> TreeNodes { get; } = [];
    public int TreePopCount { get; private set; }
    public Queue<bool> TreeNodeResults { get; } = new();

    public bool TreeNode(string label, bool defaultOpen, bool? openState = null, bool forceOpen = false)
    {
        TreeNodes.Add((label, defaultOpen, openState, forceOpen));
        if (TreeNodeResults.Count == 0)
            return false;

        return TreeNodeResults.Dequeue();
    }

    public void TreePop() => TreePopCount++;
}
