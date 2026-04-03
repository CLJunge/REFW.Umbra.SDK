namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the category used to group parameters or nested groups in the configuration UI.
/// </summary>
/// <remarks>
/// Applied to a settings type, this attribute provides the fallback category for that type's direct parameters. Applied to a nested-group property, it defines the visible container category for that group. Applied directly to a parameter member, it overrides any inherited category.
/// </remarks>
/// <param name="name">The category name used in the configuration UI.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class UmbraCategoryAttribute(string name) : Attribute
{
    /// <summary>
    /// Gets the category name used for UI grouping.
    /// </summary>
    /// <value>The declared category name.</value>
    public string Name { get; } = name;
}
