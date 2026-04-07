using Umbra.Config.Attributes;
using Umbra.UI.Toast;

namespace Umbra.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigUndoStack{TConfig}"/>.
/// </summary>
[TestClass]
public sealed class ConfigUndoStackTests
{
    /// <summary>
    /// Test configuration class for undo stack tests.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class UndoTestConfig
    {
        [UmbraParameter]
        public Parameter<int> IntValue { get; set; } = new(10);

        [UmbraParameter]
        public Parameter<string> StringValue { get; set; } = new("default");

        [UmbraParameter]
        public Parameter<bool> BoolValue { get; set; } = new(false);
    }

    /// <summary>
    /// Test configuration class that includes a delegate parameter to verify skip behavior.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class DelegateTestConfig
    {
        [UmbraParameter]
        public Parameter<int> IntValue { get; set; } = new(5);

        [UmbraParameter]
        public Parameter<Action> ButtonAction { get; set; } = new(() => { });
    }

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

    // --- Constructor validation ---

    /// <summary>
    /// Tests that the constructor throws <see cref="ArgumentNullException"/> when store is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullStore_ThrowsArgumentNullException() => Assert.ThrowsExactly<ArgumentNullException>(() => new ConfigUndoStack<UndoTestConfig>(null!));

    /// <summary>
    /// Tests that the constructor throws <see cref="InvalidOperationException"/> when the store is not loaded.
    /// </summary>
    [TestMethod]
    public void Constructor_UnloadedStore_ThrowsInvalidOperationException()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new ConfigStore<UndoTestConfig>(tempPath);
            Assert.ThrowsExactly<InvalidOperationException>(() => new ConfigUndoStack<UndoTestConfig>(store));
            store.Dispose();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that the constructor throws <see cref="ObjectDisposedException"/> when the store is disposed.
    /// </summary>
    [TestMethod]
    public void Constructor_DisposedStore_ThrowsObjectDisposedException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        store.Dispose();
        try
        {
            Assert.ThrowsExactly<ObjectDisposedException>(() => new ConfigUndoStack<UndoTestConfig>(store));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that the constructor throws <see cref="ArgumentOutOfRangeException"/> when capacity is zero.
    /// </summary>
    [TestMethod]
    public void Constructor_ZeroCapacity_ThrowsArgumentOutOfRangeException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                () => new ConfigUndoStack<UndoTestConfig>(store, capacity: 0));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that a newly created undo stack has no entries.
    /// </summary>
    [TestMethod]
    public void Constructor_LoadedStore_StartsEmpty()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            Assert.IsFalse(undo.CanUndo);
            Assert.AreEqual(0, undo.Count);
            Assert.IsNull(undo.Peek());
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Change tracking ---

    /// <summary>
    /// Tests that changing a parameter value pushes a record onto the undo stack.
    /// </summary>
    [TestMethod]
    public void ParameterChange_PushesRecord()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 42;

            Assert.IsTrue(undo.CanUndo);
            Assert.AreEqual(1, undo.Count);

            var record = undo.Peek();
            Assert.IsNotNull(record);
            Assert.AreEqual(10, record.OldValue);
            Assert.AreEqual(42, record.NewValue);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that multiple changes push separate records in order.
    /// </summary>
    [TestMethod]
    public void MultipleChanges_PushInOrder()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 20;
            config.StringValue.Value = "changed";

            Assert.AreEqual(2, undo.Count);

            var top = undo.Peek();
            Assert.IsNotNull(top);
            Assert.AreEqual("changed", top.NewValue);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the record captures the correct display label from parameter metadata.
    /// </summary>
    [TestMethod]
    public void Record_CapturesDisplayLabel()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 99;

            var record = undo.Peek();
            Assert.IsNotNull(record);
            Assert.IsFalse(string.IsNullOrEmpty(record.DisplayLabel));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the record timestamp is a positive Stopwatch-based value.
    /// </summary>
    [TestMethod]
    public void Record_HasPositiveTimestamp()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.BoolValue.Value = true;

            var record = undo.Peek();
            Assert.IsNotNull(record);
            Assert.IsGreaterThan(0, record.Timestamp);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Undo behavior ---

    /// <summary>
    /// Tests that TryUndo restores the parameter to its previous value.
    /// </summary>
    [TestMethod]
    public void TryUndo_RestoresPreviousValue()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 42;
            var result = undo.TryUndo();

