namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Records disposal calls for <see cref="ConfigDrawer{TConfig}"/> unit tests.
/// </summary>
internal sealed class TestDisposable : IDisposable
{
    public int DisposeCount { get; private set; }

    public void Dispose()
    {
        DisposeCount++;
    }
}
