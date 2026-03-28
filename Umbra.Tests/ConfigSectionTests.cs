using Umbra.Tests.TestConfigs;
using Umbra.UI.Config;
using Xunit;

namespace Umbra.Tests;

public sealed class ConfigSectionTests
{
    [Fact]
    public void Constructor_Throws_WhenConfigIsNull()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ConfigSection<BasicConfig>(null!));

        Assert.Equal("config", exception.ParamName);
    }

    [Fact]
    public void Constructor_Throws_WhenIdScopeIsWhitespace()
    {
        var config = new BasicConfig();

        var exception = Assert.Throws<ArgumentException>(() => new ConfigSection<BasicConfig>(config, "   "));

        Assert.Equal("idScope", exception.ParamName);
    }
}
