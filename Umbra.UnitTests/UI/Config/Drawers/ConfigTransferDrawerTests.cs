using Umbra.Config;
using Umbra.UI.Config.Transfer;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigTransferDrawer"/>.
/// </summary>
[TestClass]
public sealed class ConfigTransferDrawerTests
{
    private TestConfigTransferDrawerRenderer _renderer = null!;
    private TestConfigTransferFilePicker _picker = null!;
    private static readonly string[] _expectedModes = ["Import", "Export"];

    [TestInitialize]
    public void TestInitialize()
    {
        _renderer = new TestConfigTransferDrawerRenderer();
        _picker = new TestConfigTransferFilePicker();
    }

    /// <summary>
    /// Verifies that drawing a null transfer group renders disabled explanatory text.
    /// </summary>
    [TestMethod]
    public void Draw_NullGroup_ShowsDisabledText()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);

        drawer.Draw(null, null, null);

        Assert.HasCount(1, _renderer.DisabledTexts);
        Assert.AreEqual("(ConfigTransferDrawer requires a non-null config-file path parameter)", _renderer.DisabledTexts[0]);
    }

    /// <summary>
    /// Verifies that a bad transfer-group implementation with a null path parameter fails safely.
    /// </summary>
    [TestMethod]
    public void Draw_NullPathParameter_ShowsDisabledText()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);

        drawer.Draw(modeParameter, null, static () => { }, static () => { });

        Assert.HasCount(1, _renderer.DisabledTexts);
        Assert.AreEqual("(ConfigTransferDrawer requires a non-null config-file path parameter)", _renderer.DisabledTexts[0]);
    }

    /// <summary>
    /// Verifies that the first row reserves width for the mode dropdown and the second row reserves width for the inline status indicator.
    /// </summary>
    [TestMethod]
    public void Draw_Layout_ComputesModeAndPathInputWidths()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.HasCount(2, _renderer.Widths);
        Assert.AreEqual(488f, _renderer.Widths[0]);
        Assert.AreEqual(408f, _renderer.Widths[1]);
    }

    /// <summary>
    /// Verifies that selecting a different transfer mode updates the persisted mode parameter.
    /// </summary>
    [TestMethod]
    public void Draw_WhenModeSelectionChanges_UpdatesModeParameter()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");
        _renderer.ComboResults.Enqueue((true, 1));

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.AreEqual(ConfigTransferMode.Export, modeParameter.Value);
        Assert.HasCount(1, _renderer.Combos);
        Assert.AreEqual("##transferMode", _renderer.Combos[0].Label);
        CollectionAssert.AreEqual(_expectedModes, _renderer.Combos[0].Items);
    }

    /// <summary>
    /// Verifies that editing the shared path updates the parameter through the validating setter.
    /// </summary>
    [TestMethod]
    public void Draw_ConfigFilePathEdited_UpdatesParameterValue()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter(
            "configFilePath",
            "old-config.json",
            new ParameterMetadata { MaxLength = 512 });
        _renderer.InputResults.Enqueue((true, "new-config.json"));

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.AreEqual("new-config.json", pathParameter.Value);
        Assert.HasCount(1, _renderer.Inputs);
        Assert.AreEqual("##configFilePath", _renderer.Inputs[0].Label);
        Assert.AreEqual((uint)512, _renderer.Inputs[0].MaxLength);
    }

    /// <summary>
    /// Verifies that the mode-specific action button fills the full third-row width.
    /// </summary>
    [TestMethod]
    public void Draw_ActionButton_UsesFullWidth()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.HasCount(1, _renderer.SizedButtons);
        Assert.AreEqual(600f, _renderer.SizedButtons[0].Size.X);
    }

    /// <summary>
    /// Verifies that the transfer drawer renders a trailing separator by default.
    /// </summary>
    [TestMethod]
    public void Draw_ByDefault_RendersTrailingSeparator()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.AreEqual(1, _renderer.SeparatorCount);
    }

    /// <summary>
    /// Verifies that the trailing separator can be disabled.
    /// </summary>
    [TestMethod]
    public void Draw_WhenSeparatorDisabled_DoesNotRenderTrailingSeparator()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { }, drawSeparatorBelowButtons: false);

        Assert.AreEqual(0, _renderer.SeparatorCount);
    }

    /// <summary>
    /// Verifies that missing import files render a red status and keep the import action disabled.
    /// </summary>
    [TestMethod]
    public void Draw_WhenImportFileMissing_DisablesImport()
    {
        var importCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter("configFilePath", "missing-config.json");
        _renderer.SizedButtonResults.Enqueue(true);

        drawer.Draw(modeParameter, pathParameter, () => importCount++, static () => { });

        Assert.AreEqual(0, importCount);
        Assert.HasCount(1, _renderer.DisabledScopes);
        Assert.IsTrue(_renderer.DisabledScopes[0]);
        Assert.AreEqual(1, _renderer.EndDisabledCallCount);
        Assert.AreEqual("Import##configFilePath", _renderer.SizedButtons[0].Label);
    }

    /// <summary>
    /// Verifies that existing import files render a green status and allow the import action.
    /// </summary>
    [TestMethod]
    public void Draw_WhenImportFileExists_InvokesImportAction()
    {
        using var tempDirectory = new TempDirectory();
        var importCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var existingFilePath = Path.Combine(tempDirectory.Path, "config.json");
        File.WriteAllText(existingFilePath, "{}");
        var pathParameter = CreateStringParameter("configFilePath", existingFilePath);
        _renderer.SizedButtonResults.Enqueue(true);

        drawer.Draw(modeParameter, pathParameter, () => importCount++, static () => { });

        Assert.AreEqual(1, importCount);
        Assert.HasCount(1, _renderer.DisabledScopes);
        Assert.IsFalse(_renderer.DisabledScopes[0]);
        Assert.AreEqual("Import##configFilePath", _renderer.SizedButtons[0].Label);
    }

    /// <summary>
    /// Verifies that export mode rejects non-json file paths and keeps the export action disabled.
    /// </summary>
    [TestMethod]
    public void Draw_WhenExportPathIsNotJson_DisablesExport()
    {
        var exportCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Export);
        var pathParameter = CreateStringParameter("configFilePath", "config.txt");
        _renderer.SizedButtonResults.Enqueue(true);

        drawer.Draw(modeParameter, pathParameter, static () => { }, () => exportCount++);

        Assert.AreEqual(0, exportCount);
        Assert.IsTrue(_renderer.DisabledScopes[0]);
        Assert.AreEqual("Export##configFilePath", _renderer.SizedButtons[0].Label);
    }

    /// <summary>
    /// Verifies that export mode accepts json file paths and invokes the export action.
    /// </summary>
    [TestMethod]
    public void Draw_WhenExportPathIsJson_InvokesExportAction()
    {
        var exportCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Export);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");
        _renderer.SizedButtonResults.Enqueue(true);

        drawer.Draw(modeParameter, pathParameter, static () => { }, () => exportCount++);

        Assert.AreEqual(1, exportCount);
        Assert.IsFalse(_renderer.DisabledScopes[0]);
        Assert.AreEqual("Export##configFilePath", _renderer.SizedButtons[0].Label);
    }

    /// <summary>
    /// Verifies that selecting an import file from the browse popup updates the shared path in import mode.
    /// </summary>
    [TestMethod]
    public void Draw_WhenBrowseImportSelected_UpdatesConfigFilePath()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");
        const string fallbackDirectory = @"C:\plugin-data";
        _renderer.ButtonResults.Enqueue(true);
        _picker.ImportResults.Enqueue((true, "selected-import.json"));

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { }, fallbackDirectory);

        Assert.AreEqual("selected-import.json", pathParameter.Value);
        Assert.AreEqual(1, _picker.ImportPickCallCount);
        Assert.AreEqual(fallbackDirectory, _picker.LastImportFallbackDirectory);
    }

    /// <summary>
    /// Verifies that selecting an export file from the browse popup updates the shared path in export mode.
    /// </summary>
    [TestMethod]
    public void Draw_WhenBrowseExportSelected_UpdatesConfigFilePath()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Export);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");
        const string fallbackDirectory = @"C:\plugin-data";
        _renderer.ButtonResults.Enqueue(true);
        _picker.ExportResults.Enqueue((true, "selected-export.json"));

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { }, fallbackDirectory);

        Assert.AreEqual("selected-export.json", pathParameter.Value);
        Assert.AreEqual(1, _picker.ExportPickCallCount);
        Assert.AreEqual(fallbackDirectory, _picker.LastExportFallbackDirectory);
    }

    /// <summary>
    /// Verifies that picker cancellation leaves the shared path unchanged.
    /// </summary>
    [TestMethod]
    public void Draw_WhenBrowsePickerCancelled_LeavesConfigFilePathUnchanged()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");
        _renderer.ButtonResults.Enqueue(true);
        _picker.ImportResults.Enqueue((false, null));

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.AreEqual("config.json", pathParameter.Value);
        Assert.AreEqual(1, _picker.ImportPickCallCount);
    }

    /// <summary>
    /// Verifies that invalid path edits keep the previous value and render inline validation feedback.
    /// </summary>
    [TestMethod]
    public void Draw_InvalidConfigFilePath_RendersValidationMessageAndPreservesPreviousValue()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var pathParameter = CreateStringParameter(
            "configFilePath",
            "valid-config.json",
            new ParameterMetadata { Required = true, AllowWhitespace = false });
        _renderer.InputResults.Enqueue((true, "   "));

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.AreEqual("valid-config.json", pathParameter.Value);
        Assert.HasCount(1, _renderer.ColoredTexts);
        Assert.AreEqual("Value cannot be whitespace only.", _renderer.ColoredTexts[0].Text);
    }

    /// <summary>
    /// Verifies that the file-existence result is cached per-path: deleting the file between frames
    /// while the path remains unchanged still reports the file as present, avoiding redundant IO.
    /// Users must edit the path to force a fresh check.
    /// </summary>
    [TestMethod]
    public void Draw_WhenPathUnchanged_UsesCachedFileExistenceResult()
    {
        using var tempDirectory = new TempDirectory();
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var existingFilePath = Path.Combine(tempDirectory.Path, "config.json");
        File.WriteAllText(existingFilePath, "{}");
        var pathParameter = CreateStringParameter("configFilePath", existingFilePath);

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        File.Delete(existingFilePath);
        _renderer.DisabledScopes.Clear();

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.HasCount(1, _renderer.DisabledScopes);
        Assert.IsFalse(_renderer.DisabledScopes[0]);
    }

    /// <summary>
    /// Verifies that changing the path invalidates the cached file-existence result and triggers a
    /// fresh check, so a newly created file is detected correctly.
    /// </summary>
    [TestMethod]
    public void Draw_WhenPathChanges_RechecksFileExistence()
    {
        using var tempDirectory = new TempDirectory();
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var modeParameter = CreateModeParameter("transferMode", ConfigTransferMode.Import);
        var missingFilePath = Path.Combine(tempDirectory.Path, "missing.json");
        var existingFilePath = Path.Combine(tempDirectory.Path, "existing.json");
        File.WriteAllText(existingFilePath, "{}");
        var pathParameter = CreateStringParameter("configFilePath", missingFilePath);

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        _renderer.DisabledScopes.Clear();
        pathParameter.Value = existingFilePath;

        drawer.Draw(modeParameter, pathParameter, static () => { }, static () => { });

        Assert.HasCount(1, _renderer.DisabledScopes);
        Assert.IsFalse(_renderer.DisabledScopes[0]);
    }

    private static Parameter<ConfigTransferMode> CreateModeParameter(string key, ConfigTransferMode value)
    {
        var parameter = new Parameter<ConfigTransferMode>(value);
        ((IParameterRegistration)parameter).Key = key;
        ((IParameterRegistration)parameter).Metadata = new ParameterMetadata { DisplayName = "Transfer Mode" };
        return parameter;
    }

    private static Parameter<string> CreateStringParameter(string key, string value, ParameterMetadata? metadata = null)
    {
        var parameter = new Parameter<string>(value);
        ((IParameterRegistration)parameter).Key = key;
        ((IParameterRegistration)parameter).Metadata = metadata ?? new ParameterMetadata { DisplayName = "Config File" };
        return parameter;
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
