using System.Numerics;
using Umbra.Config;
using Umbra.Config.Attributes;
using Umbra.Logging;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Renders a button for a <see cref="Parameter{T}"/> whose value type is <see cref="Action"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Umbra.UI.Config.ControlFactory"/> uses this drawer by default for delegate-valued parameters. One drawer instance is created per parameter during draw-tree construction, so misconfiguration-warning state remains local to that parameter.
/// </para>
/// <para>
/// Button appearance comes from the parameter's resolved metadata, including optional button style, custom button colors, description help text, and width hints.
/// </para>
/// </remarks>
public sealed class ButtonDrawer : IParameterDrawer
{
    private bool _warnedAboutMissingColors;
    private readonly IButtonDrawerRenderer _renderer;

    /// <summary>
    /// Initializes a new <see cref="ButtonDrawer"/> that renders through the shared ImGui render context.
    /// </summary>
    public ButtonDrawer()
        : this(ImGuiConfigRenderContext.Instance)
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ButtonDrawer"/> with the specified renderer seam.
    /// </summary>
    /// <param name="renderer">The renderer used for button-specific drawing operations.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> is <see langword="null"/>.</exception>
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
