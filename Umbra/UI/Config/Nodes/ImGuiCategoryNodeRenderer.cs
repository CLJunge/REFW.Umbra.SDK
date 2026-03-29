using Hexa.NET.ImGui;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Renders <see cref="CategoryNode"/> output through the active ImGui frame.
/// </summary>
internal sealed class ImGuiCategoryNodeRenderer : ICategoryNodeRenderer
{
    /// <inheritdoc/>
    public void Indent(float amount)
    {
        ImGui.Indent(amount);
    }

    /// <inheritdoc/>
    public void Unindent(float amount)
    {
        ImGui.Unindent(amount);
    }

    /// <inheritdoc/>
    public void SeparatorText(string label)
    {
        ImGui.SeparatorText(label);
    }

    /// <inheritdoc/>
    public bool TreeNode(string label, bool defaultOpen)
    {
        var flags = defaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
        return ImGui.TreeNodeEx(label, flags);
    }

    /// <inheritdoc/>
    public void TreePop()
    {
        ImGui.TreePop();
    }
}
