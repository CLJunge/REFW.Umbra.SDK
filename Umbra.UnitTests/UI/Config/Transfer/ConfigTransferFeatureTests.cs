using Umbra.Config;
using Umbra.UI.Config.Drawers;

namespace Umbra.UI.Config.Transfer.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigTransferFeature"/>.
/// </summary>
[TestClass]
public sealed class ConfigTransferFeatureTests
{
    [TestMethod]
    public void ResolveSidecarFilePath_WithoutOverride_DerivesSiblingTransferFile()
    {
        var mainFilePath = Path.Combine("temp", "config.json");
        var result = ConfigTransferFeature.ResolveSidecarFilePath(mainFilePath, null);

        Assert.AreEqual(Path.Combine("temp", "config-transfer.json"), result);
    }

    [TestMethod]
    public void ResolveSidecarFilePath_WithOverride_ReturnsOverride()
    {
        var mainFilePath = Path.Combine("temp", "config.json");
        var sidecarFilePathOverride = Path.Combine("custom", "sidecar.json");
        var result = ConfigTransferFeature.ResolveSidecarFilePath(mainFilePath, sidecarFilePathOverride);

        Assert.AreEqual(sidecarFilePathOverride, result);
    }

    [TestMethod]
    public void ResolveFallbackBrowseDirectory_WithoutOverride_ReturnsMainStoreDirectory()
    {
        var mainFilePath = Path.Combine("temp", "config.json");
        var result = ConfigTransferFeature.ResolveFallbackBrowseDirectory(mainFilePath, null);

        Assert.AreEqual("temp", result);
    }

    [TestMethod]
    public void ResolveFallbackBrowseDirectory_WithOverride_ReturnsOverride()
    {
        var mainFilePath = Path.Combine("temp", "config.json");
        const string browseInitialDirectoryOverride = "plugin-data";
        var result = ConfigTransferFeature.ResolveFallbackBrowseDirectory(mainFilePath, browseInitialDirectoryOverride);

        Assert.AreEqual(browseInitialDirectoryOverride, result);
    }

    [TestMethod]
    public void ResolveTreeNodeLabel_WithoutOverride_ReturnsDefaultLabel()
    {
        var result = ConfigTransferFeature.ResolveTreeNodeLabel(null);

        Assert.AreEqual(ConfigTransferOptions.DefaultSectionLabel, result);
    }

    [TestMethod]
    public void ResolveTreeNodeLabel_WithOverride_ReturnsOverride()
    {
        var result = ConfigTransferFeature.ResolveTreeNodeLabel("Transfer Controls");

        Assert.AreEqual("Transfer Controls", result);
    }

    [TestMethod]
    public void ResolveStatusVisibilityTimeout_WithPositiveDuration_ReturnsConfiguredValue()
    {
        var configuredTimeout = TimeSpan.FromSeconds(3);

        var result = ConfigTransferFeature.ResolveStatusVisibilityTimeout(configuredTimeout);

        Assert.AreEqual(configuredTimeout, result);
    }

    [TestMethod]
    public void ResolveStatusVisibilityTimeout_WithZeroDuration_ReturnsDefaultTimeout()
    {
        var result = ConfigTransferFeature.ResolveStatusVisibilityTimeout(TimeSpan.Zero);

        Assert.AreEqual(ConfigTransferOptions.DefaultStatusVisibilityTimeout, result);
    }

    [TestMethod]
    public void ResolveStatusVisibilityTimeout_WithNegativeDuration_ReturnsDefaultTimeout()
    {
        var result = ConfigTransferFeature.ResolveStatusVisibilityTimeout(TimeSpan.FromSeconds(-1));

        Assert.AreEqual(ConfigTransferOptions.DefaultStatusVisibilityTimeout, result);
    }

    [TestMethod]
    public void Constructor_WhenUsingRenamedOptionProperties_AppliesConfiguredValues()
    {
        using var tempDirectory = new TempDirectory();
        var mainFilePath = Path.Combine(tempDirectory.Path, "config.json");
        var transferStateFilePath = Path.Combine(tempDirectory.Path, "custom-transfer-state.json");
        var store = new TestConfigTransferStore(mainFilePath);
        var drawer = new ConfigTransferDrawer();
        var options = new ConfigTransferOptions
        {
            Enabled = true,
            ConfigFilePath = transferStateFilePath,
            BrowseFallbackDirectory = Path.Combine(tempDirectory.Path, "browse"),
            SectionLabel = "Transfer Controls",
            ExpandedByDefault = true,
            ShowSeparatorBelowButtons = false,
            StatusDisplayDuration = TimeSpan.FromSeconds(3)
        };

        var feature = new ConfigTransferFeature(store, options, drawer);
        feature.ConfigFilePath.Value = Path.Combine(tempDirectory.Path, "persisted.json");
        feature.TransferMode.Value = ConfigTransferMode.Export;
        feature.Dispose();

        Assert.AreEqual("Transfer Controls", feature.SectionLabel);
        Assert.IsTrue(feature.ExpandedByDefault);
        Assert.IsFalse(feature.ShowSeparatorBelowButtons);
        Assert.AreEqual(TimeSpan.FromSeconds(3), drawer.StatusVisibilityTimeout);

        using var sidecarStore = new ConfigStore<ConfigTransferSidecarState>(transferStateFilePath);
        var sidecarState = sidecarStore.Load();
        Assert.AreEqual(Path.Combine(tempDirectory.Path, "persisted.json"), sidecarState.ConfigFilePath.Value);
        Assert.AreEqual(ConfigTransferMode.Export, sidecarState.TransferMode.Value);
    }

