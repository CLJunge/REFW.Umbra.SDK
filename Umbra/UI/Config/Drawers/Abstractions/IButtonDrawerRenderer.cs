using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the low-level rendering operations used by <see cref="ButtonDrawer"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates button-drawer text, button, and color-stack behavior from the shared ImGui render context so tests can verify style selection and click flow without requiring an active ImGui frame.
/// </remarks>
internal interface IButtonDrawerRenderer : ITextOps, ISizedButtonOps, IButtonColorOps
{
}
