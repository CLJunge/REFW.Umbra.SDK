namespace Umbra.SDK.Config.Attributes;

/// <summary>
/// Assigns a category name to a settings parameter, property, field, or settings group class.
/// Parameters sharing the same category name are grouped together when rendered in the UI.
/// When applied to a class decorated with <c>AutoRegisterSettingsAttribute</c>, the category
/// is inherited by all parameters in that group unless overridden at the member level.
/// </summary>
/// <param name="name">The category name used to group related parameters in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class CategoryAttribute(string name) : Attribute
{
    /// <summary>Gets the category name used to group related parameters in the UI.</summary>
    public string Name { get; } = name;
}
