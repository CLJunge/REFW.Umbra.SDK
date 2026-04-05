namespace Umbra.Config.Attributes;

/// <summary>
/// Marks a member as a settings-parameter declaration consumed by Umbra's registration pipeline.
/// </summary>
/// <remarks>
/// <see cref="ConfigStore{TConfig}.Load()"/> currently discovers only public instance properties marked with this attribute. Field targets remain allowed for attribute-shape consistency and reflective tooling, but fields are ignored by <see cref="ConfigRegistrar"/> during normal registration.
/// </remarks>
/// <param name="keyOverride">The optional explicit persisted key segment for the annotated parameter.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraParameterAttribute(string? keyOverride = null) : Attribute
{
    /// <summary>
    /// Gets the explicit persisted-key override declared for the annotated member.
    /// </summary>
    /// <value>The explicit key segment, or <see langword="null"/> when Umbra should derive the segment from the member name.</value>
    public string? KeyOverride { get; } = keyOverride;
}
