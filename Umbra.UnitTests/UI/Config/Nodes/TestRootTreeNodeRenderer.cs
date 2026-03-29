namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Records <see cref="RootTreeNode"/> tree-node operations for unit tests.
/// </summary>
internal sealed class TestRootTreeNodeRenderer : IRootTreeNodeRenderer
{
    public List<(string Label, bool DefaultOpen)> TreeNodes { get; } = [];
    public int TreePopCount { get; private set; }
    public Queue<bool> TreeNodeResults { get; } = new();

    public bool TreeNode(string label, bool defaultOpen)
    {
        TreeNodes.Add((label, defaultOpen));
        if (TreeNodeResults.Count == 0)
            return false;

        return TreeNodeResults.Dequeue();
    }

    public void TreePop()
    {
        TreePopCount++;
    }
}
