using Hexa.NET.ImGui;
using Umbra.UI.Config.Drawers;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Renders a custom editable layout for <see cref="PluginConfig.NestedDrawerTest"/>.
/// The drawer intentionally uses fixed local widget labels such as <c>"Value 1"</c> so multiple
/// nested-group instances can validate that group-level ImGui ID scoping prevents collisions.
/// </summary>
internal sealed class NestedDrawerTestDrawer : INestedGroupDrawer<PluginConfig.NestedDrawerTest>
{
    /// <summary>
    /// Draws the nested group and writes changed values back to the underlying parameters.
    /// </summary>
    /// <param name="groupInstance">The nested configuration group being rendered.</param>
    public void Draw(PluginConfig.NestedDrawerTest groupInstance)
    {
        ImGui.TextWrapped("This custom nested-group drawer demonstrates full ImGui layout control while still editing persisted Umbra parameters.");
        ImGui.Separator();

        DrawValue1(groupInstance);
        DrawValue2(groupInstance);
        DrawValue3(groupInstance);
        DrawValue4(groupInstance);
    }

    /// <summary>
    /// Draws and updates the sample integer parameter.
    /// </summary>
    /// <param name="groupInstance">The nested configuration group being rendered.</param>
    private static void DrawValue1(PluginConfig.NestedDrawerTest groupInstance)
    {
        var value = groupInstance.Value1.Value;
        if (ImGui.InputInt("Value 1", ref value))
            groupInstance.Value1.Value = value;
    }

    /// <summary>
    /// Draws and updates the sample boolean parameter.
    /// </summary>
    /// <param name="groupInstance">The nested configuration group being rendered.</param>
    private static void DrawValue2(PluginConfig.NestedDrawerTest groupInstance)
    {
        var value = groupInstance.Value2.Value;
        if (ImGui.Checkbox("Value 2", ref value))
            groupInstance.Value2.Value = value;
    }

    /// <summary>
    /// Draws and updates the sample string parameter.
    /// </summary>
    /// <param name="groupInstance">The nested configuration group being rendered.</param>
    private static void DrawValue3(PluginConfig.NestedDrawerTest groupInstance)
    {
        var value = groupInstance.Value3.Value ?? string.Empty;
        if (ImGui.InputText("Value 3", ref value, 256u))
            groupInstance.Value3.Value = value;
    }

    /// <summary>
    /// Draws and updates the sample float parameter.
    /// </summary>
    /// <param name="groupInstance">The nested configuration group being rendered.</param>
    private static void DrawValue4(PluginConfig.NestedDrawerTest groupInstance)
    {
        var value = groupInstance.Value4.Value;
        if (ImGui.DragFloat("Value 4", ref value, 0.01f, 0f, 0f, "%.2f"))
            groupInstance.Value4.Value = value;
    }
}
