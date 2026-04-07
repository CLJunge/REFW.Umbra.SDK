using Umbra.Config;
using Umbra.Config.Attributes;

namespace Umbra.UI.ChangeMonitor.UnitTests;

/// <summary>
/// Unit tests for <see cref="ParameterChangeMonitorState"/>.
/// </summary>
[TestClass]
public sealed class ParameterChangeMonitorStateTests
{
    /// <summary>
    /// Test configuration class for change monitor state tests.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class MonitorTestConfig
    {
        [UmbraParameter]
        public Parameter<int> IntValue { get; set; } = new(10);

        [UmbraParameter]
        public Parameter<string> StringValue { get; set; } = new("default");
    }

    private static (ConfigStore<T> Store, T Config, string TempPath) CreateLoadedStore<T>()
        where T : class, new()
    {
        var tempPath = Path.GetTempFileName();
        var store = new ConfigStore<T>(tempPath);
        var config = store.Load();
        return (store, config, tempPath);
    }

    private static void CleanupStore<T>(ConfigStore<T> store, string tempPath)
        where T : class, new()
    {
        store.Dispose();
        if (File.Exists(tempPath))
            File.Delete(tempPath);
    }

    // --- Create (default overload) ---

    /// <summary>
    /// Tests that Create throws <see cref="ArgumentNullException"/> when store is null.
    /// </summary>
    [TestMethod]
    public void Create_NullStore_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ParameterChangeMonitorState.Create<MonitorTestConfig>(null!));
    }

    /// <summary>
    /// Tests that Create throws <see cref="InvalidOperationException"/> when the store is not loaded.
    /// </summary>
    [TestMethod]
    public void Create_UnloadedStore_ThrowsInvalidOperationException()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new ConfigStore<MonitorTestConfig>(tempPath);
            Assert.ThrowsExactly<InvalidOperationException>(
                () => ParameterChangeMonitorState.Create(store));
            store.Dispose();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Create throws <see cref="ObjectDisposedException"/> when the store is disposed.
    /// </summary>
    [TestMethod]
    public void Create_DisposedStore_ThrowsObjectDisposedException()
    {
        var (store, _, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        store.Dispose();
        try
        {
            Assert.ThrowsExactly<ObjectDisposedException>(
                () => ParameterChangeMonitorState.Create(store));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that Create with default log capacity produces a state with default display height.
    /// </summary>
    [TestMethod]
    public void Create_DefaultCapacity_HasDefaultDisplayHeight()
    {
        var (store, _, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            using var state = ParameterChangeMonitorState.Create(store);
            Assert.AreEqual(ConfigChangeMonitorOptions.DefaultDisplayHeight, state.DisplayHeight);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Create with a custom log capacity creates a log that honors the capacity.
    /// </summary>
    [TestMethod]
    public void Create_CustomLogCapacity_HonorsCapacity()
    {
        var (store, config, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            using var state = ParameterChangeMonitorState.Create(store, logCapacity: 2);

            config.IntValue.Value = 20;
            config.IntValue.Value = 30;
            config.IntValue.Value = 40;

            // Capacity 2: oldest entry should be dropped
            Assert.AreEqual(2, state.Log.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that changes to parameters are recorded in the log.
    /// </summary>
    [TestMethod]
    public void Create_ParameterChange_RecordedInLog()
    {
        var (store, config, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            using var state = ParameterChangeMonitorState.Create(store);

            config.IntValue.Value = 42;

            Assert.AreEqual(1, state.Log.Count);
            var entries = state.Log.GetEntries();
            Assert.AreEqual(10, entries[0].OldValue);
            Assert.AreEqual(42, entries[0].NewValue);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Dispose detaches listeners so subsequent changes are not recorded.
    /// </summary>
    [TestMethod]
    public void Dispose_DetachesListeners_NoFurtherRecording()
    {
        var (store, config, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            var state = ParameterChangeMonitorState.Create(store);
            config.IntValue.Value = 42;
            Assert.AreEqual(1, state.Log.Count);

            state.Dispose();

            config.IntValue.Value = 99;
            // Log was not cleared by Dispose, but no new entries are added
            Assert.AreEqual(1, state.Log.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that double Dispose does not throw.
    /// </summary>
    [TestMethod]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var (store, _, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            var state = ParameterChangeMonitorState.Create(store);
            state.Dispose();
            state.Dispose(); // Should not throw
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Options-based Create ---

    /// <summary>
    /// Tests that the options-based Create throws <see cref="ArgumentNullException"/> when store is null.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_NullStore_ThrowsArgumentNullException()
    {
        var options = new ConfigChangeMonitorOptions();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => ParameterChangeMonitorState.Create<MonitorTestConfig>(null!, options));
    }

    /// <summary>
    /// Tests that the options-based Create throws <see cref="ArgumentNullException"/> when options is null.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_NullOptions_ThrowsArgumentNullException()
    {
        var (store, _, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            Assert.ThrowsExactly<ArgumentNullException>(
                () => ParameterChangeMonitorState.Create(store, null!));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the options-based Create throws when the store is not loaded.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_UnloadedStore_ThrowsInvalidOperationException()
    {
        var tempPath = Path.GetTempFileName();
        try
        {
            var store = new ConfigStore<MonitorTestConfig>(tempPath);
            var options = new ConfigChangeMonitorOptions();
            Assert.ThrowsExactly<InvalidOperationException>(
                () => ParameterChangeMonitorState.Create(store, options));
            store.Dispose();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that the options-based Create throws when the store is disposed.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_DisposedStore_ThrowsObjectDisposedException()
    {
        var (store, _, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        store.Dispose();
        try
        {
            var options = new ConfigChangeMonitorOptions();
            Assert.ThrowsExactly<ObjectDisposedException>(
                () => ParameterChangeMonitorState.Create(store, options));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that default options produce a state with default display height and default log capacity.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_DefaultOptions_UsesDefaults()
    {
        var (store, _, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            using var state = ParameterChangeMonitorState.Create(store, new ConfigChangeMonitorOptions());
            Assert.AreEqual(ConfigChangeMonitorOptions.DefaultDisplayHeight, state.DisplayHeight);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that custom <see cref="ConfigChangeMonitorOptions.LogCapacity"/> is honored.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_CustomLogCapacity_HonorsCapacity()
    {
        var (store, config, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            var options = new ConfigChangeMonitorOptions { LogCapacity = 2 };
            using var state = ParameterChangeMonitorState.Create(store, options);

            config.IntValue.Value = 20;
            config.IntValue.Value = 30;
            config.IntValue.Value = 40;

            Assert.AreEqual(2, state.Log.Count);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that custom <see cref="ConfigChangeMonitorOptions.DisplayHeight"/> is applied.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_CustomDisplayHeight_AppliesHeight()
    {
        var (store, _, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            var options = new ConfigChangeMonitorOptions { DisplayHeight = 500f };
            using var state = ParameterChangeMonitorState.Create(store, options);

            Assert.AreEqual(500f, state.DisplayHeight);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that options with fallback values (zero/negative) produce correct defaults.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_FallbackValues_UsesDefaults()
    {
        var (store, _, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            var options = new ConfigChangeMonitorOptions { LogCapacity = 0, DisplayHeight = -1f };
            using var state = ParameterChangeMonitorState.Create(store, options);

            Assert.AreEqual(ConfigChangeMonitorOptions.DefaultDisplayHeight, state.DisplayHeight);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the options-based Create still records parameter changes correctly.
    /// </summary>
    [TestMethod]
    public void OptionsCreate_ParameterChange_RecordedInLog()
    {
        var (store, config, tempPath) = CreateLoadedStore<MonitorTestConfig>();
        try
        {
            var options = new ConfigChangeMonitorOptions { LogCapacity = 16, DisplayHeight = 300f };
            using var state = ParameterChangeMonitorState.Create(store, options);

            config.StringValue.Value = "changed";

            Assert.AreEqual(1, state.Log.Count);
            var entries = state.Log.GetEntries();
            Assert.AreEqual("default", entries[0].OldValue);
            Assert.AreEqual("changed", entries[0].NewValue);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }
}
