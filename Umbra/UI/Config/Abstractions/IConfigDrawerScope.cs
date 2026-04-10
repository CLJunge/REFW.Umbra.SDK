using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Defines the ImGui ID-scope operations required by <see cref="ConfigDrawer{TConfig}"/>.
/// </summary>
/// <remarks>
/// This abstraction isolates the outer draw scope from the concrete ImGui render context so tests can verify draw ordering and push/pop cleanup without requiring an active ImGui frame.
/// </remarks>
internal interface IConfigDrawerScope : IIdScopeOps
{
}
