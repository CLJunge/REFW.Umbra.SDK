using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the low-level rendering operations required by <see cref="HotkeyDrawer"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates hotkey-drawer state transitions from the shared ImGui render
/// context so unit tests can verify behavior without requiring an active ImGui frame.
/// </remarks>
internal interface IHotkeyDrawerRenderer : ITextOps, IButtonOps
{
}
