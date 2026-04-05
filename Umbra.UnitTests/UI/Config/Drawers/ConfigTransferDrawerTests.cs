using Umbra.Config;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigTransferDrawer"/>.
/// </summary>
[TestClass]
public sealed class ConfigTransferDrawerTests
{
    private TestConfigTransferDrawerRenderer _renderer = null!;
    private TestConfigTransferFilePicker _picker = null!;

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

        drawer.Draw(null, static () => { }, static () => { });

        Assert.HasCount(1, _renderer.DisabledTexts);
        Assert.AreEqual("(ConfigTransferDrawer requires a non-null config-file path parameter)", _renderer.DisabledTexts[0]);
    }

    /// <summary>
    /// Verifies that the shared path input reserves space for the inline import-file status indicator.
    /// </summary>
    [TestMethod]
    public void Draw_Layout_ComputesSharedPathInputWidth()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");

        drawer.Draw(pathParameter, static () => { }, static () => { });

        Assert.AreEqual(376f, _renderer.Widths[0]);
    }

    /// <summary>
    /// Verifies that editing the shared path updates the parameter through the validating setter.
    /// </summary>
    [TestMethod]
    public void Draw_ConfigFilePathEdited_UpdatesParameterValue()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter(
            "configFilePath",
            "old-config.json",
            new ParameterMetadata { MaxLength = 512 });
        _renderer.InputResults.Enqueue((true, "new-config.json"));

        drawer.Draw(pathParameter, static () => { }, static () => { });

        Assert.AreEqual("new-config.json", pathParameter.Value);
        Assert.HasCount(1, _renderer.Inputs);
        Assert.AreEqual("##configFilePath", _renderer.Inputs[0].Label);
        Assert.AreEqual((uint)512, _renderer.Inputs[0].MaxLength);
    }

    /// <summary>
    /// Verifies that the import and export buttons use equal widths and fill the second row evenly.
    /// </summary>
    [TestMethod]
    public void Draw_ActionButtons_UseEqualWidths()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");

        drawer.Draw(pathParameter, static () => { }, static () => { });

        Assert.HasCount(2, _renderer.SizedButtons);
        Assert.AreEqual(296f, _renderer.SizedButtons[0].Size.X);
        Assert.AreEqual(296f, _renderer.SizedButtons[1].Size.X);
    }

    /// <summary>
    /// Verifies that the transfer drawer renders a trailing separator by default.
    /// </summary>
    [TestMethod]
    public void Draw_ByDefault_RendersTrailingSeparator()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");

        drawer.Draw(pathParameter, static () => { }, static () => { });

        Assert.AreEqual(1, _renderer.SeparatorCount);
    }

    /// <summary>
    /// Verifies that the trailing separator can be disabled.
    /// </summary>
    [TestMethod]
    public void Draw_WhenSeparatorDisabled_DoesNotRenderTrailingSeparator()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");

        drawer.Draw(pathParameter, static () => { }, static () => { }, drawSeparatorBelowButtons: false);

        Assert.AreEqual(0, _renderer.SeparatorCount);
    }

    /// <summary>
    /// Verifies that missing import files render a red status and keep the import action disabled.
    /// </summary>
    [TestMethod]
    public void Draw_WhenImportFileMissing_RendersMissingStatusAndDisablesImport()
    {
        var importCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter("configFilePath", "missing-config.json");
        _renderer.SizedButtonResults.Enqueue(true);

        drawer.Draw(pathParameter, () => importCount++, static () => { });

        Assert.AreEqual(0, importCount);
        Assert.HasCount(1, _renderer.ColoredTexts);
        Assert.AreEqual("X", _renderer.ColoredTexts[0].Text);
        Assert.HasCount(1, _renderer.DisabledScopes);
        Assert.IsTrue(_renderer.DisabledScopes[0]);
        Assert.AreEqual(1, _renderer.EndDisabledCallCount);
        Assert.AreEqual("Import##configFilePath", _renderer.SizedButtons[0].Label);
    }

    /// <summary>
    /// Verifies that existing import files render a green status and allow the import action.
    /// </summary>
    [TestMethod]
    public void Draw_WhenImportFileExists_RendersExistsStatusAndInvokesImportAction()
    {
        using var tempDirectory = new TempDirectory();
        var importCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var existingFilePath = Path.Combine(tempDirectory.Path, "config.json");
        File.WriteAllText(existingFilePath, "{}");
        var pathParameter = CreateStringParameter("configFilePath", existingFilePath);
        _renderer.SizedButtonResults.Enqueue(true);
        _renderer.SizedButtonResults.Enqueue(false);

        drawer.Draw(pathParameter, () => importCount++, static () => { });

        Assert.AreEqual(1, importCount);
        Assert.HasCount(1, _renderer.ColoredTexts);
        Assert.AreEqual("OK", _renderer.ColoredTexts[0].Text);
        Assert.HasCount(1, _renderer.DisabledScopes);
        Assert.IsFalse(_renderer.DisabledScopes[0]);
    }

    /// <summary>
    /// Verifies that export remains enabled even when the import path does not exist.
    /// </summary>
    [TestMethod]
    public void Draw_WhenExportButtonClicked_InvokesExportActionEvenWhenImportDisabled()
    {
        var exportCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter("configFilePath", "missing-config.json");
        _renderer.SizedButtonResults.Enqueue(true);

        drawer.Draw(pathParameter, static () => { }, () => exportCount++);

        Assert.AreEqual(1, exportCount);
        Assert.AreEqual("Export##configFilePath", _renderer.SizedButtons[1].Label);
    }

    /// <summary>
    /// Verifies that selecting an import file from the browse popup updates the shared path.
    /// </summary>
    [TestMethod]
    public void Draw_WhenBrowseImportSelected_UpdatesConfigFilePath()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");
        const string fallbackDirectory = @"C:\plugin-data";
        _renderer.ButtonResults.Enqueue(true);
        _renderer.BeginPopupResults.Enqueue(true);
        _renderer.SelectableResults.Enqueue(true);
        _renderer.SelectableResults.Enqueue(false);
        _picker.ImportResults.Enqueue((true, "selected-import.json"));

        drawer.Draw(pathParameter, static () => { }, static () => { }, fallbackDirectory);

        Assert.AreEqual("selected-import.json", pathParameter.Value);
        Assert.AreEqual(1, _picker.ImportPickCallCount);
        Assert.AreEqual(fallbackDirectory, _picker.LastImportFallbackDirectory);
        Assert.AreEqual("ConfigTransferBrowse##configFilePath", _renderer.OpenedPopups[0]);
        Assert.AreEqual("Choose import file...", _renderer.Selectables[0]);
        Assert.AreEqual("Choose export destination...", _renderer.Selectables[1]);
    }

    /// <summary>
    /// Verifies that selecting an export file from the browse popup updates the shared path.
    /// </summary>
    [TestMethod]
    public void Draw_WhenBrowseExportSelected_UpdatesConfigFilePath()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter("configFilePath", "config.json");
        const string fallbackDirectory = @"C:\plugin-data";
        _renderer.ButtonResults.Enqueue(true);
        _renderer.BeginPopupResults.Enqueue(true);
        _renderer.SelectableResults.Enqueue(false);
        _renderer.SelectableResults.Enqueue(true);
        _picker.ExportResults.Enqueue((true, "selected-export.json"));

        drawer.Draw(pathParameter, static () => { }, static () => { }, fallbackDirectory);

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
        var pathParameter = CreateStringParameter("configFilePath", "config.json");
        _renderer.ButtonResults.Enqueue(true);
        _renderer.BeginPopupResults.Enqueue(true);
        _renderer.SelectableResults.Enqueue(true);
        _renderer.SelectableResults.Enqueue(false);
        _picker.ImportResults.Enqueue((false, null));

        drawer.Draw(pathParameter, static () => { }, static () => { });

        Assert.AreEqual("config.json", pathParameter.Value);
    }

    /// <summary>
    /// Verifies that invalid path edits keep the previous value and render inline validation feedback.
    /// </summary>
    [TestMethod]
    public void Draw_InvalidConfigFilePath_RendersValidationMessageAndPreservesPreviousValue()
    {
        var drawer = new ConfigTransferDrawer(_renderer, _picker);
        var pathParameter = CreateStringParameter(
            "configFilePath",
            "valid-config.json",
            new ParameterMetadata { Required = true, AllowWhitespace = false });
        _renderer.InputResults.Enqueue((true, "   "));

        drawer.Draw(pathParameter, static () => { }, static () => { });

        Assert.AreEqual("valid-config.json", pathParameter.Value);
        Assert.HasCount(1, _renderer.ColoredTexts);
        Assert.AreEqual("Value cannot be whitespace only.", _renderer.ColoredTexts[0].Text);
    }

    private static Parameter<string> CreateStringParameter(string key, string value, ParameterMetadata? metadata = null)
    {
        var parameter = new Parameter<string>(value);
        ((IParameterRegistration)parameter).Key = key;
        ((IParameterRegistration)parameter).Metadata = metadata ?? new ParameterMetadata();
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
