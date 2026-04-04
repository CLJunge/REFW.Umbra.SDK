namespace Umbra.Config.Validation;

/// <summary>
/// Represents the outcome of validating one candidate parameter value.
/// </summary>
/// <param name="IsValid"><see langword="true"/> when validation succeeded; otherwise, <see langword="false"/>.</param>
/// <param name="ErrorMessage">The failure reason when validation did not succeed; otherwise, <see langword="null"/>.</param>
public readonly record struct ParameterValidationResult(bool IsValid, string? ErrorMessage)
{
    /// <summary>
    /// Gets a successful validation result.
    /// </summary>
    public static ParameterValidationResult Success => new(true, null);

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <returns>A successful validation result.</returns>
    public static ParameterValidationResult Valid() => Success;

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="errorMessage">The deterministic failure reason.</param>
    /// <returns>A failed validation result with <paramref name="errorMessage"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is <see langword="null"/>, empty, or whitespace.</exception>
    public static ParameterValidationResult Invalid(string errorMessage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);
        return new ParameterValidationResult(false, errorMessage);
    }
}
