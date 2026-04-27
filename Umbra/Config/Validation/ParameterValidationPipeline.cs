using System.Text.RegularExpressions;

namespace Umbra.Config.Validation;

/// <summary>
/// Executes Umbra's metadata-driven parameter validation rules in their configured order.
/// </summary>
/// <remarks>
/// This pipeline validates both Umbra's built-in rules and the optional custom validator declared in
/// parameter metadata.
/// </remarks>
internal static class ParameterValidationPipeline
{
    /// <summary>
    /// Validates the candidate value described by <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The immutable validation input.</param>
    /// <param name="validatorCache">The per-parameter cache used for custom validator resolution.</param>
    /// <returns>The validation result for the Umbra rules and optional custom validator.</returns>
    internal static ParameterValidationResult Validate(
        ParameterValidationContext context,
        ParameterValidatorCache validatorCache)
    {
        var result = ValidateRequired(context);
        if (!result.IsValid)
            return result;

        result = ValidateStringLength(context);
        if (!result.IsValid)
            return result;

        result = ValidateRegex(context);
        if (!result.IsValid)
            return result;

        result = ValidateNumericRange(context);
        return !result.IsValid ? result : ValidateCustom(context, validatorCache);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Keep the straightforward if statements for readability and easier future expansion of validation rules.")]
    private static ParameterValidationResult ValidateRequired(ParameterValidationContext context)
    {
        var metadata = context.Metadata;
        if (!metadata.Required)
            return ParameterValidationResult.Valid();

        var value = context.CandidateValue;
        if (value is null)
            return ParameterValidationResult.Invalid("Value is required.");

        if (value is not string text)
            return ParameterValidationResult.Valid();

        if (text.Length == 0)
            return ParameterValidationResult.Invalid("Value is required.");

        if (!metadata.AllowWhitespace && string.IsNullOrWhiteSpace(text))
            return ParameterValidationResult.Invalid("Value cannot be whitespace only.");

        return ParameterValidationResult.Valid();
    }

    private static ParameterValidationResult ValidateStringLength(ParameterValidationContext context)
    {
        var value = context.CandidateValue;
        if (value is not string text)
            return ParameterValidationResult.Valid();

        var minLength = context.Metadata.MinLength;
        return minLength is uint requiredLength && text.Length < requiredLength
            ? ParameterValidationResult.Invalid($"Value must be at least {requiredLength} characters long.")
            : ParameterValidationResult.Valid();
    }

    private static ParameterValidationResult ValidateRegex(ParameterValidationContext context)
    {
        var pattern = context.Metadata.RegexPattern;
        if (pattern is null || context.CandidateValue is not string text || text.Length == 0)
            return ParameterValidationResult.Valid();

        try
        {
            if (Regex.IsMatch(text, pattern, RegexOptions.CultureInvariant))
                return ParameterValidationResult.Valid();
        }
        catch (ArgumentException ex)
        {
            return ParameterValidationResult.Invalid($"Regex validation could not be evaluated: {ex.Message}");
        }

        var message = context.Metadata.RegexMessage ?? $"Value must match pattern '{pattern}'.";
        return ParameterValidationResult.Invalid(message);
    }

    private static ParameterValidationResult ValidateNumericRange(ParameterValidationContext context)
    {
        var value = context.CandidateValue;
        var metadata = context.Metadata;
        if (value is null || metadata.Min is null && metadata.Max is null)
            return ParameterValidationResult.Valid();

        if (value is IComparable comparable)
        {
            try
            {
                if (metadata.Min is double min
                    && comparable.CompareTo(Convert.ChangeType(min, context.ValueType)) < 0)
                {
                    return ParameterValidationResult.Invalid($"Value must be greater than or equal to {min}.");
                }

                if (metadata.Max is double max
                    && comparable.CompareTo(Convert.ChangeType(max, context.ValueType)) > 0)
                {
                    return ParameterValidationResult.Invalid($"Value must be less than or equal to {max}.");
                }
            }
            catch (InvalidCastException)
            {
                return ParameterValidationResult.Valid();
            }
        }

        return ParameterValidationResult.Valid();
    }

    private static ParameterValidationResult ValidateCustom(
        ParameterValidationContext context,
        ParameterValidatorCache validatorCache)
    {
        var validatorType = context.Metadata.ValidatorType;
        if (validatorType is null)
            return ParameterValidationResult.Valid();

        if (!validatorCache.TryGet(validatorType, out var validator, out var failureReason))
            return ParameterValidationResult.Invalid(failureReason!);

        try
        {
            var result = validator.Validate(
                context.ParameterKey,
                context.CandidateValue,
                context.ValueType,
                context.Metadata);
            return result.IsValid
                ? ParameterValidationResult.Valid()
                : string.IsNullOrWhiteSpace(result.ErrorMessage)
                ? ParameterValidationResult.Invalid($"Validator '{validatorType.FullName ?? validatorType.Name}' rejected the value.")
                : result;
        }
        catch (Exception ex)
        {
            return ParameterValidationResult.Invalid(
                $"Validator '{validatorType.FullName ?? validatorType.Name}' threw: {ex.Message}");
        }
    }
}
