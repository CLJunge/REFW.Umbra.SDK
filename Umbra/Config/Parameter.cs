using System.Diagnostics;

namespace Umbra.Config;

/// <summary>
/// A strongly-typed configuration parameter that holds a value of type <typeparamref name="T"/>
/// and notifies listeners when that value changes.
/// </summary>
/// <typeparam name="T">The type of value stored by this parameter.</typeparam>
[DebuggerDisplay("{Key}: {Value} (Default: {DefaultValue})")]
public class Parameter<T> : IParameter
{
    /// <summary>
    /// Cached flag indicating whether <typeparamref name="T"/> is a non-nullable value type.
    /// Evaluated once per closed generic type to avoid repeated reflection on every call.
    /// </summary>
    private static readonly bool IsNonNullableValueType =
        typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null;

    private T? _value;
    private Action? _interfaceValueChanged;

    /// <summary>
    /// Raised when the parameter's value changes, providing both the previous and new values.
    /// </summary>
    /// <remarks>
    /// This event is not raised when the value is updated silently via
    /// <see cref="SetWithoutNotify"/> or <see cref="IParameter.SetValueWithoutNotify"/>.
    /// </remarks>
    public event Action<T?, T?>? ValueChanged;

    /// <inheritdoc/>
    event Action? IParameter.ValueChanged
    {
        add => _interfaceValueChanged += value;
        remove => _interfaceValueChanged -= value;
    }

    /// <inheritdoc/>
    public string Key { get; set; } = "";

    /// <inheritdoc/>
    public ParameterMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Gets the default value assigned to this parameter at construction time.
    /// Used by <see cref="Reset"/> to restore the parameter to its original state.
    /// </summary>
    public T? DefaultValue { get; }

    /// <inheritdoc/>
    public Type ValueType => typeof(T);

    /// <summary>
    /// Gets or sets the current value of this parameter.
    /// Setting this property raises <see cref="ValueChanged"/> if the value changes
    /// and passes validation defined by <see cref="ParameterMetadata.Min"/> and
    /// <see cref="ParameterMetadata.Max"/>.
    /// </summary>
    public T? Value { get => _value; set => SetValue(value); }

