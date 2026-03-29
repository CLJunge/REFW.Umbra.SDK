namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Records <see cref="ConfigDrawer{TConfig}"/> scope operations for unit tests.
/// </summary>
internal sealed class TestConfigDrawerScope : IConfigDrawerScope
{
    public List<string> PushedIds { get; } = [];
    public int PopCount { get; private set; }

    public void PushId(string idScope) => PushedIds.Add(idScope);

    public void PopId() => PopCount++;
}
