using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Builds the built-in text-input draw actions used by Umbra's configuration UI.
/// </summary>
internal static class TextControlBuilder
{
    private static readonly ITextOps _textOps = ImGuiConfigRenderContext.Instance;

    /// <summary>
    /// Builds the per-frame draw action for a <see cref="string"/> parameter.
    /// </summary>
    /// <remarks>
    /// When <see cref="ParameterMetadata.MultilineLines"/> is present, the returned action renders a multi-line text input; otherwise, it renders the standard single-line text input.
    /// When <paramref name="textEditSink"/> is non-null, the returned action notifies the sink of interaction boundaries via <see cref="ITextEditSink.BeginTextEdit"/> and <see cref="ITextEditSink.EndTextEdit"/>.
    /// </remarks>
    internal static Action BuildString(string label, IParameter parameter, LabelAlignmentGroup alignGroup, ITextEditSink? textEditSink = null)
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

                if (textEditSink is not null)
                {
                    if (ImGui.IsItemActivated()) textEditSink.BeginTextEdit(parameter);
                    if (ImGui.IsItemDeactivated()) textEditSink.EndTextEdit(parameter);
                }

                ValidationMessageRenderer.Draw(parameter, _textOps);
            };
        }

        return () =>
        {
            var v = p.Value ?? string.Empty;
            layout.Pre();
            if (ImGui.InputText(layout.HiddenLabel, ref v, maxLen)) p.Value = v;

            if (textEditSink is not null)
            {
                if (ImGui.IsItemActivated()) textEditSink.BeginTextEdit(parameter);
                if (ImGui.IsItemDeactivated()) textEditSink.EndTextEdit(parameter);
            }

            ValidationMessageRenderer.Draw(parameter, _textOps);
        };
    }
}
