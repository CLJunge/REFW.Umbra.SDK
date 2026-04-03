using System.Diagnostics;
using System.Numerics;
using Umbra.Config.Attributes;

namespace Umbra.Config;

/// <summary>
/// Stores the descriptive and UI metadata resolved for a registered <see cref="Parameter{T}"/>.
/// </summary>
/// <remarks>
/// <see cref="ParameterMetadataReader"/> builds this object from attributes applied to the parameter's declaring member and containing settings types. The settings UI consumes the resulting values to choose labels, grouping, spacing, validation hints, and drawer behavior without rescanning attributes during rendering.
/// </remarks>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public sealed class ParameterMetadata
{
    /// <summary>
    /// Gets the category name used to group related parameters in the UI.
    /// </summary>
    /// <value>The resolved category name, or <see langword="null"/> when no category is assigned.</value>
    public string? Category { get; init; }

    /// <summary>
    /// Gets the explicit display name declared for the parameter.
    /// </summary>
    /// <value>The attribute-supplied display name, or <see langword="null"/> when no explicit display name is declared.</value>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the fully resolved display label for this parameter. Equals <see cref="DisplayName"/>
    /// when a <c>[UmbraDisplayName(...)]</c> attribute is present; otherwise the property name
    /// converted to a human-readable form (e.g. <c>"FieldOfView"</c> → <c>"Field Of View"</c>).
    /// Pre-computed by <see cref="ParameterMetadataReader"/> during <see cref="SettingsStore{TConfig}.Load()"/> to
    /// avoid repeated <see cref="System.Text.StringBuilder"/> allocations at draw-tree construction time.
    /// </summary>
    internal string ResolvedLabel { get; init; } = string.Empty;

    /// <summary>
    /// Gets the descriptive text for the parameter, typically rendered as a tooltip
    /// or help label in the UI. Sourced from <see cref="UmbraDescriptionAttribute"/>.
    /// <see langword="null"/> if not specified.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the maximum character length for string input fields.
    /// Sourced from <see cref="UmbraMaxLengthAttribute"/>. <see langword="null"/> if not specified;
    /// the UI defaults to <c>256</c>.
    /// </summary>
    public uint? MaxLength { get; init; }

    /// <summary>
    /// Gets the minimum allowable value for the parameter.
    /// Used by <see cref="Parameter{T}"/> to reject values below this bound.
    /// Sourced from <see cref="UmbraRangeAttribute"/>. <see langword="null"/> if no minimum is defined.
    /// </summary>
    public double? Min { get; init; }

    /// <summary>
    /// Gets the maximum allowable value for the parameter.
    /// Used by <see cref="Parameter{T}"/> to reject values above this bound.
    /// Sourced from <see cref="UmbraRangeAttribute"/>. <see langword="null"/> if no maximum is defined.
    /// </summary>
    public double? Max { get; init; }

    /// <summary>
    /// Gets the drag speed used when adjusting the value via unconstrained drag controls
    /// (<c>DragFloat</c> / <c>DragInt</c>); the drag-speed value is not applied to slider
    /// controls (<c>SliderFloat</c> / <c>SliderInt</c>). For <c>float</c> and <c>double</c>
    /// parameters, also used to infer the display format's decimal precision when no
    /// <see cref="UmbraFormatAttribute"/> is present; this format inference applies to both
    /// <c>DragFloat</c>/<c>DragDouble</c> and <c>SliderFloat</c>/<c>SliderDouble</c>.
    /// For <c>int</c> parameters, only the drag speed applies to <c>DragInt</c>; format
    /// inference is not performed and the display format always falls back to <c>"%d"</c>.
    /// Sourced from <see cref="UmbraStepAttribute"/>. <see langword="null"/> if no step is defined.
    /// </summary>
    public double? Step { get; init; }

    /// <summary>
    /// Gets the printf-style format string used when displaying the parameter's value
    /// inside ImGui controls. Sourced from <see cref="UmbraFormatAttribute"/>.
    /// <see langword="null"/> if not specified; the UI falls back to a format inferred
    /// from <see cref="Step"/> for floating-point types and <c>"%d"</c> for integers.
    /// </summary>
    public string? Format { get; init; }

    /// <summary>
    /// Gets the visual color style applied to a button rendered by
    /// <see cref="Umbra.UI.Config.Drawers.ButtonDrawer"/>.
    /// Sourced from <see cref="UmbraButtonStyleAttribute"/>. <see langword="null"/> when not specified;
    /// the drawer defaults to <see cref="ButtonStyle.Default"/>.
    /// </summary>
    /// <remarks>
    /// When <see cref="CustomButtonColors"/> is also set, it takes priority over this value.
    /// </remarks>
    public ButtonStyle? ButtonStyle { get; init; }

    /// <summary>
    /// Gets the custom RGBA colors applied to a button rendered by
    /// <see cref="Umbra.UI.Config.Drawers.ButtonDrawer"/>.
    /// Sourced from <see cref="UmbraCustomButtonColorsAttribute"/>. <see langword="null"/> when not specified.
    /// When present, takes priority over <see cref="ButtonStyle"/>.
    /// </summary>
    public (Vector4 Normal, Vector4 Hovered, Vector4 Active)? CustomButtonColors { get; init; }

    /// <summary>
    /// Gets the explicit pixel width for a settings control's editing widget.
    /// Sourced from <see cref="UmbraControlWidthAttribute"/>.
    /// </summary>
    /// <remarks>
    /// All standard controls always use a two-column text-label layout; this property governs
    /// only the <em>width</em> of the editing widget. For non-button controls,
    /// <c>0f</c> means ImGui's default item width, negative means fill remaining horizontal
    /// space, and positive means fixed pixels. For button controls rendered by
    /// <see cref="Umbra.UI.Config.Drawers.ButtonDrawer"/>, the same value is interpreted with
    /// button semantics: <c>0f</c> = auto-size to label, negative = fill available width,
    /// positive = fixed pixels. When <see langword="null"/>, non-button controls default to
    /// <c>-1f</c> (fill available space) while buttons default to <c>0f</c> (auto-size).
    /// </remarks>
    public float? ControlWidth { get; init; }

    /// <summary>
    /// Gets the number of visible text lines used to compute the height of a multi-line
    /// <c>ImGui.InputTextMultiline</c> control for <see cref="string"/> parameters.
    /// Sourced from <see cref="UmbraMultilineAttribute"/>. <see langword="null"/> when not specified;
    /// the control falls back to a single-line <c>ImGui.InputText</c>.
    /// </summary>
    public int? MultilineLines { get; init; }

    /// <summary>
    /// Gets the explicit display order for this parameter within its local rendered scope.
    /// Lower values appear first; parameters without an order value sort after all explicitly
    /// ordered entries (using <see cref="int.MaxValue"/> as an implicit sentinel), with
    /// original declaration order preserved among equals via stable sort.
    /// Sourced from <see cref="UmbraParameterOrderAttribute"/>. <see langword="null"/> when not specified.
    /// </summary>
    public int? Order { get; init; }

    /// <summary>
    /// Gets the number of <see cref="Hexa.NET.ImGui.ImGui.Spacing()"/> calls inserted
    /// <em>above</em> this parameter's control.
    /// Sourced from <see cref="UmbraSpacingBeforeAttribute"/>. Defaults to <c>0</c> (no extra spacing) when the
    /// attribute is absent.
    /// </summary>
    public int SpacingBefore { get; init; }

    /// <summary>
    /// Gets the number of <see cref="Hexa.NET.ImGui.ImGui.Spacing()"/> calls inserted
    /// <em>below</em> this parameter's control.
    /// Sourced from <see cref="UmbraSpacingAfterAttribute"/>. Defaults to <c>0</c> (no extra spacing) when the
    /// attribute is absent.
    /// </summary>
    public int SpacingAfter { get; init; }

    /// <summary>
    /// Gets the indentation width in pixels to apply around this parameter's control, or
    /// <see langword="null"/> when no indentation is requested.
    /// Sourced from the property-level <see cref="UmbraIndentAttribute"/>.
    /// <c>0f</c> means use ImGui's default indent spacing
    /// (<see cref="Hexa.NET.ImGui.ImGui.GetStyle()"/> <c>.IndentSpacing</c>); a positive value
    /// specifies an explicit pixel width.
    /// <see langword="null"/> when the attribute is absent — the class-level <see cref="UmbraIndentAttribute"/>
    /// (if any) is used as fallback by <see cref="Umbra.UI.Config.ConfigDrawerBuilder"/>.
    /// </summary>
    public float? Indent { get; init; }

    /// <summary>
    /// Gets the concrete <see cref="Umbra.UI.Config.Drawers.IParameterDrawer"/> type used
    /// to render this parameter, or <see langword="null"/> when no <c>[UmbraDrawer&lt;TDrawer&gt;]</c>
    /// attribute is present. When non-<see langword="null"/>, <see cref="Umbra.UI.Config.ControlFactory"/> instantiates this
    /// type and delegates all rendering to it; the default two-column layout is bypassed entirely.
    /// Sourced from <see cref="UmbraDrawerAttribute{TDrawer}"/> via a single attribute scan in
    /// <see cref="ParameterMetadataReader"/>.
    /// </summary>
    internal Type? DrawerType { get; init; }

    /// <summary>
    /// Gets the concrete <see cref="Umbra.UI.Config.Drawers.ITwoColumnParameterDrawer"/> type
    /// used to render this parameter's editing widget, or <see langword="null"/> when no
    /// <c>[UmbraTwoColumnDrawer&lt;TDrawer&gt;]</c> attribute is present. When non-<see langword="null"/>,
    /// <see cref="Umbra.UI.Config.ControlFactory"/> instantiates this type and delegates widget rendering to it while retaining
    /// the standard two-column label layout.
    /// Sourced from <see cref="UmbraTwoColumnDrawerAttribute{TDrawer}"/> via a single attribute scan in
    /// <see cref="ParameterMetadataReader"/>.
    /// </summary>
    internal Type? TwoColumnDrawerType { get; init; }

    /// <summary>
    /// Gets the cached hide-condition data sourced from <see cref="UmbraHideIfAttribute{T}"/> on this
    /// parameter's declaring member, or <see langword="null"/> when no such attribute is present.
    /// Consumed by <see cref="Umbra.UI.Config.VisibilityPredicateResolver"/> to compile the per-frame visibility predicate
    /// without requiring a second attribute scan at draw-tree construction time.
    /// </summary>
    internal IHideIfAttribute? HideIf { get; init; }

    /// <summary>
    /// Gets the fully resolved printf format string used by float and double ImGui controls.
    /// Equals <see cref="Format"/> when an <see cref="UmbraFormatAttribute"/> (<c>[UmbraFormat(...)]</c>)
    /// attribute is present; otherwise the value inferred from the decimal-place count of
    /// <see cref="Step"/>, defaulting to <c>"%.2f"</c>. Precomputed by
    /// <see cref="ParameterMetadataReader"/> during
    /// <see cref="SettingsStore{TConfig}.Load()"/> to eliminate <c>Number.FormatFloat</c> overhead at
    /// draw-tree construction time.
    /// </summary>
    internal string InferredFloatFormat { get; init; } = "%.2f";

    /// <summary>
    /// Gets the pre-computed hidden ImGui control label (<c>"##" + Key</c>) for this parameter,
    /// or <see langword="null"/> when the parameter key was not available at metadata-read time.
    /// Cached by <see cref="ParameterMetadataReader"/> during <see cref="SettingsStore{TConfig}.Load()"/> to avoid
    /// a <c>string.Concat</c> allocation per parameter per <see cref="Umbra.UI.Config.ConfigDrawer{TConfig}"/> construction.
    /// </summary>
    internal string? HiddenLabel { get; init; }

    /// <summary>
    /// Gets the debugger summary used by <see cref="DebuggerDisplayAttribute"/>.
    /// </summary>
    internal string DebuggerDisplay => ParameterMetadataDebuggerDisplayFormatter.Format(this);
}
