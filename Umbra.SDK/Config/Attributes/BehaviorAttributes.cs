using Umbra.SDK.Config.UI.ParameterDrawers;

namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="CustomDrawerAttribute{TDrawer}"/>.
/// Allows custom-drawer detection via
/// <c>property.GetDrawerAttribute&lt;ICustomDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
public interface ICustomDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="IParameterDrawer"/> type used to render the parameter.</summary>
    Type DrawerType { get; }
}

/// <summary>
/// Non-generic marker interface implemented by <see cref="TwoColumnCustomDrawerAttribute{TDrawer}"/>.
/// Allows two-column custom-drawer detection via
/// <c>property.GetDrawerAttribute&lt;ITwoColumnCustomDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
public interface ITwoColumnCustomDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="ITwoColumnParameterDrawer"/> type used to render the parameter's editing widget.</summary>
    Type DrawerType { get; }
}

/// <summary>
/// Non-generic marker interface implemented by <see cref="NestedGroupDrawerAttribute{TDrawer}"/>.
/// Allows nested-group custom-drawer detection via
/// <c>propType.GetDrawerAttribute&lt;INestedGroupDrawerAttribute&gt;()</c>
/// without runtime generic type inspection.
/// </summary>
public interface INestedGroupDrawerAttribute
{
    /// <summary>Gets the concrete <see cref="INestedGroupDrawer{T}"/> type used to render the nested group instance.</summary>
    Type DrawerType { get; }
}

/// <summary>
/// Hides a settings parameter in the UI when a named member on the same configuration
/// class satisfies a condition.
/// </summary>
/// <typeparam name="T">The type of the member value to compare against.</typeparam>
/// <remarks>
/// <para>
/// When constructed with only a <paramref name="memberName"/>, the referenced member must
/// be a <c>bool</c>; the parameter is hidden while that member is <c>true</c>.
/// </para>
/// <para>
/// When constructed with both a <paramref name="memberName"/> and a <paramref name="value"/>,
/// the parameter is hidden while the member's current value equals <paramref name="value"/>.
/// Comparison is performed with <see cref="object.Equals(object, object)"/>.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class HideIfAttribute<T> : Attribute
{
    /// <summary>Gets the name of the property or field on the configuration class to evaluate.</summary>
    public string MemberName { get; }

    /// <summary>
    /// Gets the value to compare the member against. When <see cref="HasValue"/> is
    /// <see langword="false"/>, this is <see langword="default"/> and the member is
    /// treated as a plain <c>bool</c> (hidden while <c>true</c>).
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets a value indicating whether an explicit comparison value was provided.
    /// When <see langword="false"/>, the member is treated as a <c>bool</c> and the
    /// parameter is hidden while it is <c>true</c>.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Hides this parameter while the named <c>bool</c> member on the configuration class is <c>true</c>.
    /// </summary>
    /// <param name="memberName">The name of a <c>bool</c> property or field on the configuration class.</param>
    public HideIfAttribute(string memberName)
    {
        MemberName = memberName;
        Value = default;
        HasValue = false;
    }

    /// <summary>
    /// Hides this parameter while the named member on the configuration class equals <paramref name="value"/>.
    /// </summary>
    /// <param name="memberName">The name of a property or field on the configuration class.</param>
    /// <param name="value">The value that, when matched, causes the parameter to be hidden.</param>
    public HideIfAttribute(string memberName, T value)
    {
        MemberName = memberName;
        Value = value;
        HasValue = true;
    }
}

/// <summary>
/// Instructs the UI builder to render this settings parameter using a custom
/// <see cref="IParameterDrawer"/> instead of the default control inferred from the
/// parameter's value type (e.g. slider for <c>float</c>, checkbox for <c>bool</c>).
/// </summary>
/// <typeparam name="TDrawer">
/// The <see cref="IParameterDrawer"/> implementation to use. Must also provide a public
/// parameterless constructor; both constraints are enforced at compile time.
/// </typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class CustomDrawerAttribute<TDrawer> : Attribute, ICustomDrawerAttribute where TDrawer : IParameterDrawer, new()
{
    /// <summary>Gets the type of the custom drawer used to render this parameter.</summary>
    public Type DrawerType => typeof(TDrawer);
}

