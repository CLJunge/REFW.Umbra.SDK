namespace Umbra.Config.Attributes;

/// <summary>
/// Indents the decorated parameter control, or all parameter controls within a settings
/// group class, in the settings UI.
/// </summary>
/// <param name="amount">
/// The indentation width in pixels, or <c>0</c> to use ImGui's default indent spacing.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraIndentAttribute(float amount = 0f) : Attribute
{
    /// <summary>Gets the indentation width in pixels. <c>0</c> means use ImGui's default.</summary>
    public float Amount { get; } = amount;
}
