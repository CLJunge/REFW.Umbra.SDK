namespace Umbra.Config.Attributes;

/// <summary>
/// Non-generic marker interface implemented by <see cref="HideIfAttribute{T}"/>.
/// Allows hide-condition detection and data access without runtime generic type inspection
/// or reflective property reads on the attribute instance.
/// </summary>
public interface IHideIfAttribute
{
    /// <summary>Gets the name of the property or field on the configuration class to evaluate.</summary>
    string MemberName { get; }

    /// <summary>
    /// Gets a value indicating whether an explicit comparison value was provided.
    /// When <see langword="false"/>, the member is treated as a <c>bool</c> and the
    /// parameter is hidden while it is <c>true</c>.
    /// </summary>
    bool HasValue { get; }

    /// <summary>
    /// Gets the comparison value as a boxed <see cref="object"/>, or <see langword="null"/>
    /// when <see cref="HasValue"/> is <see langword="false"/>.
    /// </summary>
    object? BoxedValue { get; }
}
