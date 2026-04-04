using Umbra.Config;
using Umbra.Config.Validation;

namespace Umbra.SamplePlugin.Config;

/// <summary>
/// Validates the sample-plugin search filter string.
/// </summary>
/// <remarks>
/// This validator exists only to demonstrate Umbra's custom validation hook in the sample plugin.
/// It rejects a small set of reserved debug prefixes so the configuration UI can show inline
/// custom-validation feedback.
/// </remarks>
public sealed class SearchFilterValidator : IParameterValidator
{
    /// <inheritdoc/>
    public ParameterValidationResult Validate(string parameterKey, object? value, Type valueType, ParameterMetadata metadata)
    {
        if (value is not string text || text.Length == 0)
            return ParameterValidationResult.Valid();

        if (text.StartsWith("debug:", StringComparison.OrdinalIgnoreCase)
            || text.StartsWith("internal:", StringComparison.OrdinalIgnoreCase))
        {
            return ParameterValidationResult.Invalid(
                "Search filters cannot start with 'debug:' or 'internal:' in the sample validator.");
        }

        return ParameterValidationResult.Valid();
    }
}
