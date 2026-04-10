namespace Umbra.Config.Attributes;

/// <summary>
/// Declares the display order of a parameter within its local rendered scope.
/// </summary>
/// <param name="order">The sort key used when the parameter list is ordered. Lower values render first.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraParameterOrderAttribute(int order) : Attribute
{
    /// <summary>
    /// Gets the declared sort key.
    /// </summary>
    /// <value>The parameter order within its local rendered scope.</value>
    public int Order { get; } = order;
}
