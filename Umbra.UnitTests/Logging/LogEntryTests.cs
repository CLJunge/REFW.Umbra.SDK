namespace Umbra.Logging.UnitTests;

[TestClass]
public sealed class LogEntryTests
{
    [TestMethod]
    public void Constructor_SetsAllProperties()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;

        // Act
        var entry = new LogEntry(LogLevel.Warning, "test message", timestamp);

        // Assert
        Assert.AreEqual(LogLevel.Warning, entry.Level);
        Assert.AreEqual("test message", entry.Message);
        Assert.AreEqual(timestamp, entry.Timestamp);
    }

    [TestMethod]
    [DataRow(LogLevel.Debug)]
    [DataRow(LogLevel.Info)]
    [DataRow(LogLevel.Warning)]
    [DataRow(LogLevel.Error)]
    public void Constructor_AcceptsAllLogLevels(LogLevel level)
    {
        // Arrange & Act
        var entry = new LogEntry(level, "msg", DateTimeOffset.UtcNow);

        // Assert
        Assert.AreEqual(level, entry.Level);
    }

    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var a = new LogEntry(LogLevel.Info, "hello", timestamp);
        var b = new LogEntry(LogLevel.Info, "hello", timestamp);

        // Act & Assert
        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b);
    }

    [TestMethod]
    public void Equality_DifferentLevel_AreNotEqual()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var a = new LogEntry(LogLevel.Info, "hello", timestamp);
        var b = new LogEntry(LogLevel.Error, "hello", timestamp);

        // Act & Assert
        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void Equality_DifferentMessage_AreNotEqual()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var a = new LogEntry(LogLevel.Info, "hello", timestamp);
        var b = new LogEntry(LogLevel.Info, "world", timestamp);

        // Act & Assert
        Assert.AreNotEqual(a, b);
    }

    [TestMethod]
    public void ToString_ContainsAllFields()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2025, 6, 15, 12, 30, 0, TimeSpan.Zero);
        var entry = new LogEntry(LogLevel.Warning, "test", timestamp);

        // Act
        var result = entry.ToString();

        // Assert
        Assert.IsTrue(result.Contains("Warning"));
        Assert.IsTrue(result.Contains("test"));
    }

    [TestMethod]
    public void IsReadonly_CannotBeModified()
    {
        // Arrange & Act
        var entry = new LogEntry(LogLevel.Debug, "immutable", DateTimeOffset.UtcNow);

        // Assert
        Assert.AreEqual("immutable", entry.Message);
        Assert.AreEqual(LogLevel.Debug, entry.Level);
    }
}
