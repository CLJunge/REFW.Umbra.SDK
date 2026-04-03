namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the top-level tree node used when <see cref="UI.Config.ConfigDrawer{TConfig}"/> renders an annotated configuration type.
/// </summary>
/// <remarks>
/// This attribute affects only UI presentation. It does not change parameter registration or persisted keys.
/// </remarks>
/// <param name="label">The optional visible label shown on the root tree node.</param>
/// <param name="defaultOpen"><see langword="true"/> to start the root tree node expanded; otherwise, <see langword="false"/>.</param>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraRootNodeAttribute(string? label = null, bool defaultOpen = false) : Attribute
{
    /// <summary>
    /// Gets the visible label shown on the root tree node.
    /// </summary>
    /// <value>The declared label, or <see langword="null"/> when Umbra should infer one from the type name.</value>
    public string? Label { get; } = label;

    /// <summary>
    /// Gets a value indicating whether the root tree node starts in the open state.
    /// </summary>
    /// <value><see langword="true"/> if the root tree node should start expanded; otherwise, <see langword="false"/>.</value>
    public bool DefaultOpen { get; } = defaultOpen;
}
