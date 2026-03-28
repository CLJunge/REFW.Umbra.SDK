using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// An <see cref="IParameterDrawer"/> implementation that renders an ImGui push-button for a
/// <see cref="Parameter{T}"/> of type <see cref="Action"/>, invoking the stored action on click.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Umbra.UI.Config.ControlFactory"/> uses this drawer by default for
/// <see cref="Parameter{T}"/> values of type <see cref="Action"/>. One <see cref="ButtonDrawer"/>
/// instance is created per action parameter during draw-tree construction, so warning state remains
/// local to the parameter and no shared global drawer state is required.
/// </para>
/// <para>
/// The button label is sourced from the parameter's <c>DisplayName</c> metadata (set via
/// <c>[DisplayName("...")]</c>). An optional same-line <c>(?)</c> help marker is shown when
/// <c>[Description("...")]</c> is also present, consistent with other drawers in this namespace.
/// </para>
/// <para>
/// Appearance is controlled by optional attributes on the parameter property:
/// <list type="bullet">
///   <item>
///     <term><c>[ButtonStyle(ButtonStyle.Danger)]</c></term>
///     <description>
///       Applies a preset color scheme. See <see cref="ButtonStyle"/> for all variants.
///       Omit for the default ImGui theme colors. Ignored when <c>[CustomButtonColors]</c>
///       is also present.
///     </description>
///   </item>
///   <item>
///     <term><c>[CustomButtonColors(r, g, b)]</c> or <c>[CustomButtonColors(…×12)]</c></term>
///     <description>
///       Applies fully custom RGBA colors for the normal, hovered, and active button states.
///       Takes priority over <c>[ButtonStyle]</c> when both are specified.
///     </description>
///   </item>
///   <item>
///     <term><c>[ControlWidth(-1f)]</c></term>
///     <description>
///       <c>0f</c> (default) = auto-size to label, <c>-1f</c> = fill available width,
///       positive = fixed pixel width.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// Using <see cref="ButtonStyle.Custom"/> without a <c>[CustomButtonColors]</c> attribute on
/// the same property is a misconfiguration. The drawer logs a one-time warning and falls back
/// to <see cref="ButtonStyle.Default"/> rather than throwing, so the game process is never
/// disrupted by a configuration error in a per-frame draw path.
/// </para>
/// <para>
/// The backing <see cref="Action"/> is intentionally not persisted to JSON; the settings
/// persistence layer skips all delegate-typed parameters during save and load.
/// </para>
/// </remarks>
public sealed class ButtonDrawer : IParameterDrawer
{
    private bool _warnedAboutMissingColors;

    /// <inheritdoc/>
    public void Draw(string label, IParameter parameter)
    {
        if (parameter is not Parameter<Action> p)
        {
            ImGui.TextDisabled($"{label}: (ButtonDrawer requires Parameter<Action>)");
            return;
        }

        var meta = parameter.Metadata;
        var style = meta.ButtonStyle ?? ButtonStyle.Default;
        var size = new Vector2(meta.ControlWidth ?? 0f, 0f);
        bool colorsPushed;

        // Guard: ButtonStyle.Custom without [CustomButtonColors] is a misconfiguration.
        // Log once and fall back to Default rather than throwing from a per-frame draw path,
        // which would crash the game process on every frame.
        if (style == ButtonStyle.Custom && meta.CustomButtonColors is null)
        {
            if (!_warnedAboutMissingColors)
            {
                _warnedAboutMissingColors = true;
                Logger.Warning(
                    $"ButtonDrawer: '{label}' uses ButtonStyle.Custom without a [UmbraCustomButtonColors] attribute; " +
                    "falling back to ButtonStyle.Default. Add [UmbraCustomButtonColors(...)] to suppress this warning.");
            }

            style = ButtonStyle.Default;
        }

        if (meta.CustomButtonColors is { } custom)
            colorsPushed = ButtonStyleColors.Push(custom.Normal, custom.Hovered, custom.Active);
        else
            colorsPushed = ButtonStyleColors.Push(style);

        var clicked = ImGui.Button(label, size);
        if (colorsPushed) ButtonStyleColors.Pop();

        if (clicked) p.Value?.Invoke();

        if (meta.Description is not null)
        {
            ImGui.SameLine();
            ImGuiWidgets.DrawHelpMarker(meta.Description);
        }
    }
}
