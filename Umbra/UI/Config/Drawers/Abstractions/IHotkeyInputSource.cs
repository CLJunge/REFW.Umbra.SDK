using Umbra.Input;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Provides the keyboard operations required by the hotkey-capture workflow.
/// </summary>
/// <remarks>
/// The production implementation delegates to <see cref="Umbra.Input.KeyboardInput"/>, while tests can inject deterministic key names and captured values. <see cref="HotkeyCaptureController"/> consumes this abstraction for both <see cref="HotkeyDrawer"/> and <see cref="TwoColumnHotkeyDrawer"/>.
/// </remarks>
internal interface IHotkeyInputSource
{
    /// <summary>
    /// Attempts to capture a keyboard key for the current frame.
    /// </summary>
    /// <param name="capturedKey">When this method returns <see langword="true"/>, contains the captured key value.</param>
    /// <returns><see langword="true"/> if a key was captured; otherwise, <see langword="false"/>.</returns>
    bool TryCaptureKeyboardKey(out int capturedKey);

    /// <summary>
    /// Returns the display name for a key value.
    /// </summary>
    /// <param name="key">The key value to name.</param>
    /// <returns>A human-readable key name.</returns>
    string GetKeyName(int key);

    /// <summary>
    /// Attempts to capture a full hotkey binding (key + modifiers) for the current frame.
    /// </summary>
    /// <param name="binding">When this method returns <see langword="true"/>, contains the captured binding.</param>
    /// <returns><see langword="true"/> if a binding was captured; otherwise, <see langword="false"/>.</returns>
    bool TryCaptureHotkeyBinding(out HotkeyBinding binding);

    /// <summary>
    /// Returns the human-readable display name for a hotkey binding.
    /// </summary>
    /// <param name="binding">The binding to describe.</param>
    /// <returns>A display string such as <c>Ctrl+Shift+F5</c>.</returns>
    string GetBindingDisplayName(HotkeyBinding binding);

    /// <summary>
    /// Returns the modifier prefix for the currently held modifier keys.
    /// </summary>
    /// <returns>A string such as <c>Ctrl+Shift+</c> when those modifiers are held, or <see cref="string.Empty"/> when none are held.</returns>
    string GetHeldModifierPrefix();
}
