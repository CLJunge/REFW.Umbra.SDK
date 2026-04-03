using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level tree-node operations used by <see cref="RootTreeNode"/>.
/// </summary>
/// <remarks>
/// This abstraction isolates root tree-node control flow from the shared ImGui render context so tests can verify child ordering and tree-pop cleanup without requiring an active ImGui frame.
/// </remarks>
internal interface IRootTreeNodeRenderer : ITreeNodeOps
{
}
