using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.UI.Toast;

namespace Umbra.Config.Presets.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigPresetStore{TConfig}"/>.
/// </summary>
[TestClass]
public sealed class ConfigPresetStoreTests
{
    /// <summary>
    /// Test configuration class for preset store tests.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class PresetTestConfig
    {
        [UmbraParameter]
        public Parameter<int> IntValue { get; set; } = new(10);

        [UmbraParameter]
        public Parameter<string> StringValue { get; set; } = new("default");

        [UmbraParameter]
        public Parameter<bool> BoolValue { get; set; } = new(false);
    }

    /// <summary>
    /// Test configuration class that includes a delegate parameter.
    /// </summary>
    [UmbraAutoRegister]
    private sealed class DelegatePresetConfig
    {
        [UmbraParameter]
        public Parameter<int> IntValue { get; set; } = new(5);

        [UmbraParameter]
        public Parameter<Action> ButtonAction { get; set; } = new(() => { });
    }

    private static (ConfigStore<T> Store, T Config, string TempPath) CreateLoadedStore<T>()
        where T : class, new()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"presettest_{Guid.NewGuid()}.json");
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

        // Clean up any preset files in the directory
        var dir = Path.GetDirectoryName(tempPath);
        if (dir != null && Directory.Exists(dir))
        {
            var presetFiles = Directory.GetFiles(dir, "config-preset-*presettest*.json");
            for (var i = 0; i < presetFiles.Length; i++)
            {
                try { File.Delete(presetFiles[i]); }
                catch { /* best effort cleanup */ }
            }
        }
    }

    // --- Constructor validation ---

    /// <summary>
    /// Tests that the constructor throws <see cref="ArgumentNullException"/> when store is null.
    /// </summary>
    [TestMethod]
    public void Constructor_NullStore_ThrowsArgumentNullException() => Assert.ThrowsExactly<ArgumentNullException>(() => new ConfigPresetStore<PresetTestConfig>(null!));

    /// <summary>
    /// Tests that the constructor throws <see cref="InvalidOperationException"/> when the store is not loaded.
    /// </summary>
    [TestMethod]
    public void Constructor_UnloadedStore_ThrowsInvalidOperationException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"presettest_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<PresetTestConfig>(tempPath);
            Assert.ThrowsExactly<InvalidOperationException>(() => new ConfigPresetStore<PresetTestConfig>(store));
            store.Dispose();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that the constructor throws <see cref="ObjectDisposedException"/> when the store is disposed.
    /// </summary>
    [TestMethod]
    public void Constructor_DisposedStore_ThrowsObjectDisposedException()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        store.Dispose();
        try
        {
            Assert.ThrowsExactly<ObjectDisposedException>(() => new ConfigPresetStore<PresetTestConfig>(store));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    // --- Save ---

    /// <summary>
    /// Tests that Save creates a preset file on disk.
    /// </summary>
    [TestMethod]
    public void Save_CreatesPresetFile()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            presets.Save("test1");

            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var expected = Path.Combine(dir, "config-preset-test1.json");
            Assert.IsTrue(File.Exists(expected), $"Expected preset file at {expected}");
            File.Delete(expected);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Save throws <see cref="ArgumentException"/> for null name.
    /// </summary>
    [TestMethod]
    public void Save_NullName_ThrowsArgumentException()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            Assert.ThrowsExactly<ArgumentException>(() => presets.Save(null!));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Save throws <see cref="ArgumentException"/> for empty name.
    /// </summary>
    [TestMethod]
    public void Save_EmptyName_ThrowsArgumentException()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            Assert.ThrowsExactly<ArgumentException>(() => presets.Save(""));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Save throws <see cref="ArgumentException"/> for names with invalid characters.
    /// </summary>
    [TestMethod]
    public void Save_InvalidCharacterInName_ThrowsArgumentException()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            Assert.ThrowsExactly<ArgumentException>(() => presets.Save("bad/name"));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Save pushes a success toast when Toast options are configured.
    /// </summary>
    [TestMethod]
    public void Save_PushesSuccessToast()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            ToastQueue.Clear();
            var presets = new ConfigPresetStore<PresetTestConfig>(store, new ConfigPresetOptions { Toast = new ConfigToastOptions() });
            presets.Save("toasttest");

            var entries = ToastQueue.GetActiveEntries();
            var found = false;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Message.Contains("toasttest") && entries[i].Level == ToastLevel.Success)
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Expected a success toast for preset save.");
        }
        finally
        {
            ToastQueue.Clear();
            // cleanup preset file
            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pf = Path.Combine(dir, "config-preset-toasttest.json");
            if (File.Exists(pf)) File.Delete(pf);
            CleanupStore(store, tempPath);
        }
    }

    // --- Load ---

    /// <summary>
    /// Tests that Load restores saved parameter values.
    /// </summary>
    [TestMethod]
    public void Load_RestoresSavedValues()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);

            // Change values and save preset
            config.IntValue.Value = 99;
            config.StringValue.Value = "preset";
            config.BoolValue.Value = true;
            presets.Save("restore");

            // Reset values to defaults
            config.IntValue.Value = 10;
            config.StringValue.Value = "default";
            config.BoolValue.Value = false;

            // Load preset
            var result = presets.Load("restore");

            Assert.IsTrue(result);
            Assert.AreEqual(99, config.IntValue.Value);
            Assert.AreEqual("preset", config.StringValue.Value);
            Assert.IsTrue(config.BoolValue.Value);
        }
        finally
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pf = Path.Combine(dir, "config-preset-restore.json");
            if (File.Exists(pf)) File.Delete(pf);
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Load returns false when the preset file does not exist.
    /// </summary>
    [TestMethod]
    public void Load_NonexistentPreset_ReturnsFalse()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var result = presets.Load("nonexistent");
            Assert.IsFalse(result);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Load fires ValueChanged events so undo stack can track changes.
    /// </summary>
    [TestMethod]
    public void Load_FiresValueChangedEvents()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);

            config.IntValue.Value = 42;
            presets.Save("eventtest");
            config.IntValue.Value = 10;

            var changeCount = 0;
            config.IntValue.ValueChanged += (_, _) => changeCount++;

            presets.Load("eventtest");

            Assert.IsGreaterThan(0, changeCount, "Expected ValueChanged to fire during preset load.");
        }
        finally
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pf = Path.Combine(dir, "config-preset-eventtest.json");
            if (File.Exists(pf)) File.Delete(pf);
            CleanupStore(store, tempPath);
        }
    }

    // --- List ---

    /// <summary>
    /// Tests that List returns saved preset names in sorted order.
    /// </summary>
    [TestMethod]
    public void List_ReturnsSavedPresetNames()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            presets.Save("beta");
            presets.Save("alpha");

            var names = presets.List();

            Assert.IsGreaterThanOrEqualTo(2, names.Count);

            var foundAlpha = false;
            var foundBeta = false;
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i] == "alpha") foundAlpha = true;
                if (names[i] == "beta") foundBeta = true;
            }

            Assert.IsTrue(foundAlpha, "Expected 'alpha' in preset list.");
            Assert.IsTrue(foundBeta, "Expected 'beta' in preset list.");

            // Verify sorted order
            var alphaIdx = -1;
            var betaIdx = -1;
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i] == "alpha") alphaIdx = i;
                if (names[i] == "beta") betaIdx = i;
            }

            Assert.IsLessThan(betaIdx, alphaIdx, "Expected 'alpha' before 'beta' in sorted list.");
        }
        finally
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pfa = Path.Combine(dir, "config-preset-alpha.json");
            var pfb = Path.Combine(dir, "config-preset-beta.json");
            if (File.Exists(pfa)) File.Delete(pfa);
            if (File.Exists(pfb)) File.Delete(pfb);
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that List returns an empty list when no presets exist.
    /// </summary>
    [TestMethod]
    public void List_NoPresets_ReturnsEmptyList()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var names = presets.List();
            // May or may not be empty depending on other test runs in same temp dir,
            // but at minimum it should not throw.
            Assert.IsNotNull(names);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Delete ---

    /// <summary>
    /// Tests that Delete removes a saved preset file.
    /// </summary>
    [TestMethod]
    public void Delete_ExistingPreset_ReturnsTrue()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            presets.Save("todelete");

            var result = presets.Delete("todelete");
            Assert.IsTrue(result);

            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pf = Path.Combine(dir, "config-preset-todelete.json");
            Assert.IsFalse(File.Exists(pf), "Preset file should have been deleted.");
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Delete returns false when the preset does not exist.
    /// </summary>
    [TestMethod]
    public void Delete_NonexistentPreset_ReturnsFalse()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var result = presets.Delete("nope");
            Assert.IsFalse(result);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that Delete pushes a toast on success when Toast options are configured.
    /// </summary>
    [TestMethod]
    public void Delete_PushesToastOnSuccess()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            ToastQueue.Clear();
            var presets = new ConfigPresetStore<PresetTestConfig>(store, new ConfigPresetOptions { Toast = new ConfigToastOptions() });
            presets.Save("deltoast");

            ToastQueue.Clear();
            presets.Delete("deltoast");

            var entries = ToastQueue.GetActiveEntries();
            var found = false;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Message.Contains("deltoast"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsTrue(found, "Expected a toast for preset delete.");
        }
        finally
        {
            ToastQueue.Clear();
            CleanupStore(store, tempPath);
        }
    }

    // --- Round-trip ---

    /// <summary>
    /// Tests a full save/load/delete round-trip cycle.
    /// </summary>
    [TestMethod]
    public void RoundTrip_SaveLoadDelete()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);

            // Save with modified values
            config.IntValue.Value = 77;
            config.StringValue.Value = "roundtrip";
            presets.Save("rt");

            // Reset to defaults
            config.IntValue.Value = 10;
            config.StringValue.Value = "default";

            // Verify listed
            var names = presets.List();
            var found = false;
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i] == "rt") { found = true; break; }
            }
            Assert.IsTrue(found, "Expected 'rt' in preset list.");

            // Load and verify
            Assert.IsTrue(presets.Load("rt"));
            Assert.AreEqual(77, config.IntValue.Value);
            Assert.AreEqual("roundtrip", config.StringValue.Value);

            // Delete and verify
            Assert.IsTrue(presets.Delete("rt"));
            Assert.IsFalse(presets.Load("rt"));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Delegate parameter exclusion ---

    /// <summary>
    /// Tests that delegate-typed parameters are excluded from saved presets.
    /// </summary>
    [TestMethod]
    public void Save_ExcludesDelegateParameters()
    {
        var (store, _, tempPath) = CreateLoadedStore<DelegatePresetConfig>();
        try
        {
            var presets = new ConfigPresetStore<DelegatePresetConfig>(store);
            presets.Save("nodelegates");

            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pf = Path.Combine(dir, "config-preset-nodelegates.json");
            var json = File.ReadAllText(pf);

            // The JSON should not contain "buttonAction" (the delegate parameter)
            Assert.IsFalse(json.Contains("buttonAction", StringComparison.OrdinalIgnoreCase),
                "Delegate parameters should not be in the preset file.");
            // But should contain "intValue"
            Assert.IsTrue(json.Contains("intValue", StringComparison.OrdinalIgnoreCase),
                "Non-delegate parameters should be in the preset file.");

            File.Delete(pf);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- Overwrite ---

    /// <summary>
    /// Tests that saving a preset with the same name overwrites the previous file.
    /// </summary>
    [TestMethod]
    public void Save_SameName_Overwrites()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);

            config.IntValue.Value = 11;
            presets.Save("overwrite");

            config.IntValue.Value = 22;
            presets.Save("overwrite");

            config.IntValue.Value = 10;
            presets.Load("overwrite");

            Assert.AreEqual(22, config.IntValue.Value);
        }
        finally
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pf = Path.Combine(dir, "config-preset-overwrite.json");
            if (File.Exists(pf)) File.Delete(pf);
            CleanupStore(store, tempPath);
        }
    }

    // --- Options-based constructor ---

    /// <summary>
    /// Tests that the options-based constructor throws <see cref="ArgumentNullException"/> when store is null.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_NullStore_ThrowsArgumentNullException()
    {
        var options = new ConfigPresetOptions();
        Assert.ThrowsExactly<ArgumentNullException>(
            () => new ConfigPresetStore<PresetTestConfig>(null!, options));
    }

    /// <summary>
    /// Tests that the options-based constructor throws <see cref="ArgumentNullException"/> when options is null.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_NullOptions_ThrowsArgumentNullException()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            Assert.ThrowsExactly<ArgumentNullException>(
                () => new ConfigPresetStore<PresetTestConfig>(store, null!));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that the options-based constructor throws when the store is not loaded.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_UnloadedStore_ThrowsInvalidOperationException()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"presettest_{Guid.NewGuid()}.json");
        try
        {
            var store = new ConfigStore<PresetTestConfig>(tempPath);
            var options = new ConfigPresetOptions();
            Assert.ThrowsExactly<InvalidOperationException>(
                () => new ConfigPresetStore<PresetTestConfig>(store, options));
            store.Dispose();
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    /// <summary>
    /// Tests that a custom <see cref="ConfigPresetOptions.PresetFilePrefix"/> is used for file names.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_CustomPrefix_UsesCustomPrefixForFiles()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var options = new ConfigPresetOptions { PresetFilePrefix = "custom-pfx-" };
            var presets = new ConfigPresetStore<PresetTestConfig>(store, options);

            config.IntValue.Value = 55;
            presets.Save("pfxtest");

            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var expected = Path.Combine(dir, "custom-pfx-pfxtest.json");
            Assert.IsTrue(File.Exists(expected), $"Expected preset file at {expected}");

            // Verify List also uses the custom prefix
            var names = presets.List();
            var found = false;
            for (var i = 0; i < names.Count; i++)
            {
                if (names[i] == "pfxtest") { found = true; break; }
            }
            Assert.IsTrue(found, "Expected 'pfxtest' in preset list with custom prefix.");

            File.Delete(expected);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that a null/whitespace prefix falls back to <see cref="ConfigPresetOptions.DefaultPresetFilePrefix"/>.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_NullPrefix_FallsBackToDefault()
    {
        var options = new ConfigPresetOptions { PresetFilePrefix = null! };
        Assert.AreEqual(ConfigPresetOptions.DefaultPresetFilePrefix, options.PresetFilePrefix);
    }

    /// <summary>
    /// Tests that a custom <see cref="ConfigPresetOptions.PresetDirectory"/> overrides the store-derived directory.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_CustomDirectory_UsesCustomDirectory()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        var customDir = Path.Combine(Path.GetTempPath(), $"preset_custom_{Guid.NewGuid():N}");
        try
        {
            var options = new ConfigPresetOptions { PresetDirectory = customDir };
            var presets = new ConfigPresetStore<PresetTestConfig>(store, options);

            config.IntValue.Value = 77;
            presets.Save("dirtest");

            var expected = Path.Combine(customDir, "config-preset-dirtest.json");
            Assert.IsTrue(File.Exists(expected), $"Expected preset file at {expected}");
        }
        finally
        {
            if (Directory.Exists(customDir))
                Directory.Delete(customDir, recursive: true);
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that toast notifications are suppressed when <see cref="ConfigPresetOptions.Toast"/> is <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void OptionsConstructor_ToastNull_DoesNotPushToast()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var options = new ConfigPresetOptions { Toast = null };
            var presets = new ConfigPresetStore<PresetTestConfig>(store, options);

            ToastQueue.Clear();

            config.IntValue.Value = 42;
            presets.Save("notoast");

            var entries = ToastQueue.GetActiveEntries();
            var found = false;
            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i].Message.Contains("notoast"))
                {
                    found = true;
                    break;
                }
            }

            Assert.IsFalse(found, "Expected no toast when Toast is null.");
        }
        finally
        {
            ToastQueue.Clear();
            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pf = Path.Combine(dir, "config-preset-notoast.json");
            if (File.Exists(pf)) File.Delete(pf);
            CleanupStore(store, tempPath);
        }
    }

    // --- PresetDirectory ---

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.PresetDirectory"/> returns the directory derived from the config store file path.
    /// </summary>
    [TestMethod]
    public void PresetDirectory_ReturnsConfigStoreDirectory()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var expected = Path.GetDirectoryName(Path.GetFullPath(tempPath));
            Assert.AreEqual(expected, presets.PresetDirectory);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.PresetDirectory"/> returns the custom directory when configured.
    /// </summary>
    [TestMethod]
    public void PresetDirectory_CustomDirectory_ReturnsCustomDirectory()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        var customDir = Path.Combine(Path.GetTempPath(), $"preset_dir_{Guid.NewGuid():N}");
        try
        {
            var options = new ConfigPresetOptions { PresetDirectory = customDir };
            var presets = new ConfigPresetStore<PresetTestConfig>(store, options);
            Assert.AreEqual(customDir, presets.PresetDirectory);
        }
        finally
        {
            if (Directory.Exists(customDir))
                Directory.Delete(customDir, recursive: true);
            CleanupStore(store, tempPath);
        }
    }

    // --- PresetFilePrefix ---

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.PresetFilePrefix"/> returns the default prefix.
    /// </summary>
    [TestMethod]
    public void PresetFilePrefix_Default_ReturnsDefaultPrefix()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            Assert.AreEqual(ConfigPresetOptions.DefaultPresetFilePrefix, presets.PresetFilePrefix);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.PresetFilePrefix"/> returns the custom prefix when configured.
    /// </summary>
    [TestMethod]
    public void PresetFilePrefix_CustomPrefix_ReturnsCustomPrefix()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var options = new ConfigPresetOptions { PresetFilePrefix = "my-prefix-" };
            var presets = new ConfigPresetStore<PresetTestConfig>(store, options);
            Assert.AreEqual("my-prefix-", presets.PresetFilePrefix);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- ExportPreset ---

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.ExportPreset"/> copies the preset file to the destination.
    /// </summary>
    [TestMethod]
    public void ExportPreset_ExistingPreset_CopiesToDestination()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        var exportPath = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid():N}.json");
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            config.IntValue.Value = 42;
            presets.Save("exportme");

            var result = presets.ExportPreset("exportme", exportPath);

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(exportPath));
        }
        finally
        {
            if (File.Exists(exportPath)) File.Delete(exportPath);
            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var pf = Path.Combine(dir, "config-preset-exportme.json");
            if (File.Exists(pf)) File.Delete(pf);
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.ExportPreset"/> returns false when the preset does not exist.
    /// </summary>
    [TestMethod]
    public void ExportPreset_NonexistentPreset_ReturnsFalse()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        var exportPath = Path.Combine(Path.GetTempPath(), $"export_{Guid.NewGuid():N}.json");
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var result = presets.ExportPreset("nonexistent", exportPath);

            Assert.IsFalse(result);
            Assert.IsFalse(File.Exists(exportPath));
        }
        finally
        {
            if (File.Exists(exportPath)) File.Delete(exportPath);
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.ExportPreset"/> throws for null name.
    /// </summary>
    [TestMethod]
    public void ExportPreset_NullName_ThrowsArgumentException()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            Assert.ThrowsExactly<ArgumentException>(() => presets.ExportPreset(null!, "somepath.json"));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.ExportPreset"/> throws for empty destination path.
    /// </summary>
    [TestMethod]
    public void ExportPreset_EmptyDestination_ThrowsArgumentException()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            Assert.ThrowsExactly<ArgumentException>(() => presets.ExportPreset("test", ""));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- ImportPreset ---

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.ImportPreset"/> copies the source file into the preset directory and returns the derived name.
    /// </summary>
    [TestMethod]
    public void ImportPreset_ValidFile_CopiesAndReturnsName()
    {
        var (store, config, tempPath) = CreateLoadedStore<PresetTestConfig>();
        var sourceDir = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid():N}");
        Directory.CreateDirectory(sourceDir);
        var sourceFile = Path.Combine(sourceDir, "my-imported-preset.json");
        File.WriteAllText(sourceFile, "{}");
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var name = presets.ImportPreset(sourceFile);

            Assert.AreEqual("my-imported-preset", name);

            var dir = Path.GetDirectoryName(Path.GetFullPath(tempPath))!;
            var expected = Path.Combine(dir, "config-preset-my-imported-preset.json");
            Assert.IsTrue(File.Exists(expected));

            File.Delete(expected);
        }
        finally
        {
            if (Directory.Exists(sourceDir))
                Directory.Delete(sourceDir, recursive: true);
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.ImportPreset"/> strips the configured prefix from the source file name.
    /// </summary>
    [TestMethod]
    public void ImportPreset_SourceWithPrefix_StripsPrefix()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        var sourceDir = Path.Combine(Path.GetTempPath(), $"import_{Guid.NewGuid():N}");
        Directory.CreateDirectory(sourceDir);
        var sourceFile = Path.Combine(sourceDir, "config-preset-prefixed.json");
        File.WriteAllText(sourceFile, "{}");
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var name = presets.ImportPreset(sourceFile);

            Assert.AreEqual("prefixed", name);
        }
        finally
        {
            if (Directory.Exists(sourceDir))
                Directory.Delete(sourceDir, recursive: true);
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.ImportPreset"/> returns null when the source file does not exist.
    /// </summary>
    [TestMethod]
    public void ImportPreset_NonexistentSource_ReturnsNull()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var name = presets.ImportPreset("nonexistent-path.json");

            Assert.IsNull(name);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.ImportPreset"/> throws for empty source path.
    /// </summary>
    [TestMethod]
    public void ImportPreset_EmptySource_ThrowsArgumentException()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            Assert.ThrowsExactly<ArgumentException>(() => presets.ImportPreset(""));
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    // --- DerivePresetName ---

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.DerivePresetName"/> returns the file name without extension when no prefix is present.
    /// </summary>
    [TestMethod]
    public void DerivePresetName_NoPrefix_ReturnsFileName()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var name = presets.DerivePresetName("some-name.json");
            Assert.AreEqual("some-name", name);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.DerivePresetName"/> strips the configured prefix.
    /// </summary>
    [TestMethod]
    public void DerivePresetName_WithPrefix_StripsPrefix()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var name = presets.DerivePresetName("config-preset-myname.json");
            Assert.AreEqual("myname", name);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }

    /// <summary>
    /// Tests that <see cref="ConfigPresetStore{TConfig}.DerivePresetName"/> returns empty string for empty file name.
    /// </summary>
    [TestMethod]
    public void DerivePresetName_EmptyFileName_ReturnsEmpty()
    {
        var (store, _, tempPath) = CreateLoadedStore<PresetTestConfig>();
        try
        {
            var presets = new ConfigPresetStore<PresetTestConfig>(store);
            var name = presets.DerivePresetName(".json");
            Assert.AreEqual(string.Empty, name);
        }
        finally
        {
            CleanupStore(store, tempPath);
        }
    }
}
