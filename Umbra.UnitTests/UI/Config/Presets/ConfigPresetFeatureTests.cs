using Umbra.Config;
using Umbra.Config.Presets;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;
using Umbra.UI.Config.Drawers.UnitTests;

namespace Umbra.UI.Config.Presets.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigPresetFeature{TConfig}"/>.
/// </summary>
[TestClass]
public sealed class ConfigPresetFeatureTests
{
    // --- ResolveTreeNodeLabel ---

    /// <summary>
    /// Verifies that a null section label resolves to the default.
    /// </summary>
    [TestMethod]
    public void ResolveTreeNodeLabel_Null_ReturnsDefault()
    {
        var result = ConfigPresetFeature<TestPresetConfig>.ResolveTreeNodeLabel(null);
        Assert.AreEqual(ConfigPresetOptions.DefaultSectionLabel, result);
    }

    /// <summary>
    /// Verifies that a whitespace section label resolves to the default.
    /// </summary>
    [TestMethod]
    public void ResolveTreeNodeLabel_Whitespace_ReturnsDefault()
    {
        var result = ConfigPresetFeature<TestPresetConfig>.ResolveTreeNodeLabel("   ");
        Assert.AreEqual(ConfigPresetOptions.DefaultSectionLabel, result);
    }

    /// <summary>
    /// Verifies that an empty section label resolves to the default.
    /// </summary>
    [TestMethod]
    public void ResolveTreeNodeLabel_Empty_ReturnsDefault()
    {
        var result = ConfigPresetFeature<TestPresetConfig>.ResolveTreeNodeLabel(string.Empty);
        Assert.AreEqual(ConfigPresetOptions.DefaultSectionLabel, result);
    }

    /// <summary>
    /// Verifies that an explicit section label is returned as-is.
    /// </summary>
    [TestMethod]
    public void ResolveTreeNodeLabel_ExplicitLabel_ReturnsLabel()
    {
        var result = ConfigPresetFeature<TestPresetConfig>.ResolveTreeNodeLabel("My Presets");
        Assert.AreEqual("My Presets", result);
    }

    // --- Constructor property propagation ---

