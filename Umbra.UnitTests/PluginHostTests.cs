using Umbra.Input;
using Umbra.Logging;
using Umbra.Logging.UnitTests;
using Umbra.UI.Toast;
using Umbra.UnitTests.Input;

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

        KeyboardInput.SetKeyStateProvider(new NullKeyStateProvider());
        Logger.EnableAll();
        Logger.SetLogSink(_sink);
        ToastQueue.Clear();
        ToastOverlay.ResetDrawFrame();
        ToastOverlay.SetRenderer(null);
        ToastOverlay.SetFrameIdProvider(static () => 42L);
        PluginInstanceGuard.Reset();
        PluginRegistry.Reset();
        LifecyclePlugin.Reset();
        InitializeFailurePlugin.Reset();
        PluginHost<LifecyclePlugin>.ResetCurrent();
        PluginHost<InitializeFailurePlugin>.ResetCurrent();
    }

    /// <summary>
    /// Restores the default logging sink and clears any remaining plugin leases after each test.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        PluginInstanceGuard.Reset();
        PluginRegistry.Reset();
        PluginHost<LifecyclePlugin>.ResetCurrent();
        PluginHost<InitializeFailurePlugin>.ResetCurrent();
        ToastQueue.Clear();
        ToastOverlay.ResetDrawFrame();
        ToastOverlay.SetRenderer(null);
        ToastOverlay.SetFrameIdProvider(null);
        KeyboardInput.ResetKeyStateProvider();
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
    /// Verifies that <see cref="PluginHost{TPlugin}.Current"/> is <see langword="null"/> before
    /// <see cref="PluginHost{TPlugin}.Load"/> is called.
    /// </summary>
    [TestMethod]
    public void Current_BeforeLoad_ReturnsNull()
    {
        // Arrange
        _ = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        // Act / Assert
        Assert.IsNull(PluginHost<LifecyclePlugin>.Current);
    }

    /// <summary>
    /// Verifies that <see cref="PluginHost{TPlugin}.Current"/> exposes the live plugin instance
    /// after a successful <see cref="PluginHost{TPlugin}.Load"/>.
    /// </summary>
    [TestMethod]
    public void Current_AfterSuccessfulLoad_ReturnsLivePlugin()
    {
        // Arrange
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        // Act
        host.Load();

        // Assert
        Assert.IsNotNull(PluginHost<LifecyclePlugin>.Current);
        host.Unload();
    }

    /// <summary>
    /// Verifies that <see cref="PluginHost{TPlugin}.Current"/> is <see langword="null"/> after
    /// <see cref="PluginHost{TPlugin}.Unload"/>.
    /// </summary>
    [TestMethod]
    public void Current_AfterUnload_ReturnsNull()
    {
        // Arrange
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        // Act
        host.Load();
        host.Unload();

        // Assert
        Assert.IsNull(PluginHost<LifecyclePlugin>.Current);
    }

    /// <summary>
    /// Verifies that <see cref="PluginHost{TPlugin}.Current"/> remains <see langword="null"/>
    /// when <see cref="PluginHost{TPlugin}.Load"/> fails due to an initialization exception.
    /// </summary>
    [TestMethod]
    public void Current_WhenInitializeThrows_ReturnsNull()
    {
        // Arrange
        var host = new PluginHost<InitializeFailurePlugin>(static () => new InitializeFailurePlugin());

        // Act
        Assert.ThrowsExactly<InvalidOperationException>(() => host.Load());

        // Assert
        Assert.IsNull(PluginHost<InitializeFailurePlugin>.Current);
    }

    /// <summary>
    /// Verifies that <see cref="PluginHost{TPlugin}.Current"/> remains <see langword="null"/>
    /// when <see cref="PluginHost{TPlugin}.Load"/> returns <see langword="false"/> because the
    /// mutex is already held and becomes non-null only after the first host unloads and the
    /// second host acquires the mutex.
    /// </summary>
    [TestMethod]
    public void Current_WhenMutexBlocked_StaysFromFirstHost()
    {
        // Arrange
        var firstHost = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());
        var secondHost = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        // Act
        firstHost.Load();
        var firstCurrent = PluginHost<LifecyclePlugin>.Current;
        secondHost.Load();

        // Assert
        Assert.IsNotNull(firstCurrent);
        Assert.AreSame(firstCurrent, PluginHost<LifecyclePlugin>.Current);

        firstHost.Unload();
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
    /// Verifies that multiple hosts sharing the same tick only render the toast overlay once.
    /// </summary>
    [TestMethod]
    public void OnPreImGuiRenderer_WhenMultipleHostsDrawSameTick_RendersToastOverlayOnce()
    {
        var renderer = new TestToastRenderer();
        ToastOverlay.SetRenderer(renderer);
        var hostA = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());
        var hostB = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        hostA.Load();
        ToastQueue.Push("Shared toast");

        hostA.OnPreImGuiRenderer();
        hostA.Unload();

        hostB.Load();
        hostB.OnPreImGuiRenderer();
        hostB.Unload();

        Assert.AreEqual(1, renderer.DrawCallCount);
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

        public string PluginName => "LifecyclePlugin";
        public string? PluginVersion => "1.0.0";
        public string? PluginAuthor => "Test";

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

        public string PluginName => "InitializeFailurePlugin";
        public string? PluginVersion => null;
        public string? PluginAuthor => null;

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

    #region State transition tests

    [TestMethod]
    public void State_BeforeLoad_IsUnloaded()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        Assert.AreEqual(PluginState.Unloaded, host.State);
    }

    [TestMethod]
    public void State_AfterSuccessfulLoad_IsLoaded()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        host.Load();

        Assert.AreEqual(PluginState.Loaded, host.State);
    }

    [TestMethod]
    public void State_AfterUnload_IsUnloaded()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());
        host.Load();

        host.Unload();

        Assert.AreEqual(PluginState.Unloaded, host.State);
    }

    [TestMethod]
    public void State_WhenInitializeThrows_IsFailed()
    {
        var host = new PluginHost<InitializeFailurePlugin>(static () => new InitializeFailurePlugin());

        Assert.ThrowsExactly<InvalidOperationException>(() => host.Load());

        Assert.AreEqual(PluginState.Failed, host.State);
    }

    #endregion

    #region LastError tests

    [TestMethod]
    public void LastError_AfterFailedInit_ContainsException()
    {
        var host = new PluginHost<InitializeFailurePlugin>(static () => new InitializeFailurePlugin());

        Assert.ThrowsExactly<InvalidOperationException>(() => host.Load());

        Assert.IsNotNull(host.LastError);
        Assert.IsInstanceOfType<InvalidOperationException>(host.LastError);
    }

    [TestMethod]
    public void LastError_BeforeLoad_IsNull()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        Assert.IsNull(host.LastError);
    }

    [TestMethod]
    public void LastError_AfterSuccessfulLoad_IsNull()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        host.Load();

        Assert.IsNull(host.LastError);
    }

    #endregion

    #region LoadedAt tests

    [TestMethod]
    public void LoadedAt_AfterSuccessfulLoad_IsSet()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());
        var before = DateTimeOffset.UtcNow;

        host.Load();

        Assert.IsNotNull(host.LoadedAt);
        Assert.IsGreaterThanOrEqualTo(before, host.LoadedAt.Value);
        Assert.IsLessThanOrEqualTo(DateTimeOffset.UtcNow, host.LoadedAt.Value);
    }

    [TestMethod]
    public void LoadedAt_BeforeLoad_IsNull()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        Assert.IsNull(host.LoadedAt);
    }

    [TestMethod]
    public void LoadedAt_AfterUnload_IsNull()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());
        host.Load();

        host.Unload();

        Assert.IsNull(host.LoadedAt);
    }

    #endregion

    #region GetStatus tests

    [TestMethod]
    public void GetStatus_BeforeLoad_ReturnsUnloadedSnapshot()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        var status = host.GetStatus();

        Assert.AreEqual(nameof(LifecyclePlugin), status.Name);
        Assert.IsNull(status.Version);
        Assert.IsNull(status.Author);
        Assert.AreEqual(PluginState.Unloaded, status.State);
        Assert.IsNull(status.LastError);
        Assert.IsNull(status.LoadedAt);
    }

    [TestMethod]
    public void GetStatus_AfterLoad_ReturnsFullSnapshot()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());

        host.Load();
        var status = host.GetStatus();

        Assert.AreEqual("LifecyclePlugin", status.Name);
        Assert.AreEqual("1.0.0", status.Version);
        Assert.AreEqual("Test", status.Author);
        Assert.AreEqual(PluginState.Loaded, status.State);
        Assert.IsNull(status.LastError);
        Assert.IsNotNull(status.LoadedAt);
    }

    [TestMethod]
    public void GetStatus_AfterFailure_ReturnsFailedSnapshot()
    {
        var host = new PluginHost<InitializeFailurePlugin>(static () => new InitializeFailurePlugin());

        Assert.ThrowsExactly<InvalidOperationException>(() => host.Load());
        var status = host.GetStatus();

        Assert.AreEqual(nameof(InitializeFailurePlugin), status.Name);
        Assert.AreEqual(PluginState.Failed, status.State);
        Assert.IsNotNull(status.LastError);
    }

    [TestMethod]
    public void GetStatus_AfterUnload_ReturnsUnloadedSnapshot()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());
        host.Load();
        host.Unload();

        var status = host.GetStatus();

        Assert.AreEqual(PluginState.Unloaded, status.State);
        Assert.IsNull(status.LastError);
        Assert.IsNull(status.LoadedAt);
    }

    [TestMethod]
    public void GetStatus_MetadataFromPlugin_FlowsThrough()
    {
        var host = new PluginHost<LifecyclePlugin>(static () => new LifecyclePlugin());
        host.Load();

        var status = host.GetStatus();

        Assert.AreEqual("LifecyclePlugin", status.Name);
        Assert.AreEqual("1.0.0", status.Version);
        Assert.AreEqual("Test", status.Author);
    }

    #endregion
}
