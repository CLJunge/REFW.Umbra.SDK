using Umbra.UI.LiveState;

namespace Umbra.Tests.TestConfigs;

[LiveStateSectionDrawer<BasicLiveStateDrawer>]
internal sealed class BasicLiveState
{
    internal int Value { get; set; }
}

internal sealed class BasicLiveStateDrawer : ILiveStateSectionDrawer<BasicLiveState>
{
    public void Draw(BasicLiveState state)
    {
    }
}