    /// <summary>
    /// Verifies that section label is resolved from options.
    /// </summary>
    [TestMethod]
    public void Constructor_SectionLabel_PropagatesFromOptions()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);
        var options = new ConfigPresetOptions { SectionLabel = "Custom Presets" };

        using var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, options,
            new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
            new TestConfigTransferFilePicker());

        Assert.AreEqual("Custom Presets", feature.SectionLabel);
    }

    /// <summary>
    /// Verifies that ExpandedByDefault is propagated from options.
    /// </summary>
    [TestMethod]
    public void Constructor_ExpandedByDefault_PropagatesFromOptions()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);
        var options = new ConfigPresetOptions { ExpandedByDefault = true };

        using var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, options,
            new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
            new TestConfigTransferFilePicker());

        Assert.IsTrue(feature.ExpandedByDefault);
    }

    /// <summary>
    /// Verifies that ShowSeparatorBelowButtons defaults to true.
    /// </summary>
    [TestMethod]
    public void Constructor_ShowSeparatorBelowButtons_DefaultsToTrue()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);
        var options = new ConfigPresetOptions();

        using var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, options,
            new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
            new TestConfigTransferFilePicker());

        Assert.IsTrue(feature.ShowSeparatorBelowButtons);
    }

    /// <summary>
    /// Verifies that ShowSeparatorBelowButtons propagates false when configured.
    /// </summary>
    [TestMethod]
    public void Constructor_ShowSeparatorBelowButtonsFalse_PropagatesFromOptions()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);
        var options = new ConfigPresetOptions { ShowSeparatorBelowButtons = false };

        using var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, options,
            new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
            new TestConfigTransferFilePicker());

        Assert.IsFalse(feature.ShowSeparatorBelowButtons);
    }

    /// <summary>
    /// Verifies that ActivePresetName starts as null.
    /// </summary>
    [TestMethod]
    public void Constructor_ActivePresetName_IsInitiallyNull()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);

        using var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, new ConfigPresetOptions(),
            new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
            new TestConfigTransferFilePicker());

        Assert.IsNull(feature.ActivePresetName);
    }

    // --- Constructor null guards ---

    /// <summary>
    /// Verifies that a null preset store throws.
    /// </summary>
    [TestMethod]
    public void Constructor_NullPresetStore_ThrowsArgumentNullException()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new ConfigPresetFeature<TestPresetConfig>(
                null!, () => { }, new ConfigPresetOptions(),
                new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
                new TestConfigTransferFilePicker()));
    }

    /// <summary>
    /// Verifies that a null save callback throws.
    /// </summary>
    [TestMethod]
    public void Constructor_NullSaveConfig_ThrowsArgumentNullException()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new ConfigPresetFeature<TestPresetConfig>(
                presetStore, null!, new ConfigPresetOptions(),
                new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
                new TestConfigTransferFilePicker()));
    }

    /// <summary>
    /// Verifies that null options throws.
    /// </summary>
    [TestMethod]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new ConfigPresetFeature<TestPresetConfig>(
                presetStore, () => { }, null!,
                new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
                new TestConfigTransferFilePicker()));
    }

    /// <summary>
    /// Verifies that a null drawer throws.
    /// </summary>
    [TestMethod]
    public void Constructor_NullDrawer_ThrowsArgumentNullException()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new ConfigPresetFeature<TestPresetConfig>(
                presetStore, () => { }, new ConfigPresetOptions(),
                null!,
                new TestConfigTransferFilePicker()));
    }

    /// <summary>
    /// Verifies that a null file picker throws.
    /// </summary>
    [TestMethod]
    public void Constructor_NullFilePicker_ThrowsArgumentNullException()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new ConfigPresetFeature<TestPresetConfig>(
                presetStore, () => { }, new ConfigPresetOptions(),
                new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
                null!));
    }

    // --- Dispose ---

    /// <summary>
    /// Verifies that Draw becomes a no-op after disposal.
    /// </summary>
    [TestMethod]
    public void Draw_AfterDispose_IsNoop()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);
        var renderer = new TestConfigPresetDrawerRenderer();

        var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, new ConfigPresetOptions(),
            new ConfigPresetDrawer(renderer),
            new TestConfigTransferFilePicker());

        feature.Dispose();
        feature.Draw();

        // No combo should have been rendered after disposal
        Assert.HasCount(0, renderer.Combos);
    }

    /// <summary>
    /// Verifies that multiple dispose calls are safe.
    /// </summary>
    [TestMethod]
    public void Dispose_MultipleCalls_DoesNotThrow()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);

        var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, new ConfigPresetOptions(),
            new ConfigPresetDrawer(new TestConfigPresetDrawerRenderer()),
            new TestConfigTransferFilePicker());

        feature.Dispose();
        feature.Dispose();
        feature.Dispose();
    }

    /// <summary>
    /// Verifies that Draw with disableControls=false enables normal control interaction.
    /// </summary>
    [TestMethod]
    public void Draw_DisableControlsFalse_ControlsEnabled()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);
        var renderer = new TestConfigPresetDrawerRenderer();

        using var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, new ConfigPresetOptions(),
            new ConfigPresetDrawer(renderer),
            new TestConfigTransferFilePicker());

        feature.Draw(disableControls: false);

        Assert.IsTrue(renderer.DisabledScopes.Count > 0);
        Assert.IsFalse(renderer.DisabledScopes[0]);
    }

    /// <summary>
    /// Verifies that Draw with disableControls=true disables all preset controls.
    /// </summary>
    [TestMethod]
    public void Draw_DisableControlsTrue_ControlsDisabled()
    {
        using var tempDir = new TempDirectory();
        var store = CreateLoadedStore(tempDir.Path);
        var presetStore = new ConfigPresetStore<TestPresetConfig>(store);
        var renderer = new TestConfigPresetDrawerRenderer();

        using var feature = new ConfigPresetFeature<TestPresetConfig>(
            presetStore, () => { }, new ConfigPresetOptions(),
            new ConfigPresetDrawer(renderer),
            new TestConfigTransferFilePicker());

        feature.Draw(disableControls: true);

        // A disabled scope should have been active
        var hasDisabledScope = false;
        foreach (var disabled in renderer.DisabledScopes)
            if (disabled)
                hasDisabledScope = true;

        Assert.IsTrue(hasDisabledScope);
        // EndDisabled should have been called to exit the disabled scope
        Assert.IsTrue(renderer.EndDisabledCallCount > 0);
    }

    // --- Helpers ---

    private static ConfigStore<TestPresetConfig> CreateLoadedStore(string directory)
    {
        var filePath = Path.Combine(directory, "config.json");
        var store = new ConfigStore<TestPresetConfig>(filePath);
        store.Load();
        return store;
    }

    /// <summary>
    /// Minimal config type for preset feature tests.
    /// </summary>
    internal sealed class TestPresetConfig
    {
        [UmbraParameter("Value")]
        public Parameter<int> Value { get; } = new(42);
    }

    /// <summary>
    /// Creates a temporary directory that is deleted when disposed.
    /// </summary>
    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());

        public TempDirectory() => Directory.CreateDirectory(Path);

        public void Dispose()
        {
            try { Directory.Delete(Path, true); }
            catch { /* best-effort cleanup */ }
        }
    }
}
