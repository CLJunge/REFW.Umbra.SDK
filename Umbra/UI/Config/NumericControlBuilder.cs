using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Builds per-frame ImGui draw actions for built-in numeric parameter types.
/// </summary>
/// <remarks>
/// This type isolates slider/drag composition for <see cref="int"/>, <see cref="float"/>, and
/// <see cref="double"/> parameters from <see cref="ControlFactory"/>.
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
    /// Builds a per-frame draw action that renders either a <c>SliderFloat</c> or an unconstrained
    /// <c>DragFloat</c> for a <see cref="double"/> parameter, narrowing to <see cref="float"/> for
    /// the ImGui call and widening back on assignment.
    /// </summary>
    internal static Action BuildDouble(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<double>)parameter;
        var meta = p.Metadata;
        var fmt = meta.InferredFloatFormat;
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);

        if (meta.Min is not null && meta.Max is not null)
        {
            var fMin = (float)meta.Min.Value;
            var fMax = (float)meta.Max.Value;
            return () =>
            {
                var v = (float)p.Value;
                layout.Pre();
                if (ImGui.SliderFloat(layout.HiddenLabel, ref v, fMin, fMax, fmt)) p.Value = (double)v;
            };
        }

        return () =>
        {
            var v = (float)p.Value;
            var step = meta.Step.HasValue ? (float)meta.Step : 1f;
            layout.Pre();
            if (ImGui.DragFloat(layout.HiddenLabel, ref v, step, 0f, 0f, fmt)) p.Value = (double)v;
        };
    }
}
