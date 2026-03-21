namespace Umbra.SDK.Config.Attributes;

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
