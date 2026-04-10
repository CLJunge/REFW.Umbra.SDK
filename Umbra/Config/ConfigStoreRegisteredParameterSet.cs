using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Owns registration and parameter-map operations for one <see cref="ConfigStore{TConfig}"/>
/// instance.
/// </summary>
/// <remarks>
/// This type keeps whole-store parameter operations together: creating registered defaults,
/// resetting registered values, and copying matching values into another compatible store.
/// </remarks>
internal sealed class ConfigStoreRegisteredParameterSet<TConfig>
    where TConfig : class, new()
{
    private readonly Dictionary<string, IParameter> _parameters;

    /// <summary>
    /// Initializes a new registered-parameter set over the supplied shared parameter map.
    /// </summary>
    /// <param name="parameters">The shared parameter map owned by the config store.</param>
    internal ConfigStoreRegisteredParameterSet(Dictionary<string, IParameter> parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        _parameters = parameters;
    }

    /// <summary>
    /// Gets the registered parameters keyed by fully qualified persisted key.
    /// </summary>
    internal IReadOnlyDictionary<string, IParameter> Parameters => _parameters;

    /// <summary>
    /// Creates a fresh config instance and repopulates the shared parameter map from its declared
    /// defaults.
    /// </summary>
    /// <returns>The newly created registered config instance.</returns>
    internal TConfig CreateRegisteredDefaults()
    {
        var instance = new TConfig();
        _parameters.Clear();

        var discovered = ConfigRegistrar.Register(instance);
        foreach (var (key, param) in discovered)
            _parameters[key] = param;

        return instance;
    }

    /// <summary>
    /// Copies this set's registered parameter values into the corresponding parameters of
    /// <paramref name="target"/>, matched by key.
    /// </summary>
    /// <param name="target">The destination config store.</param>
    /// <param name="setWithoutNotifying">
    /// <see langword="true"/> to use the silent mutation path on the target parameters; otherwise,
    /// <see langword="false"/>.
    /// </param>
    internal void CopyValuesTo(IConfigStore<TConfig> target, bool setWithoutNotifying)
    {
        ArgumentNullException.ThrowIfNull(target);
        ObjectDisposedException.ThrowIf(target.IsDisposed, target);
        if (!target.IsLoaded)
        {
            throw new InvalidOperationException(
                $"ConfigStore<{typeof(TConfig).Name}>.CopyValuesTo() requires a target store that has already completed Load().");
        }

        if (target is not IConfigStoreCopyTarget<TConfig> copyTarget)
        {
            throw new InvalidOperationException(
                $"ConfigStore<{typeof(TConfig).Name}>.CopyValuesTo() requires a target store implementation that supports Umbra parameter-map copy operations.");
        }

        foreach (var (key, parameter) in _parameters)
        {
            if (!copyTarget.Parameters.TryGetValue(key, out var destination))
                continue;

            if (setWithoutNotifying)
                destination.SetValueWithoutNotify(parameter.GetValue());
            else
                destination.SetValue(parameter.GetValue());
        }
    }

    /// <summary>
    /// Resets every registered non-delegate parameter to its default value.
    /// </summary>
    internal void ResetAll()
    {
        var count = 0;
        foreach (var parameter in _parameters.Values)
        {
            if (typeof(Delegate).IsAssignableFrom(parameter.ValueType))
                continue;

            parameter.Reset();
            count++;
        }

        Logger.Info($"ConfigStore<{typeof(TConfig).Name}>: reset {count} parameter(s) to defaults.");
    }
}
