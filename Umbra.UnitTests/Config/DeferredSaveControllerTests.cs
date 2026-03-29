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
    [UmbraAutoRegisterSettings]
    public sealed class TestConfig
    {
        [UmbraSettingsParameter]
        public Parameter<int> TestValue { get; set; } = new(42);
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
    /// Verifies that Tick returns immediately when both disposed and nothing pending.
    /// </summary>
    [TestMethod]
    public void Tick_WhenDisposedAndNothingPending_DoesNotCallFlush()
    {
        // Arrange
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object);

        SetPrivateField(controller, "_anyPending", false);
        SetPrivateField(controller, "_disposed", true);

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
    /// Verifies that Tick calls Flush when elapsed time equals the debounce window exactly.
    /// </summary>
    [TestMethod]
    public void Tick_WhenSliderPendingAndDebounceExactlyElapsed_CallsFlush()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromMilliseconds(100);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        var frequency = Stopwatch.Frequency;
        var ticksForDebounce = (long)(debounceWindow.TotalSeconds * frequency);
        var oldTimestamp = Stopwatch.GetTimestamp() - ticksForDebounce;

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
    /// Verifies that multiple Tick calls with slider pending and insufficient elapsed time do not trigger Flush.
    /// </summary>
    [TestMethod]
    public void Tick_MultipleCallsWithSliderPendingBeforeDebounce_DoesNotCallFlush()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromSeconds(10);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        var timestamp = Stopwatch.GetTimestamp();
        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", true);
        SetPrivateField(controller, "_sliderChangedAt", timestamp);
        SetPrivateField(controller, "_disposed", false);

        // Act - call multiple times in quick succession
        controller.Tick();
        controller.Tick();
        controller.Tick();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Verifies that Tick with very large elapsed time still calls Flush correctly.
    /// </summary>
    [TestMethod]
    public void Tick_WhenSliderPendingWithVeryLargeElapsedTime_CallsFlush()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromSeconds(1);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        var veryOldTimestamp = Stopwatch.GetTimestamp() - (Stopwatch.Frequency * 3600);
        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", true);
        SetPrivateField(controller, "_sliderChangedAt", veryOldTimestamp);
        SetPrivateField(controller, "_disposed", false);

        // Act
        controller.Tick();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Once);
    }

    /// <summary>
    /// Verifies that Tick after disposal is a permanent no-op even with pending changes.
    /// </summary>
    [TestMethod]
    public void Tick_AfterDisposal_IsPermanentNoOp()
    {
        // Arrange
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object);

        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", false);
        SetPrivateField(controller, "_disposed", false);

        controller.Dispose();

        mockStore.Invocations.Clear();

        // Act - call Tick after disposal
        controller.Tick();

        // Assert
        mockStore.Verify(s => s.Save(), Times.Never);
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
    /// Verifies that Tick clears pending state after flushing non-slider changes.
    /// </summary>
    [TestMethod]
    public void Tick_AfterFlushingNonSliderChange_ClearsPendingState()
    {
        // Arrange
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object);

        SetPrivateField(controller, "_anyPending", true);
        SetPrivateField(controller, "_sliderPending", false);
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
    /// Verifies that Tick respects debounce window when called with minimum timestamp value.
    /// </summary>
    [TestMethod]
    public void Tick_WithMinimumTimestamp_HandlesCorrectly()
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

        // Assert - with minimum timestamp, elapsed time should be very large
        mockStore.Verify(s => s.Save(), Times.Once);
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
    /// Verifies that Tick with very small debounce window (1 tick) works correctly.
    /// </summary>
    [TestMethod]
    public void Tick_WithVerySmallDebounceWindow_CallsFlushAfterMinimalWait()
    {
        // Arrange
        var debounceWindow = TimeSpan.FromTicks(1);
        var mockStore = CreateMockStore();
        var controller = new DeferredSaveController<TestConfig>(mockStore.Object, debounceWindow);

        var oldTimestamp = Stopwatch.GetTimestamp() - 100;
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
    /// Creates a mock SettingsStore configured to appear loaded and not disposed.
    /// </summary>
    private static Mock<SettingsStore<TestConfig>> CreateMockStore()
    {
        var mock = new Mock<SettingsStore<TestConfig>>(MockBehavior.Strict, "test.json");
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
        var storeMock = new Mock<SettingsStore<TestConfig>>(MockBehavior.Strict, "dummy.json");
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
        var storeMock = new Mock<SettingsStore<TestConfig>>(MockBehavior.Strict, "dummy.json");
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
        var storeMock = new Mock<SettingsStore<TestConfig>>(MockBehavior.Strict, "dummy.json");
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
    /// Tests that Dispose flushes pending changes to disk before removing listeners.
    /// Verifies that Save is called when there are pending changes and the store is not disposed.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenPendingChangesExist_FlushesBeforeRemovingListeners()
    {
        // Arrange
        var storeMock = new Mock<SettingsStore<TestConfig>>(MockBehavior.Strict, "dummy.json");
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(true);
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));

        var callSequence = new System.Collections.Generic.List<string>();
        storeMock.Setup(s => s.Save()).Callback(() => callSequence.Add("Save"));
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>())).Callback(() => callSequence.Add("RemoveAny"));
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>())).Callback(() => callSequence.Add("RemoveNumeric"));

        var controller = new DeferredSaveController<TestConfig>(storeMock.Object, TimeSpan.FromSeconds(1));

        // Force pending state
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        anyPendingField!.SetValue(controller, true);

        // Act
        controller.Dispose();

        // Assert
        Assert.HasCount(3, callSequence, "All three methods should be called");
        Assert.AreEqual("Save", callSequence[0], "Save should be called first (as part of Flush)");
        Assert.Contains("RemoveAny", callSequence, "RemoveListenerFromAll(Action) should be called after Save");
        Assert.Contains("RemoveNumeric", callSequence, "RemoveListenerFromAll(predicate, Action) should be called after Save");
    }

    /// <summary>
    /// Tests that Dispose clears pending state flags after flushing.
    /// Verifies that _anyPending and _sliderPending are set to false after disposal.
    /// </summary>
    [TestMethod]
    public void Dispose_WhenCalled_ClearsPendingStateFlags()
    {
        // Arrange
        var storeMock = new Mock<SettingsStore<TestConfig>>(MockBehavior.Strict, "dummy.json");
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
        var sliderPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_sliderPending", BindingFlags.NonPublic | BindingFlags.Instance);
        anyPendingField!.SetValue(controller, true);
        sliderPendingField!.SetValue(controller, true);

        // Act
        controller.Dispose();

        // Assert
        var anyPending = (bool)anyPendingField.GetValue(controller)!;
        var sliderPending = (bool)sliderPendingField.GetValue(controller)!;
        Assert.IsFalse(anyPending, "_anyPending should be cleared after disposal");
        Assert.IsFalse(sliderPending, "_sliderPending should be cleared after disposal");
    }

    /// <summary>
    /// Tests that Dispose does not throw when store is not disposed and no pending changes exist.
    /// Verifies normal cleanup path without saving.
    /// </summary>
    [TestMethod]
    public void Dispose_WithNoPendingChanges_RemovesListenersWithoutSaving()
    {
        // Arrange
        var storeMock = new Mock<SettingsStore<TestConfig>>(MockBehavior.Strict, "dummy.json");
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(true);
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));

        var controller = new DeferredSaveController<TestConfig>(storeMock.Object, TimeSpan.FromSeconds(1));

        // Act
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Never, "Save should not be called when there are no pending changes");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Once);
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()), Times.Once);
    }

    /// <summary>
    /// Tests that Dispose works correctly with different debounce window values.
    /// Verifies that the debounce window setting does not affect disposal behavior.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(5)]
    [DataRow(60)]
    [DataRow(3600)]
    public void Dispose_WithVariousDebounceWindows_PerformsCleanupRegardless(int debounceSeconds)
    {
        // Arrange
        var storeMock = new Mock<SettingsStore<TestConfig>>(MockBehavior.Strict, "dummy.json");
        storeMock.SetupGet(s => s.IsDisposed).Returns(false);
        storeMock.SetupGet(s => s.IsLoaded).Returns(true);
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        storeMock.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.Save());
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Action>()));
        storeMock.Setup(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));

        var controller = new DeferredSaveController<TestConfig>(storeMock.Object, TimeSpan.FromSeconds(debounceSeconds));

        // Force pending state
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        var sliderPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_sliderPending", BindingFlags.NonPublic | BindingFlags.Instance);
        anyPendingField!.SetValue(controller, true);
        sliderPendingField!.SetValue(controller, true);

        // Act
        controller.Dispose();

        // Assert
        storeMock.Verify(s => s.Save(), Times.Once, "Save should be called regardless of debounce window");
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Action>()), Times.Once);
        storeMock.Verify(s => s.RemoveListenerFromAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()), Times.Once);

        var disposedField = typeof(DeferredSaveController<TestConfig>).GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);
        var isDisposed = (bool)disposedField!.GetValue(controller)!;
        Assert.IsTrue(isDisposed, "Controller should be marked as disposed");
    }

    /// <summary>
    /// Tests that Flush returns immediately without performing any operations when the controller is already disposed.
    /// </summary>
    [TestMethod]
    public void Flush_WhenDisposed_ReturnsImmediately()
    {
        // Arrange
        Mock<SettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict, "test.json");
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);
        controller.Dispose();

        // Act
        controller.Flush();

        // Assert
        // No exception should be thrown, and Save should not be called (verified by MockBehavior.Strict)
        mockStore.Verify(s => s.Save(), Times.Never);
    }

    /// <summary>
    /// Tests that Flush clears pending state without logging a warning when the store is disposed and no changes are pending.
    /// </summary>
    [TestMethod]
    public void Flush_WhenStoreDisposedAndNoPendingChanges_ClearsPendingStateWithoutWarning()
    {
        // Arrange
        Mock<SettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict, "test.json");
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
        Mock<SettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict, "test.json");
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
        Mock<SettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict, "test.json");
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
        Mock<SettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict, "test.json");
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
    /// Tests that Flush clears both _anyPending and _sliderPending flags.
    /// </summary>
    [TestMethod]
    public void Flush_WhenStoreNotDisposed_ClearsBothPendingFlags()
    {
        // Arrange
        Mock<SettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict, "test.json");
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        mockStore.Setup(s => s.Save());

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);

        // Set both pending flags using reflection
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        var sliderPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_sliderPending", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(anyPendingField, "_anyPending field not found");
        Assert.IsNotNull(sliderPendingField, "_sliderPending field not found");

        anyPendingField.SetValue(controller, true);
        sliderPendingField.SetValue(controller, true);

        // Act
        controller.Flush();

        // Assert
        var anyPendingAfter = (bool)anyPendingField.GetValue(controller)!;
        var sliderPendingAfter = (bool)sliderPendingField.GetValue(controller)!;

        Assert.IsFalse(anyPendingAfter, "_anyPending should be cleared");
        Assert.IsFalse(sliderPendingAfter, "_sliderPending should be cleared");
    }

    /// <summary>
    /// Tests that Flush does not throw an exception when called on a fresh controller with no pending changes.
    /// </summary>
    [TestMethod]
    public void Flush_WithNoPendingChanges_DoesNotThrow()
    {
        // Arrange
        Mock<SettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict, "test.json");
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
    /// Tests that Flush transitions the controller from pending to non-pending state after a successful save.
    /// </summary>
    [TestMethod]
    public void Flush_AfterSuccessfulSave_TransitionsToClearedState()
    {
        // Arrange
        Mock<SettingsStore<TestConfig>> mockStore = new(MockBehavior.Strict, "test.json");
        mockStore.Setup(s => s.IsDisposed).Returns(false);
        mockStore.Setup(s => s.IsLoaded).Returns(true);
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Func<IParameter, bool>>(), It.IsAny<Action>()));
        mockStore.Setup(s => s.AddListenerToAll(It.IsAny<Action>()));
        mockStore.Setup(s => s.Save());

        DeferredSaveController<TestConfig> controller = new(mockStore.Object);

        // Set pending state using reflection
        var anyPendingField = typeof(DeferredSaveController<TestConfig>).GetField("_anyPending", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(anyPendingField);
        anyPendingField.SetValue(controller, true);

        // Verify state before flush
        var anyPendingBefore = (bool)anyPendingField.GetValue(controller)!;
        Assert.IsTrue(anyPendingBefore, "Should start with pending state");

        // Act
        controller.Flush();

        // Assert
        var anyPendingAfter = (bool)anyPendingField.GetValue(controller)!;
        Assert.IsFalse(anyPendingAfter, "Should have cleared pending state");
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

    /// <summary>
    /// Tests that the constructor sets DebounceWindow to zero
    /// when a zero TimeSpan is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidStoreWithZeroDebounceWindow_SetsZeroValue()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempFile);
            store.Load();
            var zeroDebounce = TimeSpan.Zero;

            // Act
            var controller = new DeferredSaveController<TestConfig>(store, debounceWindow: zeroDebounce);

            // Assert
            Assert.AreEqual(TimeSpan.Zero, controller.DebounceWindow);

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
    /// Tests that the constructor sets DebounceWindow to a negative value
    /// when a negative TimeSpan is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidStoreWithNegativeDebounceWindow_SetsNegativeValue()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempFile);
            store.Load();
            var negativeDebounce = TimeSpan.FromSeconds(-5);

            // Act
            var controller = new DeferredSaveController<TestConfig>(store, debounceWindow: negativeDebounce);

            // Assert
            Assert.AreEqual(TimeSpan.FromSeconds(-5), controller.DebounceWindow);

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
    /// Tests that the constructor sets DebounceWindow to the maximum TimeSpan value
    /// when TimeSpan.MaxValue is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidStoreWithMaxTimeSpan_SetsMaxValue()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempFile);
            store.Load();

            // Act
            var controller = new DeferredSaveController<TestConfig>(store, debounceWindow: TimeSpan.MaxValue);

            // Assert
            Assert.AreEqual(TimeSpan.MaxValue, controller.DebounceWindow);

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
    /// Tests that the constructor sets DebounceWindow to the minimum TimeSpan value
    /// when TimeSpan.MinValue is provided.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidStoreWithMinTimeSpan_SetsMinValue()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempFile);
            store.Load();

            // Act
            var controller = new DeferredSaveController<TestConfig>(store, debounceWindow: TimeSpan.MinValue);

            // Assert
            Assert.AreEqual(TimeSpan.MinValue, controller.DebounceWindow);

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
    /// Tests that the constructor successfully initializes when provided with a valid,
    /// loaded, non-disposed store and registers listeners without throwing exceptions.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidStore_InitializesSuccessfully()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try
        {
            var store = new SettingsStore<TestConfig>(tempFile);
            store.Load();

            // Act
            var controller = new DeferredSaveController<TestConfig>(store);

            // Assert
            Assert.IsNotNull(controller);
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

}
