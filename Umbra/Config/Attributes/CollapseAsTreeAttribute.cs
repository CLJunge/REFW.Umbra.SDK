namespace Umbra.Config.Attributes;

/// <summary>
/// Instructs <see cref="Umbra.Config.UI.ConfigDrawer{TConfig}"/> to render the decorated settings group's
/// category as a collapsible <c>ImGui.TreeNode</c> instead of an <c>ImGui.SeparatorText</c> header.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to a nested settings-group property to control how that property's
/// section is rendered in the UI. For backward compatibility it may also be applied to an
/// <c>AutoRegisterSettingsAttribute</c>-decorated class or struct, in which case it acts as a
/// fallback when the parent property declares no <c>[CollapseAsTree]</c> of its own.
/// All categories defined by that group will use <c>ImGui.TreeNode</c> for their header, with
/// all child controls rendered inside the expanded node and <c>ImGui.TreePop()</c> called
/// automatically when the scope closes.
/// </para>
/// <para>
/// To indent the category block (header and child controls), combine this attribute with
/// <see cref="IndentAttribute"/> on the same property or settings-group type.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CollapseAsTreeAttribute(bool defaultOpen = false) : Attribute
{
    /// <summary>
    /// Gets whether the tree node is rendered in its open (expanded) state by default.
    /// When <see langword="false"/> (the default) the node starts collapsed.
    /// </summary>
    public bool DefaultOpen { get; } = defaultOpen;
}
