using System.Diagnostics;

namespace Umbra.SDK.Config;

/// <summary>
/// A strongly-typed configuration parameter that holds a value of type <typeparamref name="T"/>
/// and notifies listeners when that value changes.
/// </summary>
/// <typeparam name="T">The type of value stored by this parameter.</typeparam>
[DebuggerDisplay("{Key}: {Value} (Default: {DefaultValue})")]
public class Parameter<T> : IParameter
{
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
    public void SetWithoutNotify(T? value)
    {
        _value = value;
    }

    /// <inheritdoc/>
    object? IParameter.GetValue()
    {
        return Value;
    }

    /// <inheritdoc/>
    void IParameter.SetValue(object? value)
    {
        if (value is not T && value != null)
            throw new ArgumentException($"Value must be of type {typeof(T)} or null.", nameof(value));

        SetValue((T?)value);
    }

    /// <inheritdoc/>
    void IParameter.SetValueWithoutNotify(object? value)
    {
        _value = (T?)value;
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
}
