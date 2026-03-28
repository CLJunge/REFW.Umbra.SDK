using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class SettingsStoreNullArgumentTests
{
    [Fact]
    public void AddListenerToAll_Throws_WhenListenerIsNull()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);
        store.Load();

        var exception = Assert.Throws<ArgumentNullException>(() => store.AddListenerToAll(null!));

        Assert.Equal("listener", exception.ParamName);
    }

    [Fact]
    public void AddListenerToAll_Generic_Throws_WhenListenerIsNull()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);
        store.Load();

        var exception = Assert.Throws<ArgumentNullException>(() => store.AddListenerToAll<int>(null!));

        Assert.Equal("listener", exception.ParamName);
    }

    [Fact]
    public void AddListenerToAll_WithPredicate_Throws_WhenPredicateIsNull()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);
        store.Load();

        var exception = Assert.Throws<ArgumentNullException>(() => store.AddListenerToAll(null!, static () => { }));

        Assert.Equal("predicate", exception.ParamName);
    }

    [Fact]
    public void RemoveListenerFromAll_WithPredicate_Throws_WhenListenerIsNull()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);
        store.Load();

        var exception = Assert.Throws<ArgumentNullException>(() =>
            store.RemoveListenerFromAll(static _ => true, null!));

        Assert.Equal("listener", exception.ParamName);
    }
}
