namespace Umbra.Input.UnitTests;

/// <summary>
/// Deterministic <see cref="INativeKeyStateProvider"/> for unit tests.
/// Call <see cref="SetKeyDown"/> or <see cref="SetKeyUp"/> to control which keys appear pressed.
/// </summary>
internal sealed class TestNativeKeyStateProvider : INativeKeyStateProvider
{
    private readonly HashSet<int> _downKeys = [];

    public void SetKeyDown(int virtualKeyCode) => _downKeys.Add(virtualKeyCode);

    public void SetKeyUp(int virtualKeyCode) => _downKeys.Remove(virtualKeyCode);

    public void ClearAll() => _downKeys.Clear();

    public bool IsKeyDown(int virtualKeyCode) => _downKeys.Contains(virtualKeyCode);
}

/// <summary>
/// Unit tests for the <see cref="KeyStateTracker"/> snapshot-diff edge tracker.
/// </summary>
[TestClass]
public class KeyStateTrackerTests
{
    private const int VK_A = 0x41;
    private const int VK_B = 0x42;
    private const int VK_C = 0x43;

    private static readonly int[] TrackedKeys = [VK_A, VK_B, VK_C];

    /// <summary>
    /// Tests that a key pressed between two Update calls produces a JustPressed edge.
    /// </summary>
    [TestMethod]
    public void Update_KeyBecomesDown_JustPressedReturnsTrue()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);

        tracker.Update(); // baseline: no keys

        // Act
        provider.SetKeyDown(VK_A);
        tracker.Update();

        // Assert
        Assert.IsTrue(tracker.JustPressed(VK_A));
    }

    /// <summary>
    /// Tests that a key released between two Update calls produces a JustReleased edge.
    /// </summary>
    [TestMethod]
    public void Update_KeyBecomesUp_JustReleasedReturnsTrue()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);

        provider.SetKeyDown(VK_A);
        tracker.Update(); // A is down

        // Act
        provider.SetKeyUp(VK_A);
        tracker.Update(); // A is now up

        // Assert
        Assert.IsTrue(tracker.JustReleased(VK_A));
    }

    /// <summary>
    /// Tests that a key held across two Update calls is reported as down but not JustPressed on the second tick.
    /// </summary>
    [TestMethod]
    public void Update_KeyStaysDown_IsDownTrueButJustPressedFalse()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);

        provider.SetKeyDown(VK_A);
        tracker.Update(); // first tick: A pressed

        // Act
        tracker.Update(); // second tick: A still held

        // Assert
        Assert.IsTrue(tracker.IsDown(VK_A));
        Assert.IsFalse(tracker.JustPressed(VK_A));
    }

    /// <summary>
    /// Tests that a key that was never pressed is not reported as down or pressed.
    /// </summary>
    [TestMethod]
    public void Update_KeyNeverPressed_AllFalse()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);

        // Act
        tracker.Update();

        // Assert
        Assert.IsFalse(tracker.IsDown(VK_A));
        Assert.IsFalse(tracker.JustPressed(VK_A));
        Assert.IsFalse(tracker.JustReleased(VK_A));
    }

    /// <summary>
    /// Tests that TryGetFirstPressed returns the first pressed key when one is available.
    /// </summary>
    [TestMethod]
    public void TryGetFirstPressed_OneKeyPressed_ReturnsTrueWithKey()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);
        tracker.Update();

        provider.SetKeyDown(VK_B);

        // Act
        tracker.Update();
        var result = tracker.TryGetFirstPressed(out var vk);

        // Assert
        Assert.IsTrue(result);
        Assert.AreEqual(VK_B, vk);
    }

    /// <summary>
    /// Tests that TryGetFirstPressed returns false when no keys are pressed.
    /// </summary>
    [TestMethod]
    public void TryGetFirstPressed_NoKeysPressed_ReturnsFalse()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);

        // Act
        tracker.Update();
        var result = tracker.TryGetFirstPressed(out var vk);

        // Assert
        Assert.IsFalse(result);
        Assert.AreEqual(-1, vk);
    }

    /// <summary>
    /// Tests that consecutive Update calls with the same state produce no edges.
    /// </summary>
    [TestMethod]
    public void Update_SameStateTwice_NoEdges()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);

        provider.SetKeyDown(VK_A);
        tracker.Update(); // first: A just pressed

        // Act
        tracker.Update(); // second: A still held, no edge

        // Assert
        Assert.IsFalse(tracker.JustPressed(VK_A));
        Assert.IsFalse(tracker.JustReleased(VK_A));
    }

    /// <summary>
    /// Tests that multiple keys pressed simultaneously are all reported.
    /// </summary>
    [TestMethod]
    public void Update_MultipleKeysPressed_AllReported()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);
        tracker.Update(); // baseline

        provider.SetKeyDown(VK_A);
        provider.SetKeyDown(VK_B);

        // Act
        tracker.Update();

        // Assert
        Assert.IsTrue(tracker.JustPressed(VK_A));
        Assert.IsTrue(tracker.JustPressed(VK_B));
        Assert.IsFalse(tracker.JustPressed(VK_C));
    }

    /// <summary>
    /// Tests the full lifecycle: key pressed, held, released.
    /// </summary>
    [TestMethod]
    public void FullLifecycle_PressHoldRelease_CorrectEdges()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);
        tracker.Update(); // baseline

        // Press
        provider.SetKeyDown(VK_A);
        tracker.Update();
        Assert.IsTrue(tracker.JustPressed(VK_A), "Expected JustPressed on press tick");
        Assert.IsTrue(tracker.IsDown(VK_A), "Expected IsDown on press tick");
        Assert.IsFalse(tracker.JustReleased(VK_A), "Expected no JustReleased on press tick");

        // Hold
        tracker.Update();
        Assert.IsFalse(tracker.JustPressed(VK_A), "Expected no JustPressed on hold tick");
        Assert.IsTrue(tracker.IsDown(VK_A), "Expected IsDown on hold tick");
        Assert.IsFalse(tracker.JustReleased(VK_A), "Expected no JustReleased on hold tick");

        // Release
        provider.SetKeyUp(VK_A);
        tracker.Update();
        Assert.IsFalse(tracker.JustPressed(VK_A), "Expected no JustPressed on release tick");
        Assert.IsFalse(tracker.IsDown(VK_A), "Expected not IsDown on release tick");
        Assert.IsTrue(tracker.JustReleased(VK_A), "Expected JustReleased on release tick");

        // After release
        tracker.Update();
        Assert.IsFalse(tracker.JustPressed(VK_A), "Expected no JustPressed after release");
        Assert.IsFalse(tracker.IsDown(VK_A), "Expected not IsDown after release");
        Assert.IsFalse(tracker.JustReleased(VK_A), "Expected no JustReleased after release");
    }

    /// <summary>
    /// Tests that the constructor throws when provider is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullProvider_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new KeyStateTracker(null!, TrackedKeys));
    }

    /// <summary>
    /// Tests that the constructor throws when tracked keys array is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullTrackedKeys_ThrowsArgumentNullException()
    {
        var provider = new TestNativeKeyStateProvider();
        Assert.ThrowsExactly<ArgumentNullException>(() => new KeyStateTracker(provider, null!));
    }

    /// <summary>
    /// Tests that untracked keys are never reported even if the provider says they are down.
    /// </summary>
    [TestMethod]
    public void Update_UntrackedKey_NotReported()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, [VK_A]); // only tracks A
        tracker.Update();

        // Act
        provider.SetKeyDown(VK_B); // B is not tracked
        tracker.Update();

        // Assert
        Assert.IsFalse(tracker.IsDown(VK_B));
        Assert.IsFalse(tracker.JustPressed(VK_B));
    }

    /// <summary>
    /// Tests that <see cref="KeyStateTracker.JustPressedCount"/> returns zero when no keys are pressed.
    /// </summary>
    [TestMethod]
    public void JustPressedCount_NoKeysPressed_ReturnsZero()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);

        // Act
        tracker.Update();

        // Assert
        Assert.AreEqual(0, tracker.JustPressedCount);
    }

    /// <summary>
    /// Tests that <see cref="KeyStateTracker.JustPressedCount"/> reflects the number of keys that just transitioned to pressed.
    /// </summary>
    [TestMethod]
    public void JustPressedCount_TwoKeysPressed_ReturnsTwo()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);
        tracker.Update(); // baseline

        provider.SetKeyDown(VK_A);
        provider.SetKeyDown(VK_B);

        // Act
        tracker.Update();

        // Assert
        Assert.AreEqual(2, tracker.JustPressedCount);
    }

    /// <summary>
    /// Tests that <see cref="KeyStateTracker.GetJustPressedAt"/> returns all just-pressed VK codes.
    /// </summary>
    [TestMethod]
    public void GetJustPressedAt_TwoKeysPressed_ReturnsAllKeys()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);
        tracker.Update(); // baseline

        provider.SetKeyDown(VK_A);
        provider.SetKeyDown(VK_B);
        tracker.Update();

        // Act
        var pressed = new HashSet<int>();
        for (var i = 0; i < tracker.JustPressedCount; i++)
            pressed.Add(tracker.GetJustPressedAt(i));

        // Assert
        Assert.IsTrue(pressed.Contains(VK_A));
        Assert.IsTrue(pressed.Contains(VK_B));
        Assert.IsFalse(pressed.Contains(VK_C));
    }

    /// <summary>
    /// Tests that <see cref="KeyStateTracker.JustPressedCount"/> returns zero for held keys (no new edge).
    /// </summary>
    [TestMethod]
    public void JustPressedCount_KeyHeld_ReturnsZeroOnSecondTick()
    {
        // Arrange
        var provider = new TestNativeKeyStateProvider();
        var tracker = new KeyStateTracker(provider, TrackedKeys);
        tracker.Update();

        provider.SetKeyDown(VK_A);
        tracker.Update(); // A just pressed

        // Act
        tracker.Update(); // A still held, no edge

        // Assert
        Assert.AreEqual(0, tracker.JustPressedCount);
    }
}
