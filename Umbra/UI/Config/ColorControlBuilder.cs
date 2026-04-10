using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Builds the built-in color control draw action for <see cref="Vector4"/> parameters.
/// </summary>
/// <remarks>
/// Renders an <see cref="ImGui.ColorEdit4(string, ref Vector4, ImGuiColorEditFlags)"/> widget inside
/// the standard two-column parameter layout.
/// </remarks>
internal static class ColorControlBuilder
{
    /// <summary>
    /// Builds the per-frame draw action for a <see cref="Vector4"/> color parameter.
    /// </summary>
    internal static Action BuildColor(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<Vector4>)parameter;
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);

        return () =>
        {
            var v = p.Value;
            layout.Pre();
            if (ImGui.ColorEdit4(layout.HiddenLabel, ref v))
                p.Value = v;
        };
    }
}
