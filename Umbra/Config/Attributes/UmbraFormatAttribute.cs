namespace Umbra.Config.Attributes;

/// <summary>
/// Overrides the printf-style format string used when displaying a numeric parameter's value
/// inside ImGui controls such as sliders and drag inputs.
/// </summary>
/// <param name="format">A printf-style format string compatible with ImGui.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraFormatAttribute(string format) : Attribute
{
    /// <summary>Gets the printf-style format string used to display the parameter's value.</summary>
    public string Format { get; } = format;
}
