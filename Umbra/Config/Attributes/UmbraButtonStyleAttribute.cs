namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the built-in button color style used when a parameter is rendered by <see cref="UI.Config.Drawers.ButtonDrawer"/>.
/// </summary>
/// <remarks>
/// When both this attribute and <see cref="UmbraCustomButtonColorsAttribute"/> are present on the same member, the explicit custom colors take precedence.
/// </remarks>
/// <param name="style">The built-in button style to apply.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraButtonStyleAttribute(ButtonStyle style) : Attribute
{
    /// <summary>
    /// Gets the built-in button style applied to the annotated member.
    /// </summary>
    /// <value>The declared button style.</value>
    public ButtonStyle Style { get; } = style;
}
