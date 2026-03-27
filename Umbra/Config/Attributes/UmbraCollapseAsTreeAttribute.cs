using Hexa.NET.ImGui;

namespace Umbra.Config.Attributes;

/// <summary>
/// Instructs <see cref="UI.Config.ConfigDrawer{TConfig}"/> to render the decorated settings group's
/// category as a collapsible <see cref="ImGui.TreeNode(string)"/> instead of a
/// <see cref="ImGui.SeparatorText(string)"/> header.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraCollapseAsTreeAttribute(bool defaultOpen = false) : Attribute
{
    /// <summary>
    /// Gets whether the tree node is rendered in its open (expanded) state by default.
    /// </summary>
    public bool DefaultOpen { get; } = defaultOpen;
}
