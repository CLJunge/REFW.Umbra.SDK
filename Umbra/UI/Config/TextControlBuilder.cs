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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Guard clauses with early returns are more readable than a single chained conditional expression for multi-condition validation.")]
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
                var changed = ImGui.InputTextMultiline(layout.HiddenLabel, ref v, maxLen, new Vector2(0f, height));
                var activated = textEditSink is not null && ImGui.IsItemActivated();
                var deactivated = textEditSink is not null && ImGui.IsItemDeactivated();

                if (activated) textEditSink!.BeginTextEdit(parameter);
                if (changed) p.Value = v;
                if (deactivated) textEditSink!.EndTextEdit(parameter);

                ValidationMessageRenderer.Draw(parameter, _textOps);
            };
        }

        return () =>
        {
            var v = p.Value ?? string.Empty;
            layout.Pre();
            var changed = ImGui.InputText(layout.HiddenLabel, ref v, maxLen);
            var activated = textEditSink is not null && ImGui.IsItemActivated();
            var deactivated = textEditSink is not null && ImGui.IsItemDeactivated();

            if (activated) textEditSink!.BeginTextEdit(parameter);
            if (changed) p.Value = v;
            if (deactivated) textEditSink!.EndTextEdit(parameter);

            ValidationMessageRenderer.Draw(parameter, _textOps);
        };
    }
}
