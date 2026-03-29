namespace Umbra.Config.Attributes;

/// <summary>
/// Applies fully custom RGBA button colors to a button parameter rendered by
/// <see cref="UI.Config.Drawers.ButtonDrawer"/>.
/// </summary>
/// <remarks>
/// When both <see cref="UmbraCustomButtonColorsAttribute"/> (<c>[UmbraCustomButtonColors]</c>) and
/// <see cref="UmbraButtonStyleAttribute"/> (<c>[UmbraButtonStyle]</c>) are present on the same
/// parameter, the explicit RGBA values from this attribute take precedence.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraCustomButtonColorsAttribute : Attribute
{
    /// <summary>Gets the red channel used for the button's normal state.</summary>
    public float NormalR { get; }

    /// <summary>Gets the green channel used for the button's normal state.</summary>
    public float NormalG { get; }

    /// <summary>Gets the blue channel used for the button's normal state.</summary>
    public float NormalB { get; }

    /// <summary>Gets the alpha channel used for the button's normal state.</summary>
    public float NormalA { get; }

    /// <summary>Gets the red channel used for the button's hovered state.</summary>
    public float HoveredR { get; }

    /// <summary>Gets the green channel used for the button's hovered state.</summary>
    public float HoveredG { get; }

    /// <summary>Gets the blue channel used for the button's hovered state.</summary>
    public float HoveredB { get; }

    /// <summary>Gets the alpha channel used for the button's hovered state.</summary>
    public float HoveredA { get; }

    /// <summary>Gets the red channel used for the button's active state.</summary>
    public float ActiveR { get; }

    /// <summary>Gets the green channel used for the button's active state.</summary>
    public float ActiveG { get; }

    /// <summary>Gets the blue channel used for the button's active state.</summary>
    public float ActiveB { get; }

    /// <summary>Gets the alpha channel used for the button's active state.</summary>
    public float ActiveA { get; }

    /// <summary>
    /// Initializes a new <see cref="UmbraCustomButtonColorsAttribute"/> from a single base RGB color.
    /// </summary>
    /// <remarks>
    /// The normal state uses the supplied RGB values with an alpha of <c>1.0</c>. The hovered
    /// state is derived by brightening each RGB channel by <c>0.10</c>, and the active state is
    /// derived by darkening each RGB channel by <c>0.08</c>. Derived channel values are clamped
    /// to the inclusive range <c>[0, 1]</c>.
    /// </remarks>
    /// <param name="r">The red channel of the base color, typically in the range <c>0</c> to <c>1</c>.</param>
    /// <param name="g">The green channel of the base color, typically in the range <c>0</c> to <c>1</c>.</param>
    /// <param name="b">The blue channel of the base color, typically in the range <c>0</c> to <c>1</c>.</param>
    public UmbraCustomButtonColorsAttribute(float r, float g, float b)
    {
        NormalR = r; NormalG = g; NormalB = b; NormalA = 1f;
        HoveredR = Math.Clamp(r + 0.10f, 0f, 1f);
        HoveredG = Math.Clamp(g + 0.10f, 0f, 1f);
        HoveredB = Math.Clamp(b + 0.10f, 0f, 1f);
        HoveredA = 1f;
        ActiveR = Math.Clamp(r - 0.08f, 0f, 1f);
        ActiveG = Math.Clamp(g - 0.08f, 0f, 1f);
        ActiveB = Math.Clamp(b - 0.08f, 0f, 1f);
        ActiveA = 1f;
    }

    /// <summary>
    /// Initializes a new <see cref="UmbraCustomButtonColorsAttribute"/> with explicit RGBA values
    /// for the normal, hovered, and active button states.
    /// </summary>
    /// <param name="normalR">The red channel used for the normal button state.</param>
    /// <param name="normalG">The green channel used for the normal button state.</param>
    /// <param name="normalB">The blue channel used for the normal button state.</param>
    /// <param name="normalA">The alpha channel used for the normal button state.</param>
    /// <param name="hoveredR">The red channel used for the hovered button state.</param>
    /// <param name="hoveredG">The green channel used for the hovered button state.</param>
    /// <param name="hoveredB">The blue channel used for the hovered button state.</param>
    /// <param name="hoveredA">The alpha channel used for the hovered button state.</param>
    /// <param name="activeR">The red channel used for the active button state.</param>
    /// <param name="activeG">The green channel used for the active button state.</param>
    /// <param name="activeB">The blue channel used for the active button state.</param>
    /// <param name="activeA">The alpha channel used for the active button state.</param>
    public UmbraCustomButtonColorsAttribute(
        float normalR, float normalG, float normalB, float normalA,
        float hoveredR, float hoveredG, float hoveredB, float hoveredA,
        float activeR, float activeG, float activeB, float activeA)
    {
        NormalR = normalR; NormalG = normalG; NormalB = normalB; NormalA = normalA;
        HoveredR = hoveredR; HoveredG = hoveredG; HoveredB = hoveredB; HoveredA = hoveredA;
        ActiveR = activeR; ActiveG = activeG; ActiveB = activeB; ActiveA = activeA;
    }
}
