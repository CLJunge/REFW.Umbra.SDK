using Umbra.Config;

namespace Umbra.UI.Config.Drawers.UnitTests;

/// <summary>
/// Unit tests for <see cref="ConfigTransferDrawer"/>.
/// </summary>
[TestClass]
public sealed class ConfigTransferDrawerTests
{
    private TestConfigTransferDrawerRenderer _renderer = null!;

    [TestInitialize]
    public void TestInitialize() => _renderer = new TestConfigTransferDrawerRenderer();

    /// <summary>
    /// Verifies that drawing a null transfer group renders disabled explanatory text.
    /// </summary>
    [TestMethod]
    public void Draw_NullGroup_ShowsDisabledText()
    {
        var drawer = new ConfigTransferDrawer(_renderer);

        drawer.Draw(null!);

        Assert.AreEqual(1, _renderer.DisabledTexts.Count);
        Assert.AreEqual("(ConfigTransferDrawer requires a non-null config transfer group)", _renderer.DisabledTexts[0]);
    }

    /// <summary>
    /// Verifies that a bad transfer-group implementation with a null path parameter fails safely.
    /// </summary>
    [TestMethod]
    public void Draw_GroupWithNullImportPath_ShowsDisabledText()
    {
        var drawer = new ConfigTransferDrawer(_renderer);
        var group = new InvalidTransferGroup
        {
            ImportPath = null!,
            ExportPath = CreateStringParameter("exportPath", "export.json"),
            ImportConfig = new Parameter<Action>(static () => { }),
            ExportConfig = new Parameter<Action>(static () => { })
        };

        drawer.Draw(group);

        Assert.AreEqual(1, _renderer.DisabledTexts.Count);
        Assert.AreEqual("(ConfigTransferDrawer requires a non-null 'Import Path' parameter)", _renderer.DisabledTexts[0]);
    }

    /// <summary>
    /// Verifies that editing the import path updates the parameter through the validating setter.
    /// </summary>
    [TestMethod]
    public void Draw_ImportPathEdited_UpdatesParameterValue()
    {
        var drawer = new ConfigTransferDrawer(_renderer);
        var group = CreateValidGroup();
        group.ImportPath = CreateStringParameter(
            "importPath",
            "old-import.json",
            new ParameterMetadata { MaxLength = 512 });
        _renderer.InputResults.Enqueue((true, "new-import.json"));

        drawer.Draw(group);

        Assert.AreEqual("new-import.json", group.ImportPath.Value);
        Assert.AreEqual(2, _renderer.Inputs.Count);
        Assert.AreEqual("##importPath", _renderer.Inputs[0].Label);
        Assert.AreEqual((uint)512, _renderer.Inputs[0].MaxLength);
    }

    /// <summary>
    /// Verifies that editing the export path updates the parameter through the validating setter.
    /// </summary>
    [TestMethod]
    public void Draw_ExportPathEdited_UpdatesParameterValue()
    {
        var drawer = new ConfigTransferDrawer(_renderer);
        var group = CreateValidGroup();
        _renderer.InputResults.Enqueue((false, group.ImportPath.Value ?? string.Empty));
        _renderer.InputResults.Enqueue((true, "new-export.json"));

        drawer.Draw(group);

        Assert.AreEqual("new-export.json", group.ExportPath.Value);
        Assert.AreEqual("##exportPath", _renderer.Inputs[1].Label);
    }

    /// <summary>
    /// Verifies that clicking the import button invokes the configured import action exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WhenImportButtonClicked_InvokesImportAction()
    {
        var importCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer);
        var group = CreateValidGroup();
        group.ImportConfig = new Parameter<Action>(() => importCount++);
        _renderer.ButtonResults.Enqueue(true);
        _renderer.ButtonResults.Enqueue(false);

        drawer.Draw(group);

        Assert.AreEqual(1, importCount);
        Assert.AreEqual("Import##importPath", _renderer.Buttons[0]);
    }

    /// <summary>
    /// Verifies that clicking the export button invokes the configured export action exactly once.
    /// </summary>
    [TestMethod]
    public void Draw_WhenExportButtonClicked_InvokesExportAction()
    {
        var exportCount = 0;
        var drawer = new ConfigTransferDrawer(_renderer);
        var group = CreateValidGroup();
        group.ExportConfig = new Parameter<Action>(() => exportCount++);
        _renderer.ButtonResults.Enqueue(false);
        _renderer.ButtonResults.Enqueue(true);

        drawer.Draw(group);

        Assert.AreEqual(1, exportCount);
        Assert.AreEqual("Export##exportPath", _renderer.Buttons[1]);
    }

    /// <summary>
    /// Verifies that invalid path edits keep the previous value and render inline validation feedback.
    /// </summary>
    [TestMethod]
    public void Draw_InvalidImportPath_RendersValidationMessageAndPreservesPreviousValue()
    {
        var drawer = new ConfigTransferDrawer(_renderer);
        var group = CreateValidGroup();
        group.ImportPath = CreateStringParameter(
            "importPath",
            "valid-import.json",
            new ParameterMetadata { Required = true, AllowWhitespace = false });
        _renderer.InputResults.Enqueue((true, "   "));

        drawer.Draw(group);

        Assert.AreEqual("valid-import.json", group.ImportPath.Value);
        Assert.AreEqual(1, _renderer.ColoredTexts.Count);
        Assert.AreEqual("Value cannot be whitespace only.", _renderer.ColoredTexts[0].Text);
    }

    private static TestTransferGroup CreateValidGroup()
        => new()
        {
            ImportPath = CreateStringParameter("importPath", "import.json"),
            ExportPath = CreateStringParameter("exportPath", "export.json"),
            ImportConfig = new Parameter<Action>(static () => { }),
            ExportConfig = new Parameter<Action>(static () => { })
        };

    private static Parameter<string> CreateStringParameter(string key, string value, ParameterMetadata? metadata = null)
    {
        var parameter = new Parameter<string>(value);
        ((IParameterRegistration)parameter).Key = key;
        ((IParameterRegistration)parameter).Metadata = metadata ?? new ParameterMetadata();
        return parameter;
    }

    private sealed class TestTransferGroup : IConfigTransferGroup
    {
        public Parameter<string> ImportPath { get; set; } = null!;

        public Parameter<string> ExportPath { get; set; } = null!;

        public Parameter<Action> ImportConfig { get; set; } = null!;

        public Parameter<Action> ExportConfig { get; set; } = null!;
    }

    private sealed class InvalidTransferGroup : IConfigTransferGroup
    {
        public Parameter<string> ImportPath { get; set; } = null!;

        public Parameter<string> ExportPath { get; set; } = null!;

        public Parameter<Action> ImportConfig { get; set; } = null!;

        public Parameter<Action> ExportConfig { get; set; } = null!;
    }
}
