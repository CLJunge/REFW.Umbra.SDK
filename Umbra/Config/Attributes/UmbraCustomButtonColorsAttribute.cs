namespace Umbra.Config.Attributes;

/// <summary>
/// Applies fully custom RGBA button colors to a button parameter rendered by
/// <see cref="UI.Config.Drawers.ButtonDrawer"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraCustomButtonColorsAttribute : Attribute
{
    public float NormalR { get; }
    public float NormalG { get; }
    public float NormalB { get; }
    public float NormalA { get; }
    public float HoveredR { get; }
    public float HoveredG { get; }
    public float HoveredB { get; }
    public float HoveredA { get; }
    public float ActiveR { get; }
    public float ActiveG { get; }
    public float ActiveB { get; }
    public float ActiveA { get; }

    /// <summary>
    /// Initializes a new <see cref="UmbraCustomButtonColorsAttribute"/> from a single base RGB color.
    /// </summary>
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
