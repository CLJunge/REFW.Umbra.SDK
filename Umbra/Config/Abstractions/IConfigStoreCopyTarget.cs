namespace Umbra.Config;

/// <summary>
/// Exposes the internal parameter map required by <see cref="IConfigStore{TConfig}.CopyValuesTo"/>.
/// </summary>
/// <remarks>
/// The public settings-store abstraction intentionally does not expose its registered parameter map. Copy operations bridge that gap through this internal contract so Umbra can keep the public API focused on lifecycle and listener operations.
/// </remarks>
/// <typeparam name="TConfig">The configuration type managed by the store.</typeparam>
internal interface IConfigStoreCopyTarget<TConfig>
    where TConfig : class, new()
{
    /// <summary>
    /// Gets the registered parameters keyed by fully qualified persisted key.
    /// </summary>
    /// <value>The registered parameter map used for copy operations.</value>
    IReadOnlyDictionary<string, IParameter> Parameters { get; }
}
