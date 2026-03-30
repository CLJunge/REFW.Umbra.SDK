using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level ImGui ID-scope operations required by <see cref="IdScopeNode"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates ID-scope control flow from the shared ImGui render context so unit
/// tests can verify child ordering and scope cleanup without requiring an active ImGui frame.
/// </remarks>
internal interface IIdScopeNodeRenderer : IIdScopeOps
{
}
