namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Shared mutable state for all hotkey-capture drawers in this assembly.
/// Tracks the total number of drawer instances currently in capture-waiting state
/// so that <see cref="HotkeyDrawer"/> and <see cref="TwoColumnHotkeyDrawer"/>
/// instances mutually exclude one another — only one key capture can be active at a time.
/// </summary>
/// <remarks>
/// ImGui runs single-threaded; the count never exceeds 1 in normal usage.
/// Drawers are responsible for keeping this counter accurate: increment when entering
/// capture mode, decrement when leaving it or when <see cref="IDisposable.Dispose"/> is called while
/// the drawer is still waiting.
/// </remarks>
internal static class HotkeyCaptureState
{
    /// <summary>
    /// The number of hotkey-capture drawers currently waiting for a key press.
    /// </summary>
    internal static int WaitingCount;
}
