using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config.Attributes;
using Umbra.UI.Config.Drawers;
using Umbra.UI.Config.Nodes;

namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Provides the shared ImGui-backed implementation of the rendering seams used by configuration drawers and draw nodes.
/// </summary>
/// <remarks>
/// A single production instance centralizes direct <see cref="ImGui"/> and <see cref="ImGuiWidgets"/> calls while the consumer-facing interfaces remain narrow and test-friendly.
/// </remarks>
internal sealed class ImGuiConfigRenderContext :
    IButtonDrawerRenderer,
    IConfigTransferDrawerRenderer,
    IHotkeyDrawerRenderer,
    ICategoryNodeRenderer,
    IRootTreeNodeRenderer,
    IIdScopeNodeRenderer,
    IParameterNodeRenderer,
    IConfigDrawerRenderer,
    INumericControlOps,
    IColorControlOps,
    IButtonStyleColorSink
{
    /// <summary>
    /// Gets the shared production render context instance.
    /// </summary>
    internal static ImGuiConfigRenderContext Instance { get; } = new();

    private ImGuiConfigRenderContext()
    {
    }

    /// <inheritdoc/>
    public void Text(string text) => ImGui.Text(text);

    /// <inheritdoc/>
    public void TextDisabled(string text) => ImGui.TextDisabled(text);

    /// <inheritdoc/>
    public void TextColored(Vector4 color, string text) => ImGui.TextColored(color, text);

    /// <inheritdoc/>
    public void SameLine() => ImGui.SameLine();

    /// <inheritdoc/>
    public void DrawHelpMarker(string description) => ImGuiWidgets.DrawHelpMarker(description);

    /// <inheritdoc/>
    public bool Button(string label) => ImGui.Button(label);

    /// <inheritdoc/>
    public bool Button(string label, Vector2 size) => ImGui.Button(label, size);

    /// <inheritdoc/>
    public void BeginDisabled(bool disabled) => ImGui.BeginDisabled(disabled);

    /// <inheritdoc/>
    public void EndDisabled() => ImGui.EndDisabled();

    /// <inheritdoc/>
    public float GetAvailableWidth() => ImGui.GetContentRegionAvail().X;

    /// <inheritdoc/>
    public float GetItemSpacingX() => ImGui.GetStyle().ItemSpacing.X;

    /// <inheritdoc/>
    public float GetTextWidth(string text) => ImGui.CalcTextSize(text).X;

    /// <inheritdoc/>
    public float GetButtonWidth(string label)
        => ImGui.CalcTextSize(GetVisibleLabelText(label)).X + (ImGui.GetStyle().FramePadding.X * 2f);

    /// <inheritdoc/>
    public void SetNextItemWidth(float width) => ImGui.SetNextItemWidth(width);

    /// <inheritdoc/>
    public bool InputText(string label, ref string value, uint maxLength) => ImGui.InputText(label, ref value, maxLength);

    /// <inheritdoc/>
    public void Separator() => ImGui.Separator();

    /// <inheritdoc/>
    public bool PushButtonColors(ButtonStyle style) => ButtonStyleColors.Push(style);

    /// <inheritdoc/>
    public bool PushButtonColors(Vector4 normal, Vector4 hovered, Vector4 active) => ButtonStyleColors.Push(normal, hovered, active);

    /// <inheritdoc/>
    public void PopButtonColors() => ButtonStyleColors.Pop();

    /// <inheritdoc/>
    public void Indent(float amount) => ImGui.Indent(amount);

    /// <inheritdoc/>
    public void Unindent(float amount) => ImGui.Unindent(amount);

    /// <inheritdoc/>
    public void SeparatorText(string label) => ImGui.SeparatorText(label);

    /// <inheritdoc/>
    public bool TreeNode(string label, bool defaultOpen, bool? openState = null, bool forceOpen = false)
    {
        if (forceOpen)
            ImGui.SetNextItemOpen(true, ImGuiCond.Always);
        else if (openState.HasValue)
            ImGui.SetNextItemOpen(openState.Value, ImGuiCond.Always);

        var flags = defaultOpen ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
        return ImGui.TreeNodeEx(label, flags);
    }

    /// <inheritdoc/>
    public void TreePop() => ImGui.TreePop();

    /// <inheritdoc/>
    public void PushId(string id) => ImGui.PushID(id);

    /// <inheritdoc/>
    public void PopId() => ImGui.PopID();

    /// <inheritdoc/>
    public void Spacing() => ImGui.Spacing();

    /// <inheritdoc/>
    public void SetScrollHereY(float centerYRatio) => ImGui.SetScrollHereY(centerYRatio);

    /// <inheritdoc/>
    public void SetKeyboardFocusHere() => ImGui.SetKeyboardFocusHere();

    /// <inheritdoc/>
    public void PushStyleColor(ImGuiCol color, Vector4 value) => ImGui.PushStyleColor(color, value);

    /// <inheritdoc/>
    public void PopStyleColor(int count) => ImGui.PopStyleColor(count);

    /// <inheritdoc/>
    public bool Combo(string label, ref int selectedIndex, string[] items, int itemCount)
        => ImGui.Combo(label, ref selectedIndex, items, itemCount);

    /// <inheritdoc/>
    public bool SliderInt(string label, ref int value, int min, int max, string format)
        => ImGui.SliderInt(label, ref value, min, max, format);

    /// <inheritdoc/>
    public bool DragInt(string label, ref int value, float speed, int min, int max, string format)
        => ImGui.DragInt(label, ref value, speed, min, max, format);

    /// <inheritdoc/>
    public bool SliderFloat(string label, ref float value, float min, float max, string format)
        => ImGui.SliderFloat(label, ref value, min, max, format);

    /// <inheritdoc/>
    public bool DragFloat(string label, ref float value, float speed, float min, float max, string format)
        => ImGui.DragFloat(label, ref value, speed, min, max, format);

    /// <inheritdoc/>
    public bool SliderDouble(string label, ref double value, double min, double max, string format)
        => SliderDoubleCore(label, ref value, min, max, format);

    /// <inheritdoc/>
    public bool DragDouble(string label, ref double value, float speed, string format)
        => DragDoubleCore(label, ref value, speed, format);

    /// <inheritdoc/>
    public bool IsItemActivated() => ImGui.IsItemActivated();

    /// <inheritdoc/>
    public bool IsItemDeactivated() => ImGui.IsItemDeactivated();

    /// <inheritdoc/>
    public bool ColorEdit4(string label, ref Vector4 value) => ImGui.ColorEdit4(label, ref value);

    /// <inheritdoc/>
    public bool IsMouseDown() => ImGui.IsMouseDown(ImGuiMouseButton.Left);

    private static unsafe bool SliderDoubleCore(string label, ref double value, double min, double max, string format)
    {
        fixed (double* pValue = &value)
            return ImGui.SliderScalar(label, ImGuiDataType.Double, pValue, &min, &max, format);
    }

    private static unsafe bool DragDoubleCore(string label, ref double value, float speed, string format)
    {
        fixed (double* pValue = &value)
            return ImGui.DragScalar(label, ImGuiDataType.Double, pValue, speed, format);
    }

    private static string GetVisibleLabelText(string label)
    {
        var hiddenIdIndex = label.IndexOf("##", StringComparison.Ordinal);
        return hiddenIdIndex >= 0 ? label[..hiddenIdIndex] : label;
    }
}
