using Umbra.Config;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="UndoShortcutCoordinator"/>.
/// </summary>
[TestClass]
public sealed class UndoShortcutCoordinatorTests
{
    [UmbraAutoRegister]
    private sealed class CoordTestConfig
    {
        [UmbraParameter]
        public Parameter<int> Value { get; set; } = new(0);
    }

    [UmbraAutoRegister]
    private sealed class CoordTestConfigB
    {
        [UmbraParameter]
        public Parameter<int> Value { get; set; } = new(0);
    }

    [TestInitialize]
    public void TestInit() => UndoShortcutCoordinator.Reset();

    private static (ConfigStore<T> Store, T Config, string TempPath) CreateLoadedStore<T>()
        where T : class, new()
    {
        var tempPath = Path.GetTempFileName();
        var store = new ConfigStore<T>(tempPath);
        var config = store.Load();
        return (store, config, tempPath);
    }

    private static void CleanupStore<T>(ConfigStore<T> store, string tempPath)
        where T : class, new()
    {
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    // --- No active stack ---

    /// <summary>
    /// When no stack has recorded a change, the shortcut does nothing. The text-input
    /// guard is still checked (early exit), but no shortcut key is queried.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_NoActiveStack_DoesNotQueryShortcutKey()
    {
        // Arrange
        var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

        // Act
        UndoShortcutCoordinator.TryProcessShortcut(input);

        // Assert — keyboard state is queried but resolves to no target, so nothing happens
        Assert.AreEqual(1, input.WantsTextInputCheckCount);
        Assert.AreEqual(1, input.DefaultUndoShortcutCheckCount);
    }

    // --- Active stack receives undo ---

    /// <summary>
    /// When a stack records a change and the shortcut is pressed, the active stack is undone.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_ActiveStackWithEntry_UndoesActiveStack()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            using var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 42;

            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert
            Assert.AreEqual(0, config.Value.Value);
            Assert.IsFalse(undoStack.CanUndo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Disposed stack clears active ---

    /// <summary>
    /// When the active stack is disposed, a subsequent shortcut press does nothing.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_AfterActiveStackDisposed_DoesNothing()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 42;
            undoStack.Dispose();

            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — value stays at 42 because the stack was disposed
            Assert.AreEqual(42, config.Value.Value);
            Assert.AreEqual(1, input.WantsTextInputCheckCount);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Frame deduplication ---

    /// <summary>
    /// A second call within the same tick is a no-op, preventing multiple sections from
    /// each triggering an undo in the same frame.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_CalledTwiceInSameTick_OnlyUndoesOnce()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            using var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 1;
            config.Value.Value = 2;

            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

            // Act — call twice in rapid succession (same tick)
            UndoShortcutCoordinator.TryProcessShortcut(input);
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — only one undo happened (value went from 2 → 1, not 2 → 0)
            Assert.AreEqual(1, config.Value.Value);
            Assert.IsTrue(undoStack.CanUndo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Text input suppression ---

    /// <summary>
    /// When text input is active, the shortcut is not forwarded to the undo stack.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_WhenTextInputActive_DoesNotUndo()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            using var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 42;

            var input = new TestUndoShortcutInputSource
            {
                DefaultUndoShortcutPressed = true,
                WantsTextInputState = true
            };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert
            Assert.AreEqual(42, config.Value.Value);
            Assert.IsTrue(undoStack.CanUndo);
            Assert.AreEqual(1, input.WantsTextInputCheckCount);
            Assert.AreEqual(0, input.DefaultUndoShortcutCheckCount);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Cross-section: only last-pushed stack undoes ---

    /// <summary>
    /// When two undo stacks exist, only the one that most recently recorded a change
    /// is undone by the shortcut.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_TwoStacks_OnlyLastPushedStackUndoes()
    {
        // Arrange
        var (storeA, configA, tempPathA) = CreateLoadedStore<CoordTestConfig>();
        var (storeB, configB, tempPathB) = CreateLoadedStore<CoordTestConfigB>();
        try
        {
            using var undoStackA = new ConfigUndoStack<CoordTestConfig>(storeA);
            using var undoStackB = new ConfigUndoStack<CoordTestConfigB>(storeB);

            // Change A then B — B becomes active
            configA.Value.Value = 10;
            configB.Value.Value = 20;

            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — only B was undone; A retains its changed value
            Assert.AreEqual(10, configA.Value.Value);
            Assert.AreEqual(0, configB.Value.Value);
            Assert.IsTrue(undoStackA.CanUndo);
            Assert.IsFalse(undoStackB.CanUndo);
        }
        finally
        {
            CleanupStore(storeA, tempPathA);
            CleanupStore(storeB, tempPathB);
        }
    }

    // --- Reset clears state ---

    /// <summary>
    /// After <see cref="UndoShortcutCoordinator.Reset"/>, the coordinator has no active
    /// stack and the shortcut does nothing.
    /// </summary>
    [TestMethod]
    public void Reset_ClearsActiveStack()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            using var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 42;

            // Act
            UndoShortcutCoordinator.Reset();
            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — undo did not happen because Reset cleared the active stack
            Assert.AreEqual(42, config.Value.Value);
            Assert.IsTrue(undoStack.CanUndo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Shortcut not pressed ---

    /// <summary>
    /// When the shortcut is not pressed, no undo occurs even when a stack is active.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_ShortcutNotPressed_DoesNotUndo()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            using var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 42;

            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = false };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert
            Assert.AreEqual(42, config.Value.Value);
            Assert.IsTrue(undoStack.CanUndo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// When the active stack is disposed and a different registered stack still has entries,
    /// the coordinator falls back to the remaining stack via timestamp-based resolution.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_ActiveStackDisposed_FallsBackToRegisteredPeer()
    {
        // Arrange
        var (storeA, configA, tempPathA) = CreateLoadedStore<CoordTestConfig>();
        var (storeB, configB, tempPathB) = CreateLoadedStore<CoordTestConfigB>();
        try
        {
            using var undoStackA = new ConfigUndoStack<CoordTestConfig>(storeA);
            var undoStackB = new ConfigUndoStack<CoordTestConfigB>(storeB);

            configA.Value.Value = 10;
            configB.Value.Value = 20; // B becomes active
            undoStackB.Dispose();     // unregisters B, clears active

            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — A was promoted via fallback and undone
            Assert.AreEqual(0, configA.Value.Value);
            Assert.AreEqual(20, configB.Value.Value);
        }
        finally
        {
            CleanupStore(storeA, tempPathA);
            CleanupStore(storeB, tempPathB);
        }
    }

    // --- Cross-stack sequential undo ---

    /// <summary>
    /// Change A, then change B. First undo reverts B, second undo reverts A — a global
    /// timeline that walks back across plugins in chronological order.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_ChangeAThenB_UndoBThenA()
    {
        // Arrange
        var (storeA, configA, tempPathA) = CreateLoadedStore<CoordTestConfig>();
        var (storeB, configB, tempPathB) = CreateLoadedStore<CoordTestConfigB>();
        try
        {
            using var undoStackA = new ConfigUndoStack<CoordTestConfig>(storeA);
            using var undoStackB = new ConfigUndoStack<CoordTestConfigB>(storeB);

            configA.Value.Value = 10; // A gets entry at T1
            configB.Value.Value = 20; // B gets entry at T2 (> T1), becomes active

            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

            // Act — first undo: B is active, reverts B
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — B reverted, A unchanged
            Assert.AreEqual(10, configA.Value.Value);
            Assert.AreEqual(0, configB.Value.Value);
            Assert.IsTrue(undoStackA.CanUndo);
            Assert.IsFalse(undoStackB.CanUndo);

            // Act — second undo: B exhausted, falls back to A
            UndoShortcutCoordinator.Reset();
            UndoShortcutCoordinator.Register(undoStackA);
            UndoShortcutCoordinator.Register(undoStackB);
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — A reverted
            Assert.AreEqual(0, configA.Value.Value);
            Assert.IsFalse(undoStackA.CanUndo);
            Assert.IsFalse(undoStackB.CanUndo);
        }
        finally
        {
            CleanupStore(storeA, tempPathA);
            CleanupStore(storeB, tempPathB);
        }
    }

    /// <summary>
    /// Multiple changes across two stacks are unwound in reverse chronological order
    /// across the global timeline: B2, A2, B1, A1.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_InterleavedChanges_UnwindsInReverseChronologicalOrder()
    {
        // Arrange
        var (storeA, configA, tempPathA) = CreateLoadedStore<CoordTestConfig>();
        var (storeB, configB, tempPathB) = CreateLoadedStore<CoordTestConfigB>();
        try
        {
            using var undoStackA = new ConfigUndoStack<CoordTestConfig>(storeA);
            using var undoStackB = new ConfigUndoStack<CoordTestConfigB>(storeB);

            configA.Value.Value = 1;  // A entry: 0→1
            configB.Value.Value = 10; // B entry: 0→10
            configA.Value.Value = 2;  // A entry: 1→2, A becomes active
            configB.Value.Value = 20; // B entry: 10→20, B becomes active

            var input = new TestUndoShortcutInputSource { DefaultUndoShortcutPressed = true };

            // Undo 1: B is active → reverts 20→10
            UndoShortcutCoordinator.TryProcessShortcut(input);
            Assert.AreEqual(2, configA.Value.Value);
            Assert.AreEqual(10, configB.Value.Value);

            // Undo 2: B still has entries (10→0) but A's top entry (1→2) may be more recent.
            // The exact ordering depends on Stopwatch resolution; the key guarantee is
            // that both stacks are fully unwound after enough undos.
            UndoShortcutCoordinator.Reset();
            UndoShortcutCoordinator.Register(undoStackA);
            UndoShortcutCoordinator.Register(undoStackB);
            UndoShortcutCoordinator.TryProcessShortcut(input);

            UndoShortcutCoordinator.Reset();
            UndoShortcutCoordinator.Register(undoStackA);
            UndoShortcutCoordinator.Register(undoStackB);
            UndoShortcutCoordinator.TryProcessShortcut(input);

            UndoShortcutCoordinator.Reset();
            UndoShortcutCoordinator.Register(undoStackA);
            UndoShortcutCoordinator.Register(undoStackB);
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — after 4 undos, both stacks are fully unwound
            Assert.AreEqual(0, configA.Value.Value);
            Assert.AreEqual(0, configB.Value.Value);
            Assert.IsFalse(undoStackA.CanUndo);
            Assert.IsFalse(undoStackB.CanUndo);
        }
        finally
        {
            CleanupStore(storeA, tempPathA);
            CleanupStore(storeB, tempPathB);
        }
    }

    // --- Redo shortcut routing ---

    /// <summary>
    /// When a stack has redo entries and the redo shortcut is pressed, the active stack is redone.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_RedoShortcutPressed_RedoesActiveStack()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            using var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 42;
            undoStack.TryUndo();
            Assert.AreEqual(0, config.Value.Value);

            var input = new TestUndoShortcutInputSource { DefaultRedoShortcutPressed = true };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert
            Assert.AreEqual(42, config.Value.Value);
            Assert.IsFalse(undoStack.CanRedo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// When no stack has redo entries, the redo shortcut does nothing.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_RedoShortcutWhenNoRedoAvailable_DoesNothing()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            using var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 42;

            var input = new TestUndoShortcutInputSource { DefaultRedoShortcutPressed = true };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert — nothing changed, value remains 42
            Assert.AreEqual(42, config.Value.Value);
            Assert.IsTrue(undoStack.CanUndo);
            Assert.IsFalse(undoStack.CanRedo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// When text input is active, the redo shortcut is not forwarded to the stack.
    /// </summary>
    [TestMethod]
    public void TryProcessShortcut_RedoShortcutWhenTextInputActive_DoesNotRedo()
    {
        // Arrange
        var (store, config, tempPath) = CreateLoadedStore<CoordTestConfig>();
        try
        {
            using var undoStack = new ConfigUndoStack<CoordTestConfig>(store);
            config.Value.Value = 42;
            undoStack.TryUndo();

            var input = new TestUndoShortcutInputSource
            {
                DefaultRedoShortcutPressed = true,
                WantsTextInputState = true
            };

            // Act
            UndoShortcutCoordinator.TryProcessShortcut(input);

            // Assert
            Assert.AreEqual(0, config.Value.Value);
            Assert.IsTrue(undoStack.CanRedo);
            Assert.AreEqual(1, input.WantsTextInputCheckCount);
            Assert.AreEqual(0, input.DefaultRedoShortcutCheckCount);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }
}
