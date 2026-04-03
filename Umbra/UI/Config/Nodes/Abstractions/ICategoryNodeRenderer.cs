using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level rendering operations used by <see cref="CategoryNode"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates category-header, tree-node, and indentation behavior from the shared ImGui render context so tests can verify category-node control flow without requiring an active ImGui frame.
/// </remarks>
internal interface ICategoryNodeRenderer : IIndentationOps, ITreeNodeOps
{
    /// <summary>
    /// Renders the non-collapsible category header.
    /// </summary>
    /// <param name="label">The visible category label.</param>
    void SeparatorText(string label);
}
