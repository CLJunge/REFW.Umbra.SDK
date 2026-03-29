using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Selects the appropriate per-frame ImGui draw action for a parameter.
/// </summary>
/// <remarks>
/// Custom-drawer activation is delegated to <see cref="ParameterDrawerResolver"/>. Built-in
/// numeric controls are delegated to <see cref="NumericControlBuilder"/>, text controls are
/// delegated to <see cref="TextControlBuilder"/>, <see cref="Parameter{T}"/> values of type
/// <see cref="Action"/> default to <see cref="Drawers.ButtonDrawer"/>, and enum or nullable-enum
/// controls are delegated to <see cref="EnumControlBuilder"/>. This type now focuses on dispatch
/// and shared layout creation.
/// All controls use a two-column text-label layout unconditionally: the parameter label (and
/// optional <c>(?)</c> help marker) is rendered on the left; the editing widget is placed on
/// the right at the column x position determined by <see cref="LabelAlignmentGroup"/>. Labels
/// are registered with the group at build time and measured once on the first draw frame via
/// <see cref="LabelAlignmentGroup.EnsureSeeded"/>; no per-frame measurement occurs after that.
/// The widget width defaults to fill-to-right-edge (<c>SetNextItemWidth(-1f)</c>) and can be
/// fixed with <see cref="Umbra.Config.Attributes.UmbraControlWidthAttribute"/> (<c>[UmbraControlWidth(px)]</c>).
/// </remarks>
internal static class ControlFactory
{
    // One entry per supported built-in value type. Enum and fallback are handled separately.
    // Add or replace entries here to change the default control for any value type.
    private static readonly Dictionary<Type, Func<string, IParameter, LabelAlignmentGroup, Action>> _defaultBuilders = new()
    {
        [typeof(Action)] = BuildActionDraw,
        [typeof(bool)] = BuildBoolDraw,
        [typeof(int)] = NumericControlBuilder.BuildInt,
        [typeof(float)] = NumericControlBuilder.BuildFloat,
        [typeof(double)] = NumericControlBuilder.BuildDouble,
        [typeof(string)] = TextControlBuilder.BuildString,
    };

    /// <summary>
    /// Builds a per-frame draw <see cref="Action"/> for <paramref name="parameter"/>,
    /// dispatching first to <see cref="ParameterDrawerResolver"/> for any custom drawer recorded in
    /// <see cref="ParameterMetadata"/>, then to the built-in default-builder table, then to
    /// <see cref="EnumControlBuilder"/>, and finally to a read-only label.
    /// </summary>
    /// <remarks>
    /// Custom drawer types are pre-resolved during <c>SettingsStore.Load()</c> by
    /// <c>ParameterMetadataReader</c> and stored in <see cref="ParameterMetadata.CustomDrawerType"/>
    /// and <see cref="ParameterMetadata.TwoColumnCustomDrawerType"/>, eliminating the need to scan
    /// property attributes at draw-tree construction time.
    /// </remarks>
    internal static (Action draw, IDisposable? resource) BuildDrawAction(
        IParameter parameter, string label, LabelAlignmentGroup alignGroup)
    {
        if (ParameterDrawerResolver.TryResolve(parameter, label, alignGroup) is { } custom)
            return custom;

        if (_defaultBuilders.TryGetValue(parameter.ValueType, out var builder))
            return (builder(label, parameter, alignGroup), null);

        var enumType = Nullable.GetUnderlyingType(parameter.ValueType) ?? parameter.ValueType;
        if (enumType.IsEnum)
            return (EnumControlBuilder.Build(label, parameter, alignGroup), null);

        return (() => ImGui.TextDisabled($"{label}: {parameter.GetValue()}"), null);
    }

