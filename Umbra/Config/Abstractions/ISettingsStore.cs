namespace Umbra.Config;

/// <summary>
/// Defines the lifecycle, persistence, and listener operations exposed by a typed settings store.
/// </summary>
/// <typeparam name="TConfig">
/// The configuration class type. Must have a public parameterless constructor.
/// </typeparam>
public interface ISettingsStore<TConfig> : IDisposable
    where TConfig : class, new()
{
    /// <summary>
    /// Gets whether <see cref="Load"/> has completed successfully for this store instance.
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets whether this store has been disposed.
    /// </summary>
    bool IsDisposed { get; }

    /// <summary>
    /// Creates, registers, and loads the configuration instance for this store.
    /// </summary>
    /// <returns>The loaded configuration instance.</returns>
    TConfig Load();

    /// <summary>
    /// Persists the current parameter values to the configured file path.
    /// </summary>
    void Save();

    /// <summary>
    /// Copies all registered parameter values from this store into the corresponding parameters of
    /// <paramref name="target"/>, matched by key.
    /// </summary>
    /// <param name="target">The destination store to copy values into.</param>
    /// <param name="setWithoutNotifying">
    /// When <see langword="true"/>, values are applied without raising
    /// <see cref="IParameter.ValueChanged"/> events on the target store.
    /// </param>
    void CopyValuesTo(SettingsStore<TConfig> target, bool setWithoutNotifying = false);

    /// <summary>
    /// Subscribes a callback to every registered parameter.
    /// </summary>
    /// <param name="listener">The callback to invoke whenever any parameter value changes.</param>
    void AddListenerToAll(Action listener);

    /// <summary>
    /// Subscribes a typed callback to every registered parameter whose value type matches <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to filter on.</typeparam>
    /// <param name="listener">The callback to invoke whenever a matching parameter value changes.</param>
    void AddListenerToAll<T>(Action<T?, T?> listener);

    /// <summary>
    /// Subscribes a callback to every registered parameter that satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The filter used to select parameters.</param>
    /// <param name="listener">The callback to invoke whenever a matching parameter value changes.</param>
    void AddListenerToAll(Func<IParameter, bool> predicate, Action listener);

    /// <summary>
    /// Removes a previously added callback from every registered parameter.
    /// </summary>
    /// <param name="listener">The callback to remove.</param>
    void RemoveListenerFromAll(Action listener);

    /// <summary>
    /// Removes a previously added typed callback from every registered parameter whose value type matches <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to filter on.</typeparam>
    /// <param name="listener">The callback to remove.</param>
    void RemoveListenerFromAll<T>(Action<T?, T?> listener);

    /// <summary>
    /// Removes a previously added callback from every registered parameter that satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The filter used to select parameters.</param>
    /// <param name="listener">The callback to remove.</param>
    void RemoveListenerFromAll(Func<IParameter, bool> predicate, Action listener);

    /// <summary>
    /// Resets every registered parameter to its default value.
    /// </summary>
    void ResetAll();
}
