namespace Umbra.Config.Attributes;

/// <summary>
/// Switches a <see cref="Parameter{T}"/> of type <see cref="string"/> from a single-line
/// <c>ImGui.InputText</c> to a multi-line <c>ImGui.InputTextMultiline</c> in the settings UI.
/// </summary>
/// <param name="lines">The visible line count used to derive the control height. Defaults to <c>3</c>.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraMultilineAttribute(int lines = 3) : Attribute
{
    /// <summary>Gets the number of visible text lines used to calculate the control height.</summary>
    public int Lines { get; } = lines;
}
