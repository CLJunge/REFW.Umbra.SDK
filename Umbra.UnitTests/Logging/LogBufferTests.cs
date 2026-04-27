namespace Umbra.Logging.UnitTests;

[TestClass]
public sealed class LogBufferTests
{
    [TestMethod]
    public void Constructor_DefaultCapacity_Uses256()
    {
        // Arrange & Act
        var buffer = new LogBuffer();

        // Assert
        Assert.AreEqual(LogBuffer.DefaultCapacity, buffer.Capacity);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(1024)]
    public void Constructor_CustomCapacity_SetsCapacity(int capacity)
    {
        // Arrange & Act
        var buffer = new LogBuffer(capacity);

        // Assert
        Assert.AreEqual(capacity, buffer.Capacity);
    }

    [TestMethod]
    public void Constructor_ZeroCapacity_Throws()
    {
        // Arrange, Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LogBuffer(0));
    }

    [TestMethod]
    public void Constructor_NegativeCapacity_Throws()
    {
        // Arrange, Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LogBuffer(-1));
    }

    [TestMethod]
    public void Count_EmptyBuffer_ReturnsZero()
    {
        // Arrange
        var buffer = new LogBuffer();

        // Act & Assert
        Assert.AreEqual(0, buffer.Count);
    }

    [TestMethod]
    public void Add_SingleEntry_IncrementsCount()
    {
        // Arrange
        var buffer = new LogBuffer();

        // Act
        buffer.Add(LogLevel.Info, "hello");

        // Assert
        Assert.AreEqual(1, buffer.Count);
    }

    [TestMethod]
    public void Add_MultipleEntries_TracksCount()
    {
        // Arrange
        var buffer = new LogBuffer(10);

        // Act
        buffer.Add(LogLevel.Info, "one");
        buffer.Add(LogLevel.Debug, "two");
        buffer.Add(LogLevel.Warning, "three");

        // Assert
        Assert.AreEqual(3, buffer.Count);
    }

    [TestMethod]
    public void Add_ExceedsCapacity_CountCapsAtCapacity()
    {
        // Arrange
        var buffer = new LogBuffer(3);

        // Act
        buffer.Add(LogLevel.Info, "1");
        buffer.Add(LogLevel.Info, "2");
        buffer.Add(LogLevel.Info, "3");
        buffer.Add(LogLevel.Info, "4");

        // Assert
        Assert.AreEqual(3, buffer.Count);
    }

    [TestMethod]
    public void GetEntries_EmptyBuffer_AddsNothing()
    {
        // Arrange
        var buffer = new LogBuffer();
        var entries = new List<LogEntry>();

        // Act
        buffer.GetEntries(entries);

        // Assert
        Assert.AreEqual(0, entries.Count);
    }

    [TestMethod]
    public void GetEntries_ReturnsEntriesOldestFirst()
    {
        // Arrange
        var buffer = new LogBuffer(10);
        buffer.Add(LogLevel.Info, "first");
        buffer.Add(LogLevel.Warning, "second");
        buffer.Add(LogLevel.Error, "third");
        var entries = new List<LogEntry>();

        // Act
        buffer.GetEntries(entries);

        // Assert
        Assert.AreEqual(3, entries.Count);
        Assert.AreEqual("first", entries[0].Message);
        Assert.AreEqual("second", entries[1].Message);
        Assert.AreEqual("third", entries[2].Message);
    }

    [TestMethod]
    public void GetEntries_AfterWrapAround_ReturnsCorrectOrder()
    {
        // Arrange
        var buffer = new LogBuffer(3);
        buffer.Add(LogLevel.Info, "1");
        buffer.Add(LogLevel.Info, "2");
        buffer.Add(LogLevel.Info, "3");
        buffer.Add(LogLevel.Info, "4");
        buffer.Add(LogLevel.Info, "5");
        var entries = new List<LogEntry>();

        // Act
        buffer.GetEntries(entries);

        // Assert
        Assert.AreEqual(3, entries.Count);
        Assert.AreEqual("3", entries[0].Message);
        Assert.AreEqual("4", entries[1].Message);
        Assert.AreEqual("5", entries[2].Message);
    }

