namespace Umbra.Config.Attributes;

/// <summary>
/// Defines the built-in button color styles consumed by <see cref="UI.Config.Drawers.ButtonDrawer"/>.
/// </summary>
/// <remarks>
/// These values describe style selection only. When <see cref="UmbraCustomButtonColorsAttribute"/> is also present, the custom RGBA colors take precedence over the style selected here.
/// </remarks>
public enum ButtonStyle
{
    /// <summary>
    /// Uses the active ImGui theme's default button colors.
    /// </summary>
    Default,

    /// <summary>
    /// Uses the built-in primary-action color scheme.
    /// </summary>
    Primary,

    /// <summary>
    /// Uses the built-in success color scheme.
    /// </summary>
    Success,

    /// <summary>
    /// Uses the built-in warning color scheme.
    /// </summary>
    Warning,

    /// <summary>
    /// Uses the built-in danger color scheme.
    /// </summary>
    Danger,

    /// <summary>
    /// Signals that a custom color set supplied by <see cref="UmbraCustomButtonColorsAttribute"/> should be used.
    /// </summary>
    /// <remarks>
    /// If this value is selected without accompanying custom button colors, <see cref="UI.Config.Drawers.ButtonDrawer"/> logs a warning and falls back to <see cref="Default"/> instead of throwing.
    /// </remarks>
    Custom,
}
