using Umbra.Config;
using Umbra.UI.Config.Transfer;

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
        var result = ConfigTransferFeature.ResolveSidecarFilePath(@"C:\temp\config.json", null);

        Assert.AreEqual(@"C:\temp\config-transfer.json", result);
    }

    [TestMethod]
    public void ResolveSidecarFilePath_WithOverride_ReturnsOverride()
    {
        var result = ConfigTransferFeature.ResolveSidecarFilePath(@"C:\temp\config.json", @"D:\custom\sidecar.json");

        Assert.AreEqual(@"D:\custom\sidecar.json", result);
    }

    [TestMethod]
    public void ResolveFallbackBrowseDirectory_WithoutOverride_ReturnsMainStoreDirectory()
    {
        var result = ConfigTransferFeature.ResolveFallbackBrowseDirectory(@"C:\temp\config.json", null);

        Assert.AreEqual(@"C:\temp", result);
    }

    [TestMethod]
    public void ResolveFallbackBrowseDirectory_WithOverride_ReturnsOverride()
    {
        var result = ConfigTransferFeature.ResolveFallbackBrowseDirectory(@"C:\temp\config.json", @"D:\plugin-data");

        Assert.AreEqual(@"D:\plugin-data", result);
    }

    [TestMethod]
    public void ResolveTreeNodeLabel_WithoutOverride_ReturnsDefaultLabel()
    {
        var result = ConfigTransferFeature.ResolveTreeNodeLabel(null);

        Assert.AreEqual(ConfigTransferOptions.DefaultTreeNodeLabel, result);
    }

    [TestMethod]
    public void ResolveTreeNodeLabel_WithOverride_ReturnsOverride()
    {
        var result = ConfigTransferFeature.ResolveTreeNodeLabel("Transfer Controls");

        Assert.AreEqual("Transfer Controls", result);
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

        Assert.AreEqual("Built-in config transfer UI requires a loaded settings store.", exception.Message);
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

        using var sidecarStore = new SettingsStore<ConfigTransferSidecarState>(sidecarFilePath);
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

        public SettingsImportReport Import(string filePath, SettingsImportOptions? options = null)
        {
            LastImportedPath = filePath;
            return new SettingsImportReport { Success = true };
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
