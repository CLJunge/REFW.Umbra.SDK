namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Stores the shared capture-state count used by the hotkey drawers in this assembly.
/// </summary>
/// <remarks>
/// <see cref="HotkeyCaptureController"/> instances keep this counter synchronized so <see cref="HotkeyDrawer"/> and <see cref="TwoColumnHotkeyDrawer"/> mutually exclude one another and only one capture workflow is active at a time.
/// </remarks>
internal static class HotkeyCaptureState
{
    /// <summary>
    /// Gets or sets the number of hotkey drawers currently waiting for a key press.
    /// </summary>
    internal static int WaitingCount;
}
