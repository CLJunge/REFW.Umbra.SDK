using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

/// <summary>
/// Defines the low-level ImGui operations required by <see cref="PluginPanel"/> and its draw pipeline.
/// </summary>
/// <remarks>
/// This seam isolates ID-scope, tree-node, and separator control flow from native ImGui calls so unit tests can verify panel behavior without requiring an active ImGui frame.
/// </remarks>
internal interface IPluginPanelRenderer
{
    /// <summary>
    /// Pushes the specified ImGui ID scope.
    /// </summary>
    /// <param name="scopeId">The stable scope identifier to push.</param>
    void PushId(string scopeId);

    /// <summary>
    /// Pops the current ImGui ID scope.
    /// </summary>
    void PopId();

    /// <summary>
    /// Renders a tree node and reports whether its contents should be drawn.
    /// </summary>
    /// <param name="label">The tree-node label, including any caller-supplied ImGui ID suffix.</param>
    /// <param name="flags">The flags that control tree-node behavior.</param>
    /// <returns><see langword="true"/> if the node is open and its children should be rendered; otherwise, <see langword="false"/>.</returns>
    bool TreeNode(string label, ImGuiTreeNodeFlags flags);

    /// <summary>
    /// Pops the current tree node.
    /// </summary>
    void TreePop();

    /// <summary>
    /// Draws a horizontal separator.
    /// </summary>
    void Separator();
}
