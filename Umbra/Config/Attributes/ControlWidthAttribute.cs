namespace Umbra.Config.Attributes;

/// <summary>
/// Sets the pixel width of the editing widget for a settings control rendered by the default
/// control factory or, through the derived <see cref="ButtonWidthAttribute"/>, the button drawer.
/// </summary>
/// <remarks>
/// <para>Semantics for the three possible values:</para>
/// <list type="bullet">
///   <item><term><c>0f</c></term><description>Control-type–specific default sizing: auto-size to label for buttons; ImGui's default item width for all other controls.</description></item>
///   <item><term><c>-1f</c> (or any negative)</term><description>Stretches the widget to fill all remaining horizontal space after the label column.</description></item>
///   <item><term>positive value</term><description>Fixes the widget to that exact pixel width.</description></item>
/// </list>
/// <para>
/// All standard controls always use a two-column text-label layout regardless of this attribute:
/// the label is rendered on the left and the widget follows at the shared column x position
/// determined by the widest label in the same category or root scope. This attribute governs
/// only the <em>width</em> of the widget. When absent, the widget fills all remaining horizontal
/// space — equivalent to explicitly specifying <c>[ControlWidth(-1f)]</c>.
/// </para>
/// <para>
/// For button parameters use the derived <see cref="ButtonWidthAttribute"/>, which passes the width
/// via <c>ImGui.Button</c>'s size vector instead of <c>SetNextItemWidth</c>, giving <c>0f</c>
/// a different auto-size meaning. For all other standard controls the width is applied via
/// <c>ImGui.SetNextItemWidth()</c> before the widget (or via the size vector for multi-line strings).
/// Custom drawers must explicitly read <see cref="ParameterMetadata.ControlWidth"/>
/// to honour this setting.
/// </para>
/// </remarks>
/// <param name="width">
/// The pixel width: <c>0f</c> = type-default, negative = fill available, positive = fixed px.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ControlWidthAttribute(float width) : Attribute
{
    /// <summary>
    /// Gets the pixel width for the control's editing widget.
    /// <c>0f</c> = type-default sizing, negative = fill remaining horizontal space,
    /// positive = fixed pixel width.
    /// </summary>
    public float Width { get; } = width;
}
