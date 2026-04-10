using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.UnitTests;

/// <summary>
/// Records draw invocations for <see cref="ConfigDrawer{TConfig}"/> unit tests.
/// </summary>
internal sealed class TestDrawNode(Action? onDraw = null) : IDrawNode
{
    private readonly Action? _onDraw = onDraw;

    public int DrawCount { get; private set; }

    public void Draw()
    {
        DrawCount++;
        _onDraw?.Invoke();
    }
}
