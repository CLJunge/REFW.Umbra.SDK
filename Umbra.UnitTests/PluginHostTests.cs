using Umbra.Logging;
using Umbra.Logging.UnitTests;
using Umbra.Input;
using Umbra.UnitTests.Input;
using Umbra.UI.Toast;

namespace Umbra.UnitTests;

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
        ToastQueue.Clear();
        ToastOverlay.SetRenderer(null);
        PluginInstanceGuard.Reset();
        LifecyclePlugin.Reset();
        InitializeFailurePlugin.Reset();
    }

    /// <summary>
    /// Restores the default logging sink and clears any remaining plugin leases after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        PluginInstanceGuard.Reset();
        ToastQueue.Clear();
        ToastOverlay.SetRenderer(null);
        Logger.ResetLogSink();
        Logger.EnableAll();
        LifecyclePlugin.Reset();
        InitializeFailurePlugin.Reset();
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
    /// Verifies that the host renders queued toast notifications internally during the ImGui renderer callback.
    /// </summary>
    [TestMethod]
    public void OnPreImGuiRenderer_WhenToastIsQueued_RendersToastOverlayInternally()
    {
        var renderer = new TestToastRenderer();
        ToastOverlay.SetRenderer(renderer);
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        host.Load();
        ToastQueue.Push("Undo: Value");

        host.OnPreImGuiRenderer();
        host.Unload();

        Assert.AreEqual(1, LifecyclePlugin.PreRendererCount);
        Assert.AreEqual(1, renderer.DrawCallCount);
        Assert.HasCount(1, renderer.LastEntries);
        Assert.AreEqual("Undo: Value", renderer.LastEntries[0].Message);
        Assert.AreEqual(1, renderer.PreRendererCountAtRender);
    }

    /// <summary>
    /// Verifies that the host's UI pre-draw callback remains a no-op when no plugin instance is loaded.
    /// </summary>
    [TestMethod]
    public void OnPreImGuiDrawUI_WhenNoInstanceIsLoaded_DoesNotRenderToastOverlay()
    {
        var renderer = new TestToastRenderer();
        ToastOverlay.SetRenderer(renderer);
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        ToastQueue.Push("Queued");
        host.OnPreImGuiDrawUI();

        Assert.AreEqual(0, renderer.DrawCallCount);
        Assert.AreEqual(0, LifecyclePlugin.PreDrawCount);
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

    /// <summary>
    /// Verifies that a failed initialize path runs best-effort shutdown cleanup and releases the
    /// mutex so the same host can load successfully later.
    /// </summary>
    [TestMethod]
    public void Load_WhenInitializeThrows_CallsShutdownAndAllowsReload()
    {
        // Arrange
        var host = new PluginHost<InitializeFailurePlugin>(static () => new InitializeFailurePlugin());

        // Act
        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => host.Load());
        InitializeFailurePlugin.ThrowOnInitialize = false;
        var reloaded = host.Load();
        host.Unload();

        // Assert
        Assert.AreEqual("initialize failed", exception.Message);
        Assert.IsTrue(reloaded);
        Assert.AreEqual(2, InitializeFailurePlugin.InitializeCount);
        Assert.AreEqual(2, InitializeFailurePlugin.ShutdownCount);
    }

    /// <summary>
    /// Verifies that initialize cleanup failure does not replace the original initialize exception.
    /// </summary>
    [TestMethod]
    public void Load_WhenInitializeAndCleanupThrow_RethrowsInitializeException()
    {
        // Arrange
        InitializeFailurePlugin.ThrowOnShutdown = true;
        var host = new PluginHost<InitializeFailurePlugin>(static () => new InitializeFailurePlugin());

        // Act
        var exception = Assert.ThrowsExactly<InvalidOperationException>(() => host.Load());

        // Assert
        Assert.AreEqual("initialize failed", exception.Message);
        Assert.AreEqual(1, InitializeFailurePlugin.InitializeCount);
        Assert.AreEqual(1, InitializeFailurePlugin.ShutdownCount);
        Assert.HasCount(1, _sink.ErrorMessages);
        Assert.Contains("Shutdown() threw while cleaning up a failed Initialize() call", _sink.ErrorMessages[0]);
        Assert.Contains("Original Initialize() exception: InvalidOperationException: initialize failed", _sink.ErrorMessages[0]);
    }

    /// <summary>
    /// Verifies that repeated failed loads do not publish a live instance, so forwarded callbacks
    /// remain safe no-ops.
    /// </summary>
    [TestMethod]
    public void Load_WhenInitializeThrows_ForwardedCallbacksRemainNoOps()
    {
        // Arrange
        var host = new PluginHost<InitializeFailurePlugin>(static () => new InitializeFailurePlugin());

        // Act
        Assert.ThrowsExactly<InvalidOperationException>(() => host.Load());
        host.OnPreUpdateBehavior();
        host.OnPreImGuiDrawUI();
        host.OnPreImGuiRenderer();

        // Assert
        Assert.AreEqual(0, InitializeFailurePlugin.PreUpdateCount);
        Assert.AreEqual(0, InitializeFailurePlugin.PreDrawCount);
        Assert.AreEqual(0, InitializeFailurePlugin.PreRendererCount);
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

    private sealed class InitializeFailurePlugin : IUmbraPlugin
    {
        internal static int InitializeCount;
        internal static int ShutdownCount;
        internal static int PreUpdateCount;
        internal static int PreDrawCount;
        internal static int PreRendererCount;
        internal static bool ThrowOnInitialize = true;
        internal static bool ThrowOnShutdown;

        public void Initialize()
        {
            InitializeCount++;

            if (ThrowOnInitialize)
                throw new InvalidOperationException("initialize failed");
        }

        public void Shutdown()
        {
            ShutdownCount++;

            if (ThrowOnShutdown)
                throw new InvalidOperationException("shutdown failed");
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
            ThrowOnInitialize = true;
            ThrowOnShutdown = false;
        }
    }

    private sealed class TestToastRenderer : IToastRenderer
    {
        internal int DrawCallCount { get; private set; }

        internal int PreRendererCountAtRender { get; private set; }

        internal List<ToastEntry> LastEntries { get; } = [];

        public void Draw(List<ToastEntry> entries)
        {
            DrawCallCount++;
            PreRendererCountAtRender = LifecyclePlugin.PreRendererCount;
            LastEntries.Clear();
            LastEntries.AddRange(entries);
        }
    }
}