/// <summary>
/// Instructs the UI builder to render this settings parameter's editing widget using a custom
/// <see cref="ITwoColumnParameterDrawer"/> while keeping the standard two-column layout.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="CustomDrawerAttribute{TDrawer}"/>, which gives the drawer full rendering
/// control, this attribute lets the factory handle label text, optional <c>(?)</c> help-marker
/// placement, and column alignment automatically. The drawer only needs to call its ImGui widget;
/// <c>SetNextItemWidth</c> is already applied before <see cref="ITwoColumnParameterDrawer.Draw"/>
/// is invoked, honouring any <c>[ControlWidth]</c> on the parameter.
/// </para>
/// <para>
/// Use <c>$"##{parameter.Key}"</c> as the ImGui widget ID inside the drawer to avoid ID
/// collisions with other controls in the same window.
/// </para>
/// </remarks>
/// <typeparam name="TDrawer">
/// The <see cref="ITwoColumnParameterDrawer"/> implementation to use. Must also provide a public
/// parameterless constructor; both constraints are enforced at compile time.
/// </typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class TwoColumnCustomDrawerAttribute<TDrawer> : Attribute, ITwoColumnCustomDrawerAttribute
    where TDrawer : ITwoColumnParameterDrawer, new()
{
    /// <summary>Gets the type of the custom drawer used to render this parameter's editing widget.</summary>
    public Type DrawerType => typeof(TDrawer);
}

/// <summary>
/// Instructs the UI builder to render the decorated nested configuration class using a custom
/// <see cref="INestedGroupDrawer{TGroup}"/> instead of the default recursive property expansion.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to a nested configuration class also decorated with
/// <c>[AutoRegisterSettings]</c>. When <see cref="ConfigDrawer{TConfig}"/> encounters a
/// property typed as this class, it instantiates <typeparamref name="TDrawer"/> and calls
/// <see cref="INestedGroupDrawer{TGroup}.Draw"/> with the group instance each frame instead of
/// recursing into the class's individual parameters.
/// </para>
/// <para>
/// The drawer has full ImGui layout control; no label, column alignment, or section header
/// is emitted by the factory. <c>[Category]</c>, <c>[SpacingBefore]</c>, <c>[SpacingAfter]</c>,
/// and <c>[HideIf]</c> on the property declaration are still honoured, while <c>[CollapseAsTree]</c>
/// must be applied to the nested group type itself.
/// </para>
/// </remarks>
/// <typeparam name="TDrawer">
/// The <see cref="INestedGroupDrawer{T}"/> implementation to use. Must provide a public
/// parameterless constructor; this constraint is enforced at compile time.
/// </typeparam>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class NestedGroupDrawerAttribute<TDrawer> : Attribute, INestedGroupDrawerAttribute
    where TDrawer : class, new()
{
    /// <summary>Gets the type of the custom drawer used to render the nested group instance.</summary>
    public Type DrawerType => typeof(TDrawer);
}

/// <summary>
/// Inserts one or more <c>ImGui.Spacing()</c> calls <em>above</em> the decorated parameter
/// in the settings UI, creating visual separation between groups of controls.
/// </summary>
/// <remarks>
/// <para>
/// The leading spacing travels with the parameter when <c>[Order]</c> reordering is active,
/// because it is owned by the same <see cref="Umbra.SDK.Config.UI.Nodes.ParameterNode"/>
/// rather than being a separate sibling node.
/// </para>
/// <para>To add spacing <em>below</em> a control instead, use <see cref="SpacingAfterAttribute"/>.</para>
/// </remarks>
/// <param name="count">The number of spacing lines to insert. Defaults to <c>1</c>.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SpacingBeforeAttribute(int count = 1) : Attribute
{
    /// <summary>Gets the number of spacing lines to insert before the parameter control.</summary>
    public int Count { get; } = count;
}

/// <summary>
/// Inserts one or more <c>ImGui.Spacing()</c> calls <em>below</em> the decorated parameter
/// in the settings UI, creating visual separation between groups of controls.
/// </summary>
/// <remarks>
/// <para>
/// The trailing spacing travels with the parameter when <c>[Order]</c> reordering is active,
/// because it is owned by the same <see cref="Umbra.SDK.Config.UI.Nodes.ParameterNode"/>
/// rather than being a separate sibling node.
/// </para>
/// <para>To add spacing <em>above</em> a control instead, use <see cref="SpacingBeforeAttribute"/>.</para>
/// </remarks>
/// <param name="count">The number of spacing lines to insert. Defaults to <c>1</c>.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class SpacingAfterAttribute(int count = 1) : Attribute
{
    /// <summary>Gets the number of spacing lines to insert after the parameter control.</summary>
    public int Count { get; } = count;
}

