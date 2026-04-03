using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the low-level rendering operations used by the hotkey drawers.
/// </summary>
/// <remarks>
/// This composed seam isolates hotkey-drawer text, inline layout, and button behavior from the shared ImGui render context so tests can verify capture-state transitions without requiring an active ImGui frame.
/// </remarks>
internal interface IHotkeyDrawerRenderer : ITextOps, IButtonOps
{
}
