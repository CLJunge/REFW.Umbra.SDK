using Umbra.Config.UI.ParameterDrawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Instructs the UI builder to render the decorated nested configuration property or type using a
/// custom <see cref="INestedGroupDrawer{TGroup}"/> instead of the default recursive property expansion.
/// </summary>
/// <remarks>
/// <para>
/// Prefer applying this attribute to the parent property that exposes a nested configuration group,
/// keeping group-specific UI behaviour next to the property declaration. For backward compatibility
/// it may also be applied to a nested configuration class also decorated with
/// <c>[AutoRegisterSettings]</c>, in which case it acts as a fallback when the property itself
/// declares no <c>[NestedGroupDrawer]</c>. When <see cref="Umbra.Config.UI.ConfigDrawer{TConfig}"/>
/// encounters the property, it instantiates <typeparamref name="TDrawer"/> and calls
/// <see cref="INestedGroupDrawer{TGroup}.Draw"/> with the group instance each frame instead of
/// recursing into the class's individual parameters.
/// </para>
/// <para>
/// The drawer has full ImGui layout control; no label, column alignment, or section header
/// is emitted by the factory. Property-level wrapper attributes such as <c>[Category]</c>,
/// <c>[CollapseAsTree]</c>, <c>[SpacingBefore]</c>, <c>[SpacingAfter]</c>, and <c>[HideIf]</c>
/// are still honoured around the drawer output.
/// </para>
/// </remarks>
/// <typeparam name="TDrawer">
/// The <see cref="INestedGroupDrawer{T}"/> implementation to use. Must provide a public
/// parameterless constructor; this constraint is enforced at compile time.
/// </typeparam>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class NestedGroupDrawerAttribute<TDrawer> : Attribute, INestedGroupDrawerAttribute
    where TDrawer : class, new()
{
    /// <summary>Gets the type of the custom drawer used to render the nested group instance.</summary>
    public Type DrawerType => typeof(TDrawer);
}
