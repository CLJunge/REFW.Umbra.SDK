using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Builds per-frame ImGui draw actions for built-in text parameter types.
/// </summary>
/// <remarks>
/// This type isolates string input control composition from <see cref="ControlFactory"/>.
/// </remarks>
internal static class TextControlBuilder
{
    /// <summary>
    /// Builds a per-frame draw action that renders an <c>InputText</c> or
    /// <c>InputTextMultiline</c> field for a <see cref="string"/> parameter.
    /// </summary>
    internal static Action BuildString(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<string>)parameter;
        var meta = p.Metadata;
        var maxLen = meta.MaxLength ?? 256u;
        var layout = ControlFactory.CreateControlLayout(label, parameter, alignGroup);

        if (meta.MultilineLines is int lines)
        {
            return () =>
            {
                var v = p.Value ?? string.Empty;
                var height = ImGui.GetTextLineHeightWithSpacing() * lines;
                layout.Pre();
                if (ImGui.InputTextMultiline(layout.HiddenLabel, ref v, maxLen, new Vector2(0f, height)))
                    p.Value = v;
            };
        }

        return () =>
        {
            var v = p.Value ?? string.Empty;
            layout.Pre();
            if (ImGui.InputText(layout.HiddenLabel, ref v, maxLen)) p.Value = v;
        };
    }
}
