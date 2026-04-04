namespace Umbra.Config;

/// <summary>
/// Exposes the last validation failure recorded for a parameter.
/// </summary>
/// <remarks>
/// Umbra's configuration UI uses this internal contract to render inline validation feedback after a non-throwing set attempt is rejected.
/// </remarks>
internal interface IParameterValidationState
{
    /// <summary>
    /// Gets a value indicating whether the parameter currently has a recorded validation error.
    /// </summary>
    bool HasValidationError { get; }

    /// <summary>
    /// Gets the recorded validation error message.
    /// </summary>
    /// <value>The last validation failure reason, or <see langword="null"/> when no validation error is recorded.</value>
    string? ValidationError { get; }

    /// <summary>
    /// Clears the recorded validation error state.
    /// </summary>
    void ClearValidationError();
}
