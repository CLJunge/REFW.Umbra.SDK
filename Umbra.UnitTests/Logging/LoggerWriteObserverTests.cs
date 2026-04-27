namespace Umbra.Logging.UnitTests;

[TestClass]
public sealed class LoggerWriteObserverTests
{
    private TestLogSink _sink = null!;

    [TestInitialize]
    public void Setup()
    {
        _sink = new TestLogSink();
        Logger.SetLogSink(_sink);
        Logger.Enabled = true;
        Logger.WriteObserver = null;
    }

    [TestCleanup]
    public void Cleanup()
    {
        Logger.WriteObserver = null;
        Logger.SuppressedFailureObserver = null;
        Logger.Enabled = true;
        Logger.ResetLogSink();
    }

    [TestMethod]
    public void WriteObserver_LoggerDebug_ReceivesDebugLevelAndMessage()
    {
        // Arrange
        LogLevel? captured = null;
        string? capturedMsg = null;
        Logger.WriteObserver = (level, msg) => { captured = level; capturedMsg = msg; };

        // Act
        Logger.Debug("test debug");

        // Assert
        Assert.AreEqual(LogLevel.Debug, captured);
        Assert.AreEqual("test debug", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_LoggerInfo_ReceivesInfoLevelAndMessage()
    {
        // Arrange
        LogLevel? captured = null;
        string? capturedMsg = null;
        Logger.WriteObserver = (level, msg) => { captured = level; capturedMsg = msg; };

        // Act
        Logger.Info("test info");

        // Assert
        Assert.AreEqual(LogLevel.Info, captured);
        Assert.AreEqual("test info", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_LoggerWarning_ReceivesWarningLevelAndMessage()
    {
        // Arrange
        LogLevel? captured = null;
        string? capturedMsg = null;
        Logger.WriteObserver = (level, msg) => { captured = level; capturedMsg = msg; };

        // Act
        Logger.Warning("test warning");

        // Assert
        Assert.AreEqual(LogLevel.Warning, captured);
        Assert.AreEqual("test warning", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_LoggerError_ReceivesErrorLevelAndMessage()
    {
        // Arrange
        LogLevel? captured = null;
        string? capturedMsg = null;
        Logger.WriteObserver = (level, msg) => { captured = level; capturedMsg = msg; };

        // Act
        Logger.Error("test error");

        // Assert
        Assert.AreEqual(LogLevel.Error, captured);
        Assert.AreEqual("test error", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_LoggerException_ReceivesErrorLevelWithFormattedMessage()
    {
        // Arrange
        LogLevel? captured = null;
        string? capturedMsg = null;
        Logger.WriteObserver = (level, msg) => { captured = level; capturedMsg = msg; };
        var ex = new InvalidOperationException("boom");

        // Act
        Logger.Exception(ex, "context");

        // Assert
        Assert.AreEqual(LogLevel.Error, captured);
        Assert.IsNotNull(capturedMsg);
        Assert.Contains("context", capturedMsg);
        Assert.Contains("InvalidOperationException", capturedMsg);
        Assert.Contains("boom", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_PluginLoggerDebug_ReceivesFormattedMessage()
    {
        // Arrange
        var log = new PluginLogger("Test") { MinLevel = LogLevel.Debug };
        string? capturedMsg = null;
        Logger.WriteObserver = (_, msg) => capturedMsg = msg;

        // Act
        log.Debug("hello");

        // Assert
        Assert.IsNotNull(capturedMsg);
        Assert.Contains("[Test]", capturedMsg);
        Assert.Contains("hello", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_PluginLoggerInfo_ReceivesFormattedMessage()
    {
        // Arrange
        var log = new PluginLogger("PL");
        string? capturedMsg = null;
        Logger.WriteObserver = (_, msg) => capturedMsg = msg;

        // Act
        log.Info("info msg");

        // Assert
        Assert.IsNotNull(capturedMsg);
        Assert.Contains("[PL]", capturedMsg);
        Assert.Contains("info msg", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_PluginLoggerWarning_ReceivesWarningLevel()
    {
        // Arrange
        var log = new PluginLogger("W");
        LogLevel? captured = null;
        Logger.WriteObserver = (level, _) => captured = level;

        // Act
        log.Warning("warn");

        // Assert
        Assert.AreEqual(LogLevel.Warning, captured);
    }

    [TestMethod]
    public void WriteObserver_PluginLoggerError_ReceivesErrorLevel()
    {
        // Arrange
        var log = new PluginLogger("E");
        LogLevel? captured = null;
        Logger.WriteObserver = (level, _) => captured = level;

        // Act
        log.Error("err");

        // Assert
        Assert.AreEqual(LogLevel.Error, captured);
    }

    [TestMethod]
    public void WriteObserver_PluginLoggerException_ReceivesErrorLevel()
    {
        // Arrange
        var log = new PluginLogger("EX");
        LogLevel? captured = null;
        string? capturedMsg = null;
        Logger.WriteObserver = (level, msg) => { captured = level; capturedMsg = msg; };

        // Act
        log.Exception(new InvalidOperationException("fail"), "ctx");

        // Assert
        Assert.AreEqual(LogLevel.Error, captured);
        Assert.IsNotNull(capturedMsg);
        Assert.Contains("[EX]", capturedMsg);
        Assert.Contains("ctx", capturedMsg);
        Assert.Contains("fail", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_NullObserver_NoExceptionThrown()
    {
        // Arrange
        Logger.WriteObserver = null;

        // Act & Assert (no exception)
        Logger.Info("safe");
        Assert.HasCount(1, _sink.InfoMessages);
    }

    [TestMethod]
    public void WriteObserver_ObserverThrows_ExceptionSwallowed()
    {
        // Arrange
        Logger.WriteObserver = (_, _) => throw new InvalidOperationException("observer crash");

        // Act (should not throw)
        Logger.Info("still works");

        // Assert
        Assert.HasCount(1, _sink.InfoMessages);
    }

    [TestMethod]
    public void WriteObserver_LoggingDisabled_ObserverNotCalled()
    {
        // Arrange
        var called = false;
        Logger.WriteObserver = (_, _) => called = true;
        Logger.Enabled = false;

        // Act
        Logger.Info("suppressed");

        // Assert
        Assert.IsFalse(called);
    }

    [TestMethod]
    public void WriteObserver_LoggingSuppressed_ObserverNotCalled()
    {
        // Arrange
        var called = false;
        Logger.WriteObserver = (_, _) => called = true;

        // Act
        using (Logger.Suppress())
        {
            Logger.Info("suppressed");
        }

        // Assert
        Assert.IsFalse(called);
    }

    [TestMethod]
    public void WriteObserver_PluginLoggerBelowMinLevel_ObserverNotCalled()
    {
        // Arrange
        var log = new PluginLogger { MinLevel = LogLevel.Warning };
        var called = false;
        Logger.WriteObserver = (_, _) => called = true;

        // Act
        log.Info("filtered out");

        // Assert
        Assert.IsFalse(called);
    }

    [TestMethod]
    public void WriteObserver_LoggerFormatOverload_ObserverReceivesFormattedMessage()
    {
        // Arrange
        string? capturedMsg = null;
        Logger.WriteObserver = (_, msg) => capturedMsg = msg;

        // Act
        Logger.Info("hello {0}", "world");

        // Assert
        Assert.AreEqual("hello world", capturedMsg);
    }

    [TestMethod]
    public void WriteObserver_IntegrationWithLogBuffer_CapturesEntries()
    {
        // Arrange
        var buffer = new LogBuffer(16);
        Logger.WriteObserver = (level, msg) => buffer.Add(level, msg);

        // Act
        Logger.Debug("d");
        Logger.Info("i");
        Logger.Warning("w");
        Logger.Error("e");

        // Assert
        var entries = new List<LogEntry>();
        buffer.GetEntries(entries);
        Assert.HasCount(4, entries);
        Assert.AreEqual(LogLevel.Debug, entries[0].Level);
        Assert.AreEqual("d", entries[0].Message);
        Assert.AreEqual(LogLevel.Info, entries[1].Level);
        Assert.AreEqual(LogLevel.Warning, entries[2].Level);
        Assert.AreEqual(LogLevel.Error, entries[3].Level);
    }
}
