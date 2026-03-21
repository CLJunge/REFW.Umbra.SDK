using System.Diagnostics;
using System.Numerics;
using Umbra.SDK.Config.Attributes;

namespace Umbra.SDK.Config;

/// <summary>
/// Holds descriptive metadata associated with a <see cref="Parameter{T}"/> instance,
/// sourced from attributes applied to the parameter's declaring member or its
/// enclosing settings class.
/// </summary>
/// <remarks>
/// Metadata is populated by <c>ParameterMetadataReader</c> via reflection and is
/// consumed by the settings UI to render appropriate labels, tooltips, sliders,
/// and input constraints without requiring each parameter to carry that information
/// itself. All properties are optional; absent values are represented as
/// <see langword="null"/>.
/// </remarks>
[DebuggerDisplay("{GetDebuggerDisplay()}")]
public sealed class ParameterMetadata
{
    /// <summary>
    /// Gets the category name used to group related parameters together in the UI.
    /// Sourced from <c>CategoryAttribute</c> on the parameter's declaring member or its
    /// enclosing settings group. <see langword="null"/> if no category has been assigned.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the human-readable display name for the parameter, shown in the UI.
    /// Sourced from <c>DisplayNameAttribute</c>. <see langword="null"/> if not specified.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the descriptive text for the parameter, typically rendered as a tooltip
    /// or help label in the UI. Sourced from <c>DescriptionAttribute</c>.
    /// <see langword="null"/> if not specified.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the maximum character length for string input fields.
    /// Sourced from <c>MaxLengthAttribute</c>. <see langword="null"/> if not specified;
    /// the UI defaults to <c>256</c>.
    /// </summary>
    public uint? MaxLength { get; init; }

    /// <summary>
    /// Gets the minimum allowable value for the parameter.
    /// Used by <see cref="Parameter{T}"/> to reject values below this bound.
    /// Sourced from <c>RangeAttribute</c>. <see langword="null"/> if no minimum is defined.
    /// </summary>
    public double? Min { get; init; }

    /// <summary>
    /// Gets the maximum allowable value for the parameter.
    /// Used by <see cref="Parameter{T}"/> to reject values above this bound.
    /// Sourced from <c>RangeAttribute</c>. <see langword="null"/> if no maximum is defined.
    /// </summary>
    public double? Max { get; init; }

    /// <summary>
    /// Gets the drag speed used when adjusting the value via unconstrained drag controls
    /// (<c>DragFloat</c> / <c>DragInt</c>); the drag-speed value is not applied to slider
    /// controls (<c>SliderFloat</c> / <c>SliderInt</c>). For <c>float</c> and <c>double</c>
    /// parameters, also used to infer the display format's decimal precision when no
    /// <c>FormatAttribute</c> is present; this format inference applies to both
    /// <c>DragFloat</c>/<c>DragDouble</c> and <c>SliderFloat</c>/<c>SliderDouble</c>.
    /// For <c>int</c> parameters, only the drag speed applies to <c>DragInt</c>; format
    /// inference is not performed and the display format always falls back to <c>"%d"</c>.
    /// Sourced from <c>StepAttribute</c>. <see langword="null"/> if no step is defined.
    /// </summary>
    public double? Step { get; init; }

    /// <summary>
    /// Gets the printf-style format string used when displaying the parameter's value
    /// inside ImGui controls. Sourced from <c>FormatAttribute</c>.
    /// <see langword="null"/> if not specified; the UI falls back to a format inferred
    /// from <see cref="Step"/> for floating-point types and <c>"%d"</c> for integers.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the visual color style applied to a button rendered by
    /// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>.
    /// Sourced from <c>ButtonStyleAttribute</c>. <see langword="null"/> when not specified;
    /// the drawer defaults to <see cref="ButtonStyle.Default"/>.
    /// </summary>
    /// <remarks>
    /// When <see cref="CustomButtonColors"/> is also set, it takes priority over this value.
    /// </remarks>
    public ButtonStyle? ButtonStyle { get; init; }

    /// <summary>
    /// Gets the custom RGBA colors applied to a button rendered by
    /// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>.
    /// Sourced from <c>CustomButtonColorsAttribute</c>. <see langword="null"/> when not specified.
    /// When present, takes priority over <see cref="ButtonStyle"/>.
    /// </summary>
    public (Vector4 Normal, Vector4 Hovered, Vector4 Active)? CustomButtonColors { get; init; }