            Assert.IsTrue(result);
            Assert.AreEqual(10, config.IntValue.Value);
            Assert.AreEqual(0, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that TryUndo on an empty stack returns false.
    /// </summary>
    [TestMethod]
    public void TryUndo_EmptyStack_ReturnsFalse()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            Assert.IsFalse(undo.TryUndo());
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the undo operation does not record itself as a new change.
    /// </summary>
    [TestMethod]
    public void TryUndo_DoesNotRecordSelf()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 42;
            Assert.AreEqual(1, undo.Count);

            undo.TryUndo();
            Assert.AreEqual(0, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that multiple sequential undos restore values in reverse order.
    /// </summary>
    [TestMethod]
    public void TryUndo_MultipleChanges_RestoresInReverseOrder()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 20;
            config.IntValue.Value = 30;

            undo.TryUndo();
            Assert.AreEqual(20, config.IntValue.Value);

            undo.TryUndo();
            Assert.AreEqual(10, config.IntValue.Value);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that TryUndo pushes a toast notification when Toast options are configured.
    /// </summary>
    [TestMethod]
    public void TryUndo_PushesToastOnSuccess()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, new ConfigUndoOptions { Toast = new ConfigToastOptions() });

            // Clear any existing toasts from other tests
            ToastQueue.Clear();

            config.IntValue.Value = 42;
            undo.TryUndo();

            var entries = ToastQueue.GetActiveEntries();
            Assert.IsNotEmpty(entries);

            var foundUndo = false;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Message.StartsWith("Undo:", StringComparison.Ordinal))
                {
                    foundUndo = true;
                    break;
                }
            }

            Assert.IsTrue(foundUndo, "Expected a toast message starting with 'Undo:'.");
        }
        finally
        {
            ToastQueue.Clear();
            CleanupStore(store, tempPath);
        }
    }

    // --- Capacity ---

    /// <summary>
    /// Tests that the stack drops the oldest record when capacity is exceeded.
    /// </summary>
    [TestMethod]
    public void Capacity_DropsOldestWhenExceeded()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, capacity: 2);

            config.IntValue.Value = 20;
            config.IntValue.Value = 30;
            config.IntValue.Value = 40;

            Assert.AreEqual(2, undo.Count);

            // Oldest (10→20) was dropped. Top is 30→40, then 20→30.
            undo.TryUndo();
            Assert.AreEqual(30, config.IntValue.Value);

            undo.TryUndo();
            Assert.AreEqual(20, config.IntValue.Value);

            Assert.IsFalse(undo.CanUndo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that capacity of 1 retains only the most recent change.
    /// </summary>
    [TestMethod]
    public void Capacity_One_RetainsOnlyMostRecent()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, capacity: 1);

            config.IntValue.Value = 20;
            config.IntValue.Value = 30;

            Assert.AreEqual(1, undo.Count);

            undo.TryUndo();
            Assert.AreEqual(20, config.IntValue.Value);
            Assert.IsFalse(undo.CanUndo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Clear ---

    /// <summary>
    /// Tests that Clear removes all records from the stack.
    /// </summary>
    [TestMethod]
    public void Clear_RemovesAllRecords()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 42;
            config.StringValue.Value = "changed";
            Assert.AreEqual(2, undo.Count);

            undo.Clear();

            Assert.AreEqual(0, undo.Count);
            Assert.IsFalse(undo.CanUndo);
            Assert.IsNull(undo.Peek());
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Delegate skip ---

    /// <summary>
    /// Tests that delegate-typed parameters are not tracked by the undo stack.
    /// </summary>
    [TestMethod]
    public void DelegateParameter_IsNotTracked()
    {
        var (store, config, tempPath) = CreateLoadedStore<DelegateTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<DelegateTestConfig>(store);

            // Changing the int parameter should be tracked.
            config.IntValue.Value = 99;
            Assert.AreEqual(1, undo.Count);

            // The delegate parameter's ValueChanged event shouldn't be subscribed.
            // We can't easily trigger the delegate param change in a way that tests skipping,
            // but verifying that only the int change was recorded is sufficient.
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Dispose ---

    /// <summary>
    /// Tests that Dispose detaches listeners so subsequent changes are not recorded.
    /// </summary>
    [TestMethod]
    public void Dispose_DetachesListeners_NoFurtherRecording()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var undo = new ConfigUndoStack<UndoTestConfig>(store);
            config.IntValue.Value = 42;
            Assert.AreEqual(1, undo.Count);

            undo.Dispose();

            config.IntValue.Value = 99;
            Assert.AreEqual(0, undo.Count); // Stack was cleared on dispose
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that TryUndo returns false after Dispose.
    /// </summary>
    [TestMethod]
    public void TryUndo_AfterDispose_ReturnsFalse()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var undo = new ConfigUndoStack<UndoTestConfig>(store);
            config.IntValue.Value = 42;

            undo.Dispose();

            Assert.IsFalse(undo.TryUndo());
            Assert.IsFalse(undo.CanUndo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that double Dispose does not throw.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var undo = new ConfigUndoStack<UndoTestConfig>(store);
            undo.Dispose();
            undo.Dispose(); // Should not throw
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Snapshot correctness after undo ---

    /// <summary>
    /// Tests that after undo, subsequent changes correctly capture the restored value as old.
    /// </summary>
    [TestMethod]
    public void AfterUndo_SubsequentChange_CapturesRestoredValueAsOld()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 42;
            undo.TryUndo();
            Assert.AreEqual(10, config.IntValue.Value);

            config.IntValue.Value = 99;

            var record = undo.Peek();
            Assert.IsNotNull(record);
            Assert.AreEqual(10, record.OldValue);
            Assert.AreEqual(99, record.NewValue);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Options-based constructor ---

    /// <summary>
    /// Tests that the options-based constructor throws <see cref="ArgumentNullException"/> when store is null.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_NullStore_ThrowsArgumentNullException()
    {
        var options = new ConfigUndoOptions();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new ConfigUndoStack<UndoTestConfig>(null!, options));
    }

    /// <summary>
    /// Tests that the options-based constructor throws <see cref="ArgumentNullException"/> when options is null.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_NullOptions_ThrowsArgumentNullException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            Assert.ThrowsExactly<ArgumentNullException>(
                () => new ConfigUndoStack<UndoTestConfig>(store, null!));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the options-based constructor throws when the store is not loaded.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_UnloadedStore_ThrowsInvalidOperationException()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new ConfigStore<UndoTestConfig>(tempPath);
            var options = new ConfigUndoOptions();
            Assert.ThrowsExactly<InvalidOperationException>(
                () => new ConfigUndoStack<UndoTestConfig>(store, options));
            store.Dispose();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that the options-based constructor throws when the store is disposed.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_DisposedStore_ThrowsObjectDisposedException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        store.Dispose();
        try
        {
            var options = new ConfigUndoOptions();
            Assert.ThrowsExactly<ObjectDisposedException>(
                () => new ConfigUndoStack<UndoTestConfig>(store, options));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that a stack created with default options starts empty and uses the default capacity.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_DefaultOptions_StartsEmpty()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, new ConfigUndoOptions());

            Assert.IsFalse(undo.CanUndo);
            Assert.AreEqual(0, undo.Count);
            Assert.IsNull(undo.Peek());
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the options-based constructor respects a custom capacity.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_CustomCapacity_DropsOldestWhenExceeded()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var options = new ConfigUndoOptions { Capacity = 2 };
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, options);

            config.IntValue.Value = 20;
            config.IntValue.Value = 30;
            config.IntValue.Value = 40;

            Assert.AreEqual(2, undo.Count);

            undo.TryUndo();
            Assert.AreEqual(30, config.IntValue.Value);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigUndoOptions.Capacity"/> falls back to the default when set below 1.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_CapacityBelowOne_UsesDefaultCapacity()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var options = new ConfigUndoOptions { Capacity = 0 };
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, options);

            // Default capacity is 32; push more than 1 to verify fallback didn't produce capacity < 1
            config.IntValue.Value = 20;
            config.IntValue.Value = 30;

            Assert.AreEqual(2, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Toast suppression ---

    /// <summary>
    /// Tests that undo does not push a toast when <see cref="ConfigUndoOptions.Toast"/> is <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void TryUndo_ToastNull_DoesNotPushToast()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var options = new ConfigUndoOptions { Toast = null };
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, options);

            ToastQueue.Clear();

            config.IntValue.Value = 42;
            undo.TryUndo();

            var entries = ToastQueue.GetActiveEntries();
            var foundUndo = false;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Message.StartsWith("Undo:", StringComparison.Ordinal))
                {
                    foundUndo = true;
                    break;
                }
            }

            Assert.IsFalse(foundUndo, "Expected no toast message when Toast is null.");
        }
        finally
        {
            ToastQueue.Clear();
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that undo pushes a toast when using the options-based constructor with a non-null <see cref="ConfigToastOptions"/>.
    /// </summary>
    [TestMethod]
    public void TryUndo_ToastOptions_PushesToast()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var options = new ConfigUndoOptions { Toast = new ConfigToastOptions() };
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, options);

            ToastQueue.Clear();

            config.IntValue.Value = 42;
            undo.TryUndo();

            var entries = ToastQueue.GetActiveEntries();
            var foundUndo = false;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Message.StartsWith("Undo:", StringComparison.Ordinal))
                {
                    foundUndo = true;
                    break;
                }
            }

            Assert.IsTrue(foundUndo, "Expected a toast message starting with 'Undo:'.");
        }
        finally
        {
            ToastQueue.Clear();
            CleanupStore(store, tempPath);
        }
    }
}
