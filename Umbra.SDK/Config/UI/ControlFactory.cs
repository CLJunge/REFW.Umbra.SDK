using System.Numerics;
using Hexa.NET.ImGui;
using Umbra.SDK.Config.Attributes;
using Umbra.SDK.Config.UI.ParameterDrawers;
using Umbra.SDK.Logging;
using Umbra.SDK.UI;

namespace Umbra.SDK.Config.UI;

/// <summary>
/// Builds per-frame ImGui draw <see cref="Action"/> instances for each supported parameter
/// value type. Dispatches to the correct ImGui control based on <see cref="ParameterMetadata"/>
/// and value type, with fallback to a read-only label for unrecognised types.
/// </summary>
/// <remarks>
/// All controls use a two-column text-label layout unconditionally: the parameter label (and
/// optional <c>(?)</c> help marker) is rendered on the left; the editing widget is placed on
/// the right at the column x position determined by <see cref="LabelAlignmentGroup"/>. Within
/// each category or root scope every label is measured each frame so that all controls share
/// the same column x regardless of individual label length.
/// The widget width defaults to fill-to-right-edge (<c>SetNextItemWidth(-1f)</c>) and can be
/// fixed with <c>[ControlWidth(px)]</c>.
/// </remarks>
internal static class ControlFactory
{
    // One entry per supported primitive type. Enum and fallback are handled separately.
    // Add or replace entries here to change the default control for any value type.
    private static readonly Dictionary<Type, Func<string, IParameter, LabelAlignmentGroup, Action>> _defaultBuilders = new()
    {
        [typeof(bool)] = BuildBoolDraw,
        [typeof(int)] = BuildIntDraw,
        [typeof(float)] = BuildFloatDraw,
        [typeof(double)] = BuildDoubleDraw,
        [typeof(string)] = BuildStringDraw,
    };

    /// <summary>
    /// Builds a per-frame draw <see cref="Action"/> for <paramref name="parameter"/>,
    /// dispatching first to a custom drawer recorded in <see cref="ParameterMetadata"/>, then to the
    /// primitive type table, then to the enum fallback, then to a read-only label.
    /// </summary>
    /// <remarks>
    /// Custom drawer types are pre-resolved during <c>SettingsStore.Load()</c> by
    /// <c>ParameterMetadataReader</c> and stored in <see cref="ParameterMetadata.CustomDrawerType"/>
    /// and <see cref="ParameterMetadata.TwoColumnCustomDrawerType"/>, eliminating the need to scan
    /// property attributes at draw-tree construction time.
    /// </remarks>
    /// <param name="parameter">The parameter whose value the action will render and edit.</param>
    /// <param name="label">The display label passed to the ImGui control.</param>
    /// <param name="alignGroup">
    /// The <see cref="LabelAlignmentGroup"/> shared by all parameters in the same category or
    /// root scope. Used to align all editing widgets to a common column x position within the group.
    /// Custom drawers are exempt and manage their own layout.
    /// </param>
    /// <returns>
    /// A tuple containing the per-frame draw <see cref="Action"/> and an optional
    /// <see cref="IDisposable"/> that must be disposed when the owning
    /// <see cref="ConfigDrawer{TConfig}"/> is disposed. The resource is non-<see langword="null"/>
    /// only for custom drawers that override <see cref="IDisposable.Dispose"/>.
    /// </returns>
    internal static (Action draw, IDisposable? resource) BuildDrawAction(
        IParameter parameter, string label, LabelAlignmentGroup alignGroup)
    {
        var meta = parameter.Metadata;

        // 1. Custom drawer — explicit opt-in, highest priority.
        // DrawerType is pre-resolved by ParameterMetadataReader; no attribute scan needed here.
        if (meta.CustomDrawerType is { } customDrawerType)
        {
            try
            {
                var drawer = (IParameterDrawer)Activator.CreateInstance(customDrawerType)!;
                return (() => drawer.Draw(label, parameter), drawer);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"ConfigDrawer: failed to instantiate custom drawer '{customDrawerType.Name}'.");
            }
        }

