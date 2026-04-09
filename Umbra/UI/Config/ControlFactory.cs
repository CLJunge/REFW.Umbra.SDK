using Hexa.NET.ImGui;
using Umbra.Config;

namespace Umbra.UI.Config;

/// <summary>
/// Selects the draw action used to render a registered parameter in Umbra's configuration UI.
/// </summary>
/// <remarks>
/// Custom-drawer resolution is delegated to <see cref="ParameterDrawerResolver"/>. Built-in numeric, text, enum, and button rendering is delegated to specialized builders, while shared two-column row layout comes from <see cref="ControlLayoutFactory"/>.
/// </remarks>
internal static class ControlFactory
{
    // One entry per supported built-in value type. Enum and fallback are handled separately.
    // Add or replace entries here to change the default control for any value type.
    private static readonly Dictionary<Type, Func<string, IParameter, LabelAlignmentGroup, INumericEditSink?, ITextEditSink?, Action>> _defaultBuilders = new()
    {
        [typeof(Action)] = BuildActionDraw,
        [typeof(bool)] = BuildBoolDraw,
        [typeof(int)] = static (label, parameter, alignGroup, numSink, _) => NumericControlBuilder.BuildInt(label, parameter, alignGroup, numSink),
        [typeof(float)] = static (label, parameter, alignGroup, numSink, _) => NumericControlBuilder.BuildFloat(label, parameter, alignGroup, numSink),
        [typeof(double)] = static (label, parameter, alignGroup, numSink, _) => NumericControlBuilder.BuildDouble(label, parameter, alignGroup, numSink),
        [typeof(string)] = static (label, parameter, alignGroup, _, textSink) => TextControlBuilder.BuildString(label, parameter, alignGroup, textSink),
    };

    /// <summary>
    /// Builds the per-frame draw action for <paramref name="parameter"/> together with any disposable resource created while resolving its renderer.
    /// </summary>
    internal static (Action draw, IDisposable? resource) BuildDrawAction(
        IParameter parameter, string label, LabelAlignmentGroup alignGroup, INumericEditSink? numericEditSink = null, ITextEditSink? textEditSink = null)
    {
        if (ParameterDrawerResolver.TryResolve(parameter, label, alignGroup) is { } custom)
            return custom;

        if (_defaultBuilders.TryGetValue(parameter.ValueType, out var builder))
            return (builder(label, parameter, alignGroup, numericEditSink, textEditSink), null);

        var enumType = Nullable.GetUnderlyingType(parameter.ValueType) ?? parameter.ValueType;
        if (enumType.IsEnum)
            return (EnumControlBuilder.Build(label, parameter, alignGroup), null);

        return (() => ImGui.TextDisabled($"{label}: {parameter.GetValue()}"), null);
    }

    /// <summary>
    /// Builds the per-frame draw action used for an <see cref="Action"/>-typed parameter.
    /// </summary>
    /// <param name="label">The visible button label.</param>
    /// <param name="parameter">The parameter that supplies the action to invoke.</param>
    /// <param name="alignGroup">Accepted for signature consistency. Button drawers own their full row layout and do not use the shared two-column alignment group.</param>
    /// <param name="numericEditSink">Accepted for signature consistency. Action buttons do not participate in numeric edit tracking.</param>
    /// <param name="textEditSink">Accepted for signature consistency. Action buttons do not participate in text edit tracking.</param>
    /// <returns>A draw action that renders the button each frame.</returns>
    private static Action BuildActionDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup, INumericEditSink? numericEditSink, ITextEditSink? textEditSink)
    {
        _ = alignGroup;
        _ = numericEditSink;
        _ = textEditSink;
        var drawer = new Drawers.ButtonDrawer();
        return () => drawer.Draw(label, parameter);
    }

    /// <summary>
    /// Builds the per-frame draw action used for a <see cref="bool"/> parameter.
    /// </summary>
    /// <param name="label">The visible label for the parameter row.</param>
    /// <param name="parameter">The Boolean parameter being rendered.</param>
    /// <param name="alignGroup">The shared alignment group for the owning scope.</param>
    /// <param name="numericEditSink">Accepted for signature consistency. Boolean controls do not participate in numeric edit tracking.</param>
    /// <param name="textEditSink">Accepted for signature consistency. Boolean controls do not participate in text edit tracking.</param>
    /// <returns>A draw action that renders and updates the checkbox each frame.</returns>
    private static Action BuildBoolDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup, INumericEditSink? numericEditSink, ITextEditSink? textEditSink)
    {
        _ = numericEditSink;
        _ = textEditSink;
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
    /// Creates the precomputed two-column layout data used for one parameter row.
    /// </summary>
    internal static ControlLayout CreateControlLayout(
        string label, IParameter parameter, LabelAlignmentGroup alignGroup)
        => ControlLayoutFactory.Create(label, parameter, alignGroup);
}
