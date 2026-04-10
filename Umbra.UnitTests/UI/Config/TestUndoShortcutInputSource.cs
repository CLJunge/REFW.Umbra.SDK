namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Provides deterministic undo-shortcut and text-input ownership states for <see cref="ConfigSection{TConfig}"/> tests.
/// </summary>
internal sealed class TestUndoShortcutInputSource : IUndoShortcutInputSource
{
    public bool DefaultUndoShortcutPressed { get; set; }

    public bool DefaultRedoShortcutPressed { get; set; }

    public bool WantsTextInputState { get; set; }

    public int DefaultUndoShortcutCheckCount { get; private set; }

    public int DefaultRedoShortcutCheckCount { get; private set; }

    public int WantsTextInputCheckCount { get; private set; }

    public bool IsDefaultUndoShortcutPressed()
    {
        DefaultUndoShortcutCheckCount++;
        return DefaultUndoShortcutPressed;
    }

    public bool IsDefaultRedoShortcutPressed()
    {
        DefaultRedoShortcutCheckCount++;
        return DefaultRedoShortcutPressed;
    }

    public bool WantsTextInput()
    {
        WantsTextInputCheckCount++;
        return WantsTextInputState;
    }
}
