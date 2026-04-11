using System.Diagnostics;

namespace Umbra.Config;

/// <summary>
/// Owns the public lifecycle and registered parameter set of a typed config store.
/// </summary>
/// <remarks>
/// Persistence orchestration is delegated to <see cref="ConfigStorePersistenceCoordinator{TConfig}"/>,
/// transfer orchestration is delegated to <see cref="ConfigStoreTransferCoordinator{TConfig}"/>,
/// registered-parameter operations are delegated to
/// <see cref="ConfigStoreRegisteredParameterSet{TConfig}"/>, and listener bookkeeping is
/// delegated to <see cref="ConfigStoreListenerRegistry"/>. This type remains responsible for
/// public lifecycle guards and composition of those collaborators.
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type. Must have a public parameterless constructor.
/// </typeparam>
[DebuggerDisplay("ConfigStore for {typeof(TConfig).Name}, Parameters: {_parameters.Count}")]
public class ConfigStore<TConfig> : IConfigStore<TConfig>, IConfigStoreCopyTarget<TConfig>, IConfigTransferStore
    where TConfig : class, new()
{
    private readonly Dictionary<string, IParameter> _parameters = [];
    private readonly ConfigStoreListenerRegistry _listenerRegistry = new();
    private readonly ConfigStorePersistenceCoordinator<TConfig> _persistenceCoordinator;
    private readonly ConfigStoreTransferCoordinator<TConfig> _transferCoordinator;
    private readonly ConfigStoreRegisteredParameterSet<TConfig> _registeredParameters;
    private bool _loaded;
    private bool _disposed;

    IReadOnlyDictionary<string, IParameter> IConfigStoreCopyTarget<TConfig>.Parameters => _registeredParameters.Parameters;

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigStore{TConfig}"/> with the specified file path.
    /// </summary>
    /// <param name="filePath">
    /// The absolute or relative path to the JSON file used for persisting config data.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="filePath"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty or whitespace.
    /// </exception>
    public ConfigStore(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        _persistenceCoordinator = new ConfigStorePersistenceCoordinator<TConfig>(filePath, _parameters);
        _transferCoordinator = new ConfigStoreTransferCoordinator<TConfig>(_parameters);
        _registeredParameters = new ConfigStoreRegisteredParameterSet<TConfig>(_parameters);
    }

    /// <summary>
    /// Gets whether <see cref="Load"/> has completed successfully for this store instance.
    /// </summary>
    /// <remarks>
    /// A store can only transition from <see langword="false"/> to <see langword="true"/> once.
    /// After that, the loaded parameter set remains fixed for the lifetime of the instance.
    /// If <see cref="Load"/> throws before registration finishes, this property remains
    /// <see langword="false"/>.
    /// </remarks>
    public bool IsLoaded => _loaded;

    /// <summary>
    /// Gets whether this store has been disposed.
    /// </summary>
    /// <remarks>
    /// After disposal, methods that mutate or inspect the registered parameter set throw
    /// <see cref="ObjectDisposedException"/>.
    /// </remarks>
    public bool IsDisposed => _disposed;

    /// <summary>
    /// Gets the configured main config file path for this store.
    /// </summary>
    public string FilePath => _persistenceCoordinator.FilePath;

    /// <summary>
    /// Persists the current registered parameter values to the configured file path.
    /// </summary>
    /// <remarks>
    /// This method requires <see cref="Load"/> to have completed successfully so the store has a stable registered parameter set to persist. If a previous <see cref="Load"/> attempt encountered an unreadable config file that could not be backed up safely, saves are suppressed for the lifetime of this store instance so the original file is not overwritten later in the same session.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet completed successfully.</exception>
    public void Save()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _persistenceCoordinator.Save();
    }

    /// <summary>
    /// Exports the current registered parameter values to a versioned config exchange document.
    /// </summary>
    /// <param name="filePath">The destination file path.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or whitespace.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet completed successfully.</exception>
    public void Export(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();

        _transferCoordinator.Export(filePath);
    }

    /// <summary>
    /// Imports compatible values from a versioned config exchange document or a legacy flat config file.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="options">Optional import finalization options.</param>
    /// <returns>A structured report describing the import outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is empty or whitespace.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet completed successfully.</exception>
    public ConfigImportReport Import(string filePath, ConfigImportOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();

        return _transferCoordinator.Import(filePath, options ?? ConfigImportOptions.Default, Save);
    }

    /// <summary>
    /// Creates, registers, and loads a fresh <typeparamref name="TConfig"/> instance for this store.
    /// </summary>
    /// <returns>A fully initialized <typeparamref name="TConfig"/> instance populated from persisted values or declared defaults.</returns>
    /// <remarks>
    /// <para>
    /// <typeparamref name="TConfig"/> must be decorated with <see cref="Attributes.UmbraAutoRegisterAttribute"/> for Umbra to discover its public instance properties marked with <see cref="Attributes.UmbraParameterAttribute"/>. Nested config-group properties must expose types that are also decorated with that attribute.
    /// </para>
    /// <para>
    /// Persisted values are matched by exact fully qualified key during load. Changing key derivation, such as by changing an <see cref="Attributes.UmbraPrefixAttribute"/>, effectively renames the persisted entries and does not migrate existing JSON automatically.
    /// </para>
    /// <para>
    /// If the existing JSON file is unreadable, Umbra attempts to move it aside to a timestamped backup and rewrite declared defaults. When that backup step fails, the current session is rebuilt from fresh declared defaults and later <see cref="Save"/> calls on the same store instance are suppressed so the original unreadable file is preserved.
    /// </para>
    /// <para>
    /// This method is single-use per store instance. The store transitions to the loaded state only after parameter discovery and load orchestration complete successfully.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has already been called on this instance, or when registration fails such as from duplicate fully qualified keys.</exception>
    public TConfig Load()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_loaded)
            throw new InvalidOperationException(
                $"ConfigStore<{typeof(TConfig).Name}>.Load() must only be called once per instance. " +
                "Create a new ConfigStore to load a fresh configuration.");

        var instance = _persistenceCoordinator.Load(_registeredParameters.CreateRegisteredDefaults);
        _loaded = true;
        return instance;
    }

    /// <summary>
    /// Copies this store's registered parameter values into the corresponding parameters of <paramref name="target"/>, matched by key.
    /// </summary>
    /// <param name="target">The destination config store to copy values into.</param>
    /// <param name="setWithoutNotifying"><see langword="true"/> to apply copied values through the target store's silent mutation path; otherwise, <see langword="false"/>.</param>
    /// <remarks>
    /// Both stores must already be loaded so their registered parameter maps are stable. Parameters that exist in this store but not in <paramref name="target"/> are ignored. When <paramref name="setWithoutNotifying"/> is <see langword="true"/>, copied values bypass both change notification and metadata-based validation on the target parameters.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance or <paramref name="target"/> has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this instance or <paramref name="target"/> has not completed <see cref="Load"/>, or when <paramref name="target"/> does not support Umbra's internal copy-target contract.</exception>
    public void CopyValuesTo(IConfigStore<TConfig> target, bool setWithoutNotifying = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _registeredParameters.CopyValuesTo(target, setWithoutNotifying);
    }

    /// <summary>
    /// Subscribes a callback to the <see cref="IParameter.ValueChanged"/> event of every registered parameter,
    /// and registers cleanup so it is removed on <see cref="Dispose"/>.
    /// </summary>
    /// <remarks>
    /// If the same listener is added multiple times through this method, each subscription is tracked
    /// independently and must be removed separately. This method requires <see cref="Load"/> to
    /// have completed so there is a stable registered parameter set to subscribe to.
    /// </remarks>
    /// <param name="listener">The callback to invoke whenever any parameter value changes.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listener"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void AddListenerToAll(Action listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _listenerRegistry.AddToAll(_parameters, listener);
    }

    /// <summary>
    /// Subscribes a typed callback to the <see cref="Parameter{T}.ValueChanged"/> event of every <see cref="Parameter{T}"/>
    /// whose value type matches <typeparamref name="T"/>, and registers cleanup so it is removed on <see cref="Dispose"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to filter on.</typeparam>
    /// <param name="listener">
    /// The callback to invoke with the previous and new value whenever a matching parameter changes.
    /// </param>
    /// <remarks>
    /// <para>
    /// Due to a C# type-inference limitation with unconstrained <c>T?</c>, passing a delegate whose
    /// type arguments are already nullable value types (e.g. <see cref="Action{T1,T2}"/> of
    /// <c>int?</c>) causes the compiler to infer <typeparamref name="T"/> = <c>int?</c> rather
    /// than <c>int</c>, so the <c>is Parameter&lt;T&gt;</c> filter never matches. Always supply
    /// the type argument explicitly when calling this overload with nullable-value-type delegates,
    /// or prefer <see cref="AddListenerToAll(Func{IParameter,bool},Action)"/> instead.
    /// </para>
    /// <para>
    /// This method requires <see cref="Load"/> to have completed so there is a stable registered
    /// parameter set to subscribe to.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listener"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void AddListenerToAll<T>(Action<T?, T?> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _listenerRegistry.AddToAll(_parameters, listener);
    }

    /// <summary>
    /// Subscribes a callback to the <see cref="IParameter.ValueChanged"/> event of every registered parameter
    /// that satisfies <paramref name="predicate"/>, and registers cleanup so it is removed on <see cref="Dispose"/>.
    /// </summary>
    /// <param name="predicate">
    /// A function evaluated once per registered parameter at subscription time; the listener
    /// is attached only to parameters for which it returns <see langword="true"/>.
    /// The matched parameter set is captured at subscription time — the predicate is <em>not</em>
    /// re-evaluated during cleanup, so predicate results that depend on mutable external state
    /// will not affect whether listeners are removed on <see cref="Dispose"/>.
    /// </param>
    /// <param name="listener">The callback to invoke whenever a matching parameter's value changes.</param>
    /// <remarks>
    /// Prefer this overload over <see cref="AddListenerToAll{T}(Action{T,T})"/> when the
    /// selection criterion is based on <see cref="IParameter.ValueType"/> (e.g. to detect changes
    /// to numeric parameters), because it avoids the generic type-inference pitfall described
    /// on that overload.
    /// Each call captures the exact matched parameter set and tracks it independently for later
    /// removal, even when the same predicate/listener pair is added more than once.
    /// This method requires <see cref="Load"/> to have completed so there is a stable registered
    /// parameter set to subscribe to.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listener"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void AddListenerToAll(Func<IParameter, bool> predicate, Action listener)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(listener);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _listenerRegistry.AddToAll(_parameters, predicate, listener);
    }

    /// <summary>
    /// Removes a previously added callback from the <see cref="IParameter.ValueChanged"/> event of every registered parameter.
    /// </summary>
    /// <remarks>
    /// When the listener was originally added through <see cref="AddListenerToAll(Action)"/>, this
    /// method also removes one matching dispose-time cleanup registration so <see cref="Dispose"/>
    /// does not repeat unnecessary unsubscription work. This method requires <see cref="Load"/> to
    /// have completed so there is a stable registered parameter set to unsubscribe from.
    /// </remarks>
    /// <param name="listener">The callback to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listener"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void RemoveListenerFromAll(Action listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _listenerRegistry.RemoveFromAll(_parameters, listener);
    }

    /// <summary>
    /// Removes a previously added typed callback from the <see cref="Parameter{T}.ValueChanged"/> event of every
    /// <see cref="Parameter{T}"/> whose value type matches <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to filter on.</typeparam>
    /// <param name="listener">The typed callback to remove.</param>
    /// <remarks>
    /// See <see cref="AddListenerToAll{T}(Action{T,T})"/> for the type-inference caveat that
    /// applies equally here. Supply the type argument explicitly when needed.
    /// When the listener was originally added through <see cref="AddListenerToAll{T}(Action{T,T})"/>,
    /// this method also removes one matching dispose-time cleanup registration.
    /// This method requires <see cref="Load"/> to have completed so there is a stable registered
    /// parameter set to unsubscribe from.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="listener"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void RemoveListenerFromAll<T>(Action<T?, T?> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _listenerRegistry.RemoveFromAll(_parameters, listener);
    }

    /// <summary>
    /// Removes a previously added callback from the <see cref="IParameter.ValueChanged"/> event of every registered
    /// parameter that satisfies <paramref name="predicate"/>.
    /// </summary>
    /// <param name="predicate">
    /// A filtering function applied to the current parameter set to identify which parameters to
    /// unsubscribe from. For deterministic removal, the predicate must produce the same set of
    /// matches as it did at subscription time. If the predicate closes over mutable external state
    /// that has changed since subscription, parameters that were originally subscribed may not be
    /// unsubscribed. When lifecycle cleanup is the primary concern, rely on <see cref="Dispose"/>
    /// instead — <see cref="AddListenerToAll(Func{IParameter,bool},Action)"/> captures the matched
    /// set at subscription time and removes exactly those listeners on disposal.
    /// This method requires <see cref="Load"/> to have completed so there is a stable registered
    /// parameter set to unsubscribe from.
    /// </param>
    /// <param name="listener">The callback to remove.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="predicate"/> or <paramref name="listener"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void RemoveListenerFromAll(Func<IParameter, bool> predicate, Action listener)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(listener);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _listenerRegistry.RemoveFromAll(_parameters, predicate, listener);
    }

    /// <summary>
    /// Resets every registered parameter to its default value, raising <see cref="IParameter.ValueChanged"/>
    /// for each parameter whose value actually changes.
    /// </summary>
    /// <remarks>
    /// Delegate-typed parameters (e.g. <see cref="Parameter{T}"/> of type <see cref="Action"/>
    /// used by button drawers) are skipped because their default values carry no meaningful
    /// persistent state. This method requires <see cref="Load"/> to have completed so there is a
    /// stable registered parameter set to reset.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void ResetAll()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        _registeredParameters.ResetAll();
    }

    /// <summary>
    /// Releases this store's remaining listener registrations and marks the instance as disposed.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing. This method does not persist config automatically.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _listenerRegistry.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Throws when this store has not successfully completed <see cref="Load"/> yet.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Load"/> has not completed successfully, such as when it has not been called yet.
    /// </exception>
    private void ThrowIfNotLoaded()
    {
        if (_loaded)
            return;

        throw new InvalidOperationException(
            $"ConfigStore<{typeof(TConfig).Name}> requires Load() to complete successfully before this operation can be used.");
    }

}
