namespace Umbra.Config.Attributes;

/// <summary>
/// Inserts one or more <c>ImGui.Spacing()</c> calls below the decorated parameter
/// in the settings UI.
/// </summary>
/// <param name="count">The number of spacing lines to insert. Defaults to <c>1</c>.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraSpacingAfterAttribute(int count = 1) : Attribute
{
    /// <summary>Gets the number of spacing lines to insert after the parameter control.</summary>
    public int Count { get; } = count;
}
