using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Builds per-frame ImGui draw actions for built-in numeric parameter types.
/// </summary>
/// <remarks>
/// This type isolates slider/drag composition for <see cref="int"/>, <see cref="float"/>, and
/// <see cref="double"/> parameters from <see cref="ControlFactory"/>. Double-valued controls use
/// ImGui's scalar APIs with <see cref="ImGuiDataType.Double"/> so values are edited at native
/// double precision rather than being narrowed through <see cref="float"/>.
/// </remarks>
internal static class NumericControlBuilder
{
    /// <summary>
    /// Builds a per-frame draw action that renders either a <c>SliderInt</c> or an unconstrained
    /// <c>DragInt</c> for an <see cref="int"/> parameter.
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
            };
        }

        return () =>
        {
            var v = p.Value;
            var step = meta.Step.HasValue ? (float)meta.Step : 1f;
            layout.Pre();
            if (ImGui.DragInt(layout.HiddenLabel, ref v, step, 0, 0, fmt)) p.Value = v;
        };
    }

    /// <summary>
    /// Builds a per-frame draw action that renders either a <c>SliderFloat</c> or an unconstrained
    /// <c>DragFloat</c> for a <see cref="float"/> parameter.
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
            };
        }

        return () =>
        {
            var v = p.Value;
            var step = meta.Step.HasValue ? (float)meta.Step : 1f;
            layout.Pre();
            if (ImGui.DragFloat(layout.HiddenLabel, ref v, step, 0f, 0f, fmt)) p.Value = v;
        };
    }

    /// <summary>
    /// Builds a per-frame draw action that renders either a double-precision scalar slider or an
    /// unconstrained scalar drag control for a <see cref="double"/> parameter.
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
            };
        }

        return () =>
        {
            var v = p.Value;
            var step = meta.Step.HasValue ? (float)meta.Step : 1f;
            layout.Pre();
            if (DragDouble(layout.HiddenLabel, ref v, step, fmt)) p.Value = v;
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
