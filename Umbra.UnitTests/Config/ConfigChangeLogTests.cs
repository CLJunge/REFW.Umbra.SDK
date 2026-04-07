using System.Diagnostics;

namespace Umbra.Config.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigChangeLog"/> circular buffer behavior.
/// </summary>
[TestClass]
public sealed class ConfigChangeLogTests
{
    private static ConfigChangeRecord MakeRecord(string key, object? oldVal, object? newVal) => new(key, key, oldVal, newVal, Stopwatch.GetTimestamp());

    // --- Constructor ---

    /// <summary>
    /// Tests that the default constructor creates a log with default capacity.
    /// </summary>
    [TestMethod]
    public void Constructor_Default_HasDefaultCapacity()
    {
        var log = new ConfigChangeLog();
        Assert.AreEqual(ConfigChangeLog.DefaultCapacity, log.Capacity);
        Assert.AreEqual(0, log.Count);
    }

    /// <summary>
    /// Tests that the constructor accepts a custom capacity.
    /// </summary>
    [TestMethod]
    public void Constructor_CustomCapacity_SetsCapacity()
    {
        var log = new ConfigChangeLog(8);
        Assert.AreEqual(8, log.Capacity);
    }

    /// <summary>
    /// Tests that the constructor throws for zero capacity.
    /// </summary>
    [TestMethod]
    public void Constructor_ZeroCapacity_ThrowsArgumentOutOfRangeException() => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ConfigChangeLog(0));

    /// <summary>
    /// Tests that the constructor throws for negative capacity.
    /// </summary>
    [TestMethod]
    public void Constructor_NegativeCapacity_ThrowsArgumentOutOfRangeException() => Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ConfigChangeLog(-1));

    // --- Push and Count ---

    /// <summary>
    /// Tests that Push increments the count.
    /// </summary>
    [TestMethod]
    public void Push_IncrementsCount()
    {
        var log = new ConfigChangeLog(4);
        log.Push(MakeRecord("a", 1, 2));
        Assert.AreEqual(1, log.Count);

        log.Push(MakeRecord("b", 3, 4));
        Assert.AreEqual(2, log.Count);
    }

    /// <summary>
    /// Tests that Push throws for null record.
    /// </summary>
    [TestMethod]
    public void Push_NullRecord_ThrowsArgumentNullException()
    {
        var log = new ConfigChangeLog();
        Assert.ThrowsExactly<ArgumentNullException>(() => log.Push(null!));
    }

    // --- GetEntries ---

    /// <summary>
    /// Tests that GetEntries returns entries in oldest-to-newest order.
    /// </summary>
    [TestMethod]
    public void GetEntries_ReturnsOldestToNewest()
    {
        var log = new ConfigChangeLog(4);
        log.Push(MakeRecord("first", 1, 2));
        log.Push(MakeRecord("second", 3, 4));
        log.Push(MakeRecord("third", 5, 6));

        var entries = log.GetEntries();
        Assert.AreEqual(3, entries.Count);
        Assert.AreEqual("first", entries[0].ParameterKey);
        Assert.AreEqual("second", entries[1].ParameterKey);
        Assert.AreEqual("third", entries[2].ParameterKey);
    }

    /// <summary>
    /// Tests that GetEntries returns an empty list when no entries exist.
    /// </summary>
    [TestMethod]
    public void GetEntries_Empty_ReturnsEmptyList()
    {
        var log = new ConfigChangeLog();
        var entries = log.GetEntries();
        Assert.AreEqual(0, entries.Count);
    }

    /// <summary>
    /// Tests that GetEntries returns a snapshot (modifying the list doesn't affect the log).
    /// </summary>
    [TestMethod]
    public void GetEntries_ReturnsSnapshot()
    {
        var log = new ConfigChangeLog(4);
        log.Push(MakeRecord("a", 1, 2));

        var entries = log.GetEntries();
        entries.Clear();

        Assert.AreEqual(1, log.Count);
    }

    // --- Circular wrapping ---

    /// <summary>
    /// Tests that the buffer wraps correctly when capacity is exceeded.
    /// </summary>
    [TestMethod]
    public void Push_ExceedsCapacity_OverwritesOldest()
    {
        var log = new ConfigChangeLog(3);
        log.Push(MakeRecord("a", 1, 2));
        log.Push(MakeRecord("b", 3, 4));
        log.Push(MakeRecord("c", 5, 6));
        log.Push(MakeRecord("d", 7, 8));

        Assert.AreEqual(3, log.Count);

        var entries = log.GetEntries();
        Assert.AreEqual(3, entries.Count);
        Assert.AreEqual("b", entries[0].ParameterKey);
        Assert.AreEqual("c", entries[1].ParameterKey);
        Assert.AreEqual("d", entries[2].ParameterKey);
    }

    /// <summary>
    /// Tests double wrap around the circular buffer.
    /// </summary>
    [TestMethod]
    public void Push_DoubleWrap_MaintainsCorrectOrder()
    {
        var log = new ConfigChangeLog(2);
        log.Push(MakeRecord("a", 1, 2));
        log.Push(MakeRecord("b", 3, 4));
        log.Push(MakeRecord("c", 5, 6));
        log.Push(MakeRecord("d", 7, 8));
        log.Push(MakeRecord("e", 9, 10));

        Assert.AreEqual(2, log.Count);

        var entries = log.GetEntries();
        Assert.AreEqual("d", entries[0].ParameterKey);
        Assert.AreEqual("e", entries[1].ParameterKey);
    }

    /// <summary>
    /// Tests capacity of 1 — only the most recent entry is retained.
    /// </summary>
    [TestMethod]
    public void Capacity_One_RetainsOnlyMostRecent()
    {
        var log = new ConfigChangeLog(1);
        log.Push(MakeRecord("a", 1, 2));
        log.Push(MakeRecord("b", 3, 4));

        Assert.AreEqual(1, log.Count);

        var entries = log.GetEntries();
        Assert.AreEqual("b", entries[0].ParameterKey);
    }

    // --- Clear ---

    /// <summary>
    /// Tests that Clear resets the log to empty.
    /// </summary>
    [TestMethod]
    public void Clear_ResetsToEmpty()
    {
        var log = new ConfigChangeLog(4);
        log.Push(MakeRecord("a", 1, 2));
        log.Push(MakeRecord("b", 3, 4));

        log.Clear();

        Assert.AreEqual(0, log.Count);
        Assert.AreEqual(0, log.GetEntries().Count);
    }

    /// <summary>
    /// Tests that Clear on an already empty log does not throw.
    /// </summary>
    [TestMethod]
    public void Clear_WhenEmpty_DoesNotThrow()
    {
        var log = new ConfigChangeLog();
        log.Clear();
        Assert.AreEqual(0, log.Count);
    }

    /// <summary>
    /// Tests that after Clear, pushing new entries works correctly.
    /// </summary>
    [TestMethod]
    public void Push_AfterClear_WorksCorrectly()
    {
        var log = new ConfigChangeLog(3);
        log.Push(MakeRecord("a", 1, 2));
        log.Push(MakeRecord("b", 3, 4));
        log.Push(MakeRecord("c", 5, 6));
        log.Clear();

        log.Push(MakeRecord("x", 10, 20));

        Assert.AreEqual(1, log.Count);
        var entries = log.GetEntries();
        Assert.AreEqual("x", entries[0].ParameterKey);
    }

    // --- Full capacity without wrap ---

    /// <summary>
    /// Tests filling exactly to capacity without exceeding.
    /// </summary>
    [TestMethod]
    public void Push_ExactCapacity_NoWrap()
    {
        var log = new ConfigChangeLog(3);
        log.Push(MakeRecord("a", 1, 2));
        log.Push(MakeRecord("b", 3, 4));
        log.Push(MakeRecord("c", 5, 6));

        Assert.AreEqual(3, log.Count);

        var entries = log.GetEntries();
        Assert.AreEqual("a", entries[0].ParameterKey);
        Assert.AreEqual("b", entries[1].ParameterKey);
        Assert.AreEqual("c", entries[2].ParameterKey);
    }

    // --- Record values ---

    /// <summary>
    /// Tests that pushed records retain their values.
    /// </summary>
    [TestMethod]
    public void Push_RecordRetainsValues()
    {
        var log = new ConfigChangeLog();
        var record = new ConfigChangeRecord("key1", "Label One", 42, 99, 12345L);
        log.Push(record);

        var entries = log.GetEntries();
        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("key1", entries[0].ParameterKey);
        Assert.AreEqual("Label One", entries[0].DisplayLabel);
        Assert.AreEqual(42, entries[0].OldValue);
        Assert.AreEqual(99, entries[0].NewValue);
        Assert.AreEqual(12345L, entries[0].Timestamp);
    }
}
