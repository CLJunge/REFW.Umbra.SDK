using Umbra.UI.Config.Drawers;

namespace Umbra.Config.Attributes;

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
