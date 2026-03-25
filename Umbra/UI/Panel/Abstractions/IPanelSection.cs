namespace Umbra.UI.Panel;

/// <summary>
/// Defines the rendering and disposal contract that all sections owned by a
/// <see cref="PluginPanel"/> must satisfy.
/// </summary>
/// <remarks>
/// Implement this interface directly only when neither <see cref="ConfigSection{TConfig}"/>
/// nor <see cref="LiveSection{T}"/> suits the use case. For settings UI, use
/// <see cref="ConfigSection{TConfig}"/>. For live game state display, use
/// <see cref="LiveSection{T}"/>.
/// </remarks>
public interface IPanelSection : IDisposable
{
    /// <summary>
    /// Gets the render position of this section within its owning <see cref="PluginPanel"/>.
    /// Lower values render first. Sections that do not override this property sort last.
    /// </summary>
    /// <remarks>
    /// <see cref="LiveSection{T}"/> and <see cref="ConfigSection{TConfig}"/> derive this value
    /// from a <see cref="SectionOrderAttribute"/> placed on the state or config type.
    /// Custom <see cref="IPanelSection"/> implementations can override this property directly.
    /// </remarks>
    int Order => int.MaxValue;

    /// <summary>
    /// Gets the optional label for the tree node that wraps this section's content within the
    /// owning <see cref="PluginPanel"/>, or <see langword="null"/> to render the section flat
    /// with no tree node.
    /// </summary>
    /// <remarks>
    /// When non-<see langword="null"/>, <see cref="PluginPanel.Draw"/> wraps this section's
    /// <see cref="Draw"/> call inside a collapsible <c>ImGui.TreeNode</c> with this label.
    /// Custom <see cref="IPanelSection"/> implementations can override this property to opt in.
    /// <see cref="ConfigSection{TConfig}"/> derives this value from
    /// <see cref="Config.Attributes.ConfigRootNodeAttribute"/> on the config type, or from an
    /// explicit constructor argument. <see cref="LiveSection{T}"/> accepts it as a constructor
    /// parameter.
    /// </remarks>
    string? TreeNodeLabel => null;

    /// <summary>
    /// Gets whether the tree node wrapping this section starts in its open (expanded) state.
    /// </summary>
    /// <remarks>
    /// Ignored when <see cref="TreeNodeLabel"/> is <see langword="null"/>.
    /// When <see langword="false"/> (the default), the node starts collapsed.
    /// </remarks>
    bool TreeNodeDefaultOpen => false;

    /// <summary>
    /// Renders the section. Must be called from within an active ImGui window or child window.
    /// </summary>
    /// <remarks>
    /// Called every frame by <see cref="PluginPanel.Draw"/> while the panel is active.
    /// Implementations must be safe to call on the game's render thread.
    /// </remarks>
    void Draw();
}
