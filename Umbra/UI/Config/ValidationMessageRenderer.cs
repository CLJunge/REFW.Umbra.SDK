using System.Numerics;
using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Renders inline validation feedback for configuration parameters that expose validation state.
/// </summary>
/// <remarks>
/// This renderer consumes only the last recorded validation failure from <see cref="IParameterValidationState"/>. It does not execute validation rules itself.
/// </remarks>
internal static class ValidationMessageRenderer
{
    private static readonly Vector4 _errorColor = new(1f, 0.35f, 0.35f, 1f);

    /// <summary>
    /// Renders the current validation error for <paramref name="parameter"/> when one is present.
    /// </summary>
    /// <param name="parameter">The parameter whose validation state should be rendered.</param>
    /// <param name="textOps">The text renderer used to display the message.</param>
    internal static void Draw(IParameter parameter, ITextOps textOps)
    {
        ArgumentNullException.ThrowIfNull(parameter);
        ArgumentNullException.ThrowIfNull(textOps);

        if (parameter is not IParameterValidationState validationState
            || !validationState.HasValidationError)
        {
            return;
        }

        string? validationMessage = validationState.ValidationError;
        if (string.IsNullOrWhiteSpace(validationMessage))
        {
            return;
        }

        textOps.TextColored(_errorColor, validationMessage);
    }
}
