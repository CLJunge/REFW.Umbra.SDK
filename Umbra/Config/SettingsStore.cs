using System.Diagnostics;
using Umbra.Config.Attributes;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Owns the public lifecycle and registered parameter set of a typed settings store.
/// </summary>
/// <remarks>
/// Persistence orchestration is delegated to <see cref="SettingsStorePersistenceCoordinator{TConfig}"/>, and listener bookkeeping is delegated to <see cref="SettingsStoreListenerRegistry"/>. This type remains responsible for public lifecycle guards, the shared registered parameter map, and parameter-level operations over that map.
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type. Must have a public parameterless constructor.
/// </typeparam>
[DebuggerDisplay("SettingsStore for {typeof(TConfig).Name}, Parameters: {_parameters.Count}")]
public class SettingsStore<TConfig> : ISettingsStore<TConfig>, ISettingsStoreCopyTarget<TConfig>
    where TConfig : class, new()
{
    private readonly Dictionary<string, IParameter> _parameters = [];
    private readonly SettingsStoreListenerRegistry _listenerRegistry = new();
    private readonly SettingsStorePersistenceCoordinator<TConfig> _persistenceCoordinator;
    private bool _loaded;
    private bool _disposed;

    IReadOnlyDictionary<string, IParameter> ISettingsStoreCopyTarget<TConfig>.Parameters => _parameters;

    /// <summary>
    /// Initializes a new instance of <see cref="SettingsStore{TConfig}"/> with the specified file path.
    /// </summary>
    /// <param name="filePath">
    /// The absolute or relative path to the JSON file used for persisting settings.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is <see langword="null"/>, empty, or whitespace.
    /// </exception>
    public SettingsStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null, empty, or whitespace.", nameof(filePath));

        _persistenceCoordinator = new SettingsStorePersistenceCoordinator<TConfig>(filePath, _parameters);
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
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet completed successfully.</exception>
    public void Export(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();

        SettingsExchangePersistence.Export(filePath, _parameters, GetSchemaId(), GetSchemaVersion());
    }

    /// <summary>
    /// Imports compatible values from a versioned config exchange document or a legacy flat settings file.
    /// </summary>
    /// <param name="filePath">The source file path.</param>
    /// <param name="options">Optional import finalization settings.</param>
    /// <returns>A structured report describing the import outcome.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet completed successfully.</exception>
    public SettingsImportReport Import(string filePath, SettingsImportOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();

        options ??= SettingsImportOptions.Default;
        var report = SettingsExchangePersistence.Import(filePath, _parameters, GetSchemaId(), GetSchemaVersion());
        if (!report.Success || !options.SaveAfterImport || report.AppliedCount == 0)
            return report;

        Save();
        report.Saved = true;
        return report;
    }

    /// <summary>
    /// Creates, registers, and loads a fresh <typeparamref name="TConfig"/> instance for this store.
    /// </summary>
    /// <returns>A fully initialized <typeparamref name="TConfig"/> instance populated from persisted values or declared defaults.</returns>
    /// <remarks>
    /// <para>
    /// <typeparamref name="TConfig"/> must be decorated with <see cref="Attributes.UmbraAutoRegisterAttribute"/> for Umbra to discover its public instance properties marked with <see cref="Attributes.UmbraParameterAttribute"/>. Nested settings-group properties must expose types that are also decorated with that attribute.
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
                $"SettingsStore<{typeof(TConfig).Name}>.Load() must only be called once per instance. " +
                "Create a new SettingsStore to load a fresh configuration.");

        var instance = _persistenceCoordinator.Load(CreateRegisteredDefaults);
        _loaded = true;
        return instance;
    }

    /// <summary>
    /// Copies this store's registered parameter values into the corresponding parameters of <paramref name="target"/>, matched by key.
    /// </summary>
    /// <param name="target">The destination settings store to copy values into.</param>
    /// <param name="setWithoutNotifying"><see langword="true"/> to apply copied values through the target store's silent mutation path; otherwise, <see langword="false"/>.</param>
    /// <remarks>
    /// Both stores must already be loaded so their registered parameter maps are stable. Parameters that exist in this store but not in <paramref name="target"/> are ignored. When <paramref name="setWithoutNotifying"/> is <see langword="true"/>, copied values bypass both change notification and metadata-based validation on the target parameters.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance or <paramref name="target"/> has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when this instance or <paramref name="target"/> has not completed <see cref="Load"/>, or when <paramref name="target"/> does not support Umbra's internal copy-target contract.</exception>
    public void CopyValuesTo(ISettingsStore<TConfig> target, bool setWithoutNotifying = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        ArgumentNullException.ThrowIfNull(target);
        ObjectDisposedException.ThrowIf(target.IsDisposed, target);
        if (!target.IsLoaded)
        {
            throw new InvalidOperationException(
                $"SettingsStore<{typeof(TConfig).Name}>.CopyValuesTo() requires a target store that has already completed Load().");
        }

        if (target is not ISettingsStoreCopyTarget<TConfig> copyTarget)
        {
            throw new InvalidOperationException(
                $"SettingsStore<{typeof(TConfig).Name}>.CopyValuesTo() requires a target store implementation that supports Umbra parameter-map copy operations.");
        }

        foreach (var (key, param) in _parameters)
        {
            if (!copyTarget.Parameters.TryGetValue(key, out var dest))
                continue;

            if (setWithoutNotifying)
                dest.SetValueWithoutNotify(param.GetValue());
            else
                dest.SetValue(param.GetValue());
        }
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
        var count = 0;
        foreach (var parameter in _parameters.Values)
        {
            if (typeof(Delegate).IsAssignableFrom(parameter.ValueType))
                continue;

            parameter.Reset();
            count++;
        }

        Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: reset {count} parameter(s) to defaults.");
    }

    /// <summary>
    /// Releases this store's remaining listener registrations and marks the instance as disposed.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing. This method does not persist settings automatically.
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
            $"SettingsStore<{typeof(TConfig).Name}> requires Load() to complete successfully before this operation can be used.");
    }

    /// <summary>
    /// Creates a fresh <typeparamref name="TConfig"/> instance and repopulates the shared parameter map from that instance's declared defaults.
    /// </summary>
    /// <returns>A newly created config instance registered into this store.</returns>
    /// <remarks>
    /// This method clears the existing shared parameter map before re-registering the newly created instance.
    /// </remarks>
    private TConfig CreateRegisteredDefaults()
    {
        var instance = new TConfig();
        _parameters.Clear();

        var discovered = SettingsRegistrar.Register(instance);
        foreach (var (key, param) in discovered)
            _parameters[key] = param;

        return instance;
    }

    private static string GetSchemaId()
        => typeof(TConfig).FullName ?? typeof(TConfig).Name;

    private static int GetSchemaVersion()
        => typeof(TConfig).GetCustomAttributes(typeof(UmbraConfigVersionAttribute), inherit: true)
            is [UmbraConfigVersionAttribute attribute, ..]
                ? attribute.Version
                : 1;
}
