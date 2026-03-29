using System.Numerics;
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
/// <see cref="Umbra.Config.Attributes.UmbraDisplayNameAttribute"/> (<c>[UmbraDisplayName("...")]</c>)).
/// An optional same-line <c>(?)</c> help marker is shown when
/// <see cref="Umbra.Config.Attributes.UmbraDescriptionAttribute"/> (<c>[UmbraDescription("...")]</c>)
/// is also present, consistent with other drawers in this namespace.
/// </para>
/// <para>
/// Appearance is controlled by optional attributes on the parameter property:
/// <list type="bullet">
///   <item>
///     <term><see cref="Umbra.Config.Attributes.UmbraButtonStyleAttribute"/> (<c>[UmbraButtonStyle(ButtonStyle.Danger)]</c>)</term>
///     <description>
///       Applies a preset color scheme. See <see cref="ButtonStyle"/> for all variants.
///       Omit for the default ImGui theme colors. Ignored when
///       <see cref="Umbra.Config.Attributes.UmbraCustomButtonColorsAttribute"/> (<c>[UmbraCustomButtonColors]</c>)
///       is also present.
///     </description>
///   </item>
///   <item>
///     <term><see cref="Umbra.Config.Attributes.UmbraCustomButtonColorsAttribute"/> (<c>[UmbraCustomButtonColors(r, g, b)]</c> or <c>[UmbraCustomButtonColors(…×12)]</c>)</term>
///     <description>
///       Applies fully custom RGBA colors for the normal, hovered, and active button states.
///       Takes priority over
///       <see cref="Umbra.Config.Attributes.UmbraButtonStyleAttribute"/> (<c>[UmbraButtonStyle]</c>)
///       when both are specified.
///     </description>
///   </item>
///   <item>
///     <term><see cref="Umbra.Config.Attributes.UmbraControlWidthAttribute"/> (<c>[UmbraControlWidth(-1f)]</c>)</term>
///     <description>
///       <c>0f</c> (default) = auto-size to label, <c>-1f</c> = fill available width,
///       positive = fixed pixel width.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// Using <see cref="ButtonStyle.Custom"/> without
/// <see cref="Umbra.Config.Attributes.UmbraCustomButtonColorsAttribute"/> (<c>[UmbraCustomButtonColors]</c>)
/// on the same property is a misconfiguration. The drawer logs a one-time warning and falls back
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
    private readonly IButtonDrawerRenderer _renderer;

    /// <summary>
    /// Initialises a new <see cref="ButtonDrawer"/> that renders through the active ImGui frame.
    /// </summary>
    public ButtonDrawer()
        : this(new ImGuiButtonDrawerRenderer())
    {
    }

    /// <summary>
    /// Initialises a new <see cref="ButtonDrawer"/> with the specified low-level renderer.
    /// </summary>
    /// <param name="renderer">The renderer used for button-specific drawing operations.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> is <see langword="null"/>.
    /// </exception>
    internal ButtonDrawer(IButtonDrawerRenderer renderer)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        _renderer = renderer;
    }

    /// <inheritdoc/>
    public void Draw(string label, IParameter parameter)
    {
        if (parameter is not Parameter<Action> p)
        {
            _renderer.TextDisabled($"{label}: (ButtonDrawer requires Parameter<Action>)");
            return;
        }

        var meta = parameter.Metadata;
        var style = meta.ButtonStyle ?? ButtonStyle.Default;
        var size = new Vector2(meta.ControlWidth ?? 0f, 0f);
        bool colorsPushed;

        // Guard: ButtonStyle.Custom without [UmbraCustomButtonColors] is a misconfiguration.
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
            colorsPushed = _renderer.PushButtonColors(custom.Normal, custom.Hovered, custom.Active);
        else
            colorsPushed = _renderer.PushButtonColors(style);

        var clicked = _renderer.Button(label, size);
        if (colorsPushed) _renderer.PopButtonColors();

        if (clicked) p.Value?.Invoke();

        if (meta.Description is not null)
        {
            _renderer.SameLine();
            _renderer.DrawHelpMarker(meta.Description);
        }
    }
}
