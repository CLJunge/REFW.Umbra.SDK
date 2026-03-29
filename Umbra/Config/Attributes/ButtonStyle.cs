namespace Umbra.Config.Attributes;

/// <summary>
/// Defines the visual color-style variants available for a button rendered by
/// <see cref="UI.Config.Drawers.ButtonDrawer"/>.
/// </summary>
public enum ButtonStyle
{
    /// <summary>Renders the button using the active ImGui theme's default button colors.</summary>
    Default,
    /// <summary>Renders the button with a blue primary-action color scheme.</summary>
    Primary,
    /// <summary>Renders the button with a green success-action color scheme.</summary>
    Success,
    /// <summary>Renders the button with an orange warning-action color scheme.</summary>
    Warning,
    /// <summary>Renders the button with a red destructive-action color scheme.</summary>
    Danger,
    /// <summary>
    /// Signals that a fully custom color set supplied via
    /// <see cref="UmbraCustomButtonColorsAttribute"/> (<c>[UmbraCustomButtonColors(...)]</c>) should be used.
    /// <para>
    /// When this value is set but no <see cref="UmbraCustomButtonColorsAttribute"/> (<c>[UmbraCustomButtonColors]</c>)
    /// is present on the same property, <see cref="UI.Config.Drawers.ButtonDrawer"/> logs a
    /// one-time warning and falls back to <see cref="Default"/> rather than throwing, so the
    /// game process is never disrupted by a configuration error in a per-frame draw path.
    /// </para>
    /// </summary>
    Custom,
}
