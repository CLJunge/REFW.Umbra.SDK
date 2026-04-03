namespace Umbra.Config;

/// <summary>
/// Defines the internal registration-time mutation contract for <see cref="IParameter"/> instances.
/// </summary>
/// <remarks>
/// Public consumers can observe a parameter's resolved identity and metadata through
/// <see cref="IParameter.Key"/> and <see cref="IParameter.Metadata"/>, but only Umbra's
/// registration pipeline is allowed to assign those values.
/// </remarks>
internal interface IParameterRegistration
{
    /// <summary>
    /// Assigns the fully-qualified persisted key resolved for the parameter.
    /// </summary>
    string Key { set; }

    /// <summary>
    /// Assigns the resolved metadata produced during registration.
    /// </summary>
    ParameterMetadata Metadata { set; }
}
