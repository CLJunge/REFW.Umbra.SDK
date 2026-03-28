using Umbra.Config;
using Umbra.Tests.TestConfigs;
using Umbra.Tests.TestSupport;
using Xunit;

namespace Umbra.Tests;

public sealed class DeferredSaveControllerTests
{
    [Fact]
    public void Constructor_Throws_WhenStoreHasNotBeenLoaded()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        using var store = new SettingsStore<BasicConfig>(filePath);

        Assert.Throws<InvalidOperationException>(() => new DeferredSaveController<BasicConfig>(store));
    }

    [Fact]
    public void Constructor_Throws_WhenStoreHasAlreadyBeenDisposed()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        var store = new SettingsStore<BasicConfig>(filePath);
        store.Dispose();

        var exception = Assert.Throws<ObjectDisposedException>(() => new DeferredSaveController<BasicConfig>(store));
        Assert.Equal("store", exception.ObjectName);
    }

    [Fact]
    public void Dispose_DoesNotThrow_WhenStoreIsDisposedFirst()
    {
        using var temp = new TemporaryDirectory();
        using var _ = new LoggerTestScope();
        var filePath = Path.Combine(temp.Path, "config.json");
        var store = new SettingsStore<BasicConfig>(filePath);
        store.Load();
        var controller = new DeferredSaveController<BasicConfig>(store);

        store.Dispose();
        var exception = Record.Exception(controller.Dispose);

        Assert.Null(exception);
    }
}
