namespace Umbra.Config;

/// <summary>
/// Defines the internal parameter-map contract required by <see cref="ISettingsStore{TConfig}.CopyValuesTo"/>.
/// </summary>
/// <remarks>
/// The public settings-store abstraction intentionally does not expose its registered parameter map.
/// Copy operations bridge that gap through this internal contract so Umbra can keep the public API
/// focused on lifecycle and listener operations while still supporting interface-based copy calls.
/// </remarks>
/// <typeparam name="TConfig">The configuration class type.</typeparam>
internal interface ISettingsStoreCopyTarget<TConfig>
    where TConfig : class, new()
{
    /// <summary>
    /// Gets the registered parameters keyed by their fully-qualified setting name.
    /// </summary>
    IReadOnlyDictionary<string, IParameter> Parameters { get; }
}
