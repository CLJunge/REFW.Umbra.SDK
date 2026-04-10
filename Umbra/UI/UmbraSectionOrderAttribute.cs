using Umbra.UI.Panel;

namespace Umbra.UI;

/// <summary>
/// Declares the render order used when a section type is added to a <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// Apply this attribute to a configuration type or live-state type whose corresponding section should render at a specific position inside the panel. Lower values render first, while section types without this attribute sort last.
/// </remarks>
/// <param name="order">The non-negative sort key for the section type. Lower values render earlier.</param>
/// <exception cref="ArgumentOutOfRangeException"><paramref name="order"/> is negative.</exception>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraSectionOrderAttribute(int order) : Attribute
{
    /// <summary>
    /// Gets the render-order value associated with the annotated section type.
    /// </summary>
    /// <value>The non-negative sort key used by <see cref="PluginPanel"/> section ordering.</value>
    public int Order { get; } = order >= 0
        ? order
        : throw new ArgumentOutOfRangeException(nameof(order), order, "order must be non-negative.");
}
