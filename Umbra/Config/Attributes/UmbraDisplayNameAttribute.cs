namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the human-readable label used for a parameter in the configuration UI.
/// </summary>
/// <param name="name">The explicit display label to use.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraDisplayNameAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the explicit display label declared for the annotated member.
    /// </summary>
    /// <value>The display label used instead of the inferred member-name label.</value>
    public string Name { get; } = name;
}
