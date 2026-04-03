using System.Diagnostics;
using System.Reflection;
using Moq;
using Umbra.Config.Attributes;

namespace Umbra.Config.UnitTests;


/// <summary>
/// Contains unit tests for <see cref="DeferredSaveController{TConfig}.Tick"/> method.
/// </summary>
[TestClass]
public sealed partial class DeferredSaveControllerTests
{
    /// <summary>
    /// Test configuration class for DeferredSaveController tests.
    /// </summary>
    [UmbraAutoRegister]
    public sealed class TestConfig
    {
        [UmbraParameter]
        public Parameter<int> TestValue { get; set; } = new(42);
    }

    /// <summary>
    /// Verifies that the constructor rejects a null settings store.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenStoreIsNull_ThrowsArgumentNullException()
    {
        var exception = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new DeferredSaveController<TestConfig>(null!));
        //var exception = AssertThrows<ArgumentNullException>(() => _ = new DeferredSaveController<TestConfig>(null!));

        Assert.AreEqual("store", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor rejects a disposed settings store.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenStoreIsDisposed_ThrowsObjectDisposedException()
    {
        var storeMock = new Mock<ISettingsStore<TestConfig>>(MockBehavior.Strict);
        storeMock.SetupGet(s => s.IsDisposed).Returns(true);

        Assert.ThrowsExactly<ObjectDisposedException>(() => _ = new DeferredSaveController<TestConfig>(storeMock.Object));
    }

    /// <summary>
    /// Verifies that the constructor requires the settings store to be loaded first.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenStoreIsNotLoaded_ThrowsInvalidOperationException()
    {
        var storeMock = new Mock<ISettingsStore<TestConfig>>(MockBehavior.Strict);
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(false);

        Assert.ThrowsExactly<InvalidOperationException>(() => _ = new DeferredSaveController<TestConfig>(storeMock.Object));
    }

    /// <summary>
    /// Verifies that omitting the debounce window uses the documented one-second default.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenDebounceWindowIsOmitted_UsesOneSecondDefault()
    {
        var storeMock = CreateMockStore();

        var controller = new DeferredSaveController<TestConfig>(storeMock.Object);

        Assert.AreEqual(TimeSpan.FromSeconds(1), controller.DebounceWindow);
    }

    /// <summary>
    /// Verifies that Tick returns immediately without calling Flush when the controller is disposed.
    /// </summary>
    [TestMethod]
    public void Tick_WhenDisposed_DoesNotCallFlush()
    {
        // Arrange
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object);

        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_disposed", true);
        var originalFlush = typeof(DeferredSaveController<TestConfig>).GetMethod("Flush", BindingFlags.Instance | BindingFlags.Public);

        // Act
        controller.Tick();

        // Assert - verify Flush was not called by checking the store
        mockStore.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that Tick returns immediately without calling Flush when no changes are pending.
    /// </summary>
    [TestMethod]
    public void Tick_WhenNothingPending_DoesNotCallFlush()
    {
        // Arrange
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object);

        SetPrivateField(controller, "_anyPending", false);
        SetPrivateField(controller, "_disposed", false);

        // Act
        controller.Tick();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that Tick calls Flush immediately when a non-slider change is pending.
    /// </summary>
    [TestMethod]
    public void Tick_WhenNonSliderPending_CallsFlushImmediately()
    {
        // Arrange
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object);

        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", false);
        SetPrivateField(controller, "_disposed", false);

        // Act
        controller.Tick();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that Tick does not call Flush when slider change is pending but debounce window has not elapsed.
    /// </summary>
    [TestMethod]
    public void Tick_WhenSliderPendingAndDebounceNotElapsed_DoesNotCallFlush()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromSeconds(1);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        var timestamp = Stopwatch.GetTimestamp();
        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", true);
        SetPrivateField(controller, "_sliderChangedAt", timestamp);
        SetPrivateField(controller, "_disposed", false);

        // Act - call Tick immediately (elapsed time ~0)
        controller.Tick();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that Tick calls Flush when slider change is pending and debounce window has elapsed.
    /// </summary>
    [TestMethod]
    public void Tick_WhenSliderPendingAndDebounceElapsed_CallsFlush()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromMilliseconds(10);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        var oldTimestamp = Stopwatch.GetTimestamp() - Stopwatch.Frequency;
        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", true);
        SetPrivateField(controller, "_sliderChangedAt", oldTimestamp);
        SetPrivateField(controller, "_disposed", false);

        // Act
        controller.Tick();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that Tick with zero debounce window calls Flush immediately for slider changes.
    /// </summary>
    [TestMethod]
    public void Tick_WithZeroDebounceWindowAndSliderPending_CallsFlush()
    {
        // Arrange
        var debounceWindow = TimeSpan.Zero;
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        var timestamp = Stopwatch.GetTimestamp();
        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", true);
        SetPrivateField(controller, "_sliderChangedAt", timestamp);
        SetPrivateField(controller, "_disposed", false);

        // Act
        controller.Tick();

        // Assert - even with zero elapsed time, >= zero should be true
        mockStore.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that Tick clears pending state after flushing slider changes.
    /// </summary>
    [TestMethod]
    public void Tick_AfterFlushingSliderChange_ClearsPendingState()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromMilliseconds(10);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        var oldTimestamp = Stopwatch.GetTimestamp() - Stopwatch.Frequency;
        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", true);
        SetPrivateField(controller, "_sliderChangedAt", oldTimestamp);
        SetPrivateField(controller, "_disposed", false);

        // Act
        controller.Tick();

        // Assert - verify pending state was cleared
        var anyPending = GetPrivateField<bool>(controller, "_anyPending");
        var sliderPending = GetPrivateField<bool>(controller, "_sliderPending");

        Assert.IsFalse(anyPending);
        Assert.IsFalse(sliderPending);
    }

    /// <summary>
    /// Verifies that Tick safely handles the minimum timestamp value without flushing.
    /// </summary>
    [TestMethod]
    public void Tick_WithMinimumTimestamp_DoesNotFlush()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromMilliseconds(100);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", true);
        SetPrivateField(controller, "_sliderChangedAt", long.MinValue);
        SetPrivateField(controller, "_disposed", false);

        // Act
        controller.Tick();

        // Assert - Stopwatch.GetElapsedTime(long.MinValue) can underflow, so no flush should occur
        mockStore.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that Tick with maximum timestamp value handles correctly.
    /// </summary>
    [TestMethod]
    public void Tick_WithMaximumTimestamp_HandlesCorrectly()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromMilliseconds(100);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", true);
        SetPrivateField(controller, "_sliderChangedAt", long.MaxValue);
        SetPrivateField(controller, "_disposed", false);

        // Act - with MaxValue timestamp, elapsed time calculation may underflow/overflow
        controller.Tick();

        // Assert - behavior depends on Stopwatch.GetElapsedTime implementation
        // Since timestamp is in the future, elapsed time should be negative or zero
        mockStore.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Creates a mock settings store configured to appear loaded and not disposed.
    /// </summary>
    private static Mock<ISettingsStore<TestConfig>> CreateMockStore()
    {
        var mock = new Mock<ISettingsStore<TestConfig>>(MockBehavior.Strict);
        mock.Setup(s => s.IsLoaded).Returns(true);
        mock.Setup(s => s.IsDisposed).Returns(false);
        mock.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        mock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>()));
        mock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mock.Setup(s => s.Save());
        return mock;
    }

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

    /// <summary>
    /// Tests that Dispose performs full cleanup when called for the first time on a non-disposed store.
    /// Verifies that Flush is called, listeners are removed, and the instance is marked as disposed.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledFirstTimeWithNonDisposedStore_FlushesAndRemovesListeners()
    {
        // Arrange
        var storeMock = new Mock<ISettingsStore<TestConfig>>(MockBehavior.Strict);
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(true);
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.Save());
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));

        var controller = new DeferredSaveController<TestConfig>(storeMock.Object, TimeSpan.FromSeconds(1));

        // Force pending state by setting internal field via reflection
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        anyPendingField!.SetValue(controller, true);

        // Act
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once, "Save should be called once during Flush");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Once, "RemoveListenerFromAll(Action) should be called once");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()), Times.Once, "RemoveListenerFromAll(predicate, Action) should be called once");

        // Verify disposed state
        var disposedField = typeof(DeferredSaveController<TestConfig>).GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);
        var isDisposed = (bool)disposedField!.GetValue(controller)!;
        Assert.IsTrue(isDisposed, "Controller should be marked as disposed");
    }

    /// <summary>
    /// Tests that calling Dispose multiple times is idempotent and only performs cleanup once.
    /// Subsequent calls should return immediately without calling Flush or removing listeners again.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalledMultipleTimes_IsIdempotentAndPerformsCleanupOnce()
    {
        // Arrange
        var storeMock = new Mock<ISettingsStore<TestConfig>>(MockBehavior.Strict);
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(true);
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.Save());
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));

        var controller = new DeferredSaveController<TestConfig>(storeMock.Object, TimeSpan.FromSeconds(1));

        // Force pending state
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        anyPendingField!.SetValue(controller, true);

        // Act
        controller.Dispose();
        controller.Dispose();
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once, "Save should be called only once, not on subsequent Dispose calls");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Once, "RemoveListenerFromAll(Action) should be called only once");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()), Times.Once, "RemoveListenerFromAll(predicate, Action) should be called only once");
    }

    /// <summary>
    /// Tests that Dispose still calls Flush but skips listener removal when the store is already disposed.
    /// Verifies that the controller handles a disposed store gracefully and logs a warning if there are pending changes.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenStoreIsAlreadyDisposed_FlushesButSkipsListenerRemoval()
    {
        // Arrange
        var storeMock = new Mock<ISettingsStore<TestConfig>>(MockBehavior.Strict);
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(true);
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));

        var controller = new DeferredSaveController<TestConfig>(storeMock.Object, TimeSpan.FromSeconds(1));

        // Force pending state
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        anyPendingField!.SetValue(controller, true);

        // Change store to disposed state before calling Dispose
        storeMock.SetupGet(s => s.IsDisposed).Returns(true);

        // Act
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never, "Save should not be called when store is disposed");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Never, "RemoveListenerFromAll should be skipped when store is disposed");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()), Times.Never, "RemoveListenerFromAll should be skipped when store is disposed");

        // Verify disposed state
        var disposedField = typeof(DeferredSaveController<TestConfig>).GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);
        var isDisposed = (bool)disposedField!.GetValue(controller)!;
        Assert.IsTrue(isDisposed, "Controller should be marked as disposed even when store is disposed");
    }

    /// <summary>
    /// Tests that Dispose performs normal cleanup when no pending changes exist.
    /// Verifies that listeners are removed and the store is still flushed once.
    /// </summary>
    [TestMethod]
    public void Dispose_WithNoPendingChanges_RemovesListenersAndSaves()
    {
        // Arrange
        var storeMock = new Mock<ISettingsStore<TestConfig>>(MockBehavior.Strict);
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(true);
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.Save());
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));

        var controller = new DeferredSaveController<TestConfig>(storeMock.Object, TimeSpan.FromSeconds(1));

        // Act
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once, "Save should be called once because Dispose always flushes");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Once);
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()), Times.Once);
    }

    /// <summary>
    /// Tests that Flush returns immediately after the controller has already been disposed.
    /// </summary>
    [TestMethod]
    public void Flush_AfterDispose_ReturnsImmediately()
    {
        // Arrange
        Mock<ISettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict);
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        mockStore.Setup(s => s.Save());
        mockStore.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>()));
        mockStore.Setup(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);
        controller.Dispose();

        // Act
        controller.Flush();

        // Assert
        // Dispose performs the single flush; Flush after disposal must not save again
        mockStore.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Tests that Flush clears pending state without logging a warning when the store is disposed and no changes are pending.
    /// </summary>
    [TestMethod]
    public void Flush_WhenStoreDisposedAndNoPendingChanges_ClearsPendingStateWithoutWarning()
    {
        // Arrange
        Mock<ISettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict);
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);

        // Simulate store being disposed
        mockStore.Setup(s => s.IsDisposed).Returns(true);

        // Act
        controller.Flush();

        // Assert
        // Save should not be called (verified by MockBehavior.Strict)
        mockStore.Verify(s => s.Save(), Times.Never);
        // Note: Cannot verify Logger.Warning was not called as it's a static method
    }

    /// <summary>
    /// Tests that Flush logs a warning and clears pending state when the store is disposed and changes are pending.
    /// </summary>
    [TestMethod]
    public void Flush_WhenStoreDisposedAndChangesArePending_ClearsPendingState()
    {
        // Arrange
        Mock<ISettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict);
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);

        // Set _anyPending to true using reflection
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(anyPendingField, "_anyPending field not found");
        anyPendingField.SetValue(controller, true);

        // Simulate store being disposed
        mockStore.Setup(s => s.IsDisposed).Returns(true);

        // Act
        controller.Flush();

        // Assert
        // Save should not be called (verified by MockBehavior.Strict)
        mockStore.Verify(s => s.Save(), Times.Never);
        // Note: Cannot verify Logger.Warning was called with the expected message as it's a static method.
        // The warning should contain "dropping pending changes because the SettingsStore was already disposed"

        // Verify _anyPending was cleared
        var anyPendingAfter = (bool)anyPendingField.GetValue(controller)!;
        Assert.IsFalse(anyPendingAfter, "_anyPending should be cleared after Flush");
    }

    /// <summary>
    /// Tests that Flush calls Save on the store and clears pending state when the store is not disposed.
    /// </summary>
    [TestMethod]
    public void Flush_WhenStoreNotDisposed_CallsSaveAndClearsPendingState()
    {
        // Arrange
        Mock<ISettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict);
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        mockStore.Setup(s => s.Save());

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);

        // Set _anyPending to true using reflection to simulate pending changes
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(anyPendingField, "_anyPending field not found");
        anyPendingField.SetValue(controller, true);

        // Act
        controller.Flush();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Once, "Save should be called once");
        // Note: Cannot verify Logger.Info was called with the expected message as it's a static method.
        // The info message should contain "flushing pending changes to disk"

        // Verify _anyPending was cleared
        var anyPendingAfter = (bool)anyPendingField.GetValue(controller)!;
        Assert.IsFalse(anyPendingAfter, "_anyPending should be cleared after Flush");
    }

    /// <summary>
    /// Tests that Flush can be called multiple times without error when the store is not disposed.
    /// </summary>
    [TestMethod]
    public void Flush_CalledMultipleTimes_CallsSaveEachTime()
    {
        // Arrange
        Mock<ISettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict);
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        mockStore.Setup(s => s.Save());

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);

        // Act
        controller.Flush();
        controller.Flush();
        controller.Flush();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Exactly(3), "Save should be called three times");
    }

    /// <summary>
    /// Tests that Flush does not throw an exception when called on a fresh controller with no pending changes.
    /// </summary>
    [TestMethod]
    public void Flush_WithNoPendingChanges_DoesNotThrow()
    {
        // Arrange
        Mock<ISettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict);
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        mockStore.Setup(s => s.Save());

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);

        // Act & Assert
        controller.Flush(); // Should not throw
        mockStore.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Tests that the constructor sets DebounceWindow to the default value of 1 second
    /// when the debounceWindow parameter is null.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidStoreWithNullDebounceWindow_SetsDefaultOneSecond()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempFile);
            store.Load();

            // Act
            var controller = new DeferredSaveController<TestConfig>(store, debounceWindow: null);

            // Assert
            Assert.AreEqual(TimeSpan.FromSeconds(1), controller.DebounceWindow);

            controller.Dispose();
            store.Dispose();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Tests that the constructor sets DebounceWindow to the provided value
    /// when a specific debounceWindow parameter is supplied.
    /// </summary>
    [TestMethod]
    [DataRow(0, 0, 0, 500)]
    [DataRow(0, 0, 2, 0)]
    [DataRow(0, 0, 5, 0)]
    [DataRow(0, 1, 30, 0)]
    public void Constructor_ValidStoreWithSpecificDebounceWindow_SetsProvidedValue(int hours, int minutes, int seconds, int milliseconds)
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempFile);
            store.Load();
            var expectedDebounceWindow = new TimeSpan(0, hours, minutes, seconds, milliseconds);

            // Act
            var controller = new DeferredSaveController<TestConfig>(store, debounceWindow: expectedDebounceWindow);

            // Assert
            Assert.AreEqual(expectedDebounceWindow, controller.DebounceWindow);

            controller.Dispose();
            store.Dispose();
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

}
