namespace Umbra.Config;

/// <summary>
/// Defines the untyped contract for a registered config parameter.
/// </summary>
/// <remarks>
/// Umbra uses this abstraction to store heterogeneous <see cref="Parameter{T}"/> instances in one registered parameter set, persist their values, and attach cross-parameter listeners through <see cref="ConfigStore{TConfig}"/>.
/// </remarks>
public interface IParameter
{
    /// <summary>
    /// Occurs when the parameter's value changes through the notifying mutation paths.
    /// </summary>
    /// <remarks>
    /// This event is raised by <see cref="SetValue(object?)"/> and by <see cref="Reset(bool)"/> when <c>raiseEvent</c> is <see langword="true"/> and the reset changes the current value. It is not raised by <see cref="SetValueWithoutNotify(object?)"/>.
    /// </remarks>
    event Action? ValueChanged;

    /// <summary>
    /// Gets the fully qualified persisted key assigned to this parameter during registration.
    /// </summary>
    /// <value>The stable key used for persistence and parameter-map lookups.</value>
    string Key { get; }

    /// <summary>
    /// Gets the metadata resolved for this parameter during registration.
    /// </summary>
    /// <value>The read-only descriptive and UI metadata associated with the parameter.</value>
    ParameterMetadata Metadata { get; }

    /// <summary>
    /// Gets the CLR type of the value held by this parameter.
    /// </summary>
    /// <value>The parameter value type.</value>
    Type ValueType { get; }

    /// <summary>
    /// Gets a value indicating whether the current value differs from the default value captured at construction time.
    /// </summary>
    /// <value><see langword="true"/> if the current value differs from the default value; otherwise, <see langword="false"/>.</value>
    bool IsModified { get; }

    /// <summary>
    /// Returns the current parameter value as an untyped object.
    /// </summary>
    /// <returns>The current value, or <see langword="null"/> if the parameter currently holds no value.</returns>
    object? GetValue();

    /// <summary>
    /// Sets the parameter value through the validating, notifying mutation path.
    /// </summary>
    /// <param name="value">The value to assign.</param>
    /// <remarks>
    /// Implementations may reject values that violate metadata-defined constraints such as numeric bounds.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not assignable to <see cref="ValueType"/>, or it is <see langword="null"/> for a non-nullable value type.</exception>
    void SetValue(object? value);

    /// <summary>
    /// Resets the parameter to its default value.
    /// </summary>
    /// <param name="raiseEvent"><see langword="true"/> to raise <see cref="ValueChanged"/> when the reset changes the current value; otherwise, <see langword="false"/>.</param>
    /// <remarks>
    /// Reset bypasses metadata validation so the parameter always returns to the original default state.
    /// </remarks>
    void Reset(bool raiseEvent = true);

    /// <summary>
    /// Sets the parameter value without raising <see cref="ValueChanged"/>.
    /// </summary>
    /// <param name="value">The value to assign.</param>
    /// <remarks>
    /// This silent path performs type checks but intentionally bypasses metadata-based validation such as numeric bounds.
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="value"/> is not assignable to <see cref="ValueType"/>, or it is <see langword="null"/> for a non-nullable value type.</exception>
    void SetValueWithoutNotify(object? value);
}
