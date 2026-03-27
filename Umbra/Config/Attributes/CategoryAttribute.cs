namespace Umbra.Config.Attributes;

/// <summary>
/// Assigns a category name to a settings parameter property, a nested-group property,
/// or a settings-group type.
/// Parameters sharing the same category name are grouped together when rendered in the UI.
/// When applied to an <see cref="AutoRegisterSettingsAttribute"/> type, the category becomes the
/// fallback category for that group's direct parameters unless a member overrides it. When applied
/// to a nested settings-group property, the category creates the visible container section for that
/// group, and direct child controls without their own category render inside that container.
/// </summary>
/// <remarks>
/// The built-in settings registrar and config drawer inspect public instance properties and types.
/// Although this attribute permits field targets, field-applied categories are not consumed by the
/// default settings pipeline.
/// </remarks>
/// <param name="name">The category name used to group related parameters in the UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
[Obsolete("Use UmbraCategoryAttribute instead for the collision-safe Umbra-prefixed name.")]
public class CategoryAttribute(string name) : Attribute
{
    /// <summary>Gets the category name used to group related parameters in the UI.</summary>
    public string Name { get; } = name;
}
