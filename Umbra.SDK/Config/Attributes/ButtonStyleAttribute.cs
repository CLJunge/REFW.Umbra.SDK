namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Defines the visual color-style variants available for a button rendered by
/// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>.
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
    /// <c>[CustomButtonColors(...)]</c> should be used.
    /// <para>
    /// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/> will throw an
    /// <see cref="InvalidOperationException"/> at render time when this value is set but no
    /// <c>[CustomButtonColors]</c> attribute is present on the same property.
    /// </para>
    /// </summary>
    Custom,
}

/// <summary>
/// Sets the visual color style of a button parameter rendered by
/// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>.
/// When absent, the button uses the active ImGui theme's default colors.
/// </summary>
/// <remarks>
/// When both <c>[ButtonStyle]</c> and <c>[CustomButtonColors]</c> are present on the same
/// property, <see cref="CustomButtonColorsAttribute"/> takes priority.
/// </remarks>
/// <param name="style">The color style to apply to the rendered button.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ButtonStyleAttribute(ButtonStyle style) : Attribute
{
    /// <summary>Gets the visual color style applied to the button.</summary>
    public ButtonStyle Style { get; } = style;
}
