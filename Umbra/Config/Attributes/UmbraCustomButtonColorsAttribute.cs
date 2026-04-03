namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the explicit RGBA colors used when a parameter is rendered by <see cref="UI.Config.Drawers.ButtonDrawer"/>.
/// </summary>
/// <remarks>
/// When this attribute is present, its explicit colors take precedence over <see cref="UmbraButtonStyleAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraCustomButtonColorsAttribute : Attribute
{
    /// <summary>
    /// Gets the red channel used for the normal button state.
    /// </summary>
    public float NormalR { get; }

    /// <summary>
    /// Gets the green channel used for the normal button state.
    /// </summary>
    public float NormalG { get; }

    /// <summary>
    /// Gets the blue channel used for the normal button state.
    /// </summary>
    public float NormalB { get; }

    /// <summary>
    /// Gets the alpha channel used for the normal button state.
    /// </summary>
    public float NormalA { get; }

    /// <summary>
    /// Gets the red channel used for the hovered button state.
    /// </summary>
    public float HoveredR { get; }

    /// <summary>
    /// Gets the green channel used for the hovered button state.
    /// </summary>
    public float HoveredG { get; }

    /// <summary>
    /// Gets the blue channel used for the hovered button state.
    /// </summary>
    public float HoveredB { get; }

    /// <summary>
    /// Gets the alpha channel used for the hovered button state.
    /// </summary>
    public float HoveredA { get; }

    /// <summary>
    /// Gets the red channel used for the active button state.
    /// </summary>
    public float ActiveR { get; }

    /// <summary>
    /// Gets the green channel used for the active button state.
    /// </summary>
    public float ActiveG { get; }

    /// <summary>
    /// Gets the blue channel used for the active button state.
    /// </summary>
    public float ActiveB { get; }

    /// <summary>
    /// Gets the alpha channel used for the active button state.
    /// </summary>
    public float ActiveA { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="UmbraCustomButtonColorsAttribute"/> class from one base RGB color.
    /// </summary>
    /// <param name="r">The red channel of the base color.</param>
    /// <param name="g">The green channel of the base color.</param>
    /// <param name="b">The blue channel of the base color.</param>
    /// <remarks>
    /// The normal state uses the supplied RGB values with full alpha. Hovered and active states are derived by brightening and darkening the base color respectively, with each channel clamped to the inclusive range <c>[0, 1]</c>.
    /// </remarks>
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
    /// Initializes a new instance of the <see cref="UmbraCustomButtonColorsAttribute"/> class with explicit RGBA values for each button state.
    /// </summary>
    /// <param name="normalR">The red channel used for the normal state.</param>
    /// <param name="normalG">The green channel used for the normal state.</param>
    /// <param name="normalB">The blue channel used for the normal state.</param>
    /// <param name="normalA">The alpha channel used for the normal state.</param>
    /// <param name="hoveredR">The red channel used for the hovered state.</param>
    /// <param name="hoveredG">The green channel used for the hovered state.</param>
    /// <param name="hoveredB">The blue channel used for the hovered state.</param>
    /// <param name="hoveredA">The alpha channel used for the hovered state.</param>
    /// <param name="activeR">The red channel used for the active state.</param>
    /// <param name="activeG">The green channel used for the active state.</param>
    /// <param name="activeB">The blue channel used for the active state.</param>
    /// <param name="activeA">The alpha channel used for the active state.</param>
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
