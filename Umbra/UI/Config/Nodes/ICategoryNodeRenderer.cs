using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level rendering operations required by <see cref="CategoryNode"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates category-node control flow from the shared ImGui render context so
/// unit tests can verify header/tree behavior and indent balancing without requiring an active
/// ImGui frame.
/// </remarks>
internal interface ICategoryNodeRenderer : IIndentationOps, ITreeNodeOps
{
    /// <summary>
    /// Renders the non-collapsible category header.
    /// </summary>
    /// <param name="label">The category label to display.</param>
    void SeparatorText(string label);
}
