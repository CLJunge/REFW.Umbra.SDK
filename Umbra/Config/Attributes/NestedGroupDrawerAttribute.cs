using Umbra.Config.UI.ParameterDrawers;

namespace Umbra.Config.Attributes;

/// <summary>
/// Instructs the UI builder to render the decorated nested configuration class using a custom
/// <see cref="INestedGroupDrawer{TGroup}"/> instead of the default recursive property expansion.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to a nested configuration class also decorated with
/// <c>[AutoRegisterSettings]</c>. When <see cref="Umbra.Config.UI.ConfigDrawer{TConfig}"/> encounters a
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
