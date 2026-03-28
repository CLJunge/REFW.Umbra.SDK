using System.Reflection;
using Umbra.Tests.TestSupport;
using Umbra.UI.Panel;
using Xunit;

namespace Umbra.Tests;

public sealed class PluginPanelTreeNodeLabelHelperTests
{
    [Fact]
    public void WarnIfInvalid_TracksEachInvalidSectionLabelOnlyOnce()
    {
        using var _ = new LoggerTestScope();
        var helperType = typeof(PluginPanel).Assembly.GetType("Umbra.UI.Panel.PluginPanelTreeNodeLabelHelper")!;
        var warnedField = helperType.GetField("s_warnedInvalidLabels", BindingFlags.Static | BindingFlags.NonPublic)!;
        var warnedLabels = (HashSet<string>)warnedField.GetValue(null)!;
        warnedLabels.Clear();

        var warnMethod = helperType.GetMethod("WarnIfInvalid", BindingFlags.Static | BindingFlags.NonPublic)!;
        var section = new TestPanelSection("SectionA", "Label##bad");

        warnMethod.Invoke(null, [section]);
        warnMethod.Invoke(null, [section]);

        Assert.Single(warnedLabels);
    }

    private sealed class TestPanelSection(string sectionId, string? treeNodeLabel) : IPanelSection
    {
        public int Order => 0;
        public string SectionId => sectionId;
        public string? TreeNodeLabel => treeNodeLabel;
        public bool TreeNodeDefaultOpen => false;

        public void Draw()
        {
        }

        public void Dispose()
        {
        }
    }
 }
