namespace Umbra.Config.Attributes;

/// <summary>
/// Switches a <see cref="Parameter{T}"/> of type <see cref="string"/> from a single-line
/// <c>ImGui.InputText</c> to a multi-line <c>ImGui.InputTextMultiline</c> in the settings UI.
/// </summary>
/// <remarks>
/// The control height is calculated as <c>ImGui.GetTextLineHeightWithSpacing() × lines</c>.
/// The width is always <c>-1f</c> (fill available), consistent with ImGui convention for
/// multi-line inputs. Apply <see cref="MaxLengthAttribute"/> alongside this attribute to
/// increase the character buffer beyond the default 256.
/// </remarks>
/// <param name="lines">The visible line count used to derive the control height. Defaults to <c>3</c>.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class MultilineAttribute(int lines = 3) : Attribute
{
    /// <summary>Gets the number of visible text lines used to calculate the control height.</summary>
    public int Lines { get; } = lines;
}
