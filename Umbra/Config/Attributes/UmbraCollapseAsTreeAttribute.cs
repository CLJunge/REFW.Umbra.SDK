namespace Umbra.Config.Attributes;

/// <summary>
/// Instructs <see cref="UI.Config.ConfigDrawer{TConfig}"/> to render the annotated configuration scope as a collapsible tree node instead of a separator header.
/// </summary>
/// <remarks>
/// This attribute can be applied to a configuration type or nested-group property. It affects only how the scope is presented in the UI; it does not change parameter discovery or persisted keys.
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraCollapseAsTreeAttribute(bool defaultOpen = false) : Attribute
{
    /// <summary>
    /// Gets a value indicating whether the rendered tree node starts in the open state.
    /// </summary>
    /// <value><see langword="true"/> if the tree node should start expanded; otherwise, <see langword="false"/>.</value>
    public bool DefaultOpen { get; } = defaultOpen;
}
