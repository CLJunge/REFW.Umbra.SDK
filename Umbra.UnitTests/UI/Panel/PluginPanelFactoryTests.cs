using Umbra.UI.Config;
using Umbra.UI.Panel;

namespace Umbra.UI.Panel.UnitTests;

[TestClass]
public sealed class PluginPanelFactoryTests
{
    [TestMethod]
    public void Create_WithNullConfigFilePath_ThrowsArgumentNullException()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => PluginPanelFactory.Create<StubConfig>(null!, "panel", new ConfigDrawerOptions()));

        Assert.AreEqual("configFilePath", ex.ParamName);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_WithEmptyOrWhitespaceConfigFilePath_ThrowsArgumentException(string configFilePath)
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(
            () => PluginPanelFactory.Create<StubConfig>(configFilePath, "panel", new ConfigDrawerOptions()));

        Assert.AreEqual("configFilePath", ex.ParamName);
    }

    [TestMethod]
    public void CreateSimple_WithNullConfigFilePath_ThrowsArgumentNullException()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => PluginPanelFactory.Create<StubConfig>(null!, "panel"));

        Assert.AreEqual("configFilePath", ex.ParamName);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void CreateSimple_WithEmptyOrWhitespaceConfigFilePath_ThrowsArgumentException(string configFilePath)
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(
            () => PluginPanelFactory.Create<StubConfig>(configFilePath, "panel"));

        Assert.AreEqual("configFilePath", ex.ParamName);
    }

    [TestMethod]
    public void Create_WithNullPanelIdScope_ThrowsArgumentNullException()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => PluginPanelFactory.Create<StubConfig>("config.json", null!, new ConfigDrawerOptions()));

        Assert.AreEqual("panelIdScope", ex.ParamName);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Create_WithEmptyOrWhitespacePanelIdScope_ThrowsArgumentException(string panelIdScope)
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(
            () => PluginPanelFactory.Create<StubConfig>("config.json", panelIdScope, new ConfigDrawerOptions()));

        Assert.AreEqual("panelIdScope", ex.ParamName);
    }

    [TestMethod]
    public void CreateSimple_WithNullPanelIdScope_ThrowsArgumentNullException()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => PluginPanelFactory.Create<StubConfig>("config.json", null!));

        Assert.AreEqual("panelIdScope", ex.ParamName);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void CreateSimple_WithEmptyOrWhitespacePanelIdScope_ThrowsArgumentException(string panelIdScope)
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(
            () => PluginPanelFactory.Create<StubConfig>("config.json", panelIdScope));

        Assert.AreEqual("panelIdScope", ex.ParamName);
    }

    private sealed class StubConfig
    {
    }
}
