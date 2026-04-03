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
/// controls are delegated to <see cref="EnumControlBuilder"/>. Shared two-column layout creation is
/// delegated to <see cref="ControlLayoutFactory"/>, so this type remains focused on control dispatch.
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
    /// <c>ParameterMetadataReader</c> and stored in <see cref="ParameterMetadata.DrawerType"/>
    /// and <see cref="ParameterMetadata.TwoColumnDrawerType"/>, eliminating the need to scan
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
    /// single parameter row.
    /// </summary>
    /// <remarks>
    /// Shared by built-in controls in <see cref="ControlFactory"/>,
    /// <see cref="NumericControlBuilder"/>, <see cref="TextControlBuilder"/>,
    /// <see cref="EnumControlBuilder"/>, and two-column custom drawers resolved by
    /// <see cref="ParameterDrawerResolver"/>. Layout-value construction is delegated to
    /// <see cref="ControlLayoutFactory"/>.
    /// </remarks>
    /// <param name="label">The display label resolved for the parameter.</param>
    /// <param name="parameter">The parameter being rendered.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>The precomputed row layout.</returns>
    internal static ControlLayout CreateControlLayout(
        string label, IParameter parameter, LabelAlignmentGroup alignGroup)
        => ControlLayoutFactory.Create(label, parameter, alignGroup);
}
