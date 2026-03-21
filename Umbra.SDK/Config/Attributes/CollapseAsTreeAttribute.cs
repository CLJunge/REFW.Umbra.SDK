namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Instructs <see cref="Umbra.SDK.Config.UI.ConfigDrawer{TConfig}"/> to render the decorated settings group's
/// category as a collapsible <c>ImGui.TreeNode</c> instead of an <c>ImGui.SeparatorText</c> header.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to any class decorated with <c>AutoRegisterSettingsAttribute</c>.
/// All categories defined by that class will use <c>ImGui.TreeNode</c> for their header,
/// with all child controls rendered inside the expanded node and <c>ImGui.TreePop()</c>
/// called automatically when the scope closes.
/// </para>
/// <para>
/// To indent the category block (header and child controls), combine this attribute with
/// <see cref="IndentAttribute"/> on the same class.
/// </para>
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CollapseAsTreeAttribute(bool defaultOpen = false) : Attribute
{
    /// <summary>
    /// Gets whether the tree node is rendered in its open (expanded) state by default.
    /// When <see langword="false"/> (the default) the node starts collapsed.
    /// </summary>
    public bool DefaultOpen { get; } = defaultOpen;
}
