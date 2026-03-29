using Umbra.Input;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Retrieves key names and captured keys through <see cref="KeyboardInput"/>.
/// </summary>
internal sealed class KeyboardHotkeyInputSource : IHotkeyInputSource
{
    /// <inheritdoc/>
    public bool TryCaptureKeyboardKey(out int capturedKey) => KeyboardInput.TryCaptureKeyboardKey(out capturedKey);

    /// <inheritdoc/>
    public string GetKeyName(int key) => KeyboardInput.GetKeyName(key);
}
