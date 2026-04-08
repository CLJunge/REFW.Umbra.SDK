using Hexa.NET.ImGui;
using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Builds the built-in numeric control draw actions used by Umbra's configuration UI.
/// </summary>
/// <remarks>
/// This helper chooses between slider and drag controls based on whether <see cref="ParameterMetadata.Min"/> and <see cref="ParameterMetadata.Max"/> are both present. Double-valued parameters use ImGui scalar APIs so editing stays at native double precision.
/// </remarks>
internal static class NumericControlBuilder
{
    private static readonly ITextOps _textOps = ImGuiConfigRenderContext.Instance;
    private static readonly INumericControlOps _numericControlOps = ImGuiConfigRenderContext.Instance;

    /// <summary>
    /// Builds the per-frame draw action for an <see cref="int"/> parameter.
    /// </summary>
    internal static Action BuildInt(string label, IParameter parameter, LabelAlignmentGroup alignGroup, INumericEditUndoSink? numericEditUndoSink = null)
        => BuildInt(label, parameter, alignGroup, _numericControlOps, numericEditUndoSink);

    internal static Action BuildInt(
        string label,
        IParameter parameter,
        LabelAlignmentGroup alignGroup,
        INumericControlOps numericControlOps,
        INumericEditUndoSink? numericEditUndoSink = null,
        Action? preDraw = null,
        string? hiddenLabel = null)
    {
        var p = (Parameter<int>)parameter;
        var meta = p.Metadata;
        var fmt = meta.Format ?? "%d";
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);
        var drawPre = preDraw ?? layout.Pre;
        var controlLabel = hiddenLabel ?? layout.HiddenLabel;

        if (meta.Min is not null && meta.Max is not null)
        {
            var iMin = (int)meta.Min.Value;
            var iMax = (int)meta.Max.Value;
            return () =>
            {
                var v = p.Value;
                drawPre();
                var changed = numericControlOps.SliderInt(controlLabel, ref v, iMin, iMax, fmt);
                var activated = numericControlOps.IsItemActivated();
                var deactivated = numericControlOps.IsItemDeactivated();
                if (activated)
                    numericEditUndoSink?.BeginNumericEdit(parameter);
                if (changed)
                    p.Value = v;
                if (deactivated)
                    numericEditUndoSink?.EndNumericEdit(parameter);
                ValidationMessageRenderer.Draw(parameter, _textOps);
            };
        }

