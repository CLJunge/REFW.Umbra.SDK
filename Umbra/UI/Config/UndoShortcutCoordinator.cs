using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// AppDomain-wide coordinator that ensures the Ctrl+Z undo shortcut is consumed at most
/// once per frame and routed to the correct <see cref="IUndoStackHandle"/>.
/// </summary>
/// <remarks> 
/// <para>
/// Without this coordinator, every <see cref="ConfigSection{TConfig}"/> with an enabled undo
/// stack would independently check the same global keyboard state during <c>Draw()</c> and
/// fire <c>TryUndo()</c> simultaneously — undoing one entry per loaded plugin per keypress.
/// </para>
/// <para>
/// The same deduplication applies to the redo shortcut (<c>Ctrl+Y</c>), which is checked
/// immediately after the undo shortcut within the same frame tick.
/// </para>
/// <para>
/// The focus model for undo is "last written wins with global fallback":
/// </para>
/// <list type="number">
/// <item>
/// Whichever <see cref="ConfigUndoStack{TConfig}"/> most recently called
/// <see cref="SetActive"/> (via <c>AddRecord</c> or <c>EndBatch</c>) becomes the
/// primary target.
/// </item>
/// <item>
/// When that stack's <see cref="IUndoStackHandle.CanUndo"/> is <see langword="false"/>
/// (exhausted), the coordinator scans all registered stacks and promotes the one whose
/// <see cref="IUndoStackHandle.TopEntryTimestamp"/> is highest. This allows the user's
/// undo timeline to walk back across multiple plugins in chronological order.
/// </item>
/// </list>
/// <para>
/// For redo, the active stack is preferred when it has redo entries. Otherwise the
/// coordinator scans all registered stacks for any that can redo, preferring the active
/// stack to keep undo/redo actions on the same timeline.
/// </para>
/// <para>
/// Frame deduplication uses <see cref="Environment.TickCount64"/> — the same tick-based
/// guard used by <see cref="Input.KeyboardInput.Update"/>. The first
/// <see cref="TryProcessShortcut"/> call in a frame does the real work; subsequent calls
/// within the same millisecond tick are no-ops.
/// </para>
/// </remarks>
internal static class UndoShortcutCoordinator
{
    private static readonly List<IUndoStackHandle> _registeredStacks = [];
    private static IUndoStackHandle? _activeStack;
    private static long _lastProcessedTick;

    /// <summary>
    /// Registers <paramref name="stack"/> so it participates in cross-stack fallback
    /// resolution. Called by the <see cref="ConfigUndoStack{TConfig}"/> constructor.
    /// </summary>
    /// <param name="stack">The stack to register.</param>
    internal static void Register(IUndoStackHandle stack)
    {
        _registeredStacks.Add(stack);
    }

    /// <summary>
    /// Unregisters <paramref name="stack"/> and clears the active reference if it points
    /// to this stack. Called by <see cref="ConfigUndoStack{TConfig}.Dispose"/>.
    /// </summary>
    /// <param name="stack">The stack being disposed.</param>
    internal static void Unregister(IUndoStackHandle stack)
    {
        _registeredStacks.Remove(stack);
        if (ReferenceEquals(_activeStack, stack))
            _activeStack = null;
    }

    /// <summary>
    /// Marks <paramref name="stack"/> as the active undo target. Called by
    /// <see cref="ConfigUndoStack{TConfig}"/> when a change is recorded.
    /// </summary>
    /// <param name="stack">The stack that just recorded a change.</param>
    internal static void SetActive(IUndoStackHandle stack)
    {
        _activeStack = stack;
    }

    /// <summary>
    /// Checks the undo and redo shortcuts via <paramref name="inputSource"/> and, when pressed,
    /// routes the operation to the resolved target stack. Deduplicates by tick so only one
    /// undo or redo happens per frame regardless of how many sections call this method.
    /// </summary>
    /// <param name="inputSource">The keyboard input source to query.</param>
    internal static void TryProcessShortcut(IUndoShortcutInputSource inputSource)
    {
        var tick = Environment.TickCount64;
        if (tick == _lastProcessedTick)
            return;

        _lastProcessedTick = tick;

        if (inputSource.WantsTextInput())
            return;

        if (inputSource.IsDefaultUndoShortcutPressed())
        {
            var undoTarget = ResolveUndoTarget();
            undoTarget?.TryUndo();
            return;
        }

        if (inputSource.IsDefaultRedoShortcutPressed())
        {
            var redoTarget = ResolveRedoTarget();
            redoTarget?.TryRedo();
        }
    }

    /// <summary>
    /// Resets all coordinator state. Intended for test cleanup between test methods.
    /// </summary>
    internal static void Reset()
    {
        _activeStack = null;
        _registeredStacks.Clear();
        _lastProcessedTick = 0;
    }

    /// <summary>
    /// Returns the stack that should receive the next undo, or <see langword="null"/>
    /// when no registered stack has entries.
    /// </summary>
    /// <remarks>
    /// Prefers <see cref="_activeStack"/> when it still has entries. Otherwise scans
    /// all registered stacks and picks the one with the highest
    /// <see cref="IUndoStackHandle.TopEntryTimestamp"/>. The winner is promoted to
    /// <see cref="_activeStack"/> so consecutive undos within the same stack avoid
    /// repeated scans.
    /// </remarks>
    private static IUndoStackHandle? ResolveUndoTarget()
    {
        if (_activeStack is not null && _activeStack.CanUndo)
            return _activeStack;

        IUndoStackHandle? best = null;
        long bestTimestamp = 0;
        for (var i = 0; i < _registeredStacks.Count; i++)
        {
            var candidate = _registeredStacks[i];
            if (!candidate.CanUndo)
                continue;

            var ts = candidate.TopEntryTimestamp;
            if (ts > bestTimestamp)
            {
                bestTimestamp = ts;
                best = candidate;
            }
        }

        if (best is not null)
            _activeStack = best;

        return best;
    }

    /// <summary>
    /// Returns the stack that should receive the next redo operation, or <see langword="null"/>
    /// when no registered stack has redo entries.
    /// </summary>
    /// <remarks>
    /// Prefers <see cref="_activeStack"/> when it has redo entries. Otherwise scans all
    /// registered stacks and picks the first one that can redo.
    /// </remarks>
    private static IUndoStackHandle? ResolveRedoTarget()
    {
        if (_activeStack is not null && _activeStack.CanRedo)
            return _activeStack;

        for (var i = 0; i < _registeredStacks.Count; i++)
        {
            var candidate = _registeredStacks[i];
            if (candidate.CanRedo)
                return candidate;
        }

        return null;
    }
}
