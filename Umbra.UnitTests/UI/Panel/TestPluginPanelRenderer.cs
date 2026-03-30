using Hexa.NET.ImGui;

namespace Umbra.UI.Panel.UnitTests;

/// <summary>
/// Records <see cref="PluginPanel"/> rendering operations for unit tests.
/// </summary>
internal sealed class TestPluginPanelRenderer : IPluginPanelRenderer
{
    public List<string> PushIds { get; } = [];
    public int PopIdCount { get; private set; }
    public List<(string Label, ImGuiTreeNodeFlags Flags)> TreeNodes { get; } = [];
    public Queue<bool> TreeNodeResults { get; } = new();
    public int TreePopCount { get; private set; }
    public int SeparatorCount { get; private set; }

    public void PushId(string scopeId) => PushIds.Add(scopeId);

    public void PopId() => PopIdCount++;

    public bool TreeNode(string label, ImGuiTreeNodeFlags flags)
    {
        TreeNodes.Add((label, flags));
        if (TreeNodeResults.Count == 0)
            return false;

        return TreeNodeResults.Dequeue();
    }

    public void TreePop() => TreePopCount++;

    public void Separator() => SeparatorCount++;
}
