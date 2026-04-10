namespace Umbra.Config.Validation;

/// <summary>
/// Stores the immutable input required to validate one candidate parameter value.
/// </summary>
/// <param name="ParameterKey">The fully qualified key assigned to the parameter.</param>
/// <param name="ValueType">The CLR type stored by the parameter.</param>
/// <param name="Metadata">The resolved parameter metadata.</param>
/// <param name="CandidateValue">The candidate value being validated.</param>
internal readonly record struct ParameterValidationContext(
    string ParameterKey,
    Type ValueType,
    ParameterMetadata Metadata,
    object? CandidateValue);