/// <summary>
/// Indents the decorated parameter's control, or all parameter controls within a settings
/// group class, in the settings UI.
/// </summary>
/// <remarks>
/// <para>
/// When applied to a <c>Parameter&lt;T&gt;</c> property or field, only that individual
/// control is indented.
/// </para>
/// <para>
/// When applied to a settings group class decorated with <c>AutoRegisterSettingsAttribute</c>,
/// all parameters in that group inherit the indentation. A property-level <c>[Indent]</c> on
/// an individual parameter within the group overrides the class-level value for that control.
/// </para>
/// <para>
/// Wraps the control with <c>ImGui.Indent</c> / <c>ImGui.Unindent</c>.
/// When <paramref name="amount"/> is <c>0</c>, ImGui's default indent spacing
/// (<c>ImGui.GetStyle().IndentSpacing</c>) is used.
/// </para>
/// </remarks>
/// <param name="amount">
/// The indentation width in pixels, or <c>0</c> to use ImGui's default indent spacing.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class IndentAttribute(float amount = 0f) : Attribute
{
    /// <summary>Gets the indentation width in pixels. <c>0</c> means use ImGui's default.</summary>
    public float Amount { get; } = amount;
}

/// <summary>
/// Switches a <see cref="Parameter{T}"/> of type <see cref="string"/> from a single-line
/// <c>ImGui.InputText</c> to a multi-line <c>ImGui.InputTextMultiline</c> in the settings UI.
/// </summary>
/// <remarks>
/// The control height is calculated as <c>ImGui.GetTextLineHeightWithSpacing() × lines</c>.
/// The width is always <c>-1f</c> (fill available), consistent with ImGui convention for
/// multi-line inputs. Apply <see cref="MaxLengthAttribute"/> alongside this attribute to
/// increase the character buffer beyond the default 256.
/// </remarks>
/// <param name="lines">The visible line count used to derive the control height. Defaults to <c>3</c>.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class MultilineAttribute(int lines = 3) : Attribute
{
    /// <summary>Gets the number of visible text lines used to calculate the control height.</summary>
    public int Lines { get; } = lines;
}

/// <summary>
/// Controls the display order of a settings parameter within its category context.
/// Parameters with lower values are shown first.
/// </summary>
/// <remarks>
/// Parameters without this attribute receive an implicit order of <see cref="int.MaxValue"/>,
/// placing them after all explicitly ordered entries. Among parameters sharing the same order
/// value — including all unordered ones — original declaration order is preserved via stable sort.
/// Ordering is scoped per-context: root-level parameters sort among themselves, and parameters
/// inside each nested settings group sort within their own category independently.
/// </remarks>
/// <param name="order">The sort key. Lower values appear first within the same category context.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ParameterOrderAttribute(int order) : Attribute
{
    /// <summary>Gets the sort key for this parameter within its category context. Lower values appear first.</summary>
    public int Order { get; } = order;
}

/// <summary>
/// Adds extra pixels of space between the label column and the editing control for all
/// parameters in the decorated settings class.
/// </summary>
/// <remarks>
/// <para>
/// The margin is applied on top of the standard <c>ImGui.GetStyle().ItemSpacing.X</c> gap
/// that already separates labels from controls. Use it to widen the column gap when labels
/// and controls feel too close together for a specific category or root settings group.
/// </para>
/// <para>
/// The attribute is class-scoped: it affects the <see cref="Umbra.SDK.Config.UI.LabelAlignmentGroup"/>
/// shared by all parameters that belong to the same category or root scope as the decorated class.
/// When multiple classes contribute to the same alignment group with different margin values,
/// the last class processed determines the final margin.
/// </para>
/// </remarks>
/// <param name="pixels">Extra pixels to insert between the label column and the editing widget. Must be ≥ 0.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class LabelMarginAttribute(float pixels) : Attribute
{
    /// <summary>Gets the extra pixel gap inserted between the label column and the editing widget.</summary>
    public float Pixels { get; } = pixels;
}
