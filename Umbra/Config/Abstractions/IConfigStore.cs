namespace Umbra.Config;

/// <summary>
/// Defines the lifecycle, persistence, reset, copy, and listener operations exposed by a typed config store.
/// </summary>
/// <remarks>
/// Implementations discover and register a stable parameter set during <see cref="Load"/>, then expose persistence and listener operations over that registered set for the remainder of the store instance's lifetime. Unless stated otherwise, operations that inspect or mutate registered parameters require <see cref="Load"/> to have completed successfully first.
/// </remarks>
/// <typeparam name="TConfig">The configuration type managed by the store.</typeparam>
public interface IConfigStore<TConfig> : IDisposable
    where TConfig : class, new()
{
    /// <summary>
    /// Gets a value indicating whether <see cref="Load"/> has completed successfully for this store instance.
    /// </summary>
    /// <value><see langword="true"/> after registration and load complete successfully; otherwise, <see langword="false"/>.</value>
    bool IsLoaded { get; }

    /// <summary>
    /// Gets a value indicating whether this store has been disposed.
    /// </summary>
    /// <value><see langword="true"/> if the store has been disposed; otherwise, <see langword="false"/>.</value>
    bool IsDisposed { get; }

    /// <summary>
    /// Creates, registers, and loads the configuration instance for this store.
    /// </summary>
    /// <returns>The loaded configuration instance.</returns>
    /// <remarks>
    /// This method is single-use per store instance. Implementations may persist declared defaults on first run, load persisted values when the backing file exists, and recover from unreadable files by reverting the current session to declared defaults.
    /// </remarks>
    TConfig Load();

    /// <summary>
    /// Persists the current registered parameter values to the configured file path.
    /// </summary>
    /// <remarks>
    /// Requires <see cref="Load"/> to have completed successfully so the store has a stable registered parameter set to persist.
    /// </remarks>
    void Save();

    /// <summary>
    /// Writes the current registered parameter values to a versioned config exchange document.
    /// </summary>
    /// <param name="filePath">The destination file path for the exported document.</param>
    /// <remarks>
    /// Requires <see cref="Load"/> to have completed successfully so the store has a stable registered parameter set to export.
    /// </remarks>
    void Export(string filePath);

    /// <summary>
    /// Imports compatible values from a config exchange document or a legacy flat config file.
    /// </summary>
    /// <param name="filePath">The source file path to import from.</param>
    /// <param name="options">Optional import finalization options.</param>
    /// <returns>A structured report describing applied, ignored, and rejected keys.</returns>
    /// <remarks>
    /// Requires <see cref="Load"/> to have completed successfully so the store has a stable registered parameter set to receive imported values.
    /// </remarks>
    ConfigImportReport Import(string filePath, ConfigImportOptions? options = null);

    /// <summary>
    /// Copies this store's registered parameter values into the corresponding parameters of <paramref name="target"/>.
    /// </summary>
    /// <param name="target">The destination store to copy values into.</param>
    /// <param name="setWithoutNotifying"><see langword="true"/> to apply copied values through the target store's silent mutation path; otherwise, <see langword="false"/>.</param>
    /// <remarks>
    /// Both stores must already be loaded so their parameter maps are stable. Parameters present in this store but absent from <paramref name="target"/> are ignored.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">This store or <paramref name="target"/> has been disposed.</exception>
    /// <exception cref="InvalidOperationException">This store or <paramref name="target"/> has not completed <see cref="Load"/>, or <paramref name="target"/> does not support Umbra's copy-target contract.</exception>
    void CopyValuesTo(IConfigStore<TConfig> target, bool setWithoutNotifying = false);

    /// <summary>
    /// Subscribes a callback to every registered parameter.
    /// </summary>
    /// <param name="listener">The callback to invoke whenever any registered parameter changes.</param>
    /// <remarks>
    /// The callback is attached to the currently registered parameter set and is removed automatically when the store is disposed.
    /// </remarks>
    void AddListenerToAll(Action listener);

    /// <summary>
    /// Subscribes a typed callback to every registered parameter whose value type matches <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to match.</typeparam>
    /// <param name="listener">The callback to invoke whenever a matching parameter changes.</param>
    /// <remarks>
    /// The callback is attached to the currently registered parameter set and is removed automatically when the store is disposed. When using nullable value-type delegates, prefer specifying <typeparamref name="T"/> explicitly to avoid generic type-inference mismatches.
    /// </remarks>
    void AddListenerToAll<T>(Action<T?, T?> listener);

    /// <summary>
    /// Subscribes a callback to every registered parameter that satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The filter used to select parameters.</param>
    /// <param name="listener">The callback to invoke whenever a matching parameter changes.</param>
    /// <remarks>
    /// The predicate is evaluated against the registered parameter set at subscription time, and that matched set is what later removal and disposal cleanup operate on.
    /// </remarks>
    void AddListenerToAll(Func<IParameter, bool> predicate, Action listener);

    /// <summary>
    /// Removes a previously added callback from every registered parameter.
    /// </summary>
    /// <param name="listener">The callback to remove.</param>
    void RemoveListenerFromAll(Action listener);

    /// <summary>
    /// Removes a previously added typed callback from every registered parameter whose value type matches <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to match.</typeparam>
    /// <param name="listener">The callback to remove.</param>
    void RemoveListenerFromAll<T>(Action<T?, T?> listener);

    /// <summary>
    /// Removes a previously added callback from every registered parameter that satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">The filter used to identify the subscription shape being removed.</param>
    /// <param name="listener">The callback to remove.</param>
    /// <remarks>
    /// Implementations may use the tracked subscription set captured during the original add call instead of re-evaluating <paramref name="predicate"/> against current parameter state.
    /// </remarks>
    void RemoveListenerFromAll(Func<IParameter, bool> predicate, Action listener);

    /// <summary>
    /// Resets every registered parameter to its default value.
    /// </summary>
    void ResetAll();
}
