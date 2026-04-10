using System.Diagnostics;
using Umbra.UI.Toast;

namespace Umbra.UnitTests.UI.Toast;

[TestClass]
public sealed class ToastQueueTests
{
    [TestInitialize]
    public void Setup() => ToastQueue.Clear();

    [TestCleanup]
    public void Cleanup() => ToastQueue.Clear();

    [TestMethod]
    public void Push_WithValidMessage_AddsEntry()
    {
        // Arrange & Act
        ToastQueue.Push("Hello");

        // Assert
        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual("Hello", entries[0].Message);
        Assert.AreEqual(ToastLevel.Info, entries[0].Level);
    }

    [TestMethod]
    public void Push_WithNullMessage_IsIgnored()
    {
        // Arrange & Act
        ToastQueue.Push(null!);

        // Assert
        Assert.AreEqual(0, ToastQueue.Count);
    }

    [TestMethod]
    public void Push_WithEmptyMessage_IsIgnored()
    {
        // Arrange & Act
        ToastQueue.Push("");

        // Assert
        Assert.AreEqual(0, ToastQueue.Count);
    }

    [TestMethod]
    public void Push_WithWhitespaceMessage_IsIgnored()
    {
        // Arrange & Act
        ToastQueue.Push("   ");

        // Assert
        Assert.AreEqual(0, ToastQueue.Count);
    }

    [TestMethod]
    public void Push_RespectsCustomLevel()
    {
        // Arrange & Act
        ToastQueue.Push("Error occurred", ToastLevel.Error);

        // Assert
        var entries = ToastQueue.GetActiveEntries();
        Assert.AreEqual(ToastLevel.Error, entries[0].Level);
    }

    [TestMethod]
    public void Push_RespectsCustomDuration()
    {
        // Arrange
        var duration = TimeSpan.FromSeconds(10);

        // Act
        ToastQueue.Push("Long toast", duration: duration);

        // Assert
        var entries = ToastQueue.GetActiveEntries();
        Assert.AreEqual(duration, entries[0].Duration);
    }

    [TestMethod]
    public void Push_DefaultDuration_UsesQueueDefault()
    {
        // Arrange & Act
        ToastQueue.Push("Default");

        // Assert
        var entries = ToastQueue.GetActiveEntries();
        Assert.AreEqual(ToastQueue.DefaultDuration, entries[0].Duration);
    }

    [TestMethod]
    public void Push_ExceedingCapacity_TrimsOldestEntries()
    {
        // Arrange & Act — push 9 entries to exceed the internal capacity of 8
        for (var i = 1; i <= 9; i++)
            ToastQueue.Push($"Toast {i}");

        // Assert
        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(8, entries);
        Assert.AreEqual("Toast 2", entries[0].Message);
        Assert.AreEqual("Toast 9", entries[7].Message);
    }

    [TestMethod]
    public void GetActiveEntries_PrunesExpiredEntries()
    {
        // Arrange — push with a near-zero duration so it expires immediately
        ToastQueue.Push("Expired", duration: TimeSpan.Zero);
        ToastQueue.Push("Active", duration: TimeSpan.FromSeconds(60));

        // Allow the zero-duration entry to be recognized as expired
        Thread.Sleep(1);

        // Act
        var entries = ToastQueue.GetActiveEntries();

        // Assert
        Assert.HasCount(1, entries);
        Assert.AreEqual("Active", entries[0].Message);
    }

    [TestMethod]
    public void GetActiveEntries_ReturnsSnapshotCopy()
    {
        // Arrange
        ToastQueue.Push("A");
        var first = ToastQueue.GetActiveEntries();

        // Act — push another entry; the first snapshot should be unaffected
        ToastQueue.Push("B");
        var second = ToastQueue.GetActiveEntries();

        // Assert
        Assert.HasCount(1, first);
        Assert.HasCount(2, second);
    }

    [TestMethod]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        ToastQueue.Push("A");
        ToastQueue.Push("B");

        // Act
        ToastQueue.Clear();

        // Assert
        Assert.AreEqual(0, ToastQueue.Count);
    }

    [TestMethod]
    public void ToastEntry_IsExpired_ReturnsTrueWhenExpired()
    {
        // Arrange
        var entry = new ToastEntry("Test", ToastLevel.Info, Stopwatch.GetTimestamp(), TimeSpan.Zero);
        Thread.Sleep(1);

        // Act & Assert
        Assert.IsTrue(entry.IsExpired());
    }

    [TestMethod]
    public void ToastEntry_IsExpired_ReturnsFalseWhenActive()
    {
        // Arrange
        var entry = new ToastEntry("Test", ToastLevel.Info, Stopwatch.GetTimestamp(), TimeSpan.FromSeconds(60));

        // Act & Assert
        Assert.IsFalse(entry.IsExpired());
    }

    [TestMethod]
    public void ToastEntry_GetProgress_ReturnsZeroWhenJustCreated()
    {
        // Arrange
        var entry = new ToastEntry("Test", ToastLevel.Info, Stopwatch.GetTimestamp(), TimeSpan.FromSeconds(60));

        // Act
        var progress = entry.GetProgress();

        // Assert — should be very close to zero
        Assert.IsLessThan(0.01f, progress, $"Expected near-zero progress but got {progress}");
    }

    [TestMethod]
    public void ToastEntry_GetProgress_ReturnsOneWhenExpired()
    {
        // Arrange — expired entry
        var entry = new ToastEntry("Test", ToastLevel.Info, Stopwatch.GetTimestamp(), TimeSpan.Zero);
        Thread.Sleep(1);

        // Act
        var progress = entry.GetProgress();

        // Assert
        Assert.AreEqual(1f, progress);
    }

    [TestMethod]
    [DataRow(ToastLevel.Info)]
    [DataRow(ToastLevel.Success)]
    [DataRow(ToastLevel.Warning)]
    [DataRow(ToastLevel.Error)]
    public void Push_AllLevels_AreStored(ToastLevel level)
    {
        // Arrange & Act
        ToastQueue.Push("Test", level);

        // Assert
        var entries = ToastQueue.GetActiveEntries();
        Assert.HasCount(1, entries);
        Assert.AreEqual(level, entries[0].Level);
    }
}
