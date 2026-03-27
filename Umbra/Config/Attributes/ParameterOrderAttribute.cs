namespace Umbra.Config.Attributes;

/// <summary>
/// Controls the display order of a settings parameter within its category context.
/// Parameters with lower values are shown first.
/// </summary>
/// <remarks>
/// Parameters without this attribute receive an implicit order of <see cref="int.MaxValue"/>,
/// placing them after all explicitly ordered entries. Among parameters sharing the same order
/// value — including all unordered ones — original declaration order is preserved via stable sort.
/// Ordering is scoped per-context: root-level parameters sort among themselves, and parameters
/// inside each nested settings group sort within their own category independently.
/// </remarks>
/// <param name="order">The sort key. Lower values appear first within the same category context.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[Obsolete("Use UmbraParameterOrderAttribute instead for the collision-safe Umbra-prefixed name.")]
public class ParameterOrderAttribute(int order) : Attribute
{
    /// <summary>Gets the sort key for this parameter within its category context. Lower values appear first.</summary>
    public int Order { get; } = order;
}
