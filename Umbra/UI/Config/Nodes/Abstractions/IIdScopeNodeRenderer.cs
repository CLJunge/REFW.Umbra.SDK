using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level ImGui ID-scope operations used by <see cref="IdScopeNode"/>.
/// </summary>
/// <remarks>
/// This abstraction isolates scope push and pop behavior from the shared ImGui render context so tests can verify subtree ordering and cleanup without requiring an active ImGui frame.
/// </remarks>
internal interface IIdScopeNodeRenderer : IIdScopeOps
{
}
