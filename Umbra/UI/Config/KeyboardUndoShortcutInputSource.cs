using Hexa.NET.ImGui;
using Umbra.Input;

namespace Umbra.UI.Config;

/// <summary>
/// Implements <see cref="IUndoShortcutInputSource"/> using the current ImGui frame keyboard state.
/// </summary>
internal sealed class KeyboardUndoShortcutInputSource : IUndoShortcutInputSource
{
    /// <inheritdoc/>
    public bool IsDefaultUndoShortcutPressed()
        => KeyboardInput.IsCtrlHeld && ImGui.IsKeyPressed(ImGuiKey.Z, false);

    /// <inheritdoc/>
    public bool WantsTextInput()
        => ImGui.GetIO().WantTextInput;
}