    [TestMethod]
    public void Constructor_WhenStoreIsNotLoaded_ThrowsInvalidOperationException()
    {
        var store = new TestConfigTransferStore(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "config.json"))
        {
            IsLoaded = false
        };

        var exception = Assert.ThrowsExactly<InvalidOperationException>(
            () => _ = new ConfigTransferFeature(store, new ConfigTransferOptions { Enabled = true }));

        Assert.AreEqual("Built-in config transfer UI requires a loaded config store.", exception.Message);
    }

    [TestMethod]
    public void ImportConfig_WhenInvoked_UsesCurrentPath()
    {
        using var tempDirectory = new TempDirectory();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));
        using var feature = new ConfigTransferFeature(store, new ConfigTransferOptions { Enabled = true });
        var importFilePath = Path.Combine(tempDirectory.Path, "import.json");
        File.WriteAllText(importFilePath, "{}");
        feature.ConfigFilePath.Value = importFilePath;

        feature.ImportConfig.Value!.Invoke();

        Assert.AreEqual(feature.ConfigFilePath.Value, store.LastImportedPath);
    }

    [TestMethod]
    public void ImportConfig_WhenFileDoesNotExist_DoesNotCallStore()
    {
        using var tempDirectory = new TempDirectory();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));
        using var feature = new ConfigTransferFeature(store, new ConfigTransferOptions { Enabled = true });
        feature.ConfigFilePath.Value = Path.Combine(tempDirectory.Path, "missing-import.json");

        feature.ImportConfig.Value!.Invoke();

        Assert.IsNull(store.LastImportedPath);
    }

    [TestMethod]
    public void ExportConfig_WhenInvoked_UsesCurrentPath()
    {
        using var tempDirectory = new TempDirectory();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));
        using var feature = new ConfigTransferFeature(store, new ConfigTransferOptions { Enabled = true });
        feature.ConfigFilePath.Value = Path.Combine(tempDirectory.Path, "export.json");

        feature.ExportConfig.Value!.Invoke();

        Assert.AreEqual(feature.ConfigFilePath.Value, store.LastExportedPath);
    }

    [TestMethod]
    public void ExportConfig_WhenPathDoesNotUseJsonExtension_DoesNotCallStore()
    {
        using var tempDirectory = new TempDirectory();
        var store = new TestConfigTransferStore(Path.Combine(tempDirectory.Path, "config.json"));
        using var feature = new ConfigTransferFeature(store, new ConfigTransferOptions { Enabled = true });
        feature.ConfigFilePath.Value = Path.Combine(tempDirectory.Path, "export.txt");

        feature.ExportConfig.Value!.Invoke();

        Assert.IsNull(store.LastExportedPath);
    }

    [TestMethod]
    public void Dispose_PersistsUpdatedSidecarPathAndMode()
    {
        using var tempDirectory = new TempDirectory();
        var mainFilePath = Path.Combine(tempDirectory.Path, "config.json");
        var sidecarFilePath = ConfigTransferFeature.ResolveSidecarFilePath(mainFilePath, null);
        var store = new TestConfigTransferStore(mainFilePath);
        var feature = new ConfigTransferFeature(store, new ConfigTransferOptions { Enabled = true });
        feature.ConfigFilePath.Value = Path.Combine(tempDirectory.Path, "persisted.json");
        feature.TransferMode.Value = ConfigTransferMode.Export;

        feature.Dispose();

        using var sidecarStore = new ConfigStore<ConfigTransferSidecarState>(sidecarFilePath);
        var sidecarState = sidecarStore.Load();
        Assert.AreEqual(Path.Combine(tempDirectory.Path, "persisted.json"), sidecarState.ConfigFilePath.Value);
        Assert.AreEqual(ConfigTransferMode.Export, sidecarState.TransferMode.Value);
    }

    private sealed class TestConfigTransferStore(string filePath) : IConfigTransferStore
    {
        public string FilePath { get; } = filePath;

        public bool IsLoaded { get; set; } = true;

        public bool IsDisposed { get; set; }

        public string? LastExportedPath { get; private set; }

        public string? LastImportedPath { get; private set; }

        public void Export(string filePath) => LastExportedPath = filePath;

        public ConfigImportReport Import(string filePath, ConfigImportOptions? options = null)
        {
            LastImportedPath = filePath;
            return new ConfigImportReport { Success = true };
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        internal TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        internal string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);

            GC.SuppressFinalize(this);
        }
    }
}
