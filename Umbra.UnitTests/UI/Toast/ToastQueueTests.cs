using System.Diagnostics;
using Umbra.UI.Toast;

namespace Umbra.UnitTests.UI.Toast;

[TestClass]
public sealed class ToastQueueTests
{
    [TestInitialize]
    public void Setup()
    {
        ToastQueue.Clear();
        ToastQueue.MaxCapacity = 8;
    }

    [TestCleanup]
    public void Cleanup()
    {
        ToastQueue.Clear();
        ToastQueue.MaxCapacity = 8;
    }

    [TestMethod]
    public void Push_WithValidMessage_AddsEntry()
    {
        // Arrange & Act
        ToastQueue.Push("Hello");

        // Assert
        var entries = ToastQueue.GetActiveEntries();
        Assert.AreEqual(1, entries.Count);
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
        // Arrange
        ToastQueue.MaxCapacity = 3;

        // Act
        ToastQueue.Push("First");
        ToastQueue.Push("Second");
        ToastQueue.Push("Third");
        ToastQueue.Push("Fourth");

        // Assert
        var entries = ToastQueue.GetActiveEntries();
        Assert.AreEqual(3, entries.Count);
        Assert.AreEqual("Second", entries[0].Message);
        Assert.AreEqual("Third", entries[1].Message);
        Assert.AreEqual("Fourth", entries[2].Message);
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
        Assert.AreEqual(1, entries.Count);
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
        Assert.AreEqual(1, first.Count);
        Assert.AreEqual(2, second.Count);
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
        Assert.IsTrue(progress < 0.01f, $"Expected near-zero progress but got {progress}");
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
        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual(level, entries[0].Level);
    }
}
