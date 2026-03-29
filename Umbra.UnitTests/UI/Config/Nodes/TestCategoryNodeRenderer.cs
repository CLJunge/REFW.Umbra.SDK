namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Records <see cref="CategoryNode"/> rendering operations for unit tests.
/// </summary>
internal sealed class TestCategoryNodeRenderer : ICategoryNodeRenderer
{
    public List<float> Indents { get; } = [];
    public List<float> Unindents { get; } = [];
    public List<string> SeparatorLabels { get; } = [];
    public List<(string Label, bool DefaultOpen)> TreeNodes { get; } = [];
    public int TreePopCount { get; private set; }
    public Queue<bool> TreeNodeResults { get; } = new();

    public void Indent(float amount) => Indents.Add(amount);

    public void Unindent(float amount) => Unindents.Add(amount);

    public void SeparatorText(string label) => SeparatorLabels.Add(label);

    public bool TreeNode(string label, bool defaultOpen)
    {
        TreeNodes.Add((label, defaultOpen));
        if (TreeNodeResults.Count == 0)
            return false;

        return TreeNodeResults.Dequeue();
    }

    public void TreePop() => TreePopCount++;
}
