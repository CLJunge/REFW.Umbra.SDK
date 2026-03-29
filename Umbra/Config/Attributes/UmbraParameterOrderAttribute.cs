namespace Umbra.Config.Attributes;

/// <summary>
/// Controls the display order of a settings parameter within its local rendered scope.
/// </summary>
/// <param name="order">The sort key. Lower values appear first within the same rendered scope.</param>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class UmbraParameterOrderAttribute(int order) : Attribute
{
    /// <summary>Gets the sort key for this parameter within its local rendered scope.</summary>
    public int Order { get; } = order;
}
