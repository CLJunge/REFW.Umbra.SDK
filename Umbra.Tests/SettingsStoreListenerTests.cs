using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class SettingsStoreListenerTests
{
    [Fact]
    public void RemoveListenerFromAll_WithPredicate_RemovesOriginallyMatchedListeners_EvenIfPredicateStateChanges()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);
        var config = store.Load();

        var callbackCount = 0;
        var matchIntegers = true;
        Func<IParameter, bool> predicate = parameter => matchIntegers && parameter.ValueType == typeof(int);
        Action listener = () => callbackCount++;

        store.AddListenerToAll(predicate, listener);
        matchIntegers = false;

        store.RemoveListenerFromAll(predicate, listener);
        config.Count.Value = 42;

        Assert.Equal(0, callbackCount);
    }

    [Fact]
    public void RemoveListenerFromAll_Generic_RemovesOneTrackedSubscription()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);
        var config = store.Load();

        var callbackCount = 0;
        Action<int, int> listener = (_, _) => callbackCount++;

        store.AddListenerToAll<int>(listener);
        store.AddListenerToAll<int>(listener);
        store.RemoveListenerFromAll<int>(listener);

        config.Count.Value = 11;

        Assert.Equal(1, callbackCount);
    }
}
