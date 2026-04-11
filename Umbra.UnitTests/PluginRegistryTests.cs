namespace Umbra.UnitTests;

[TestClass]
public sealed class PluginRegistryTests
{
    [TestInitialize]
    public void TestInitialize()
    {
        PluginRegistry.Reset();
        PluginInstanceGuard.Reset();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        PluginRegistry.Reset();
        PluginInstanceGuard.Reset();
    }

    [TestMethod]
    public void Register_AddsProvider()
    {
        var provider = new StubStatusProvider(new PluginStatus("TestPlugin", "1.0", "Author", PluginState.Loaded, null, null));

        PluginRegistry.Register(provider);

        Assert.AreEqual(1, PluginRegistry.Count);
    }

    [TestMethod]
    public void Deregister_RemovesProvider()
    {
        var provider = new StubStatusProvider(new PluginStatus("TestPlugin", "1.0", "Author", PluginState.Loaded, null, null));
        PluginRegistry.Register(provider);

        PluginRegistry.Deregister(provider);

        Assert.AreEqual(0, PluginRegistry.Count);
    }

    [TestMethod]
    public void Deregister_UnknownProvider_DoesNothing()
    {
        var registered = new StubStatusProvider(new PluginStatus("A", null, null, PluginState.Loaded, null, null));
        var unknown = new StubStatusProvider(new PluginStatus("B", null, null, PluginState.Loaded, null, null));
        PluginRegistry.Register(registered);

        PluginRegistry.Deregister(unknown);

        Assert.AreEqual(1, PluginRegistry.Count);
    }

    [TestMethod]
    public void GetAll_ReturnsAllStatuses()
    {
        var providerA = new StubStatusProvider(new PluginStatus("A", "1.0", null, PluginState.Loaded, null, DateTimeOffset.UtcNow));
        var providerB = new StubStatusProvider(new PluginStatus("B", "2.0", "Author", PluginState.Unloaded, null, null));
        PluginRegistry.Register(providerA);
        PluginRegistry.Register(providerB);
        var results = new List<PluginStatus>();

        PluginRegistry.GetAll(results);

        Assert.HasCount(2, results);
        Assert.AreEqual("A", results[0].Name);
        Assert.AreEqual("B", results[1].Name);
    }

    [TestMethod]
    public void GetAll_AppendsToExistingList()
    {
        var provider = new StubStatusProvider(new PluginStatus("X", null, null, PluginState.Loaded, null, null));
        PluginRegistry.Register(provider);
        var results = new List<PluginStatus>
        {
            new("Existing", null, null, PluginState.Unloaded, null, null)
        };

        PluginRegistry.GetAll(results);

        Assert.HasCount(2, results);
        Assert.AreEqual("Existing", results[0].Name);
        Assert.AreEqual("X", results[1].Name);
    }

    [TestMethod]
    public void Reset_ClearsAll()
    {
        var provider = new StubStatusProvider(new PluginStatus("TestPlugin", null, null, PluginState.Loaded, null, null));
        PluginRegistry.Register(provider);

        PluginRegistry.Reset();

        Assert.AreEqual(0, PluginRegistry.Count);
    }

    [TestMethod]
    public void Register_NullProvider_Throws() => Assert.ThrowsExactly<ArgumentNullException>(() => PluginRegistry.Register(null!));

    [TestMethod]
    public void Deregister_NullProvider_Throws() => Assert.ThrowsExactly<ArgumentNullException>(() => PluginRegistry.Deregister(null!));

    [TestMethod]
    public void GetAll_NullDestination_Throws() => Assert.ThrowsExactly<ArgumentNullException>(() => PluginRegistry.GetAll(null!));

    private sealed class StubStatusProvider : IPluginStatusProvider
    {
        private readonly PluginStatus _status;

        internal StubStatusProvider(PluginStatus status)
        {
            _status = status;
        }

        public PluginStatus GetStatus() => _status;
    }
}
