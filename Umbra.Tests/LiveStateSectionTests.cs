using Umbra.Tests.TestConfigs;
using Umbra.UI.LiveState;
using Xunit;

namespace Umbra.Tests;

public sealed class LiveStateSectionTests
{
    [Fact]
    public void Constructor_Throws_WhenContextIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new LiveStateSection<BasicLiveState>((BasicLiveState)null!));

        Assert.Equal("context", exception.ParamName);
    }

    [Fact]
    public void Constructor_Throws_WhenIdScopeIsWhitespace()
    {
        var state = new BasicLiveState();

        var exception = Assert.Throws<ArgumentException>(() => new LiveStateSection<BasicLiveState>(state, "   "));

        Assert.Equal("idScope", exception.ParamName);
    }
}