    /// <summary>
    /// Initializes a new instance of <see cref="Parameter{T}"/> with <see langword="default"/>
    /// as both the current value and the <see cref="DefaultValue"/>.
    /// </summary>
    public Parameter()
    {
        (_value, DefaultValue) = (default, default);
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Parameter{T}"/> with the specified default value.
    /// </summary>
    /// <param name="defaultValue">
    /// The initial and default value for this parameter.
    /// </param>
    public Parameter(T? defaultValue)
    {
        (_value, DefaultValue) = (defaultValue, defaultValue);
    }

    /// <summary>
    /// Resets the value to its default state, optionally raising the value changed event.
    /// </summary>
    /// <remarks>If raiseEvent is set to false, the value is reset without notifying listeners of the change.
    /// Use this option when you need to update the value silently without triggering event handlers.</remarks>
    /// <param name="raiseEvent">true to raise the value changed event after resetting; otherwise, false.</param>
    public void Reset(bool raiseEvent = true)
    {
        if (raiseEvent)
            Value = DefaultValue;
        else
            SetWithoutNotify(DefaultValue);
    }

    /// <summary>
    /// Sets the parameter's value without raising <see cref="ValueChanged"/>.
    /// Useful for initializing or restoring persisted values without triggering side effects.
    /// </summary>
    /// <param name="value">The value to assign silently.</param>
    public void SetWithoutNotify(T? value) => _value = value;

    /// <summary>
    /// Sets the parameter's value, raising <see cref="ValueChanged"/> if the value changes
    /// and passes validation. Convenience alias for assigning to <see cref="Value"/> directly;
    /// prefer this form when the calling site already holds the typed parameter reference
    /// (e.g. inside <c>INestedGroupDrawer&lt;T&gt;.Draw</c>) to avoid the double-<c>.Value</c>
    /// repetition of <c>parameter.Value.Value = x</c>.
    /// </summary>
    /// <param name="value">The new value to assign.</param>
    public void Set(T? value) => Value = value;

    /// <inheritdoc/>
    object? IParameter.GetValue() => Value;

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/> and <typeparamref name="T"/>
    /// is a non-nullable value type, or when <paramref name="value"/> is non-<see langword="null"/>
    /// and is not assignable to <typeparamref name="T"/>.
    /// </exception>
    void IParameter.SetValue(object? value) => SetValue(CoerceValue(value));

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/> and <typeparamref name="T"/>
    /// is a non-nullable value type, or when <paramref name="value"/> is non-<see langword="null"/>
    /// and is not assignable to <typeparamref name="T"/>.
    /// </exception>
    void IParameter.SetValueWithoutNotify(object? value) => _value = CoerceValue(value);

    /// <summary>
    /// Validates and coerces an untyped <paramref name="value"/> to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="value">The value to coerce.</param>
    /// <returns>The coerced value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="value"/> is <see langword="null"/> and <typeparamref name="T"/>
    /// is a non-nullable value type, or when <paramref name="value"/> is not assignable to
    /// <typeparamref name="T"/>.
    /// </exception>
    private static T? CoerceValue(object? value)
    {
        if (value is null)
        {
            if (IsNonNullableValueType)
                throw new ArgumentException(
                    $"null is not valid for non-nullable value type {typeof(T)}.", nameof(value));

            return default;
        }

        if (value is not T typed)
            throw new ArgumentException($"Value must be of type {typeof(T)}.", nameof(value));

        return typed;
    }

    /// <summary>
    /// Validates <paramref name="value"/> against the <see cref="ParameterMetadata.Min"/> and
    /// <see cref="ParameterMetadata.Max"/> constraints defined in <see cref="Metadata"/>.
    /// </summary>
    /// <param name="value">The candidate value to validate.</param>
    /// <returns>
    /// <see langword="true"/> if the value is within the allowed range or no constraints are
    /// defined; <see langword="false"/> if the value falls outside the configured bounds.
    /// </returns>
    private bool Validate(T? value)
    {
        if (value == null || Metadata.Min == null && Metadata.Max == null)
            return true;

        if (value is IComparable c)
        {
            try
            {
                if (Metadata.Min != null && c.CompareTo(Convert.ChangeType(Metadata.Min.Value, typeof(T))) < 0)
                    return false;
                if (Metadata.Max != null && c.CompareTo(Convert.ChangeType(Metadata.Max.Value, typeof(T))) > 0)
                    return false;
            }
            catch (InvalidCastException)
            {
                // T does not implement IConvertible or the double bounds cannot be
                // narrowed to T; skip validation rather than blocking the assignment.
                return true;
            }
        }

        return true;
    }

    /// <summary>
    /// Applies <paramref name="newValue"/> to the parameter if it differs from the current
    /// value and passes validation, then raises <see cref="ValueChanged"/> and the untyped
    /// <see cref="IParameter.ValueChanged"/> event.
    /// </summary>
    /// <param name="newValue">The new value to assign.</param>
    private void SetValue(T? newValue)
    {
        if (EqualityComparer<T?>.Default.Equals(_value, newValue)) return;
        if (!Validate(newValue)) return;

        var oldValue = _value;
        _value = newValue;

        ValueChanged?.Invoke(oldValue, newValue);
        _interfaceValueChanged?.Invoke();
    }

    /// <summary>
    /// Implicitly converts a <see cref="Parameter{T}"/> to its underlying value of type
    /// <typeparamref name="T"/>, allowing the parameter to be used directly wherever a
    /// <typeparamref name="T"/> is expected.
    /// </summary>
    /// <param name="parameter">The parameter whose <see cref="Value"/> is returned.</param>
    /// <returns>The current <see cref="Value"/> of <paramref name="parameter"/>.</returns>
    public static implicit operator T?(Parameter<T> parameter)
    {
        return parameter.Value;
    }

    /// <summary>
    /// Returns the string representation of the current <see cref="Value"/>, or
    /// <see langword="null"/> when <see cref="Value"/> is <see langword="null"/>.
    /// This ensures that string interpolation and <c>ToString()</c> calls produce
    /// the same result as the implicit <typeparamref name="T"/> conversion operator.
    /// </summary>
    /// <returns>
    /// <c>Value?.ToString()</c>, or <see langword="null"/> if <see cref="Value"/> is
    /// <see langword="null"/>.
    /// </returns>
    public override string? ToString() => Value?.ToString();
}
