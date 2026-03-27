namespace Umbra.Config.Attributes;

/// <summary>
/// Compatibility alias for <see cref="ControlWidthAttribute"/> on button parameters.
/// </summary>
/// <remarks>
/// Use <see cref="ControlWidthAttribute"/> instead. Button rendering now reads button width from
/// the unified <see cref="ParameterMetadata.ControlWidth"/> metadata slot, interpreting the value
/// with button semantics: <c>0f</c> = auto-size to label, negative = fill available width,
/// positive = fixed pixel width.
/// </remarks>
/// <param name="width">
/// The pixel width for the button: <c>0f</c> = auto-size, <c>-1f</c> = fill available,
/// positive = fixed px.
/// </param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
[Obsolete("ButtonWidthAttribute is obsolete. Use ControlWidthAttribute instead.")]
public sealed class ButtonWidthAttribute(float width) : ControlWidthAttribute(width) { }
