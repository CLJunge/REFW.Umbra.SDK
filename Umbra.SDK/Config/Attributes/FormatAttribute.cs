namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Overrides the printf-style format string used when displaying a numeric parameter's value
/// inside ImGui controls such as sliders and drag inputs.
/// </summary>
/// <remarks>
/// <para>Follows ImGui/printf conventions. Examples:</para>
/// <list type="bullet">
///   <item><c>"%.0f°"</c> — integer degrees with a degree symbol.</item>
///   <item><c>"%.2f m"</c> — two decimal places with a unit suffix.</item>
///   <item><c>"%d px"</c> — integer pixels.</item>
/// </list>
/// <para>
/// When absent, the format is inferred from <see cref="StepAttribute"/> for floating-point
/// types (<c>"%.Nf"</c> where N matches the step's decimal places) and defaults to
/// <c>"%d"</c> for integer types.
/// </para>
/// </remarks>
/// <param name="format">A printf-style format string compatible with ImGui.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class FormatAttribute(string format) : Attribute
{
    /// <summary>Gets the printf-style format string used to display the parameter's value.</summary>
    public string Format { get; } = format;
}
