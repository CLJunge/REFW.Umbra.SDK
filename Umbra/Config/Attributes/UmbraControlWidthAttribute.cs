namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the width hint used for a parameter's editing widget in the configuration UI.
/// </summary>
/// <remarks>
/// For non-button controls, Umbra applies this value through ImGui item-width semantics. For button parameters rendered by <see cref="UI.Config.Drawers.ButtonDrawer"/>, the same value is interpreted using button-size semantics instead.
/// </remarks>
/// <param name="width">The requested width hint: <c>0f</c> for type-default behavior, a negative value to fill available width, or a positive value for fixed pixels.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraControlWidthAttribute(float width) : Attribute
{
    /// <summary>
    /// Gets the declared width hint.
    /// </summary>
    /// <value>The control-width hint associated with the annotated member.</value>
    public float Width { get; } = width;
}
