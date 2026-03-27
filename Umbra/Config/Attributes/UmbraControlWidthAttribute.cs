namespace Umbra.Config.Attributes;

/// <summary>
/// Sets the pixel width of a settings control's editing widget.
/// </summary>
/// <remarks>
/// For button parameters, the same attribute value is applied via <c>ImGui.Button</c>'s size
/// vector rather than <c>SetNextItemWidth</c>, so <c>0f</c> means auto-size-to-label for buttons
/// but means ImGui's default item width for other controls.
/// </remarks>
/// <param name="width">
/// The pixel width: <c>0f</c> = type-default, negative = fill available, positive = fixed px.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraControlWidthAttribute(float width) : Attribute
{
    /// <summary>
    /// Gets the pixel width for the control's editing widget.
    /// </summary>
    public float Width { get; } = width;
}
