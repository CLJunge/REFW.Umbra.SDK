using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Records draw invocations for <see cref="ConfigDrawer{TConfig}"/> unit tests.
/// </summary>
internal sealed class TestDrawNode : IDrawNode
{
    private readonly Action? _onDraw;

    public int DrawCount { get; private set; }

    public TestDrawNode(Action? onDraw = null)
    {
        _onDraw = onDraw;
    }

    public void Draw()
    {
        DrawCount++;
        _onDraw?.Invoke();
    }
}
