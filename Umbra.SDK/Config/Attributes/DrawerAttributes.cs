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

/// <summary>
/// Marks the decorated settings class as the root node of a plugin configuration tree,
/// causing <see cref="Umbra.SDK.Config.UI.ConfigDrawer{TConfig}"/> to wrap all child categories and parameters
/// inside a single collapsible top-level <c>ImGui.TreeNode</c>.
/// </summary>
/// <remarks>
/// <para>
/// Unlike <see cref="CollapseAsTreeAttribute"/>, which renders each individual category as
/// its own tree node, this attribute wraps the <em>entire</em> configuration — every category
/// and every parameter — inside one root-level tree node identified by <see cref="Label"/>.
/// </para>
/// <para>
/// When <paramref name="label"/> is <see langword="null"/> or omitted, the drawer derives the
/// label from the config class name by inserting spaces before uppercase letters
/// (e.g. <c>PluginConfig</c> becomes <c>"Plugin Config"</c>).
/// </para>
/// </remarks>
/// <param name="label">
/// The label displayed on the root tree node header, or <see langword="null"/> to fall back
/// to a space-separated form of the config class name.
/// </param>
/// <param name="defaultOpen">
/// When <see langword="true"/>, the root tree node starts in its expanded state.
/// Defaults to <see langword="false"/> (collapsed).
/// </param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ConfigRootNodeAttribute(string? label = null, bool defaultOpen = false) : Attribute
{
    /// <summary>
    /// Gets the label shown on the root tree node header, or <see langword="null"/> when the
    /// drawer should derive the label from the config class name.
    /// </summary>
    public string? Label { get; } = label;

    /// <summary>
    /// Gets whether the root tree node starts in its expanded state.
    /// When <see langword="false"/> (the default) the node starts collapsed.
    /// </summary>
    public bool DefaultOpen { get; } = defaultOpen;
}