    /// <summary>
    /// Builds a per-frame draw action that renders a push-button for an
    /// <see cref="Action"/>-typed parameter.
    /// </summary>
    /// <param name="label">The visible button label.</param>
    /// <param name="parameter">The <see cref="Parameter{T}"/> of type <see cref="Action"/> to render.</param>
    /// <param name="alignGroup">
    /// The shared alignment group for the owning category or root scope.
    /// Unused because <see cref="Drawers.ButtonDrawer"/> owns the full row layout.
    /// </param>
    /// <returns>
    /// An <see cref="Action"/> that renders and invokes the button each frame using a drawer
    /// instance created once for this parameter during draw-tree construction.
    /// </returns>
    private static Action BuildActionDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        _ = alignGroup;
        var drawer = new Drawers.ButtonDrawer();
        return () => drawer.Draw(label, parameter);
    }

    /// <summary>Builds a per-frame draw action that renders a checkbox for a <see cref="bool"/> parameter.</summary>
    /// <param name="label">The ImGui control label.</param>
    /// <param name="parameter">The <see cref="Parameter{T}"/> of type <see cref="bool"/> to render.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>An <see cref="Action"/> that renders and updates the parameter each frame.</returns>
    private static Action BuildBoolDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<bool>)parameter;
        var layout = CreateControlLayout(label, parameter, alignGroup);
        return () =>
        {
            var v = p.Value;
            layout.Pre();
            if (ImGui.Checkbox(layout.HiddenLabel, ref v)) p.Value = v;
        };
    }

    /// <summary>
    /// Constructs a <see cref="ControlLayout"/> capturing the pre-computed layout state for a
    /// single parameter row. All rows use a two-column layout: the visible label (and optional
    /// <c>(?)</c> help marker) on the left; the editing widget on the right at the column x
    /// position determined by <paramref name="alignGroup"/>.
    /// </summary>
    /// <remarks>
    /// Shared by built-in controls in <see cref="ControlFactory"/>,
    /// <see cref="NumericControlBuilder"/>, <see cref="TextControlBuilder"/>,
    /// <see cref="EnumControlBuilder"/>, and two-column custom drawers resolved by
    /// <see cref="ParameterDrawerResolver"/>.
    /// <list type="bullet">
    ///   <item>
    ///     <term>Column alignment</term>
    ///     <description>
    ///       Each <see cref="ControlLayout"/> registers its label with the shared
    ///       <see cref="LabelAlignmentGroup"/> at construction time via
    ///       <see cref="LabelAlignmentGroup.Register"/>. On the first draw frame,
    ///       <see cref="ControlLayout.Pre"/> triggers <see cref="LabelAlignmentGroup.EnsureSeeded"/>,
    ///       which measures all registered labels in one <see cref="ImGui.CalcTextSize(string)"/>
    ///       batch and commits the maximum. <see cref="ImGui.SetCursorPosX(float)"/> then advances
    ///       the cursor to
    ///       <c>startX + <see cref="LabelAlignmentGroup.LabelWidth"/> +
    ///       <see cref="LabelAlignmentGroup.Margin"/> + spacing</c> before the widget call.
    ///       The cursor is never moved backward: if a label is wider than the committed group
    ///       maximum (possible on frame 1), the control is placed immediately after the label
    ///       with no overlap. After seeding the committed maximum is frozen and never decreases,
    ///       so hiding parameters via
    ///       <see cref="Umbra.Config.Attributes.UmbraHideIfAttribute{T}"/> (<c>[UmbraHideIf]</c>)
    ///       cannot narrow the column.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Widget width</term>
    ///     <description>
    ///       <see cref="Umbra.Config.Attributes.UmbraControlWidthAttribute"/> (<c>[UmbraControlWidth(px)]</c>)
    ///       fixes the widget to the specified number of pixels via <c>SetNextItemWidth</c>.
    ///       When no <see cref="Umbra.Config.Attributes.UmbraControlWidthAttribute"/> (<c>[UmbraControlWidth]</c>)
    ///       is present, <c>-1f</c> is used, which fills to the right content-region edge.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="label">The display label resolved for the parameter.</param>
    /// <param name="parameter">The parameter being rendered; its <see cref="IParameter.Key"/> is used as the hidden ImGui label fallback when <see cref="ParameterMetadata.HiddenLabel"/> is <see langword="null"/>.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>
    /// A <see cref="ControlLayout"/> value capturing the label, description, alignment group,
    /// and pre-computed hidden ImGui label for the row. Callers must invoke <see cref="ControlLayout.Pre"/>
    /// immediately before the ImGui widget call to set up layout and alignment state.
    /// </returns>
    internal static ControlLayout CreateControlLayout(
        string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var meta = parameter.Metadata;
        var hiddenLabel = meta.HiddenLabel ?? string.Concat("##", parameter.Key);
        return new ControlLayout(label, meta.Description, alignGroup, meta.ControlWidth ?? -1f, hiddenLabel);
    }
}
