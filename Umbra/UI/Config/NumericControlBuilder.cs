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

    /// <summary>
    /// Builds the per-frame draw action for an <see cref="int"/> parameter.
    /// </summary>
    internal static Action BuildInt(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<int>)parameter;
        var meta = p.Metadata;
        var fmt = meta.Format ?? "%d";
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);

        if (meta.Min is not null && meta.Max is not null)
        {
            var iMin = (int)meta.Min.Value;
            var iMax = (int)meta.Max.Value;
            return () =>
            {
                var v = p.Value;
                layout.Pre();
                if (ImGui.SliderInt(layout.HiddenLabel, ref v, iMin, iMax, fmt)) p.Value = v;
                ValidationMessageRenderer.Draw(parameter, _textOps);
            };
        }

        var step = meta.Step.HasValue ? (float)meta.Step : 1f;
        return () =>
        {
            var v = p.Value;
            layout.Pre();
            if (ImGui.DragInt(layout.HiddenLabel, ref v, step, 0, 0, fmt)) p.Value = v;
            ValidationMessageRenderer.Draw(parameter, _textOps);
        };
    }

    /// <summary>
    /// Builds the per-frame draw action for a <see cref="float"/> parameter.
    /// </summary>
    internal static Action BuildFloat(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<float>)parameter;
        var meta = p.Metadata;
        var fmt = meta.InferredFloatFormat;
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);

        if (meta.Min is not null && meta.Max is not null)
        {
            var fMin = (float)meta.Min.Value;
            var fMax = (float)meta.Max.Value;
            return () =>
            {
                var v = p.Value;
                layout.Pre();
                if (ImGui.SliderFloat(layout.HiddenLabel, ref v, fMin, fMax, fmt)) p.Value = v;
                ValidationMessageRenderer.Draw(parameter, _textOps);
            };
        }

        var step = meta.Step.HasValue ? (float)meta.Step : 1f;
        return () =>
        {
            var v = p.Value;
            layout.Pre();
            if (ImGui.DragFloat(layout.HiddenLabel, ref v, step, 0f, 0f, fmt)) p.Value = v;
            ValidationMessageRenderer.Draw(parameter, _textOps);
        };
    }

    /// <summary>
    /// Builds the per-frame draw action for a <see cref="double"/> parameter.
    /// </summary>
    internal static Action BuildDouble(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<double>)parameter;
        var meta = p.Metadata;
        var fmt = meta.InferredFloatFormat;
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);

        if (meta.Min is not null && meta.Max is not null)
        {
            var dMin = meta.Min.Value;
            var dMax = meta.Max.Value;
            return () =>
            {
                var v = p.Value;
                layout.Pre();
                if (SliderDouble(layout.HiddenLabel, ref v, dMin, dMax, fmt)) p.Value = v;
                ValidationMessageRenderer.Draw(parameter, _textOps);
            };
        }

        var step = meta.Step.HasValue ? (float)meta.Step : 1f;
        return () =>
        {
            var v = p.Value;
            layout.Pre();
            if (DragDouble(layout.HiddenLabel, ref v, step, fmt)) p.Value = v;
            ValidationMessageRenderer.Draw(parameter, _textOps);
        };
    }

    /// <summary>
    /// Wraps ImGui's scalar slider API for native <see cref="double"/> editing.
    /// </summary>
    /// <param name="label">The ImGui widget label.</param>
    /// <param name="value">The value being edited.</param>
    /// <param name="min">The inclusive slider minimum.</param>
    /// <param name="max">The inclusive slider maximum.</param>
    /// <param name="format">The display format string.</param>
    /// <returns><see langword="true"/> when the value changed; otherwise <see langword="false"/>.</returns>
    private static unsafe bool SliderDouble(string label, ref double value, double min, double max, string format)
    {
        fixed (double* pValue = &value)
            return ImGui.SliderScalar(label, ImGuiDataType.Double, pValue, &min, &max, format);
    }

    /// <summary>
    /// Wraps ImGui's scalar drag API for native <see cref="double"/> editing.
    /// </summary>
    /// <param name="label">The ImGui widget label.</param>
    /// <param name="value">The value being edited.</param>
    /// <param name="speed">The drag speed.</param>
    /// <param name="format">The display format string.</param>
    /// <returns><see langword="true"/> when the value changed; otherwise <see langword="false"/>.</returns>
    private static unsafe bool DragDouble(string label, ref double value, float speed, string format)
    {
        fixed (double* pValue = &value)
            return ImGui.DragScalar(label, ImGuiDataType.Double, pValue, speed, format);
    }
}
