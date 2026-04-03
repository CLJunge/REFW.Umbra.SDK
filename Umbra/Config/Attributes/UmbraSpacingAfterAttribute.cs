namespace Umbra.Config.Attributes;

/// <summary>
/// Declares extra vertical spacing inserted below an annotated parameter in the configuration UI.
/// </summary>
/// <param name="count">The number of <c>ImGui.Spacing()</c> calls to insert below the parameter.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraSpacingAfterAttribute(int count = 1) : Attribute
{
    /// <summary>
    /// Gets the number of spacing calls inserted below the parameter.
    /// </summary>
    /// <value>The declared spacing count.</value>
    public int Count { get; } = count;
}
