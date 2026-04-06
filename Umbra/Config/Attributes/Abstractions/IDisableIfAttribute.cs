namespace Umbra.Config.Attributes;

/// <summary>
/// Exposes the disable-condition data declared by <see cref="UmbraDisableIfAttribute{T}"/> without requiring generic attribute inspection.
/// </summary>
/// <remarks>
/// Umbra's enabled-state pipeline reads this interface to determine which member to inspect and whether the attribute represents a Boolean disable flag or an explicit value comparison.
/// </remarks>
public interface IDisableIfAttribute
{
    /// <summary>
    /// Gets the name of the property or field on the configuration object evaluated by the disable condition.
    /// </summary>
    string MemberName { get; }

    /// <summary>
    /// Gets a value indicating whether the attribute declares an explicit comparison value.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="BoxedValue"/> participates in the comparison; otherwise, <see langword="false"/>.</value>
    bool HasValue { get; }

    /// <summary>
    /// Gets the comparison value as a boxed object when <see cref="HasValue"/> is <see langword="true"/>.
    /// </summary>
    /// <value>The boxed comparison value, or <see langword="null"/> when the attribute represents the Boolean overload.</value>
    object? BoxedValue { get; }
}
