using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Defines the ImGui ID-scope operations required by <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates the outer draw scope from the shared ImGui render context so unit
/// tests can verify draw ordering and cleanup behavior without requiring an active ImGui frame.
/// </remarks>
internal interface IConfigDrawerScope : IIdScopeOps
{
}
