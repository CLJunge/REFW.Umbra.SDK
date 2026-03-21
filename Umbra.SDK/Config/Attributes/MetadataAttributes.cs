namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Assigns a category name to a settings parameter, property, field, or settings group class.
/// Parameters sharing the same category name are grouped together when rendered in the UI.
/// When applied to a class decorated with <c>AutoRegisterSettingsAttribute</c>, the category
/// is inherited by all parameters in that group unless overridden at the member level.
/// </summary>
/// <param name="name">The category name used to group related parameters in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CategoryAttribute(string name) : Attribute
{
    /// <summary>Gets the category name used to group related parameters in the UI.</summary>
    public string Name { get; } = name;
}

/// <summary>
/// Specifies a human-readable display name for a settings parameter property or field.
/// This name is used when rendering the parameter in the UI.
/// </summary>
/// <param name="name">The display name to show in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DisplayNameAttribute(string name) : Attribute
{
    /// <summary>Gets the display name of the parameter.</summary>
    public string Name { get; } = name;
}

/// <summary>
/// Provides a description for a settings parameter property or field.
/// The description is typically shown as a tooltip or help text in the UI.
/// </summary>
/// <param name="text">The descriptive text to display.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class DescriptionAttribute(string text) : Attribute
{
    /// <summary>Gets the description text for the parameter.</summary>
    public string Text { get; } = text;
}

/// <summary>
/// Sets the maximum character length for a <c>string</c> settings parameter's input field.
/// When absent, the UI defaults to <c>256</c> characters.
/// </summary>
/// <param name="length">The maximum number of characters the input field will accept.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class MaxLengthAttribute(uint length) : Attribute
{
    /// <summary>Gets the maximum number of characters allowed in the input field.</summary>
    public uint Length { get; } = length;
}

/// <summary>
/// Defines the minimum and maximum allowable values for a numeric settings parameter.
/// When present, the parameter renders as a slider (<c>SliderFloat</c> / <c>SliderInt</c>)
/// rather than an unconstrained drag control.
/// </summary>
/// <param name="min">The minimum allowable value.</param>
/// <param name="max">The maximum allowable value.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class RangeAttribute(double min, double max) : Attribute
{
    /// <summary>Gets the minimum allowable value for the parameter.</summary>
    public double Min { get; } = min;

    /// <summary>Gets the maximum allowable value for the parameter.</summary>
    public double Max { get; } = max;
}

/// <summary>
/// Specifies the drag speed for an unconstrained numeric settings parameter and is also
/// used to infer the display format's decimal precision when <c>[Format]</c> is absent.
/// </summary>
/// <remarks>
/// <para>
/// For <c>float</c> and <c>double</c> parameters without a <c>[Range]</c>, this value is
/// passed directly as the speed argument to <c>ImGui.DragFloat</c>. For <c>int</c>
/// parameters without a <c>[Range]</c>, it is passed to <c>ImGui.DragInt</c>. The
/// drag-speed value has no effect on slider controls (<c>SliderFloat</c> /
/// <c>SliderInt</c>), which are used when <c>[Range]</c> is also present.
/// </para>
/// <para>
/// For <c>float</c> and <c>double</c> parameters, the decimal place count of the step
/// value is used to derive the fallback printf format string (e.g. a step of <c>0.25</c>
/// produces <c>"%.2f"</c>), and this format inference applies to both drag controls and
/// sliders. For <c>int</c> parameters, format inference is not performed; the display
/// format always falls back to <c>"%d"</c> regardless of this attribute, and
/// <c>SliderInt</c> is unaffected by step entirely. Use <c>[Format]</c> to override
/// the format string explicitly on any numeric type.
/// </para>
/// </remarks>
/// <param name="step">
/// The drag speed for unconstrained numeric controls. Also used to infer the decimal
/// precision of the fallback display format string for <c>float</c> and <c>double</c> types.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class StepAttribute(double step) : Attribute
{
    /// <summary>
    /// Gets the drag speed for the parameter. Also used to infer the decimal precision
    /// of the fallback display format string for floating-point types.
    /// </summary>
    public double Step { get; } = step;
}

/// <summary>
/// Overrides the printf-style format string used when displaying a numeric parameter's value
/// inside ImGui controls such as sliders and drag inputs.
/// </summary>
/// <remarks>
/// <para>Follows ImGui/printf conventions. Examples:</para>
/// <list type="bullet">
///   <item><c>"%.0f°"</c> — integer degrees with a degree symbol.</item>
///   <item><c>"%.2f m"</c> — two decimal places with a unit suffix.</item>
///   <item><c>"%d px"</c> — integer pixels.</item>
/// </list>
/// <para>
/// When absent, the format is inferred from <see cref="StepAttribute"/> for floating-point
/// types (<c>"%.Nf"</c> where N matches the step's decimal places) and defaults to
/// <c>"%d"</c> for integer types.
/// </para>
/// </remarks>
/// <param name="format">A printf-style format string compatible with ImGui.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class FormatAttribute(string format) : Attribute
{
    /// <summary>Gets the printf-style format string used to display the parameter's value.</summary>
    public string Format { get; } = format;
}

/// <summary>
/// Defines the visual color-style variants available for a button rendered by
/// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>.
/// </summary>
public enum ButtonStyle
{
    /// <summary>Renders the button using the active ImGui theme's default button colors.</summary>
    Default,
    /// <summary>Renders the button with a blue primary-action color scheme.</summary>
    Primary,
    /// <summary>Renders the button with a green success-action color scheme.</summary>
    Success,
    /// <summary>Renders the button with an orange warning-action color scheme.</summary>
    Warning,
    /// <summary>Renders the button with a red destructive-action color scheme.</summary>
    Danger,
    /// <summary>
    /// Signals that a fully custom color set supplied via
    /// <c>[CustomButtonColors(...)]</c> should be used.
    /// <para>
    /// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/> will throw an
    /// <see cref="InvalidOperationException"/> at render time when this value is set but no
    /// <c>[CustomButtonColors]</c> attribute is present on the same property.
    /// </para>
    /// </summary>
    Custom,
}

/// <summary>
/// Sets the visual color style of a button parameter rendered by
/// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>.
/// When absent, the button uses the active ImGui theme's default colors.
/// </summary>
/// <remarks>
/// When both <c>[ButtonStyle]</c> and <c>[CustomButtonColors]</c> are present on the same
/// property, <see cref="CustomButtonColorsAttribute"/> takes priority.
/// </remarks>
/// <param name="style">The color style to apply to the rendered button.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ButtonStyleAttribute(ButtonStyle style) : Attribute
{
    /// <summary>Gets the visual color style applied to the button.</summary>
    public ButtonStyle Style { get; } = style;
}

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
/// Custom drawers must explicitly read <see cref="Umbra.SDK.Config.ParameterMetadata.ControlWidth"/>
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

