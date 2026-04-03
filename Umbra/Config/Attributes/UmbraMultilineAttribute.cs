namespace Umbra.Config.Attributes;

/// <summary>
/// Declares that an annotated string parameter should render as a multi-line text input control.
/// </summary>
/// <remarks>
/// When this attribute is absent, Umbra renders the string parameter with its standard single-line text input control.
/// </remarks>
/// <param name="lines">The visible line count used to derive the control height.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraMultilineAttribute(int lines = 3) : Attribute
{
    /// <summary>
    /// Gets the visible line count used to derive the multi-line control height.
    /// </summary>
    /// <value>The requested visible line count.</value>
    public int Lines { get; } = lines;
}
