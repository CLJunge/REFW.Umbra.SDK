using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level tree-node operations required by <see cref="RootTreeNode"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates root tree-node control flow from the shared ImGui render context so
/// unit tests can verify child ordering and tree-pop cleanup without requiring an active ImGui
/// frame.
/// </remarks>
internal interface IRootTreeNodeRenderer : ITreeNodeOps
{
}
