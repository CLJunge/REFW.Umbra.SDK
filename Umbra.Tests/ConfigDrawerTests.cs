using Umbra.Tests.TestConfigs;
using Umbra.UI.Config;
using Xunit;

namespace Umbra.Tests;

public sealed class ConfigDrawerTests
{
    [Fact]
    public void Constructor_Throws_WhenConfigIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ConfigDrawer<BasicConfig>(null!, "TestScope"));

        Assert.Equal("config", exception.ParamName);
    }

    [Fact]
    public void Constructor_Throws_WhenIdScopeIsWhitespace()
    {
        var exception = Assert.Throws<ArgumentException>(() => new ConfigDrawer<BasicConfig>(new BasicConfig(), "   "));

        Assert.Equal("idScope", exception.ParamName);
    }
}
