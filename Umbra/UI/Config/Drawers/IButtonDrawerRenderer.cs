using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the low-level rendering operations required by <see cref="ButtonDrawer"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates the drawer's button-style selection and click-handling logic from
/// the shared ImGui render context so unit tests can verify behavior without requiring an active
/// ImGui frame.
/// </remarks>
internal interface IButtonDrawerRenderer : ITextOps, ISizedButtonOps, IButtonColorOps
{
}
