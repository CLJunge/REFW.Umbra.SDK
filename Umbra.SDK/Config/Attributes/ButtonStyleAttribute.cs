namespace Umbra.SDK.Config.Attributes;

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
