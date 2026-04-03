namespace Umbra.Config;

/// <summary>
/// Defines the internal mutation contract Umbra uses while registering <see cref="IParameter"/> instances.
/// </summary>
/// <remarks>
/// Public code can observe a parameter's resolved identity and metadata through <see cref="IParameter.Key"/> and <see cref="IParameter.Metadata"/>, but only Umbra's registration pipeline assigns those values.
/// </remarks>
internal interface IParameterRegistration
{
    /// <summary>
    /// Sets the fully qualified persisted key resolved for the parameter.
    /// </summary>
    string Key { set; }

    /// <summary>
    /// Sets the metadata resolved for the parameter during registration.
    /// </summary>
    ParameterMetadata Metadata { set; }
}
