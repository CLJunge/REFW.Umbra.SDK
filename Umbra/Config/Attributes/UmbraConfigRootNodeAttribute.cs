namespace Umbra.Config.Attributes;

/// <summary>
/// Marks the decorated settings class as the root node of a plugin configuration tree,
/// causing <see cref="UI.Config.ConfigDrawer{TConfig}"/> to wrap all child categories and parameters
/// inside a single collapsible top-level <c>ImGui.TreeNode</c>.
/// </summary>
/// <param name="label">
/// The label displayed on the root tree node header, or <see langword="null"/> to fall back
/// to a space-separated form of the config class name.
/// </param>
/// <param name="defaultOpen">
/// When <see langword="true"/>, the root tree node starts in its expanded state.
/// </param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraConfigRootNodeAttribute(string? label = null, bool defaultOpen = false) : Attribute
{
    /// <summary>Gets the label shown on the root tree node header.</summary>
    public string? Label { get; } = label;

    /// <summary>Gets whether the root tree node starts in its expanded state.</summary>
    public bool DefaultOpen { get; } = defaultOpen;
}
