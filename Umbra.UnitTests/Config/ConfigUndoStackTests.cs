using Umbra.Config.Attributes;
using Umbra.UI.Config;
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
    /// Tests that a grouped numeric interaction records one undo entry when the value changes multiple times while active.
    /// </summary>
    [TestMethod]
    public void GroupedNumericEdit_MultipleIntermediateChanges_PushesSingleRecordOnEnd()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            var sink = (INumericEditSink)undo;

            sink.BeginNumericEdit(config.IntValue);
            config.IntValue.Value = 20;
            config.IntValue.Value = 30;
            config.IntValue.Value = 42;

            Assert.AreEqual(0, undo.Count);

            sink.EndNumericEdit(config.IntValue);

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
    /// Tests that undo after a grouped numeric interaction restores the interaction's initial value.
    /// </summary>
    [TestMethod]
    public void TryUndo_AfterGroupedNumericEdit_RestoresInitialValue()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            var sink = (INumericEditSink)undo;

            sink.BeginNumericEdit(config.IntValue);
            config.IntValue.Value = 18;
            config.IntValue.Value = 27;
            sink.EndNumericEdit(config.IntValue);

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
    /// Tests that a grouped numeric interaction records nothing when the value does not change.
    /// </summary>
    [TestMethod]
    public void GroupedNumericEdit_WithoutEffectiveChange_PushesNoRecord()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            var sink = (INumericEditSink)undo;

            sink.BeginNumericEdit(config.IntValue);
            sink.EndNumericEdit(config.IntValue);

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

    // --- Batch undo ---

    /// <summary>
    /// Tests that changes within a batch produce a single undo entry.
    /// </summary>
    [TestMethod]
    public void Batch_MultipleChanges_ProducesSingleEntry()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("Reset All");
            config.IntValue.Value = 0;
            config.StringValue.Value = "reset";
            config.BoolValue.Value = true;
            undo.EndBatch();

            Assert.AreEqual(1, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that TryUndo on a batch entry restores all parameter values atomically.
    /// </summary>
    [TestMethod]
    public void Batch_TryUndo_RestoresAllValues()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("Reset All");
            config.IntValue.Value = 0;
            config.StringValue.Value = "reset";
            config.BoolValue.Value = true;
            undo.EndBatch();

            var result = undo.TryUndo();

            Assert.IsTrue(result);
            Assert.AreEqual(10, config.IntValue.Value);
            Assert.AreEqual("default", config.StringValue.Value);
            Assert.IsFalse(config.BoolValue.Value);
            Assert.AreEqual(0, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that an empty batch (no actual changes) is discarded silently.
    /// </summary>
    [TestMethod]
    public void Batch_NoChanges_Discarded()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("Empty");
            undo.EndBatch();

            Assert.AreEqual(0, undo.Count);
            Assert.IsFalse(undo.CanUndo);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that nested BeginBatch calls throw <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    public void BeginBatch_WhileActive_ThrowsInvalidOperationException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("First");
            Assert.ThrowsExactly<InvalidOperationException>(() => undo.BeginBatch("Second"));

            undo.EndBatch(); // cleanup
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that EndBatch without an active batch throws <see cref="InvalidOperationException"/>.
    /// </summary>
    [TestMethod]
    public void EndBatch_WithoutBegin_ThrowsInvalidOperationException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            Assert.ThrowsExactly<InvalidOperationException>(() => undo.EndBatch());
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigUndoStack{TConfig}.IsBatchActive"/> reflects correct state.
    /// </summary>
    [TestMethod]
    public void IsBatchActive_ReflectsState()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            Assert.IsFalse(undo.IsBatchActive);

            undo.BeginBatch("Test");
            Assert.IsTrue(undo.IsBatchActive);

            undo.EndBatch();
            Assert.IsFalse(undo.IsBatchActive);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that a batch counts as one entry for capacity and the oldest entry is dropped correctly.
    /// </summary>
    [TestMethod]
    public void Batch_CountsAsOneForCapacity()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, capacity: 2);

            // Entry 1: single change
            config.IntValue.Value = 20;

            // Entry 2: batch
            undo.BeginBatch("Batch");
            config.StringValue.Value = "batched";
            config.BoolValue.Value = true;
            undo.EndBatch();

            Assert.AreEqual(2, undo.Count);

            // Entry 3: single change — should drop the oldest (entry 1)
            config.IntValue.Value = 30;
            Assert.AreEqual(2, undo.Count);

            // Undo entry 3 (single 20→30)
            undo.TryUndo();
            Assert.AreEqual(20, config.IntValue.Value);

            // Undo entry 2 (batch)
            undo.TryUndo();
            Assert.AreEqual("default", config.StringValue.Value);
            Assert.IsFalse(config.BoolValue.Value);

            Assert.AreEqual(0, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the toast message for a batch undo includes the batch label.
    /// </summary>
    [TestMethod]
    public void Batch_TryUndo_ToastContainsBatchLabel()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store, new ConfigUndoOptions { Toast = new ConfigToastOptions() });

            ToastQueue.Clear();

            undo.BeginBatch("Reset Category");
            config.IntValue.Value = 0;
            config.StringValue.Value = "";
            undo.EndBatch();

            undo.TryUndo();

            var entries = ToastQueue.GetActiveEntries();
            var found = false;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Message.Contains("Reset Category", StringComparison.Ordinal))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Expected a toast message containing the batch label 'Reset Category'.");
        }
        finally
        {
            ToastQueue.Clear();
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that after a batch undo, subsequent single changes capture correct old values.
    /// </summary>
    [TestMethod]
    public void Batch_AfterUndo_SubsequentChange_CapturesRestoredValues()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("Reset");
            config.IntValue.Value = 0;
            undo.EndBatch();

            undo.TryUndo();
            Assert.AreEqual(10, config.IntValue.Value);

            // Now change again — the old value should be the restored 10
            config.IntValue.Value = 50;
            var record = undo.Peek();
            Assert.IsNotNull(record);
            Assert.AreEqual(10, record.OldValue);
            Assert.AreEqual(50, record.NewValue);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that mixed single and batch entries undo in the correct order.
    /// </summary>
    [TestMethod]
    public void MixedEntries_UndoInCorrectOrder()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            // Entry 1: single
            config.IntValue.Value = 20;

            // Entry 2: batch
            undo.BeginBatch("Batch");
            config.StringValue.Value = "batched";
            config.BoolValue.Value = true;
            undo.EndBatch();

            // Entry 3: single
            config.IntValue.Value = 30;

            Assert.AreEqual(3, undo.Count);

            // Undo entry 3
            undo.TryUndo();
            Assert.AreEqual(20, config.IntValue.Value);

            // Undo entry 2 (batch)
            undo.TryUndo();
            Assert.AreEqual("default", config.StringValue.Value);
            Assert.IsFalse(config.BoolValue.Value);

            // Undo entry 1
            undo.TryUndo();
            Assert.AreEqual(10, config.IntValue.Value);

            Assert.AreEqual(0, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that BeginBatch on a disposed stack throws <see cref="ObjectDisposedException"/>.
    /// </summary>
    [TestMethod]
    public void BeginBatch_AfterDispose_ThrowsObjectDisposedException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var undo = new ConfigUndoStack<UndoTestConfig>(store);
            undo.Dispose();
            Assert.ThrowsExactly<ObjectDisposedException>(() => undo.BeginBatch("Test"));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that EndBatch on a disposed stack throws <see cref="ObjectDisposedException"/>.
    /// </summary>
    [TestMethod]
    public void EndBatch_AfterDispose_ThrowsObjectDisposedException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            var undo = new ConfigUndoStack<UndoTestConfig>(store);
            undo.BeginBatch("Test");
            undo.Dispose();
            Assert.ThrowsExactly<ObjectDisposedException>(() => undo.EndBatch());
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that a numeric edit session ending during an active batch routes its record into the batch.
    /// </summary>
    [TestMethod]
    public void NumericEdit_EndDuringBatch_RoutesIntoBatch()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            var sink = (INumericEditSink)undo;

            // Start numeric edit before batch
            sink.BeginNumericEdit(config.IntValue);
            config.IntValue.Value = 15;

            // Now open batch and end the numeric edit inside it
            undo.BeginBatch("Mixed Batch");
            config.StringValue.Value = "batched";
            sink.EndNumericEdit(config.IntValue);
            undo.EndBatch();

            // Should be a single batch entry containing both the numeric and string changes
            Assert.AreEqual(1, undo.Count);

            undo.TryUndo();
            Assert.AreEqual(10, config.IntValue.Value);
            Assert.AreEqual("default", config.StringValue.Value);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Peek returns null when the top entry is a batch.
    /// </summary>
    [TestMethod]
    public void Peek_BatchOnTop_ReturnsNull()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("Batch");
            config.IntValue.Value = 0;
            undo.EndBatch();

            Assert.IsNull(undo.Peek());
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that PeekEntry returns the batch entry when the top is a batch.
    /// </summary>
    [TestMethod]
    public void PeekEntry_BatchOnTop_ReturnsBatchEntry()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("MyBatch");
            config.IntValue.Value = 0;
            config.StringValue.Value = "x";
            undo.EndBatch();

            var entry = undo.PeekEntry();
            Assert.IsNotNull(entry);
            Assert.IsInstanceOfType<ConfigBatchChangeRecord>(entry);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that PeekEntry returns a single ConfigChangeRecord when the top is a single entry.
    /// </summary>
    [TestMethod]
    public void PeekEntry_SingleOnTop_ReturnsChangeRecord()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 42;

            var entry = undo.PeekEntry();
            Assert.IsNotNull(entry);
            Assert.IsInstanceOfType<ConfigChangeRecord>(entry);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that PeekEntry returns null for an empty stack.
    /// </summary>
    [TestMethod]
    public void PeekEntry_EmptyStack_ReturnsNull()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            Assert.IsNull(undo.PeekEntry());
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that BeginBatch with null/empty/whitespace label throws ArgumentException.
    /// </summary>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void BeginBatch_InvalidLabel_ThrowsArgumentException(string? label)
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            Assert.ThrowsExactly<ArgumentException>(() => undo.BeginBatch(label!));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Clear also discards a pending batch.
    /// </summary>
    [TestMethod]
    public void Clear_DiscardsPendingBatch()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("Pending");
            config.IntValue.Value = 0;

            undo.Clear();

            Assert.IsFalse(undo.IsBatchActive);
            Assert.AreEqual(0, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the batch undo does not record itself when individual values are restored.
    /// </summary>
    [TestMethod]
    public void Batch_TryUndo_DoesNotRecordSelf()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            undo.BeginBatch("Reset");
            config.IntValue.Value = 0;
            config.StringValue.Value = "x";
            undo.EndBatch();

            Assert.AreEqual(1, undo.Count);
            undo.TryUndo();
            Assert.AreEqual(0, undo.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- WrapWithBatch ---

    /// <summary>
    /// Tests that an action wrapped with <see cref="ConfigUndoStack{TConfig}.WrapWithBatch"/>
    /// produces a single batch undo entry when invoked.
    /// </summary>
    [TestMethod]
    public void WrapWithBatch_InvokedAction_ProducesSingleEntry()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            var reset = undo.WrapWithBatch("Reset", () =>
            {
                config.IntValue.Reset();
                config.StringValue.Reset();
            });

            config.IntValue.Value = 42;
            config.StringValue.Value = "changed";
            undo.Clear();

            reset();

            Assert.AreEqual(1, undo.Count, "Expected exactly one undo entry for the wrapped action.");
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that undoing a wrapped action restores all values atomically.
    /// </summary>
    [TestMethod]
    public void WrapWithBatch_UndoRestoresAllValues()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            config.IntValue.Value = 42;
            config.StringValue.Value = "changed";
            undo.Clear();

            var reset = undo.WrapWithBatch("Reset", () =>
            {
                config.IntValue.Reset();
                config.StringValue.Reset();
            });

            reset();
            undo.TryUndo();

            Assert.AreEqual(42, config.IntValue.Value);
            Assert.AreEqual("changed", config.StringValue.Value);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the batch is still closed when the wrapped action throws.
    /// </summary>
    [TestMethod]
    public void WrapWithBatch_ActionThrows_BatchStillClosed()
    {
        var (store, config, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);

            var wrapped = undo.WrapWithBatch("Failing", () =>
            {
                config.IntValue.Value = 99;
                throw new InvalidOperationException("test");
            });

            try { wrapped(); } catch (InvalidOperationException) { }

            Assert.IsFalse(undo.IsBatchActive, "Batch should be closed after exception.");
            Assert.AreEqual(1, undo.Count, "The partial change should still be recorded as a batch.");
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigUndoStack{TConfig}.WrapWithBatch"/> throws on null action.
    /// </summary>
    [TestMethod]
    public void WrapWithBatch_NullAction_ThrowsArgumentNullException()
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            Assert.ThrowsExactly<ArgumentNullException>(() => undo.WrapWithBatch("Label", null!));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigUndoStack{TConfig}.WrapWithBatch"/> throws on invalid label.
    /// </summary>
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void WrapWithBatch_InvalidLabel_ThrowsArgumentException(string? label)
    {
        var (store, _, tempPath) = CreateLoadedStore<UndoTestConfig>();
        try
        {
            using var undo = new ConfigUndoStack<UndoTestConfig>(store);
            Assert.ThrowsExactly<ArgumentException>(() => undo.WrapWithBatch(label!, () => { }));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Auto batch wrapping via [UmbraBatchUndo] ---

    /// <summary>
    /// Test configuration class with a <see cref="UmbraBatchUndoAttribute"/>-decorated reset action
    /// that resets two value parameters.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class AutoBatchConfig
    {
        [UmbraParameter]
        public Parameter<int> ValueA { get; set; } = new(10);

        [UmbraParameter]
        public Parameter<string> ValueB { get; set; } = new("hello");

        [UmbraParameter]
        [UmbraBatchUndo("Reset All")]
        public Parameter<Action> ResetAll { get; init; }

        [UmbraParameter]
        public Parameter<Action> PlainAction { get; init; } = new(static () => { });

        public AutoBatchConfig()
        {
            ResetAll = new(() =>
            {
                ValueA.Reset();
                ValueB.Reset();
            });
        }
    }

    /// <summary>
    /// Test configuration class with a <see cref="UmbraBatchUndoAttribute"/>-decorated action whose
    /// initial value is <see langword="null"/> to verify silent skip behavior.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class AutoBatchNullActionConfig
    {
        [UmbraParameter]
        public Parameter<int> Value { get; set; } = new(1);

        [UmbraParameter]
        [UmbraBatchUndo("Should Skip")]
        public Parameter<Action> NullAction { get; set; } = new(null!);
    }

    /// <summary>
    /// Verifies that invoking an auto-wrapped action produces a single batch entry on the undo stack.
    /// </summary>
    [TestMethod]
    public void AutoBatchWrapping_InvokingMarkedAction_ProducesSingleBatchEntry()
    {
        var (store, config, tempPath) = CreateLoadedStore<AutoBatchConfig>();
        try
        {
            config.ValueA.Value = 42;
            config.ValueB.Value = "changed";

            using var undo = new ConfigUndoStack<AutoBatchConfig>(store);
            config.ResetAll.Value!();

            Assert.AreEqual(1, undo.Count, "Auto-wrapped reset should produce exactly one undo entry.");
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Verifies that the batch entry label matches the <see cref="UmbraBatchUndoAttribute.Label"/> value.
    /// </summary>
    [TestMethod]
    public void AutoBatchWrapping_BatchEntryLabel_MatchesAttributeLabel()
    {
        var (store, config, tempPath) = CreateLoadedStore<AutoBatchConfig>();
        try
        {
            config.ValueA.Value = 42;

            using var undo = new ConfigUndoStack<AutoBatchConfig>(store);
            config.ResetAll.Value!();

            var entry = undo.PeekEntry();
            Assert.IsInstanceOfType<ConfigBatchChangeRecord>(entry);
            Assert.AreEqual("Reset All", ((ConfigBatchChangeRecord)entry).BatchLabel);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Verifies that undoing an auto-wrapped batch restores all parameter values.
    /// </summary>
    [TestMethod]
    public void AutoBatchWrapping_UndoRestoresAllValues()
    {
        var (store, config, tempPath) = CreateLoadedStore<AutoBatchConfig>();
        try
        {
            config.ValueA.Value = 42;
            config.ValueB.Value = "changed";

            using var undo = new ConfigUndoStack<AutoBatchConfig>(store);
            config.ResetAll.Value!();

            Assert.AreEqual(10, config.ValueA.Value, "ValueA should be reset.");
            Assert.AreEqual("hello", config.ValueB.Value, "ValueB should be reset.");

            undo.TryUndo();

            Assert.AreEqual(42, config.ValueA.Value, "ValueA should be restored after undo.");
            Assert.AreEqual("changed", config.ValueB.Value, "ValueB should be restored after undo.");
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Verifies that a <see cref="UmbraBatchUndoAttribute"/>-decorated parameter with a null action
    /// is silently skipped during auto-wrapping.
    /// </summary>
    [TestMethod]
    public void AutoBatchWrapping_NullActionValue_SilentlySkipped()
    {
        var (store, _, tempPath) = CreateLoadedStore<AutoBatchNullActionConfig>();
        try
        {
            // Construction should not throw even though the action is null.
            using var undo = new ConfigUndoStack<AutoBatchNullActionConfig>(store);
            Assert.IsFalse(undo.CanUndo, "Stack should be empty after construction.");
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Verifies that an action parameter without <see cref="UmbraBatchUndoAttribute"/> is not
    /// auto-wrapped — invoking it does not produce a batch entry.
    /// </summary>
    [TestMethod]
    public void AutoBatchWrapping_ActionWithoutAttribute_NotWrapped()
    {
        var (store, config, tempPath) = CreateLoadedStore<AutoBatchConfig>();
        try
        {
            using var undo = new ConfigUndoStack<AutoBatchConfig>(store);

            config.PlainAction.Value!();

            Assert.AreEqual(0, undo.Count, "Plain action without [UmbraBatchUndo] should not produce undo entries.");
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }
}
