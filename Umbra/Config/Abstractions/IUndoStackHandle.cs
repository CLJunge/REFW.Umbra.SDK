namespace Umbra.Config;

/// <summary>
/// Non-generic handle exposing the minimum surface required for global undo-shortcut
/// routing without coupling the coordinator to a specific configuration type.
/// </summary>
/// <remarks>
/// <see cref="ConfigUndoStack{TConfig}"/> implements this interface so the static
/// <see cref="UI.Config.UndoShortcutCoordinator"/> can hold a reference to the
/// most recently active stack without knowing its generic type argument.
/// </remarks>
internal interface IUndoStackHandle
{
    /// <summary>
    /// Gets a value indicating whether the stack contains at least one entry that can be undone.
    /// </summary>
    bool CanUndo { get; }

    /// <summary>
    /// Gets the <see cref="System.Diagnostics.Stopwatch"/>-based timestamp of the top entry,
    /// or <c>0</c> when the stack is empty. Used by <see cref="UI.Config.UndoShortcutCoordinator"/>
    /// to compare recency across registered stacks for cross-plugin fallback.
    /// </summary>
    long TopEntryTimestamp { get; }

    /// <summary>
    /// Attempts to undo the most recent entry on the stack.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if a change was successfully undone;
    /// <see langword="false"/> if the stack is empty, disposed, or the parameter is no longer registered.
    /// </returns>
    bool TryUndo();
}
