namespace Umbra.Config.Validation;

/// <summary>
/// Resolves and caches one custom parameter validator instance for one owning parameter.
/// </summary>
/// <remarks>
/// The cache reuses the current validator while the requested validator type is unchanged. When the
/// requested type changes, a new validator instance is created and cached for subsequent calls.
/// </remarks>
internal sealed class ParameterValidatorCache
{
    private Type? _cachedValidatorType;
    private IParameterValidator? _cachedValidator;

    /// <summary>
    /// Resolves the validator for <paramref name="validatorType"/>, reusing the cached instance when possible.
    /// </summary>
    /// <param name="validatorType">The validator type declared in parameter metadata.</param>
    /// <param name="validator">The resolved validator instance when successful.</param>
    /// <param name="failureReason">The deterministic failure reason when resolution fails; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> when the validator was resolved successfully; otherwise, <see langword="false"/>.</returns>
    internal bool TryGet(Type validatorType, out IParameterValidator validator, out string? failureReason)
    {
        ArgumentNullException.ThrowIfNull(validatorType);

        failureReason = null;

        if (!typeof(IParameterValidator).IsAssignableFrom(validatorType))
        {
            failureReason = $"Validator type '{validatorType.FullName ?? validatorType.Name}' must implement IParameterValidator.";
            validator = null!;
            return false;
        }

        if (_cachedValidator is not null && _cachedValidatorType == validatorType)
        {
            validator = _cachedValidator;
            return true;
        }

        try
        {
            validator = (IParameterValidator)(Activator.CreateInstance(validatorType)
                ?? throw new InvalidOperationException("Activator.CreateInstance returned null."));
            _cachedValidator = validator;
            _cachedValidatorType = validatorType;
            return true;
        }
        catch (Exception ex)
        {
            _cachedValidator = null;
            _cachedValidatorType = null;
            failureReason = $"Validator '{validatorType.FullName ?? validatorType.Name}' could not be created: {ex.Message}";
            validator = null!;
            return false;
        }
    }
}
