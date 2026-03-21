namespace Umbra.SDK.UI.Panel;

/// <summary>
/// Specifies the render order of a section type within a <see cref="PluginPanel"/>.
/// </summary>
/// <remarks>
/// <para>
/// Apply to a state class or configuration record to control the position it occupies when
/// added to a <see cref="PluginPanel"/> via <see cref="PluginPanel.Add"/>. Lower values
/// render first. Sections whose type does not carry this attribute sort last
/// (effectively <see cref="int.MaxValue"/>).
/// </para>
/// <para>
/// This attribute is intentionally separate from <see cref="Config.Attributes.ParameterOrderAttribute"/>,
/// which controls the render order of individual parameters within a configuration group.
/// </para>
/// </remarks>
/// <param name="order">
/// A non-negative integer indicating the section's render position. Lower values appear
/// earlier in the panel.
/// </param>
/// <exception cref="ArgumentOutOfRangeException">
/// Thrown when <paramref name="order"/> is negative.
/// </exception>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class SectionOrderAttribute(int order) : Attribute
{
    /// <summary>Gets the render position of the section. Lower values render first.</summary>
    public int Order { get; } = order >= 0
        ? order
        : throw new ArgumentOutOfRangeException(nameof(order), order, "order must be non-negative.");
}
