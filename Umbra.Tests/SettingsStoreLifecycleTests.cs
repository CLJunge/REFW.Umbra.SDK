using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class SettingsStoreLifecycleTests
{
    [Fact]
    public void Save_Throws_WhenStoreHasNotBeenLoaded()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);

        var exception = Assert.Throws<InvalidOperationException>(() => store.Save());

        Assert.Contains("requires Load()", exception.Message);
    }

    [Fact]
    public void AddListenerToAll_Throws_WhenStoreHasNotBeenLoaded()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);

        var exception = Assert.Throws<InvalidOperationException>(() => store.AddListenerToAll(static () => { }));

        Assert.Contains("requires Load()", exception.Message);
    }

    [Fact]
    public void RemoveListenerFromAll_Throws_WhenStoreHasNotBeenLoaded()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);

        var exception = Assert.Throws<InvalidOperationException>(() => store.RemoveListenerFromAll(static () => { }));

        Assert.Contains("requires Load()", exception.Message);
    }

    [Fact]
    public void ResetAll_Throws_WhenStoreHasNotBeenLoaded()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);

        var exception = Assert.Throws<InvalidOperationException>(() => store.ResetAll());

        Assert.Contains("requires Load()", exception.Message);
    }
}
