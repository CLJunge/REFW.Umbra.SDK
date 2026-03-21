namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Overrides the pixel width of a button parameter rendered by
/// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>.
/// Extends <see cref="ControlWidthAttribute"/> with button-specific width semantics.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><term><c>0f</c> (default)</term><description>Auto-sizes the button to fit its label text.</description></item>
///   <item><term><c>-1f</c></term><description>Stretches the button across all remaining horizontal space.</description></item>
///   <item><term>positive value</term><description>Fixes the button to that exact pixel width.</description></item>
/// </list>
/// <para>
/// Unlike the base <see cref="ControlWidthAttribute"/>, which applies the width via
/// <c>ImGui.SetNextItemWidth()</c>, <see cref="ControlWidthAttribute.Width"/> here is passed
/// directly as the x component of <c>ImGui.Button</c>'s size vector, which gives <c>0f</c>
/// auto-size-to-label semantics rather than the default item width.
/// </para>
/// </remarks>
/// <param name="width">
/// The pixel width for the button: <c>0f</c> = auto-size, <c>-1f</c> = fill available, positive = fixed px.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ButtonWidthAttribute(float width) : ControlWidthAttribute(width) { }
