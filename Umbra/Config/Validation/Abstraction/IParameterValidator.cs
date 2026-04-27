namespace Umbra.Config.Validation;

/// <summary>
/// Validates a candidate value for an Umbra configuration parameter.
/// </summary>
/// <remarks>
/// Implementations are intended for deterministic, side-effect-free validation rules declared through <see cref="Attributes.UmbraValidateWithAttribute{TValidator}"/>.
/// </remarks>
public interface IParameterValidator
{
    /// <summary>
    /// Validates <paramref name="value"/> for the supplied parameter context.
    /// </summary>
    /// <param name="parameterKey">The fully qualified parameter key assigned during registration.</param>
    /// <param name="value">The candidate value being validated.</param>
    /// <param name="valueType">The CLR type stored by the parameter.</param>
    /// <param name="metadata">The resolved metadata for the parameter.</param>
    /// <returns>A deterministic validation result describing whether the value is accepted.</returns>
    ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata);
}
