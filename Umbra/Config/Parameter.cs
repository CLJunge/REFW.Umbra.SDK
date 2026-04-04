using System.Diagnostics;
using System.Text.RegularExpressions;
using Umbra.Config.Validation;

namespace Umbra.Config;

/// <summary>
/// Stores one typed configuration value together with its default value, resolved metadata, and change notifications.
/// </summary>
/// <remarks>
/// Umbra's registration pipeline assigns the resolved <see cref="Key"/> and <see cref="Metadata"/> after the parameter is discovered in a settings object. Public code can then read those values, observe changes, and mutate the current value through the typed or untyped APIs.
/// </remarks>
/// <typeparam name="T">The value type stored by the parameter.</typeparam>
[DebuggerDisplay("{Key}: {Value} (Default: {DefaultValue}, Modified: {IsModified})")]
public class Parameter<T> : IParameter, IParameterRegistration, IParameterValidationState
{
    /// <summary>
    /// Cached flag indicating whether <typeparamref name="T"/> is a non-nullable value type.
    /// Evaluated once per closed generic type to avoid repeated reflection on every call.
    /// </summary>
    private static readonly bool IsNonNullableValueType =
        typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null;

    private T? _value;
    private Action? _interfaceValueChanged;
    private bool _hasValidationError;
    private string? _validationError;

    /// <summary>
    /// Occurs when the parameter value changes through the typed notifying mutation paths.
    /// </summary>
    /// <remarks>
    /// This event is not raised by <see cref="SetWithoutNotify"/> or <see cref="IParameter.SetValueWithoutNotify(object?)"/>. It can also be raised by <see cref="Reset(bool)"/> when <c>raiseEvent</c> is <see langword="true"/> and resetting changes the current value.
    /// </remarks>
    public event Action<T?, T?>? ValueChanged;

    event Action? IParameter.ValueChanged
    {
        add => _interfaceValueChanged += value;
        remove => _interfaceValueChanged -= value;
    }

    /// <inheritdoc/>
    public string Key { get; internal set; } = "";

    /// <inheritdoc/>
    public ParameterMetadata Metadata { get; internal set; } = new();

    bool IParameterValidationState.HasValidationError => _hasValidationError;

    string? IParameterValidationState.ValidationError => _validationError;

    /// <summary>
    /// Gets the default value captured when this parameter instance was constructed.
    /// </summary>
    /// <value>The original value restored by <see cref="Reset(bool)"/>.</value>
    public T? DefaultValue { get; }

    /// <inheritdoc/>
    public Type ValueType => typeof(T);

    /// <summary>
    /// Gets a value indicating whether the current <see cref="Value"/> differs from <see cref="DefaultValue"/>.
    /// </summary>
    /// <value><see langword="true"/> if the current value differs from the default value; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    /// This value is computed on demand, so it always reflects changes made through the typed and untyped mutation APIs.
    /// </remarks>
    public bool IsModified => !EqualityComparer<T?>.Default.Equals(_value, DefaultValue);

    /// <summary>
    /// Gets or sets the current parameter value through the validating, notifying mutation path.
    /// </summary>
    /// <value>The current parameter value.</value>
    /// <remarks>
    /// Assignments that fail metadata validation are ignored silently so UI-driven code can attempt updates without exception handling. Use <see cref="TrySet"/> or <see cref="SetOrThrow"/> when the caller needs an explicit success or failure signal.
    /// </remarks>
    public T? Value { get => _value; set => SetValue(value); }

