namespace Umbra.UI.Config.Nodes.UnitTests;

/// <summary>
/// Records <see cref="IdScopeNode"/> push/pop operations for unit tests.
/// </summary>
internal sealed class TestIdScopeNodeRenderer : IIdScopeNodeRenderer
{
    public List<string> PushedIds { get; } = [];
    public int PopCount { get; private set; }

    public void PushId(string scopeId)
    {
        PushedIds.Add(scopeId);
    }

    public void PopId()
    {
        PopCount++;
    }
}
