namespace Umbra.UI.Config;

/// <summary>
/// Provides the keyboard state required by the built-in config undo shortcut workflow.
/// </summary>
/// <remarks>
/// The production implementation delegates to ImGui keyboard state and Umbra keyboard utilities, while tests can inject deterministic shortcut and text-input ownership states.
/// </remarks>
internal interface IUndoShortcutInputSource
{
    /// <summary>
    /// Returns a value indicating whether the built-in default undo shortcut was pressed during the current frame.
    /// </summary>
    /// <returns><see langword="true"/> when the default undo shortcut was pressed; otherwise, <see langword="false"/>.</returns>
    bool IsDefaultUndoShortcutPressed();

    /// <summary>
    /// Returns a value indicating whether text input currently owns keyboard editing behavior.
    /// </summary>
    /// <returns><see langword="true"/> when text input should keep handling editing shortcuts; otherwise, <see langword="false"/>.</returns>
    bool WantsTextInput();
}
