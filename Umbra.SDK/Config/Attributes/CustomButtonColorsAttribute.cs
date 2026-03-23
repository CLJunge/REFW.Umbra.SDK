namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Applies fully custom RGBA button colors to a button parameter rendered by
/// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>,
/// overriding any <c>[ButtonStyle]</c> preset on the same property.
/// </summary>
/// <remarks>
/// <para>
/// Two constructors are available:
/// <list type="bullet">
///   <item>
///     <term>Simple — <c>(float r, float g, float b)</c></term>
///     <description>
///       Supply only the base color as RGB; alpha defaults to <c>1.0</c>.
///       The hovered state adds <c>+0.10</c> to each channel and the active state
///       subtracts <c>0.08</c>, both clamped to the <c>[0, 1]</c> range.
///     </description>
///   </item>
///   <item>
///     <term>Full — <c>(float, float, float, float) × 3</c></term>
///     <description>
///       Supply all three states as explicit RGBA quads in the order:
///       normal, hovered, active.
///     </description>
///   </item>
/// </list>
/// </para>
/// <para>
/// When both <c>[ButtonStyle]</c> and <c>[CustomButtonColors]</c> are present on the
/// same property, <c>[CustomButtonColors]</c> takes priority.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class CustomButtonColorsAttribute : Attribute
{
    /// <summary>Gets the red channel of the normal button color, in <c>[0, 1]</c>.</summary>
    public float NormalR { get; }
    /// <summary>Gets the green channel of the normal button color, in <c>[0, 1]</c>.</summary>
    public float NormalG { get; }
    /// <summary>Gets the blue channel of the normal button color, in <c>[0, 1]</c>.</summary>
    public float NormalB { get; }
    /// <summary>Gets the alpha channel of the normal button color, in <c>[0, 1]</c>.</summary>
    public float NormalA { get; }

    /// <summary>Gets the red channel of the hovered button color, in <c>[0, 1]</c>.</summary>
    public float HoveredR { get; }
    /// <summary>Gets the green channel of the hovered button color, in <c>[0, 1]</c>.</summary>
    public float HoveredG { get; }
    /// <summary>Gets the blue channel of the hovered button color, in <c>[0, 1]</c>.</summary>
    public float HoveredB { get; }
    /// <summary>Gets the alpha channel of the hovered button color, in <c>[0, 1]</c>.</summary>
    public float HoveredA { get; }

    /// <summary>Gets the red channel of the active (pressed) button color, in <c>[0, 1]</c>.</summary>
    public float ActiveR { get; }
    /// <summary>Gets the green channel of the active (pressed) button color, in <c>[0, 1]</c>.</summary>
    public float ActiveG { get; }
    /// <summary>Gets the blue channel of the active (pressed) button color, in <c>[0, 1]</c>.</summary>
    public float ActiveB { get; }
    /// <summary>Gets the alpha channel of the active (pressed) button color, in <c>[0, 1]</c>.</summary>
    public float ActiveA { get; }

    /// <summary>
    /// Initializes a new <see cref="CustomButtonColorsAttribute"/> from a single base RGB color.
    /// Alpha is fixed at <c>1.0</c>. The hovered state is derived by adding <c>+0.10</c> to each
    /// channel and the active state by subtracting <c>0.08</c>, both clamped to <c>[0, 1]</c>.
    /// </summary>
    /// <param name="r">Red channel of the base color, in <c>[0, 1]</c>.</param>
    /// <param name="g">Green channel of the base color, in <c>[0, 1]</c>.</param>
    /// <param name="b">Blue channel of the base color, in <c>[0, 1]</c>.</param>
    public CustomButtonColorsAttribute(float r, float g, float b)
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
    /// Initializes a new <see cref="CustomButtonColorsAttribute"/> with explicit RGBA values
    /// for the normal, hovered, and active button states.
    /// </summary>
    /// <param name="normalR">Red channel of the normal state, in <c>[0, 1]</c>.</param>
    /// <param name="normalG">Green channel of the normal state, in <c>[0, 1]</c>.</param>
    /// <param name="normalB">Blue channel of the normal state, in <c>[0, 1]</c>.</param>
    /// <param name="normalA">Alpha channel of the normal state, in <c>[0, 1]</c>.</param>
    /// <param name="hoveredR">Red channel of the hovered state, in <c>[0, 1]</c>.</param>
    /// <param name="hoveredG">Green channel of the hovered state, in <c>[0, 1]</c>.</param>
    /// <param name="hoveredB">Blue channel of the hovered state, in <c>[0, 1]</c>.</param>
    /// <param name="hoveredA">Alpha channel of the hovered state, in <c>[0, 1]</c>.</param>
    /// <param name="activeR">Red channel of the active/pressed state, in <c>[0, 1]</c>.</param>
    /// <param name="activeG">Green channel of the active/pressed state, in <c>[0, 1]</c>.</param>
    /// <param name="activeB">Blue channel of the active/pressed state, in <c>[0, 1]</c>.</param>
    /// <param name="activeA">Alpha channel of the active/pressed state, in <c>[0, 1]</c>.</param>
    public CustomButtonColorsAttribute(
        float normalR, float normalG, float normalB, float normalA,
        float hoveredR, float hoveredG, float hoveredB, float hoveredA,
        float activeR, float activeG, float activeB, float activeA)
    {
        NormalR = normalR; NormalG = normalG; NormalB = normalB; NormalA = normalA;
        HoveredR = hoveredR; HoveredG = hoveredG; HoveredB = hoveredB; HoveredA = hoveredA;
        ActiveR = activeR; ActiveG = activeG; ActiveB = activeB; ActiveA = activeA;
    }
}
