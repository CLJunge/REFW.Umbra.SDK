namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the indentation applied to an annotated parameter or configuration scope in the UI.
/// </summary>
/// <remarks>
/// Applied to a type, this attribute provides the fallback indent for controls inside that scope. Applied to a member, it overrides any inherited indent for that parameter.
/// </remarks>
/// <param name="amount">The requested indentation width in pixels, or <c>0f</c> to use ImGui's default indent spacing.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraIndentAttribute(float amount = 0f) : Attribute
{
    /// <summary>
    /// Gets the requested indentation width.
    /// </summary>
    /// <value>The indentation width in pixels, or <c>0f</c> for ImGui's default indent spacing.</value>
    public float Amount { get; } = amount;
}
