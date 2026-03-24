using Hexa.NET.ImGui;
using Umbra.Config.UI.ParameterDrawers;
using static Umbra.SamplePlugin.Config.PluginConfig;

namespace Umbra.SamplePlugin.Config;

internal class NestedDrawerTestDrawer : INestedGroupDrawer<NestedDrawerTest>
{
    public void Draw(NestedDrawerTest groupInstance)
    {
        ImGui.Text("This is a custom drawer for the NestedDrawerTest group. It has full control over the layout and styling of its contents.");
        ImGui.Separator();
        ImGui.Text($"Value1: {groupInstance.Value1}");
        ImGui.Separator();
        ImGui.Text($"Value2: {groupInstance.Value2}");
        ImGui.Separator();
        ImGui.Text($"Value3: {groupInstance.Value3}");
        ImGui.Separator();
        ImGui.Text($"Value4: {groupInstance.Value4}");
        ImGui.Separator();
        ImGui.Text("You can add any ImGui controls here to allow users to edit the values, or display them in a custom way.");
    }
}
