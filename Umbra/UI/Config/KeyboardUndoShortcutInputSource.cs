using Hexa.NET.ImGui;
using Umbra.Input;

namespace Umbra.UI.Config;

/// <summary>
/// Implements <see cref="IUndoShortcutInputSource"/> using hardware-backed keyboard state
/// from <see cref="KeyboardInput"/>.
/// </summary>
internal sealed class KeyboardUndoShortcutInputSource : IUndoShortcutInputSource
{
    /// <inheritdoc/>
    public bool IsDefaultUndoShortcutPressed()
        => KeyboardInput.IsCtrlHeld && KeyboardInput.IsKeyJustPressed(UmbraKey.Z);

    /// <inheritdoc/>
    public bool WantsTextInput()
        => ImGui.GetIO().WantTextInput;
}
