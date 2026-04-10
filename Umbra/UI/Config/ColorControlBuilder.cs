using System.Numerics;
using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Builds the built-in color control draw action for <see cref="Vector4"/> parameters.
/// </summary>
/// <remarks>
/// <para>
/// Renders an <see cref="Hexa.NET.ImGui.ImGui.ColorEdit4(string, ref Vector4, Hexa.NET.ImGui.ImGuiColorEditFlags)"/>
/// widget inside the standard two-column parameter layout.
/// </para>
/// <para>
/// <c>ColorEdit4</c> is a compound ImGui widget that contains four RGBA drag controls and a
/// color-button picker popup. Because <c>IsItemActivated</c>/<c>IsItemDeactivated</c> after
/// the compound call only apply to the last rendered sub-item (the color button), not to the
/// individual RGBA drag controls or the picker popup, this builder uses mouse-state-aware
/// boundary detection: the edit begins on the first frame that reports a value change and ends
/// on the first subsequent frame where no change is reported <em>and</em> the left mouse
/// button is released. This prevents premature saves during drag pauses where the user holds
/// the mouse button without moving.
/// </para>
/// </remarks>
internal static class ColorControlBuilder
{
    private static readonly IColorControlOps _colorControlOps = ImGuiConfigRenderContext.Instance;

    /// <summary>
    /// Builds the per-frame draw action for a <see cref="Vector4"/> color parameter.
    /// </summary>
    internal static Action BuildColor(string label, IParameter parameter, LabelAlignmentGroup alignGroup, INumericEditSink? numericEditSink = null)
        => BuildColor(label, parameter, alignGroup, _colorControlOps, numericEditSink, null, null);

    /// <summary>
    /// Builds the per-frame draw action for a <see cref="Vector4"/> color parameter using the
    /// specified control operations and optional numeric-edit sink.
    /// </summary>
    internal static Action BuildColor(
        string label,
        IParameter parameter,
        LabelAlignmentGroup alignGroup,
        IColorControlOps colorControlOps,
        INumericEditSink? numericEditSink = null,
        Action? preDraw = null,
        string? hiddenLabel = null)
    {
        var p = (Parameter<Vector4>)parameter;
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);
        var drawPre = preDraw ?? layout.Pre;
        var controlLabel = hiddenLabel ?? layout.HiddenLabel;
        var editing = false;

        return () =>
        {
            var v = p.Value;
            drawPre();
            var changed = colorControlOps.ColorEdit4(controlLabel, ref v);
            var mouseDown = colorControlOps.IsMouseDown();

            if (changed && !editing)
            {
                editing = true;
                numericEditSink?.BeginNumericEdit(parameter);
            }

            if (changed)
                p.Value = v;

            if (editing && !changed && !mouseDown)
            {
                editing = false;
                numericEditSink?.EndNumericEdit(parameter);
            }
        };
    }
}
