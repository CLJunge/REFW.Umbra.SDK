namespace Umbra.Config.Attributes;

/// <summary>
/// Sets the visual color style of a button parameter rendered by
/// <see cref="UI.Config.Drawers.ButtonDrawer"/>.
/// </summary>
/// <remarks>
/// When both <c>[UmbraButtonStyle]</c> and <c>[UmbraCustomButtonColors]</c> are present on the same
/// property, <see cref="UmbraCustomButtonColorsAttribute"/> takes priority.
/// </remarks>
/// <param name="style">The color style to apply to the rendered button.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraButtonStyleAttribute(ButtonStyle style) : Attribute
{
    /// <summary>Gets the visual color style applied to the button.</summary>
    public ButtonStyle Style { get; } = style;
}
