using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the low-level spacing operations required by <see cref="ParameterNode"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates parameter-node spacing from the shared ImGui render context so unit
/// tests can verify draw ordering and spacing counts without requiring an active ImGui frame.
/// </remarks>
internal interface IParameterNodeRenderer : ISpacingOps
{
}
