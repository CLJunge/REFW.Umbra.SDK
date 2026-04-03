using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Builds the built-in text-input draw actions used by Umbra's configuration UI.
/// </summary>
internal static class TextControlBuilder
{
    /// <summary>
    /// Builds the per-frame draw action for a <see cref="string"/> parameter.
    /// </summary>
    /// <remarks>
    /// When <see cref="ParameterMetadata.MultilineLines"/> is present, the returned action renders a multi-line text input; otherwise, it renders the standard single-line text input.
    /// </remarks>
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
