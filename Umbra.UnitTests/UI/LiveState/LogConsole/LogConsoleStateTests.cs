using Umbra.Logging;

namespace Umbra.UI.LiveState.LogConsole.UnitTests;

[TestClass]
public sealed class LogConsoleStateTests
{
    [TestMethod]
    public void Constructor_Default_CreatesBufferWithDefaultCapacity()
    {
        // Arrange & Act
        var state = new LogConsoleState();

        // Assert
        Assert.IsNotNull(state.Buffer);
        Assert.AreEqual(LogConsoleState.DefaultBufferCapacity, state.Buffer.Capacity);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(64)]
    [DataRow(1024)]
    public void Constructor_CustomCapacity_CreatesBufferWithSpecifiedCapacity(int capacity)
    {
        // Arrange & Act
        var state = new LogConsoleState(capacity);

        // Assert
        Assert.AreEqual(capacity, state.Buffer.Capacity);
    }

    [TestMethod]
    public void Constructor_ZeroCapacity_Throws()
    {
        // Arrange, Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new LogConsoleState(0));
    }

    [TestMethod]
    public void Constructor_ExternalBuffer_UsesProvidedBuffer()
    {
        // Arrange
        var buffer = new LogBuffer(32);

        // Act
        var state = new LogConsoleState(buffer);

        // Assert
        Assert.AreSame(buffer, state.Buffer);
    }

    [TestMethod]
    public void Constructor_NullBuffer_Throws()
    {
        // Arrange, Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new LogConsoleState((LogBuffer)null!));
    }

    [TestMethod]
    public void MinDisplayLevel_Default_IsDebug()
    {
        // Arrange & Act
        var state = new LogConsoleState();

        // Assert
        Assert.AreEqual(LogLevel.Debug, state.MinDisplayLevel);
    }

    [TestMethod]
    [DataRow(LogLevel.Debug)]
    [DataRow(LogLevel.Info)]
    [DataRow(LogLevel.Warning)]
    [DataRow(LogLevel.Error)]
    public void MinDisplayLevel_SetAndGet_RoundTrips(LogLevel level)
    {
        // Arrange
        var state = new LogConsoleState();

        // Act
        state.MinDisplayLevel = level;

        // Assert
        Assert.AreEqual(level, state.MinDisplayLevel);
    }

    [TestMethod]
    public void AutoScroll_Default_IsTrue()
    {
        // Arrange & Act
        var state = new LogConsoleState();

        // Assert
        Assert.IsTrue(state.AutoScroll);
    }

    [TestMethod]
    public void AutoScroll_SetFalse_ReturnsFalse()
    {
        // Arrange
        var state = new LogConsoleState();

        // Act
        state.AutoScroll = false;

        // Assert
        Assert.IsFalse(state.AutoScroll);
    }

    [TestMethod]
    public void Buffer_AcceptsEntriesAfterConstruction()
    {
        // Arrange
        var state = new LogConsoleState(16);

        // Act
        state.Buffer.Add(LogLevel.Info, "test message");

        // Assert
        Assert.AreEqual(1, state.Buffer.Count);
        var entries = new List<LogEntry>();
        state.Buffer.GetEntries(entries);
        Assert.AreEqual("test message", entries[0].Message);
    }

    [TestMethod]
    public void DefaultBufferCapacity_Is512()
    {
        // Assert
        Assert.AreEqual(512, LogConsoleState.DefaultBufferCapacity);
    }
}
