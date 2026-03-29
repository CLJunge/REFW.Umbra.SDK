using Hexa.NET.ImGui;

namespace Umbra.UI.Panel;

/// <summary>
/// Defines the low-level ImGui operations required by <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// This seam isolates panel ID-scope, tree-node, and separator control flow from native ImGui
/// calls so unit tests can verify section ordering and cleanup without requiring an active ImGui
/// frame.
/// </remarks>
internal interface IPluginPanelRenderer
{
    /// <summary>
    /// Pushes the specified top-level ImGui ID scope.
    /// </summary>
    /// <param name="scopeId">The stable panel ID scope to push.</param>
    void PushId(string scopeId);

    /// <summary>
    /// Pops the current ImGui ID scope.
    /// </summary>
    void PopId();

    /// <summary>
    /// Renders a tree node and reports whether it is open.
    /// </summary>
    /// <param name="label">The tree-node label.</param>
    /// <param name="flags">The flags that control the node's initial behavior.</param>
    /// <returns><see langword="true"/> when the node is open and its children should be drawn.</returns>
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
