namespace Umbra.Config;

/// <summary>
/// Represents a configuration parameter that holds a typed value and notifies
/// listeners when that value changes.
/// </summary>
public interface IParameter
{
    /// <summary>
    /// Raised when the parameter's value changes via <see cref="SetValue"/>.
    /// Not raised when the value is updated silently via <see cref="SetValueWithoutNotify"/>.
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
    /// Resets the object to its initial state, optionally raising a change event.
    /// </summary>
    /// <param name="raiseEvent">true to raise a change event after resetting; otherwise, false.</param>
    void Reset(bool raiseEvent = true);

    /// <summary>
    /// Sets the value of this parameter without raising <see cref="ValueChanged"/>.
    /// Useful for initializing or restoring persisted values without triggering side effects.
    /// </summary>
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