        var step = meta.Step.HasValue ? (float)meta.Step : 1f;
        return () =>
        {
            var v = p.Value;
            drawPre();
            var changed = numericControlOps.DragInt(controlLabel, ref v, step, 0, 0, fmt);
            var activated = numericControlOps.IsItemActivated();
            var deactivated = numericControlOps.IsItemDeactivated();
            if (activated)
                numericEditUndoSink?.BeginNumericEdit(parameter);
            if (changed)
                p.Value = v;
            if (deactivated)
                numericEditUndoSink?.EndNumericEdit(parameter);
            ValidationMessageRenderer.Draw(parameter, _textOps);
        };
    }

    /// <summary>
    /// Builds the per-frame draw action for a <see cref="float"/> parameter.
    /// </summary>
    internal static Action BuildFloat(string label, IParameter parameter, LabelAlignmentGroup alignGroup, INumericEditUndoSink? numericEditUndoSink = null)
        => BuildFloat(label, parameter, alignGroup, _numericControlOps, numericEditUndoSink);

    internal static Action BuildFloat(
        string label,
        IParameter parameter,
        LabelAlignmentGroup alignGroup,
        INumericControlOps numericControlOps,
        INumericEditUndoSink? numericEditUndoSink = null,
        Action? preDraw = null,
        string? hiddenLabel = null)
    {
        var p = (Parameter<float>)parameter;
        var meta = p.Metadata;
        var fmt = meta.InferredFloatFormat;
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);
        var drawPre = preDraw ?? layout.Pre;
        var controlLabel = hiddenLabel ?? layout.HiddenLabel;

        if (meta.Min is not null && meta.Max is not null)
        {
            var fMin = (float)meta.Min.Value;
            var fMax = (float)meta.Max.Value;
            return () =>
            {
                var v = p.Value;
                drawPre();
                var changed = numericControlOps.SliderFloat(controlLabel, ref v, fMin, fMax, fmt);
                var activated = numericControlOps.IsItemActivated();
                var deactivated = numericControlOps.IsItemDeactivated();
                if (activated)
                    numericEditUndoSink?.BeginNumericEdit(parameter);
                if (changed)
                    p.Value = v;
                if (deactivated)
                    numericEditUndoSink?.EndNumericEdit(parameter);
                ValidationMessageRenderer.Draw(parameter, _textOps);
            };
        }

        var step = meta.Step.HasValue ? (float)meta.Step : 1f;
        return () =>
        {
            var v = p.Value;
            drawPre();
            var changed = numericControlOps.DragFloat(controlLabel, ref v, step, 0f, 0f, fmt);
            var activated = numericControlOps.IsItemActivated();
            var deactivated = numericControlOps.IsItemDeactivated();
            if (activated)
                numericEditUndoSink?.BeginNumericEdit(parameter);
            if (changed)
                p.Value = v;
            if (deactivated)
                numericEditUndoSink?.EndNumericEdit(parameter);
            ValidationMessageRenderer.Draw(parameter, _textOps);
        };
    }

    /// <summary>
    /// Builds the per-frame draw action for a <see cref="double"/> parameter.
    /// </summary>
    internal static Action BuildDouble(string label, IParameter parameter, LabelAlignmentGroup alignGroup, INumericEditUndoSink? numericEditUndoSink = null)
        => BuildDouble(label, parameter, alignGroup, _numericControlOps, numericEditUndoSink);

    internal static Action BuildDouble(
        string label,
        IParameter parameter,
        LabelAlignmentGroup alignGroup,
        INumericControlOps numericControlOps,
        INumericEditUndoSink? numericEditUndoSink = null,
        Action? preDraw = null,
        string? hiddenLabel = null)
    {
        var p = (Parameter<double>)parameter;
        var meta = p.Metadata;
        var fmt = meta.InferredFloatFormat;
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);
        var drawPre = preDraw ?? layout.Pre;
        var controlLabel = hiddenLabel ?? layout.HiddenLabel;

        if (meta.Min is not null && meta.Max is not null)
        {
            var dMin = meta.Min.Value;
            var dMax = meta.Max.Value;
            return () =>
            {
                var v = p.Value;
                drawPre();
                var changed = numericControlOps.SliderDouble(controlLabel, ref v, dMin, dMax, fmt);
                var activated = numericControlOps.IsItemActivated();
                var deactivated = numericControlOps.IsItemDeactivated();
                if (activated)
                    numericEditUndoSink?.BeginNumericEdit(parameter);
                if (changed)
                    p.Value = v;
                if (deactivated)
                    numericEditUndoSink?.EndNumericEdit(parameter);
                ValidationMessageRenderer.Draw(parameter, _textOps);
            };
        }

        var step = meta.Step.HasValue ? (float)meta.Step : 1f;
        return () =>
        {
            var v = p.Value;
            drawPre();
            var changed = numericControlOps.DragDouble(controlLabel, ref v, step, fmt);
            var activated = numericControlOps.IsItemActivated();
            var deactivated = numericControlOps.IsItemDeactivated();
            if (activated)
                numericEditUndoSink?.BeginNumericEdit(parameter);
            if (changed)
                p.Value = v;
            if (deactivated)
                numericEditUndoSink?.EndNumericEdit(parameter);
            ValidationMessageRenderer.Draw(parameter, _textOps);
        };
    }
}
