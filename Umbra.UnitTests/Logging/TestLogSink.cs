namespace Umbra.Logging.UnitTests;

internal sealed class TestLogSink : ILogSink
{
    public void Info(string message) { }

    public void Warning(string message) { }

    public void Error(string message) { }
}
