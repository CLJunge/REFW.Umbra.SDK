using Umbra.Logging;

namespace Umbra.Tests.TestSupport;

internal sealed class LoggerTestScope : IDisposable
{
    private readonly bool _wasEnabled;

    internal LoggerTestScope(bool enabled = false)
    {
        _wasEnabled = Logger.Enabled;
        Logger.Enabled = enabled;
    }

    public void Dispose() => Logger.Enabled = _wasEnabled;
}
