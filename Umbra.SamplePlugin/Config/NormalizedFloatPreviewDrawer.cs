using Hexa.NET.ImGui;
using Umbra.Config;
using Umbra.UI.Config.Drawers;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Renders a full-row custom editor for a normalized <see cref="float"/> parameter.
/// </summary>
/// <remarks>
/// This drawer intentionally bypasses Umbra's standard two-column layout so the sample plugin can
/// demonstrate the <see cref="IParameterDrawer"/> extension point with a compact self-managed UI.
/// </remarks>
internal sealed class NormalizedFloatPreviewDrawer : IParameterDrawer
{
    /// <summary>
    /// Draws the custom control and writes changes back to the underlying parameter.
    /// </summary>
    /// <param name="label">The human-readable label for the parameter.</param>
    /// <param name="parameter">The configuration parameter being edited.</param>
    public void Draw(string label, IParameter parameter)
    {
        if (parameter is not Parameter<float> typed)
            return;

        var value = typed.Value;
        var percentage = Math.Clamp(value, 0f, 1f) * 100f;

        ImGui.Text($"{label}: {percentage:F0}%");
        ImGui.TextDisabled("This row is rendered by a full custom parameter drawer.");

        var widgetLabel = parameter.Metadata.HiddenLabel ?? $"##{parameter.Key}";
        if (ImGui.SliderFloat(widgetLabel, ref value, 0f, 1f, "%.2f"))
            typed.Value = value;
    }
}