    /// <summary>
    /// Initializes a new instance of the <see cref="Parameter{T}"/> class.
    /// </summary>
    /// <remarks>
    /// The current value and <see cref="DefaultValue"/> are both initialized to <see langword="default"/>.
    /// </remarks>
    public Parameter()
    {
        (_value, DefaultValue) = (default, default);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Parameter{T}"/> class.
    /// </summary>
    /// <param name="defaultValue">The initial value and default value for the parameter.</param>
    public Parameter(T? defaultValue)
    {
        (_value, DefaultValue) = (defaultValue, defaultValue);
    }

    /// <summary>
    /// Resets the current value to <see cref="DefaultValue"/>.
    /// </summary>
    /// <param name="raiseEvent"><see langword="true"/> to raise change notifications when resetting changes the current value; otherwise, <see langword="false"/>.</param>
    /// <remarks>
    /// Reset bypasses metadata validation so <see cref="IsModified"/> always becomes <see langword="false"/> after the call.
    /// </remarks>
    public void Reset(bool raiseEvent = true)
    {
        var oldValue = _value;
        _value = DefaultValue;
        ClearValidationError();

        if (raiseEvent && !EqualityComparer<T?>.Default.Equals(oldValue, _value))
        {
            ValueChanged?.Invoke(oldValue, _value);
            _interfaceValueChanged?.Invoke();
        }
    }

    /// <summary>
    /// Sets the current value without raising change notifications.
    /// </summary>
    /// <param name="value">The value to assign silently.</param>
    /// <remarks>
    /// This method intentionally bypasses the metadata validation performed by <see cref="Value"/>, <see cref="Set(T)"/>, <see cref="TrySet"/>, and <see cref="SetOrThrow"/>.
    /// </remarks>
    public void SetWithoutNotify(T? value)
    {
        _value = value;
        ClearValidationError();
    }

    /// <summary>
    /// Sets the current value through the validating, notifying mutation path.
    /// </summary>
    /// <param name="value">The new value to assign.</param>
    /// <remarks>
    /// This is a convenience alias for assigning to <see cref="Value"/> directly. Invalid values are ignored silently.
    /// </remarks>
    public void Set(T? value) => Value = value;

    /// <summary>
    /// Attempts to set the current value while reporting whether metadata validation succeeded.
    /// </summary>
    /// <param name="value">The candidate value to assign.</param>
    /// <returns><see langword="true"/> if <paramref name="value"/> satisfies the current metadata constraints; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// A valid value that equals the current value still returns <see langword="true"/> even though no state changes and no event is raised.
    /// </remarks>
    public bool TrySet(T? value)
    {
        if (!Validate(value, out var failureReason))
        {
            SetValidationError(failureReason);
            return false;
        }

        ClearValidationError();
        SetValueCore(value);
        return true;
    }

    /// <summary>
    /// Sets the current value or throws when metadata validation fails.
    /// </summary>
    /// <param name="value">The candidate value to assign.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> falls outside the configured <see cref="ParameterMetadata.Min"/> and or <see cref="ParameterMetadata.Max"/> bounds.</exception>
    public void SetOrThrow(T? value)
    {
        if (!Validate(value, out var failureReason))
        {
            SetValidationError(failureReason);
            throw new ArgumentOutOfRangeException(nameof(value), value, failureReason);
        }

        ClearValidationError();
        SetValueCore(value);
    }

    /// <inheritdoc/>
    object? IParameter.GetValue() => Value;

    string IParameterRegistration.Key { set => Key = value; }

    ParameterMetadata IParameterRegistration.Metadata { set => Metadata = value; }

    void IParameterValidationState.ClearValidationError() => ClearValidationError();

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
    void IParameter.SetValueWithoutNotify(object? value)
    {
        _value = CoerceValue(value);
        ClearValidationError();
    }

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
    /// <param name="failureReason">
    /// Receives a human-readable explanation when validation fails; otherwise <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the value is within the allowed range or no constraints are
    /// defined; <see langword="false"/> if the value falls outside the configured bounds.
    /// </returns>
    private bool Validate(T? value, out string? failureReason)
    {
        failureReason = null;
        if (!ValidateRequired(value, out failureReason))
            return false;

        if (!ValidateStringLength(value, out failureReason))
            return false;

        if (!ValidateRegex(value, out failureReason))
            return false;

        if (!ValidateNumericRange(value, out failureReason))
            return false;

        if (!ValidateCustom(value, out failureReason))
            return false;

        return true;
    }

    private bool ValidateRequired(T? value, out string? failureReason)
    {
        failureReason = null;

        if (!Metadata.Required)
            return true;

        if (value is null)
        {
            failureReason = "Value is required.";
            return false;
        }

        if (value is not string text)
            return true;

        if (text.Length == 0)
        {
            failureReason = "Value is required.";
            return false;
        }

        if (!Metadata.AllowWhitespace && string.IsNullOrWhiteSpace(text))
        {
            failureReason = "Value cannot be whitespace only.";
            return false;
        }

        return true;
    }

    private bool ValidateStringLength(T? value, out string? failureReason)
    {
        failureReason = null;

        if (value is not string text)
            return true;

        if (Metadata.MinLength is uint minLength && text.Length < minLength)
        {
            failureReason = $"Value must be at least {minLength} characters long.";
            return false;
        }

        return true;
    }

    private bool ValidateRegex(T? value, out string? failureReason)
    {
        failureReason = null;

        if (Metadata.RegexPattern is null || value is not string text || text.Length == 0)
            return true;

        try
        {
            if (Regex.IsMatch(text, Metadata.RegexPattern, RegexOptions.CultureInvariant))
                return true;
        }
        catch (ArgumentException ex)
        {
            failureReason = $"Regex validation could not be evaluated: {ex.Message}";
            return false;
        }

        failureReason = Metadata.RegexMessage ?? $"Value must match pattern '{Metadata.RegexPattern}'.";
        return false;
    }

    private bool ValidateNumericRange(T? value, out string? failureReason)
    {
        failureReason = null;

        if (value == null || Metadata.Min == null && Metadata.Max == null)
            return true;

        if (value is IComparable c)
        {
            try
            {
                if (Metadata.Min != null && c.CompareTo(Convert.ChangeType(Metadata.Min.Value, typeof(T))) < 0)
                {
                    failureReason = $"Value must be greater than or equal to {Metadata.Min.Value}.";
                    return false;
                }

                if (Metadata.Max != null && c.CompareTo(Convert.ChangeType(Metadata.Max.Value, typeof(T))) > 0)
                {
                    failureReason = $"Value must be less than or equal to {Metadata.Max.Value}.";
                    return false;
                }
            }
            catch (InvalidCastException)
            {
                // T cannot be converted to the bounds value type (e.g. mismatched numeric types
                // from runtime metadata); skip bounds validation and treat the value as valid.
                return true;
            }
        }

        return true;
    }

    private bool ValidateCustom(T? value, out string? failureReason)
    {
        failureReason = null;

        var validatorType = Metadata.ValidatorType;
        if (validatorType is null)
            return true;

        if (!typeof(IParameterValidator).IsAssignableFrom(validatorType))
        {
            failureReason = $"Validator type '{validatorType.FullName ?? validatorType.Name}' must implement IParameterValidator.";
            return false;
        }

        IParameterValidator validator;
        try
        {
            validator = (IParameterValidator)(Activator.CreateInstance(validatorType)
                ?? throw new InvalidOperationException("Activator.CreateInstance returned null."));
        }
        catch (Exception ex)
        {
            failureReason = $"Validator '{validatorType.FullName ?? validatorType.Name}' could not be created: {ex.Message}";
            return false;
        }

        try
        {
            var result = validator.Validate(Key, value, typeof(T), Metadata);
            if (result.IsValid)
                return true;

            failureReason = string.IsNullOrWhiteSpace(result.ErrorMessage)
                ? $"Validator '{validatorType.FullName ?? validatorType.Name}' rejected the value."
                : result.ErrorMessage;
            return false;
        }
        catch (Exception ex)
        {
            failureReason = $"Validator '{validatorType.FullName ?? validatorType.Name}' threw: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// Applies <paramref name="newValue"/> to the parameter if it differs from the current
    /// value and passes validation, then raises <see cref="ValueChanged"/> and the untyped
    /// <see cref="IParameter.ValueChanged"/> event.
    /// </summary>
    /// <param name="newValue">The new value to assign.</param>
    private void SetValue(T? newValue)
    {
        if (!Validate(newValue, out var failureReason))
        {
            SetValidationError(failureReason);
            return;
        }

        ClearValidationError();
        SetValueCore(newValue);
    }

    /// <summary>
    /// Applies <paramref name="newValue"/> to the parameter when validation has already succeeded.
    /// </summary>
    /// <param name="newValue">The new value to assign.</param>
    private void SetValueCore(T? newValue)
    {
        if (EqualityComparer<T?>.Default.Equals(_value, newValue)) return;

        var oldValue = _value;
        _value = newValue;

        ValueChanged?.Invoke(oldValue, newValue);
        _interfaceValueChanged?.Invoke();
    }

    private void SetValidationError(string? failureReason)
    {
        _hasValidationError = !string.IsNullOrWhiteSpace(failureReason);
        _validationError = _hasValidationError ? failureReason : null;
    }

    private void ClearValidationError()
    {
        _hasValidationError = false;
        _validationError = null;
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
    /// Returns the string representation of the current <see cref="Value"/>.
    /// </summary>
    /// <returns><c>Value?.ToString()</c>, or <see langword="null"/> if <see cref="Value"/> is <see langword="null"/>.</returns>
    public override string? ToString() => Value?.ToString();
}