    /// <summary>
    /// Gets the explicit pixel width for a button rendered by
    /// <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>.
    /// <c>0f</c> = auto-size to label, <c>-1f</c> = fill available width, positive = fixed pixels.
    /// Sourced from <c>ButtonWidthAttribute</c> (which extends <c>ControlWidthAttribute</c>).
    /// <see langword="null"/> when not specified; the drawer defaults to <c>0f</c> (auto-size).
    /// </summary>
    /// <remarks>
    /// Stored separately from <see cref="ControlWidth"/> even though both originate from the same
    /// attribute hierarchy, because <see cref="Umbra.SDK.Config.UI.ParameterDrawers.ButtonDrawer"/>
    /// applies the width via <c>ImGui.Button</c>'s size vector rather than <c>SetNextItemWidth</c>.
    /// When <c>ButtonWidthAttribute</c> is present, <see cref="ControlWidth"/> is left
    /// <see langword="null"/> to keep the two mechanisms independent.
    /// </remarks>
    public float? ButtonWidth { get; init; }

    /// <summary>
    /// Gets the explicit pixel width for the editing widget of a standard settings control rendered
    /// by the default control factory. <c>0f</c> = use ImGui's default item width,
    /// negative = fill remaining horizontal space, positive = fixed pixels.
    /// Sourced from <c>ControlWidthAttribute</c> (the base class of <c>ButtonWidthAttribute</c>).
    /// <see langword="null"/> when not specified or when <c>ButtonWidthAttribute</c> is present
    /// (button width is stored in <see cref="ButtonWidth"/> instead).
    /// </summary>
    /// <remarks>
    /// All standard controls always use a two-column text-label layout; this property governs
    /// only the <em>width</em> of the editing widget. When <see langword="null"/>, the widget
    /// fills all remaining horizontal space after the label column, equivalent to <c>-1f</c>.
    /// </remarks>
    public float? ControlWidth { get; init; }

    /// <summary>
    /// Gets the number of visible text lines used to compute the height of a multi-line
    /// <c>ImGui.InputTextMultiline</c> control for <see cref="string"/> parameters.
    /// Sourced from <c>MultilineAttribute</c>. <see langword="null"/> when not specified;
    /// the control falls back to a single-line <c>ImGui.InputText</c>.
    /// </summary>
    public int? MultilineLines { get; init; }

    /// <summary>
    /// Gets the explicit display order for this parameter within its category context.
    /// Lower values appear first; parameters without an order value sort after all explicitly
    /// ordered entries (using <see cref="int.MaxValue"/> as an implicit sentinel), with
    /// original declaration order preserved among equals via stable sort.
    /// Sourced from <c>ParameterOrderAttribute</c>. <see langword="null"/> when not specified.
    /// </summary>
    public int? Order { get; init; }

    /// <summary>
    /// Returns a concise, human-readable summary of all non-null metadata fields,
    /// used by the debugger via <see cref="DebuggerDisplayAttribute"/>.
    /// </summary>
    /// <returns>
    /// A comma-separated string of key-value pairs for each metadata property that
    /// has a non-null, non-empty value, with no trailing comma or whitespace.
    /// </returns>
    private string GetDebuggerDisplay()
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(Category)) parts.Add($"Category: {Category}");
        if (!string.IsNullOrEmpty(DisplayName)) parts.Add($"DisplayName: {DisplayName}");
        if (!string.IsNullOrEmpty(Description)) parts.Add($"Description: {Description}");
        if (MaxLength.HasValue) parts.Add($"MaxLength: {MaxLength.Value}");
        if (Min.HasValue) parts.Add($"Min: {Min.Value}");
        if (Max.HasValue) parts.Add($"Max: {Max.Value}");
        if (Step.HasValue) parts.Add($"Step: {Step.Value}");
        if (!string.IsNullOrEmpty(Format)) parts.Add($"Format: {Format}");
        if (ButtonStyle.HasValue) parts.Add($"ButtonStyle: {ButtonStyle.Value}");
        if (CustomButtonColors.HasValue) parts.Add($"CustomButtonColors: N={CustomButtonColors.Value.Normal} H={CustomButtonColors.Value.Hovered} A={CustomButtonColors.Value.Active}");
        if (ButtonWidth.HasValue) parts.Add($"ButtonWidth: {ButtonWidth.Value}");
        if (ControlWidth.HasValue) parts.Add($"ControlWidth: {ControlWidth.Value}");
        if (MultilineLines.HasValue) parts.Add($"MultilineLines: {MultilineLines.Value}");
        if (Order.HasValue) parts.Add($"Order: {Order.Value}");

        return string.Join(", ", parts);
    }
}
