using Umbra.Logging;
using Umbra.Logging.UnitTests;
using Umbra.Runtime;
using Umbra.Runtime.Plugin;

namespace Umbra.UnitTests.Runtime.Plugin;

/// <summary>
/// Unit tests for <see cref="PluginHost{TPlugin}"/>.
/// </summary>
[TestClass]
public sealed class PluginHostTests
{
    private TestLogSink _sink = null!;

    /// <summary>
    /// Installs an in-memory log sink and clears any active plugin leases before each test.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _sink = new TestLogSink();

        Logger.EnableAll();
        Logger.SetLogSink(_sink);
        PluginInstanceGuard.Reset();
        LifecyclePlugin.Reset();
    }

    /// <summary>
    /// Restores the default logging sink and clears any remaining plugin leases after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        PluginInstanceGuard.Reset();
        Logger.ResetLogSink();
        Logger.EnableAll();
        LifecyclePlugin.Reset();
    }

    /// <summary>
    /// Verifies that the host loads, dispatches runtime callbacks, and unloads the live plugin
    /// instance.
    /// </summary>
    [TestMethod]
    public void Load_PreUpdatePreDrawPreRendererAndUnload_DispatchToLivePluginInstance()
    {
        // Arrange
        var host = new PluginHost<LifecyclePlugin>(
            static () => new LifecyclePlugin());

        // Act
        var loaded = host.Load();
        host.OnPreUpdateBehavior();
        host.OnPreImGuiDrawUI();
        host.OnPreImGuiRenderer();
        host.Unload();

        // Assert
        Assert.IsTrue(loaded);
        Assert.AreEqual(1, LifecyclePlugin.InitializeCount);
        Assert.AreEqual(1, LifecyclePlugin.PreUpdateCount);
        Assert.AreEqual(1, LifecyclePlugin.PreDrawCount);
        Assert.AreEqual(1, LifecyclePlugin.PreRendererCount);
        Assert.AreEqual(1, LifecyclePlugin.ShutdownCount);
    }

    /// <summary>
    /// Verifies that calling <see cref="PluginHost{TPlugin}.Load"/> twice on the same host is
    /// rejected by the mutex.
    /// </summary>
    [TestMethod]
    public void Load_WhenSameHostAlreadyLoaded_ReturnsFalseAndDoesNotCreateSecondInstance()
    {
        // Arrange
        var factoryCalls = 0;
        var host = new PluginHost<LifecyclePlugin>(
            () =>
            {
                factoryCalls++;
                return new LifecyclePlugin();
            });

        // Act
        var firstLoad = host.Load();
        var secondLoad = host.Load();
        host.Unload();

        // Assert
        Assert.IsTrue(firstLoad);
        Assert.IsFalse(secondLoad);
        Assert.AreEqual(1, factoryCalls);
        Assert.AreEqual(1, LifecyclePlugin.InitializeCount);
        Assert.AreEqual(1, LifecyclePlugin.ShutdownCount);
        Assert.HasCount(1, _sink.WarningMessages);
        Assert.Contains("Skipped load for plugin", _sink.WarningMessages[0]);
    }

    /// <summary>
    /// Verifies that a second <see cref="PluginHost{TPlugin}"/> instance for the same plugin type
    /// is blocked by the mutex while the first host is loaded, and that it succeeds after the first
    /// host has unloaded.
    /// </summary>
    [TestMethod]
    public void Load_WhenSeparateHostForSamePluginTypeIsLoaded_ReturnsFalseUntilFirstUnloads()
    {
        // Arrange
        var firstHost = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());
        var secondHost = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        // Act
        var firstLoad = firstHost.Load();
        var secondLoadBlocked = secondHost.Load();
        firstHost.Unload();
        var secondLoadAfter = secondHost.Load();
        secondHost.Unload();

        // Assert
        Assert.IsTrue(firstLoad);
        Assert.IsFalse(secondLoadBlocked);
        Assert.IsTrue(secondLoadAfter);
        Assert.AreEqual(2, LifecyclePlugin.InitializeCount);
        Assert.AreEqual(2, LifecyclePlugin.ShutdownCount);
        Assert.HasCount(1, _sink.WarningMessages);
        Assert.Contains("Skipped load for plugin", _sink.WarningMessages[0]);
    }

    /// <summary>
    /// Verifies that the same host instance can be reloaded after it has been unloaded.
    /// </summary>
    [TestMethod]
    public void Load_AfterUnload_Succeeds()
    {
        // Arrange
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        // Act
        var firstLoad = host.Load();
        host.Unload();
        var secondLoad = host.Load();
        host.Unload();

        // Assert
        Assert.IsTrue(firstLoad);
        Assert.IsTrue(secondLoad);
        Assert.AreEqual(2, LifecyclePlugin.InitializeCount);
        Assert.AreEqual(2, LifecyclePlugin.ShutdownCount);
    }

    private sealed class LifecyclePlugin : IUmbraPlugin
    {
        private static readonly PluginLogger _log = new("LifecyclePlugin");

        internal static int InitializeCount;
        internal static int ShutdownCount;
        internal static int PreUpdateCount;
        internal static int PreDrawCount;
        internal static int PreRendererCount;


        public void Initialize()
        {
            InitializeCount++;
            _log.Info("Initialized.");
        }

        public void Shutdown()
        {
            ShutdownCount++;
            _log.Info("Shutdown.");
        }

        public void OnPreUpdateBehavior() => PreUpdateCount++;

        public void OnPreImGuiDrawUI() => PreDrawCount++;

        public void OnPreImGuiRenderer() => PreRendererCount++;

        internal static void Reset()
        {
            InitializeCount = 0;
            ShutdownCount = 0;
            PreUpdateCount = 0;
            PreDrawCount = 0;
            PreRendererCount = 0;
        }
    }
}
