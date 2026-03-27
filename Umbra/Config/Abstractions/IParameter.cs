namespace Umbra.Config;

/// <summary>
/// Represents a configuration parameter that holds a typed value and notifies
/// listeners when that value changes.
/// </summary>
public interface IParameter
{
    /// <summary>
    /// Raised when the parameter's value changes through <see cref="SetValue(object?)"/>
    /// or <see cref="Reset(bool)"/> with <c>raiseEvent = true</c>.
    /// Not raised when the value is updated silently via <see cref="SetValueWithoutNotify(object?)"/>
    /// or when <see cref="Reset(bool)"/> is called with <c>raiseEvent = false</c>.
    /// </summary>
    event Action? ValueChanged;

    /// <summary>
    /// Gets or sets the unique key that identifies this parameter within its settings group.
    /// </summary>
    string Key { get; set; }

    /// <summary>
    /// Gets or sets the metadata associated with this parameter, such as its display
    /// name, description, and ordering information.
    /// </summary>
    ParameterMetadata Metadata { get; set; }

    /// <summary>
    /// Gets the <see cref="Type"/> of the value held by this parameter.
    /// </summary>
    Type ValueType { get; }

    /// <summary>
    /// Gets a value indicating whether the current value differs from the parameter's default value.
    /// </summary>
    bool IsModified { get; }

    /// <summary>
    /// Returns the current value of this parameter as an untyped <see cref="object"/>.
    /// </summary>
    /// <returns>
    /// The current value, or <see langword="null"/> if no value has been set.
    /// </returns>
    object? GetValue();

    /// <summary>
    /// Sets the value of this parameter and raises <see cref="ValueChanged"/> if the
    /// value differs from the current one.
    /// </summary>
    /// <remarks>
    /// Implementations may reject values that violate metadata-defined constraints,
    /// such as numeric min/max bounds.
    /// </remarks>
    /// <param name="value">
    /// The new value to assign. Must be assignable to <see cref="ValueType"/>.
    /// <see langword="null"/> is only valid when <see cref="ValueType"/> is a nullable type.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is not assignable to <see cref="ValueType"/>,
    /// or when <paramref name="value"/> is <see langword="null"/> and <see cref="ValueType"/>
    /// is a non-nullable value type.
    /// </exception>
    void SetValue(object? value);

    /// <summary>
    /// Resets the parameter to its initial state, optionally raising a change event.
    /// Validation is bypassed so that <see cref="IsModified"/> is always
    /// <see langword="false"/> after this call.
    /// </summary>
    /// <param name="raiseEvent">true to raise a change event after resetting; otherwise, false.</param>
    void Reset(bool raiseEvent = true);

    /// <summary>
    /// Sets the value of this parameter without raising <see cref="ValueChanged"/>.
    /// Useful for initializing or restoring persisted values without triggering side effects.
    /// </summary>
    /// <remarks>
    /// This silent path performs type coercion checks but intentionally bypasses any
    /// metadata-based validation such as numeric min/max bounds.
    /// </remarks>
    /// <param name="value">
    /// The new value to assign. Must be assignable to <see cref="ValueType"/>.
    /// <see langword="null"/> is only valid when <see cref="ValueType"/> is a nullable type.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is not assignable to <see cref="ValueType"/>,
    /// or when <paramref name="value"/> is <see langword="null"/> and <see cref="ValueType"/>
    /// is a non-nullable value type.
    /// </exception>
    void SetValueWithoutNotify(object? value);
}
