using Hexa.NET.ImGui;
using Umbra.UI.Config;
using Umbra.UI.LiveState;

namespace Umbra.UI.Panel;

/// <summary>
/// Defines the rendering, ordering, and disposal contract for sections owned by a <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// Implement this interface directly only when neither <see cref="ConfigSection{TConfig}"/> nor <see cref="LiveStateSection{T}"/> matches the section's responsibility. <see cref="PluginPanel"/> uses this contract to sort sections, optionally wrap them in tree nodes, and dispose them when the panel is disposed.
/// </remarks>
public interface IPanelSection : IDisposable
{
    /// <summary>
    /// Gets the render order for this section within its owning <see cref="PluginPanel"/>.
    /// </summary>
    /// <value>An ascending sort key. Lower values render first. The default value is <see cref="int.MaxValue"/>.</value>
    /// <remarks>
    /// <see cref="LiveStateSection{T}"/> and <see cref="ConfigSection{TConfig}"/> derive this value from <see cref="UmbraSectionOrderAttribute"/> on the state or config type. Custom implementations can override the property directly.
    /// </remarks>
    int Order => int.MaxValue;

    /// <summary>
    /// Gets the optional tree-node label that wraps this section inside the owning <see cref="PluginPanel"/>.
    /// </summary>
    /// <value>The visible tree-node label, or <see langword="null"/> to render the section without a wrapping tree node.</value>
    /// <remarks>
    /// <para>
    /// When this property is non-<see langword="null"/>, the panel wraps <see cref="Draw()"/> in a collapsible <see cref="ImGui.TreeNode(string)"/>. <see cref="ConfigSection{TConfig}"/> derives the label from <see cref="Umbra.Config.Attributes.UmbraRootNodeAttribute"/> on the config type or from an explicit constructor argument. <see cref="LiveStateSection{T}"/> accepts it as a constructor argument.
    /// </para>
    /// <para>
    /// The label must not contain ImGui's <c>"##"</c> label and ID separator. <see cref="PluginPanel.Add(IPanelSection)"/> warns when that token is present, and <see cref="PluginPanelTreeNodeLabels"/> strips the suffix at render time so the panel can append its own stable ID disambiguation suffix.
    /// </para>
    /// </remarks>
    string? TreeNodeLabel => null;

    /// <summary>
    /// Gets a value indicating whether the wrapping tree node starts expanded.
    /// </summary>
    /// <value><see langword="true"/> if the wrapping tree node should default to the open state; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// This property is ignored when <see cref="TreeNodeLabel"/> is <see langword="null"/>.
    /// </remarks>
    bool TreeNodeDefaultOpen => false;

    /// <summary>
    /// Gets the stable identifier used by the owning <see cref="PluginPanel"/> to disambiguate this section's tree node in ImGui.
    /// </summary>
    /// <value>A stable string appended to the visible label through ImGui's <c>##</c> suffix convention.</value>
    /// <remarks>
    /// <para>
    /// When <see cref="TreeNodeLabel"/> is non-<see langword="null"/>, the panel renders the node as <c>$"{TreeNodeLabel}##{SectionId}"</c>. The suffix is invisible in the UI but changes ImGui's hash so multiple sections with the same visible label still keep distinct persisted open and closed state.
    /// </para>
    /// <para>
    /// The identifier must remain stable for the lifetime of the panel. The default implementation returns the runtime type's <see cref="Type.FullName"/>, falling back to <see cref="System.Reflection.MemberInfo.Name"/> when the full name is unavailable. Override this property when multiple sections of the same concrete type can appear in one panel.
    /// </para>
    /// </remarks>
    string SectionId => GetType().FullName ?? GetType().Name;

    /// <summary>
    /// Renders the section inside the current ImGui window or child window.
    /// </summary>
    /// <remarks>
    /// <see cref="PluginPanel.Draw()"/> calls this method every frame while the panel is active. Implementations should be safe to execute on the game's render thread.
    /// </remarks>
    void Draw();
}
