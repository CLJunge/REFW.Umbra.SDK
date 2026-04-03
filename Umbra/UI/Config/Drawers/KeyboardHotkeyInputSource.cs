using Umbra.Input;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Implements <see cref="IHotkeyInputSource"/> by delegating to <see cref="KeyboardInput"/>.
/// </summary>
internal sealed class KeyboardHotkeyInputSource : IHotkeyInputSource
{
    /// <inheritdoc/>
    public bool TryCaptureKeyboardKey(out int capturedKey) => KeyboardInput.TryCaptureKeyboardKey(out capturedKey);

    /// <inheritdoc/>
    public string GetKeyName(int key) => KeyboardInput.GetKeyName(key);
}
