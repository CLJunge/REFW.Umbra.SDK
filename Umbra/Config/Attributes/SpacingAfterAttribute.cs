namespace Umbra.Config.Attributes;

/// <summary>
/// Inserts one or more <c>ImGui.Spacing()</c> calls <em>below</em> the decorated parameter
/// in the settings UI, creating visual separation between groups of controls.
/// </summary>
/// <remarks>
/// <para>
/// The trailing spacing travels with the parameter when <see cref="ParameterOrderAttribute"/> (<c>[ParameterOrder]</c>) reordering is active,
/// because it is owned by the same <see cref="Umbra.UI.Config.Nodes.ParameterNode"/>
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
