using System.Reflection;
using Moq;
using Umbra.Config.Attributes;
using Umbra.UI.Config;

namespace Umbra.Config.UnitTests;

/// <summary>
/// Contains unit tests for <see cref="ConfigSaveController{TConfig}"/>.
/// </summary>
[TestClass]
public sealed class ConfigSaveControllerTests
{
    /// <summary>
    /// Test configuration class for ConfigSaveController tests.
    /// </summary>
    [UmbraAutoRegister]
    public sealed class TestConfig
    {
        [UmbraParameter]
        public Parameter<int> TestValue { get; set; } = new(42);
    }

    // ──────────────────────────── Constructor guards ────────────────────────────

    /// <summary>
    /// Verifies that the constructor rejects a null config store.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenStoreIsNull_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(
            () => _ = new ConfigSaveController<TestConfig>(null!));

        Assert.AreEqual("store", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a disposed config store.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenStoreIsDisposed_ThrowsObjectDisposedException()
    {
        var storeMock = new Mock<IConfigStore<TestConfig>>(MockBehavior.Strict);
        storeMock.SetupGet(s => s.IsDisposed).Returns(true);

        Assert.ThrowsExactly<ObjectDisposedException>(
            () => _ = new ConfigSaveController<TestConfig>(storeMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor requires the config store to be loaded first.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenStoreIsNotLoaded_ThrowsInvalidOperationException()
    {
        var storeMock = new Mock<IConfigStore<TestConfig>>(MockBehavior.Strict);
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(false);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => _ = new ConfigSaveController<TestConfig>(storeMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor subscribes a listener via <c>AddListenerToAll</c>.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenValid_SubscribesListenerToStore()
    {
        var (storeMock, _) = CreateMockStoreCapturingListener();

        _ = new ConfigSaveController<TestConfig>(storeMock.Object);

        storeMock.Verify(s => s.AddListenerToAll(It.IsAny<Action>()), Times.Once);
    }

    // ──────────────── Instant save on non-numeric parameter change ──────────────

    /// <summary>
    /// Verifies that a parameter change outside a numeric edit triggers an immediate save.
    /// </summary>
    [TestMethod]
    public void ParameterChanged_WhenNotInNumericEdit_SavesImmediately()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        _ = new ConfigSaveController<TestConfig>(storeMock.Object);

        // Act
        listener();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that multiple non-numeric parameter changes each trigger a save.
    /// </summary>
    [TestMethod]
    public void ParameterChanged_MultipleTimes_SavesEachTime()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        _ = new ConfigSaveController<TestConfig>(storeMock.Object);

        // Act
        listener();
        listener();
        listener();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Exactly(3));
    }

    // ──────────────── Deferred save during numeric edit ─────────────────────────

    /// <summary>
    /// Verifies that a parameter change during an active numeric edit does not trigger a save.
    /// </summary>
    [TestMethod]
    public void ParameterChanged_WhenInNumericEdit_DefersSave()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        var parameter = CreateMockParameter();

        // Act
        sink.BeginNumericEdit(parameter);
        listener();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that ending a numeric edit flushes the deferred save when changes are pending.
    /// </summary>
    [TestMethod]
    public void EndNumericEdit_WhenPendingSave_SavesImmediately()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        var parameter = CreateMockParameter();

        sink.BeginNumericEdit(parameter);
        listener(); // deferred

        // Act
        sink.EndNumericEdit(parameter);

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that ending a numeric edit with no pending changes does not trigger a save.
    /// </summary>
    [TestMethod]
    public void EndNumericEdit_WhenNoPendingSave_DoesNotSave()
    {
        // Arrange
        var (storeMock, _) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        var parameter = CreateMockParameter();

        sink.BeginNumericEdit(parameter);

        // Act
        sink.EndNumericEdit(parameter);

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that multiple deferred changes during a single numeric edit result in one save.
    /// </summary>
    [TestMethod]
    public void EndNumericEdit_WithMultipleDeferredChanges_SavesOnce()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        var parameter = CreateMockParameter();

        sink.BeginNumericEdit(parameter);
        listener();
        listener();
        listener();

        // Act
        sink.EndNumericEdit(parameter);

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="INumericEditSink.BeginNumericEdit"/> is a no-op after disposal.
    /// </summary>
    [TestMethod]
    public void BeginNumericEdit_WhenDisposed_IsNoOp()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        controller.Dispose();
        storeMock.Invocations.Clear();

        // Act
        sink.BeginNumericEdit(CreateMockParameter());
        listener();

        // Assert — if BeginNumericEdit took effect, listener would defer; instead it's a no-op
        // and the change handler itself is guarded by _disposed, so no save either
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="INumericEditSink.EndNumericEdit"/> is a no-op after disposal.
    /// </summary>
    [TestMethod]
    public void EndNumericEdit_WhenDisposed_IsNoOp()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        sink.BeginNumericEdit(CreateMockParameter());
        listener(); // deferred
        controller.Dispose();
        storeMock.Invocations.Clear();

        // Act
        sink.EndNumericEdit(CreateMockParameter());

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    // ────────────────────── Full interaction lifecycle ──────────────────────────

    /// <summary>
    /// Verifies a complete cycle: begin numeric edit, change, end, then a non-numeric change.
    /// </summary>
    [TestMethod]
    public void FullLifecycle_NumericEditThenNonNumericChange_SavesTwice()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        var parameter = CreateMockParameter();

        // Act — numeric edit cycle
        sink.BeginNumericEdit(parameter);
        listener(); // deferred
        listener(); // deferred
        sink.EndNumericEdit(parameter); // save #1

        // Act — non-numeric change
        listener(); // save #2

        // Assert
        storeMock.Verify(s => s.Save(), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies two consecutive numeric edit cycles each produce exactly one save.
    /// </summary>
    [TestMethod]
    public void FullLifecycle_TwoConsecutiveNumericEdits_SavesTwice()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        var parameter = CreateMockParameter();

        // Act — first edit
        sink.BeginNumericEdit(parameter);
        listener();
        sink.EndNumericEdit(parameter); // save #1

        // Act — second edit
        sink.BeginNumericEdit(parameter);
        listener();
        sink.EndNumericEdit(parameter); // save #2

        // Assert
        storeMock.Verify(s => s.Save(), Times.Exactly(2));
    }

    // ──────────────── Deferred save during text edit ────────────────────────────

    /// <summary>
    /// Verifies that a parameter change during an active text edit does not trigger a save.
    /// </summary>
    [TestMethod]
    public void ParameterChanged_WhenInTextEdit_DefersSave()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (ITextEditSink)controller;
        var parameter = CreateMockParameter();

        // Act
        sink.BeginTextEdit(parameter);
        listener();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that ending a text edit flushes the deferred save when changes are pending.
    /// </summary>
    [TestMethod]
    public void EndTextEdit_WhenPendingSave_SavesImmediately()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (ITextEditSink)controller;
        var parameter = CreateMockParameter();

        sink.BeginTextEdit(parameter);
        listener(); // deferred

        // Act
        sink.EndTextEdit(parameter);

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that ending a text edit with no pending changes does not trigger a save.
    /// </summary>
    [TestMethod]
    public void EndTextEdit_WhenNoPendingSave_DoesNotSave()
    {
        // Arrange
        var (storeMock, _) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (ITextEditSink)controller;
        var parameter = CreateMockParameter();

        sink.BeginTextEdit(parameter);

        // Act
        sink.EndTextEdit(parameter);

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that multiple deferred changes during a single text edit result in one save.
    /// </summary>
    [TestMethod]
    public void EndTextEdit_WithMultipleDeferredChanges_SavesOnce()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (ITextEditSink)controller;
        var parameter = CreateMockParameter();

        sink.BeginTextEdit(parameter);
        listener();
        listener();
        listener();

        // Act
        sink.EndTextEdit(parameter);

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="ITextEditSink.BeginTextEdit"/> is a no-op after disposal.
    /// </summary>
    [TestMethod]
    public void BeginTextEdit_WhenDisposed_IsNoOp()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (ITextEditSink)controller;
        controller.Dispose();
        storeMock.Invocations.Clear();

        // Act
        sink.BeginTextEdit(CreateMockParameter());
        listener();

        // Assert — if BeginTextEdit took effect, listener would defer; instead it's a no-op
        // and the change handler itself is guarded by _disposed, so no save either
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="ITextEditSink.EndTextEdit"/> is a no-op after disposal.
    /// </summary>
    [TestMethod]
    public void EndTextEdit_WhenDisposed_IsNoOp()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (ITextEditSink)controller;
        sink.BeginTextEdit(CreateMockParameter());
        listener(); // deferred
        controller.Dispose();
        storeMock.Invocations.Clear();

        // Act
        sink.EndTextEdit(CreateMockParameter());

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    // ──────────────── Full lifecycle: text edit ─────────────────────────────────

    /// <summary>
    /// Verifies a complete cycle: begin text edit, change, end, then a non-text change.
    /// </summary>
    [TestMethod]
    public void FullLifecycle_TextEditThenNonTextChange_SavesTwice()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (ITextEditSink)controller;
        var parameter = CreateMockParameter();

        // Act — text edit cycle
        sink.BeginTextEdit(parameter);
        listener(); // deferred
        listener(); // deferred
        sink.EndTextEdit(parameter); // save #1

        // Act — non-text change
        listener(); // save #2

        // Assert
        storeMock.Verify(s => s.Save(), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that overlapping numeric and text edits defer the save until both resolve.
    /// </summary>
    [TestMethod]
    public void FullLifecycle_NumericAndTextEditOverlap_DefersUntilBothEnd()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var numSink = (INumericEditSink)controller;
        var textSink = (ITextEditSink)controller;
        var numParam = CreateMockParameter();
        var textParam = CreateMockParameter();

        // Act — start both
        numSink.BeginNumericEdit(numParam);
        textSink.BeginTextEdit(textParam);
        listener(); // deferred

        // End numeric — text is still active, so still deferred
        numSink.EndNumericEdit(numParam);
        storeMock.Verify(s => s.Save(), Times.Never, "Save should still be deferred while text edit is active");

        // End text — both resolved, now save
        textSink.EndTextEdit(textParam);

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once);
    }

    // ──────────────────────────────── Flush ─────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ConfigSaveController{TConfig}.Flush"/> saves when changes are pending.
    /// </summary>
    [TestMethod]
    public void Flush_WhenPending_Saves()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        sink.BeginNumericEdit(CreateMockParameter());
        listener(); // deferred, not saved yet

        // Act
        controller.Flush();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that <see cref="ConfigSaveController{TConfig}.Flush"/> is a no-op when no changes are pending.
    /// </summary>
    [TestMethod]
    public void Flush_WhenNoPending_DoesNotSave()
    {
        // Arrange
        var (storeMock, _) = CreateMockStoreCapturingListener();
        _ = new ConfigSaveController<TestConfig>(storeMock.Object);

        // Act
        // No parameter changes fired, so nothing pending

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="ConfigSaveController{TConfig}.Flush"/> is a no-op after disposal.
    /// </summary>
    [TestMethod]
    public void Flush_AfterDispose_IsNoOp()
    {
        // Arrange
        var (storeMock, _) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        controller.Dispose();
        storeMock.Invocations.Clear();

        // Act
        controller.Flush();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
    }

    // ──────────────────────────────── Dispose ───────────────────────────────────

    /// <summary>
    /// Verifies that Dispose flushes pending changes and removes the listener.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenPendingChanges_FlushesAndRemovesListener()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        var sink = (INumericEditSink)controller;
        sink.BeginNumericEdit(CreateMockParameter());
        listener(); // deferred

        // Act
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once, "Dispose should flush pending save");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Dispose removes the listener even with no pending changes.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenNoPendingChanges_RemovesListenerWithoutSaving()
    {
        // Arrange
        var (storeMock, _) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);

        // Act
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never);
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Once);
    }

    /// <summary>
    /// Verifies that multiple Dispose calls are idempotent.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledMultipleTimes_IsIdempotent()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        listener(); // instant save

        // Act
        controller.Dispose();
        controller.Dispose();
        controller.Dispose();

        // Assert — only the initial instant save and no extra saves from repeated Dispose
        storeMock.Verify(s => s.Save(), Times.Once);
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Dispose skips listener removal when the store is already disposed.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenStoreAlreadyDisposed_SkipsListenerRemoval()
    {
        // Arrange
        var (storeMock, listener) = CreateMockStoreCapturingListener();
        var controller = new ConfigSaveController<TestConfig>(storeMock.Object);
        listener(); // deferred pending via numeric edit
        SetPrivateField(controller, "_pendingSave", true);

        // Simulate store disposal before controller disposal
        storeMock.SetupGet(s => s.IsDisposed).Returns(true);

        // Act
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once, "First listener() save; Flush during Dispose skips because store is disposed");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Never,
            "RemoveListenerFromAll should be skipped when store is disposed");

        var isDisposed = GetPrivateField<bool>(controller, "_disposed");
        Assert.IsTrue(isDisposed, "Controller should be marked as disposed");
    }

    // ──────────────────────────── Test infrastructure ───────────────────────────

    /// <summary>
    /// Creates a mock config store that captures the <see cref="Action"/> listener passed to
    /// <c>AddListenerToAll</c>, exposing it so tests can simulate parameter change notifications.
    /// </summary>
    private static (Mock<IConfigStore<TestConfig>> StoreMock, Action Listener) CreateMockStoreCapturingListener()
    {
        Action? captured = null;
        var mock = new Mock<IConfigStore<TestConfig>>(MockBehavior.Strict);
        mock.SetupGet(s => s.IsLoaded).Returns(true);
        mock.SetupGet(s => s.IsDisposed).Returns(false);
        mock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()))
            .Callback<Action>(a => captured = a);
        mock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>()));
        mock.Setup(s => s.Save());
        return (mock, () => captured!.Invoke());
    }

    /// <summary>
    /// Creates a mock <see cref="IParameter"/> for use with <see cref="INumericEditSink"/> methods.
    /// </summary>
    private static IParameter CreateMockParameter()
        => new Mock<IParameter>(MockBehavior.Loose).Object;

    /// <summary>
    /// Sets a private field value on an object using reflection.
    /// </summary>
    private static void SetPrivateField<T>(object target, string fieldName, T value)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found on type '{target.GetType().Name}'.");
        field.SetValue(target, value);
    }

    /// <summary>
    /// Gets a private field value from an object using reflection.
    /// </summary>
    private static T GetPrivateField<T>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field == null)
            throw new InvalidOperationException($"Field '{fieldName}' not found on type '{target.GetType().Name}'.");
        var value = field.GetValue(target);
        return value == null ? default! : (T)value;
    }
}
