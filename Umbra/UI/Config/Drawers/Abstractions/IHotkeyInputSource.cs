namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Provides the keyboard operations required by hotkey capture drawers.
/// </summary>
/// <remarks>
/// The production implementation delegates to <see cref="Umbra.Input.KeyboardInput"/>, while unit
/// tests can inject deterministic key names and captured keys without depending on the runtime
/// host. <see cref="HotkeyCaptureController"/> consumes this abstraction for both
/// <see cref="HotkeyDrawer"/> and <see cref="TwoColumnHotkeyDrawer"/>.
/// </remarks>
internal interface IHotkeyInputSource
{
    /// <summary>
    /// Attempts to capture a keyboard key for the current frame.
    /// </summary>
    /// <param name="capturedKey">Receives the captured key when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> when a key was captured; otherwise <see langword="false"/>.</returns>
    bool TryCaptureKeyboardKey(out int capturedKey);

    /// <summary>
    /// Returns the display name for the given key value.
    /// </summary>
    /// <param name="key">The key value to name.</param>
    /// <returns>A human-readable key name.</returns>
    string GetKeyName(int key);
}
