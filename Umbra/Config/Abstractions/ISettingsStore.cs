namespace Umbra.Config;

/// <summary>
/// Defines the lifecycle, persistence, reset, copy, and listener operations exposed by a typed settings store.
/// </summary>
/// <remarks>
/// Implementations register a stable parameter set during <see cref="Load"/> and then expose
/// persistence and listener operations over that registered set for the remainder of the store's
/// lifetime. Unless stated otherwise, store operations that inspect or mutate registered parameters
/// require <see cref="Load"/> to have completed successfully first.
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type. Must have a public parameterless constructor.
/// </typeparam>
public interface ISettingsStore<TConfig> : IDisposable
    where TConfig : class, new()
{
    /// <summary>
    /// Gets whether <see cref="Load"/> has completed successfully for this store instance.
    /// </summary>
    /// <remarks>
    /// A store transitions to the loaded state only after parameter discovery succeeds. If loading
    /// fails before registration completes, this property remains <see langword="false"/>.
    /// </remarks>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets whether this store has been disposed.
    /// </summary>
    /// <remarks>
    /// After disposal, operations that inspect or mutate the registered parameter set throw
    /// <see cref="ObjectDisposedException"/>.
    /// </remarks>
    bool IsDisposed { get; }

    /// <summary>
    /// Creates, registers, and loads the configuration instance for this store.
    /// </summary>
    /// <returns>The loaded configuration instance.</returns>
    /// <remarks>
    /// This method is single-use per store instance. Implementations may persist declared defaults
    /// on first run, load persisted values when the backing file exists, and recover from unreadable
    /// files by reverting the current session to declared defaults.
    /// </remarks>
    TConfig Load();

    /// <summary>
    /// Persists the current parameter values to the configured file path.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="Load"/> to have completed successfully so the store has a stable
    /// registered parameter set to persist.
    /// </remarks>
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
    /// <remarks>
    /// Both stores must already be loaded so their parameter maps are stable. Parameters present in
    /// this store but absent from <paramref name="target"/> are ignored.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this store or <paramref name="target"/> has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this store or <paramref name="target"/> has not completed <see cref="Load"/> yet,
    /// or when <paramref name="target"/> does not support Umbra's copy-target contract.
    /// </exception>
    void CopyValuesTo(ISettingsStore<TConfig> target, bool setWithoutNotifying = false);

    /// <summary>
    /// Subscribes a callback to every registered parameter.
    /// </summary>
    /// <param name="listener">The callback to invoke whenever any parameter value changes.</param>
    /// <remarks>
    /// The callback is attached to the currently registered parameter set and is automatically
    /// removed when the store is disposed.
    /// </remarks>
    void AddListenerToAll(Action listener);

    /// <summary>
    /// Subscribes a typed callback to every registered parameter whose value type matches <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to filter on.</typeparam>
    /// <param name="listener">The callback to invoke whenever a matching parameter value changes.</param>
    /// <remarks>
    /// The callback is attached to the currently registered parameter set and is automatically
    /// removed when the store is disposed. When calling this overload with nullable value-type
    /// delegates, prefer specifying <typeparamref name="T"/> explicitly to avoid generic
    /// type-inference mismatches.
    /// </remarks>
    void AddListenerToAll<T>(Action<T?, T?> listener);

    /// <summary>
    /// Subscribes a callback to every registered parameter that satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The filter used to select parameters.</param>
    /// <param name="listener">The callback to invoke whenever a matching parameter value changes.</param>
    /// <remarks>
    /// The predicate is evaluated against the currently registered parameter set at subscription
    /// time, and the matched set is what later removal and disposal cleanup operate on.
    /// </remarks>
    void AddListenerToAll(Func<IParameter, bool> predicate, Action listener);

    /// <summary>
    /// Removes a previously added callback from every registered parameter.
    /// </summary>
    /// <param name="listener">The callback to remove.</param>
    /// <remarks>
    /// Requires <see cref="Load"/> to have completed so the store has a stable registered
    /// parameter set to unsubscribe from.
    /// </remarks>
    void RemoveListenerFromAll(Action listener);

    /// <summary>
    /// Removes a previously added typed callback from every registered parameter whose value type matches <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to filter on.</typeparam>
    /// <param name="listener">The callback to remove.</param>
    /// <remarks>
    /// Requires <see cref="Load"/> to have completed so the store has a stable registered
    /// parameter set to unsubscribe from.
    /// </remarks>
    void RemoveListenerFromAll<T>(Action<T?, T?> listener);

    /// <summary>
    /// Removes a previously added callback from every registered parameter that satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The filter used to select parameters.</param>
    /// <param name="listener">The callback to remove.</param>
    /// <remarks>
    /// Removal uses the matched subscription set associated with the earlier add call; it does not
    /// re-evaluate <paramref name="predicate"/> against current parameter state.
    /// </remarks>
    void RemoveListenerFromAll(Func<IParameter, bool> predicate, Action listener);

    /// <summary>
    /// Resets every registered parameter to its default value.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="Load"/> to have completed so the store has a registered parameter set
    /// to reset.
    /// </remarks>
    void ResetAll();
}
