namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the printf-style format string used when an annotated numeric parameter is rendered in ImGui controls.
/// </summary>
/// <remarks>
/// This attribute overrides the fallback format that Umbra would otherwise infer from <see cref="UmbraStepAttribute"/>.
/// </remarks>
/// <param name="format">The ImGui-compatible printf-style format string.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraFormatAttribute(string format) : Attribute
{
    /// <summary>
    /// Gets the declared ImGui format string.
    /// </summary>
    /// <value>The printf-style format string used for display.</value>
    public string Format { get; } = format;
}