        // 2. Two-column custom drawer — factory owns layout; drawer renders widget only.
        // DrawerType is pre-resolved by ParameterMetadataReader; no attribute scan needed here.
        if (meta.TwoColumnCustomDrawerType is { } twoColDrawerType)
        {
            try
            {
                var drawer = (ITwoColumnParameterDrawer)Activator.CreateInstance(twoColDrawerType)!;
                var layout = GetControlLayout(label, parameter, meta, alignGroup);
                return (() =>
                {
                    layout.Pre();
                    drawer.Draw(parameter);
                }, drawer);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"ConfigDrawer: failed to instantiate two-column custom drawer '{twoColDrawerType.Name}'.");
            }
        }

        // 3. Primitive type lookup.
        if (_defaultBuilders.TryGetValue(parameter.ValueType, out var builder))
            return (builder(label, parameter, alignGroup), null);

        // 4. Enum — combo box.
        if (parameter.ValueType.IsEnum)
            return (BuildEnumDraw(label, parameter, alignGroup), null);

        // 5. Read-only fallback for unrecognised types.
        return (() => ImGui.TextDisabled($"{label}: {parameter.GetValue()}"), null);
    }

    /// <summary>Builds a per-frame draw action that renders a checkbox for a <see cref="bool"/> parameter.</summary>
    /// <param name="label">The ImGui control label.</param>
    /// <param name="parameter">The <see cref="Parameter{T}"/> of type <see cref="bool"/> to render.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>An <see cref="Action"/> that renders and updates the parameter each frame.</returns>
    private static Action BuildBoolDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<bool>)parameter;
        var layout = GetControlLayout(label, parameter, p.Metadata, alignGroup);
        return () =>
        {
            var v = p.Value;
            layout.Pre();
            if (ImGui.Checkbox(layout.HiddenLabel, ref v)) p.Value = v;
        };
    }

    /// <summary>
    /// Builds a per-frame draw action that renders either a <c>SliderInt</c> (when <see cref="ParameterMetadata.Min"/>
    /// and <see cref="ParameterMetadata.Max"/> are set) or an unconstrained <c>DragInt</c> for an <see cref="int"/> parameter.
    /// </summary>
    /// <param name="label">The ImGui control label.</param>
    /// <param name="parameter">The <see cref="Parameter{T}"/> of type <see cref="int"/> to render.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>An <see cref="Action"/> that renders and updates the parameter each frame.</returns>
    private static Action BuildIntDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<int>)parameter;
        var meta = p.Metadata;
        var fmt = meta.Format ?? "%d";
        var layout = GetControlLayout(label, parameter, meta, alignGroup);

        if (meta.Min is not null && meta.Max is not null)
        {
            var iMin = (int)meta.Min.Value;
            var iMax = (int)meta.Max.Value;
            return () =>
            {
                var v = p.Value;
                layout.Pre();
                if (ImGui.SliderInt(layout.HiddenLabel, ref v, iMin, iMax, fmt)) p.Value = v;
            };
        }

        // No range — unconstrained drag. Use [CustomDrawer<HotkeyDrawer>] for key bindings.
        return () =>
        {
            var v = p.Value;
            var step = meta.Step.HasValue ? (float)meta.Step : 1f;
            layout.Pre();
            if (ImGui.DragInt(layout.HiddenLabel, ref v, step, 0, 0, fmt)) p.Value = v;
        };
    }

    /// <summary>
    /// Builds a per-frame draw action that renders either a <c>SliderFloat</c> (when <see cref="ParameterMetadata.Min"/>
    /// and <see cref="ParameterMetadata.Max"/> are set) or an unconstrained <c>DragFloat</c> for a <see cref="float"/> parameter.
    /// </summary>
    /// <param name="label">The ImGui control label.</param>
    /// <param name="parameter">The <see cref="Parameter{T}"/> of type <see cref="float"/> to render.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>An <see cref="Action"/> that renders and updates the parameter each frame.</returns>
    private static Action BuildFloatDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<float>)parameter;
        var meta = p.Metadata;
        // InferredFloatFormat is precomputed by ParameterMetadataReader during SettingsStore.Load();
        // no format-string construction or Number.FormatFloat overhead at draw-tree build time.
        var fmt = meta.InferredFloatFormat;
        var layout = GetControlLayout(label, parameter, meta, alignGroup);

        if (meta.Min is not null && meta.Max is not null)
        {
            var fMin = (float)meta.Min.Value;
            var fMax = (float)meta.Max.Value;
            return () =>
            {
                var v = p.Value;
                layout.Pre();
                if (ImGui.SliderFloat(layout.HiddenLabel, ref v, fMin, fMax, fmt)) p.Value = v;
            };
        }

        return () =>
        {
            var v = p.Value;
            var step = meta.Step.HasValue ? (float)meta.Step : 1f;
            layout.Pre();
            if (ImGui.DragFloat(layout.HiddenLabel, ref v, step, 0f, 0f, fmt)) p.Value = v;
        };
    }

    /// <summary>
    /// Builds a per-frame draw action that renders either a <c>SliderFloat</c> (when <see cref="ParameterMetadata.Min"/>
    /// and <see cref="ParameterMetadata.Max"/> are set) or an unconstrained <c>DragFloat</c> for a <see cref="double"/>
    /// parameter. The value is narrowed to <see cref="float"/> for the ImGui call and widened back on assignment.
    /// </summary>
    /// <param name="label">The ImGui control label.</param>
    /// <param name="parameter">The <see cref="Parameter{T}"/> of type <see cref="double"/> to render.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>An <see cref="Action"/> that renders and updates the parameter each frame.</returns>
    private static Action BuildDoubleDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<double>)parameter;
        var meta = p.Metadata;
        // InferredFloatFormat is precomputed by ParameterMetadataReader during SettingsStore.Load();
        // no format-string construction or Number.FormatFloat overhead at draw-tree build time.
        var fmt = meta.InferredFloatFormat;
        var layout = GetControlLayout(label, parameter, meta, alignGroup);

        if (meta.Min is not null && meta.Max is not null)
        {
            var fMin = (float)meta.Min.Value;
            var fMax = (float)meta.Max.Value;
            return () =>
            {
                var v = (float)p.Value;
                layout.Pre();
                if (ImGui.SliderFloat(layout.HiddenLabel, ref v, fMin, fMax, fmt)) p.Value = (double)v;
            };
        }

        return () =>
        {
            var v = (float)p.Value;
            var step = meta.Step.HasValue ? (float)meta.Step : 1f;
            layout.Pre();
            if (ImGui.DragFloat(layout.HiddenLabel, ref v, step, 0f, 0f, fmt)) p.Value = (double)v;
        };
    }

    /// <summary>Builds a per-frame draw action that renders an <c>InputText</c> or <c>InputTextMultiline</c> field for a <see cref="string"/> parameter.</summary>
    /// <remarks>
    /// When <see cref="ParameterMetadata.MultilineLines"/> is set (via <c>[Multiline(lines)]</c>),
    /// the control switches to <c>ImGui.InputTextMultiline</c> with a height derived from the line count.
    /// <c>Vector2(0, height)</c> is passed so that <c>SetNextItemWidth</c> (called inside the pre-action)
    /// governs the x dimension. Otherwise a single-line <c>ImGui.InputText</c> is used.
    /// </remarks>
    /// <param name="label">The ImGui control label.</param>
    /// <param name="parameter">The <see cref="Parameter{T}"/> of type <see cref="string"/> to render.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>An <see cref="Action"/> that renders and updates the parameter each frame.</returns>
    private static Action BuildStringDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var p = (Parameter<string>)parameter;
        var meta = p.Metadata;
        var maxLen = meta.MaxLength ?? 256u;
        var layout = GetControlLayout(label, parameter, meta, alignGroup);

        if (meta.MultilineLines is int lines)
        {
            return () =>
            {
                var v = p.Value ?? string.Empty;
                var height = ImGui.GetTextLineHeightWithSpacing() * lines;
                layout.Pre();
                // Pass Vector2(0, height): SetNextItemWidth in Pre() governs x; height is fixed.
                if (ImGui.InputTextMultiline(layout.HiddenLabel, ref v, maxLen, new Vector2(0f, height)))
                    p.Value = v;
            };
        }

        return () =>
        {
            var v = p.Value ?? string.Empty;
            layout.Pre();
            if (ImGui.InputText(layout.HiddenLabel, ref v, maxLen)) p.Value = v;
        };
    }

    /// <summary>Builds a per-frame draw action that renders a <c>Combo</c> box for an <see cref="Enum"/> parameter.</summary>
    /// <param name="label">The ImGui control label.</param>
    /// <param name="parameter">The <see cref="IParameter"/> whose <see cref="IParameter.ValueType"/> is an enum.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>An <see cref="Action"/> that renders and updates the parameter each frame.</returns>
    private static Action BuildEnumDraw(string label, IParameter parameter, LabelAlignmentGroup alignGroup)
    {
        var enumType = parameter.ValueType;
        var names = Enum.GetNames(enumType);
        var rawValues = Enum.GetValues(enumType);
        var values = new object[rawValues.Length];
        for (var i = 0; i < rawValues.Length; i++)
            values[i] = rawValues.GetValue(i)!;
        var layout = GetControlLayout(label, parameter, parameter.Metadata, alignGroup);
        return () =>
        {
            var current = parameter.GetValue();
            var idx = Array.IndexOf(values, current);
            if (idx < 0) idx = 0;
            layout.Pre();
            if (ImGui.Combo(layout.HiddenLabel, ref idx, names, names.Length))
                parameter.SetValue(values[idx]);
        };
    }

    /// <summary>
    /// Constructs a <see cref="ControlLayout"/> capturing the pre-computed layout state for a
    /// single parameter row. All rows use a two-column layout: the visible label (and optional
    /// <c>(?)</c> help marker) on the left; the editing widget on the right at the column x
    /// position determined by <paramref name="alignGroup"/>.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item>
    ///     <term>Column alignment</term>
    ///     <description>
    ///       <see cref="LabelAlignmentGroup.Observe"/> is called every frame to accumulate the
    ///       widest measured label in the group. <c>ImGui.SetCursorPosX</c> then advances the
    ///       cursor to <c>startX + <see cref="LabelAlignmentGroup.LabelWidth"/> +
    ///       <see cref="LabelAlignmentGroup.Margin"/> + spacing</c> before the widget call.
    ///       The cursor is never moved backward: if a label is wider than the committed group
    ///       maximum (possible on frame 1), the control is placed immediately after the label
    ///       with no overlap.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <term>Widget width</term>
    ///     <description>
    ///       <c>[ControlWidth(px)]</c> fixes the widget to <paramref name="px"/> pixels via
    ///       <c>SetNextItemWidth</c>. When no <c>[ControlWidth]</c> is present, <c>-1f</c> is
    ///       used, which fills to the right content-region edge.
    ///     </description>
    ///   </item>
    /// </list>
    /// </remarks>
    /// <param name="label">The display label resolved for the parameter.</param>
    /// <param name="parameter">The parameter being rendered; its <see cref="IParameter.Key"/> is used as the hidden ImGui label fallback when <see cref="ParameterMetadata.HiddenLabel"/> is <see langword="null"/>.</param>
    /// <param name="meta">The parameter metadata holding layout options.</param>
    /// <param name="alignGroup">The shared alignment group for the owning category or root scope.</param>
    /// <returns>
    /// A <see cref="ControlLayout"/> value capturing the label, description, alignment group,
    /// and pre-computed hidden ImGui label for the row. Callers must invoke <see cref="ControlLayout.Pre"/>
    /// immediately before the ImGui widget call to set up layout and alignment state.
    /// </returns>
    private static ControlLayout GetControlLayout(
        string label, IParameter parameter, ParameterMetadata meta, LabelAlignmentGroup alignGroup)
    {
        var hiddenLabel = meta.HiddenLabel ?? string.Concat("##", parameter.Key);
        return new ControlLayout(label, meta.Description, alignGroup, meta.ControlWidth ?? -1f, hiddenLabel);
    }

    }
