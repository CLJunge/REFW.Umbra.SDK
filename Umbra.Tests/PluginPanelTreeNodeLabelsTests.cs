using System.Reflection;
using Umbra.Tests.TestSupport;
using Umbra.UI.Panel;
using Xunit;

namespace Umbra.Tests;

public sealed class PluginPanelTreeNodeLabelsTests
{
    [Fact]
    public void WarnIfInvalid_TracksEachInvalidSectionLabelOnlyOnce()
    {
        using var _ = new LoggerTestScope();
        var labelType = typeof(PluginPanel).Assembly.GetType("Umbra.UI.Panel.PluginPanelTreeNodeLabels")!;
        var warnedField = labelType.GetField("s_warnedInvalidLabels", BindingFlags.Static | BindingFlags.NonPublic)!;
        var warnedLabels = (HashSet<(string, string)>)warnedField.GetValue(null)!;
        warnedLabels.Clear();

        var warnMethod = labelType.GetMethod("WarnIfInvalid", BindingFlags.Static | BindingFlags.NonPublic)!;
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
