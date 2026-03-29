using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

/// <summary>
/// Renders <see cref="PluginPanel"/> output through the active ImGui frame.
/// </summary>
internal sealed class ImGuiPluginPanelRenderer : IPluginPanelRenderer
{
    /// <inheritdoc/>
    public void PushId(string scopeId) => ImGui.PushID(scopeId);

    /// <inheritdoc/>
    public void PopId() => ImGui.PopID();

    /// <inheritdoc/>
    public bool TreeNode(string label, ImGuiTreeNodeFlags flags) => ImGui.TreeNodeEx(label, flags);

    /// <inheritdoc/>
    public void TreePop() => ImGui.TreePop();

    /// <inheritdoc/>
    public void Separator() => ImGui.Separator();
}