    [TestMethod]
    public void GetEntries_PreservesLogLevel()
    {
        // Arrange
        var buffer = new LogBuffer(10);
        buffer.Add(LogLevel.Debug, "dbg");
        buffer.Add(LogLevel.Warning, "warn");
        var entries = new List<LogEntry>();

        // Act
        buffer.GetEntries(entries);

        // Assert
        Assert.AreEqual(LogLevel.Debug, entries[0].Level);
        Assert.AreEqual(LogLevel.Warning, entries[1].Level);
    }

    [TestMethod]
    public void GetEntries_NullDestination_Throws()
    {
        // Arrange
        var buffer = new LogBuffer();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => buffer.GetEntries(null!));
    }

    [TestMethod]
    public void GetEntries_DoesNotClearExistingDestinationContents()
    {
        // Arrange
        var buffer = new LogBuffer(10);
        buffer.Add(LogLevel.Info, "new");
        var entries = new List<LogEntry>
        {
            new(LogLevel.Debug, "existing", DateTimeOffset.UtcNow)
        };

        // Act
        buffer.GetEntries(entries);

        // Assert
        Assert.AreEqual(2, entries.Count);
        Assert.AreEqual("existing", entries[0].Message);
        Assert.AreEqual("new", entries[1].Message);
    }

    [TestMethod]
    public void Clear_EmptiesBuffer()
    {
        // Arrange
        var buffer = new LogBuffer(10);
        buffer.Add(LogLevel.Info, "one");
        buffer.Add(LogLevel.Info, "two");

        // Act
        buffer.Clear();

        // Assert
        Assert.AreEqual(0, buffer.Count);
        var entries = new List<LogEntry>();
        buffer.GetEntries(entries);
        Assert.AreEqual(0, entries.Count);
    }

    [TestMethod]
    public void Clear_ThenAdd_WorksCorrectly()
    {
        // Arrange
        var buffer = new LogBuffer(3);
        buffer.Add(LogLevel.Info, "old1");
        buffer.Add(LogLevel.Info, "old2");
        buffer.Clear();

        // Act
        buffer.Add(LogLevel.Warning, "new1");
        var entries = new List<LogEntry>();
        buffer.GetEntries(entries);

        // Assert
        Assert.AreEqual(1, buffer.Count);
        Assert.AreEqual("new1", entries[0].Message);
        Assert.AreEqual(LogLevel.Warning, entries[0].Level);
    }

    [TestMethod]
    public void Add_SetsTimestamp()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;
        var buffer = new LogBuffer(10);

        // Act
        buffer.Add(LogLevel.Info, "timestamped");
        var after = DateTimeOffset.UtcNow;

        // Assert
        var entries = new List<LogEntry>();
        buffer.GetEntries(entries);
        Assert.IsTrue(entries[0].Timestamp >= before);
        Assert.IsTrue(entries[0].Timestamp <= after);
    }

    [TestMethod]
    public void Capacity1_WrapAround_RetainsOnlyLatest()
    {
        // Arrange
        var buffer = new LogBuffer(1);

        // Act
        buffer.Add(LogLevel.Info, "first");
        buffer.Add(LogLevel.Error, "second");
        var entries = new List<LogEntry>();
        buffer.GetEntries(entries);

        // Assert
        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("second", entries[0].Message);
        Assert.AreEqual(LogLevel.Error, entries[0].Level);
    }

    [TestMethod]
    public void ConcurrentAccess_DoesNotCorrupt()
    {
        // Arrange
        var buffer = new LogBuffer(64);
        var barrier = new Barrier(4);

        // Act
        var tasks = new Task[4];
        for (int t = 0; t < 4; t++)
        {
            int threadId = t;
            tasks[t] = Task.Run(() =>
            {
                barrier.SignalAndWait();
                for (int i = 0; i < 100; i++)
                    buffer.Add(LogLevel.Info, $"t{threadId}-{i}");
            });
        }
        Task.WaitAll(tasks);

        // Assert
        Assert.AreEqual(64, buffer.Count);
        var entries = new List<LogEntry>();
        buffer.GetEntries(entries);
        Assert.AreEqual(64, entries.Count);
    }
}
