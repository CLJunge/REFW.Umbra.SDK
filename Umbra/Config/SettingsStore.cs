using System.Diagnostics;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Manages loading, saving, and lifecycle of typed configuration settings for a plugin or mod.
/// </summary>
/// <typeparam name="TConfig">
/// The configuration class type. Must have a public parameterless constructor.
/// </typeparam>
[DebuggerDisplay("SettingsStore for {typeof(TConfig).Name}, Parameters: {_parameters.Count}")]
public class SettingsStore<TConfig> : IDisposable
    where TConfig : class, new()
{
    private sealed class ListenerCleanupRegistration(
        Action cleanup,
        Delegate listener,
        Type? valueType,
        Func<IParameter, bool>? predicate)
    {
        internal Action Cleanup { get; } = cleanup;
        internal Delegate Listener { get; } = listener;
        internal Type? ValueType { get; } = valueType;
        internal Func<IParameter, bool>? Predicate { get; } = predicate;
    }

    private readonly string _filePath;
    private readonly Dictionary<string, IParameter> _parameters = [];
    private readonly List<ListenerCleanupRegistration> _cleanupRegistrations = [];
    private bool _loaded;
    private bool _disposed;
    private bool _saveBlocked;
    private bool _saveBlockedWarningLogged;

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

        _filePath = filePath;
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
    /// Persists the current parameter values to the configured file path.
    /// </summary>
    /// <remarks>
    /// This method requires <see cref="Load"/> to have completed successfully so the store has a
    /// stable registered parameter set to persist.
    /// If a previous <see cref="Load"/> attempt encountered an unreadable config file that could
    /// not be backed up safely, saves are suppressed for the lifetime of this store instance so the
    /// original file is not overwritten later in the same session.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void Save()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();

        if (_saveBlocked)
        {
            WarnSaveBlockedOnce();
            return;
        }

        SettingsPersistence.Save(_filePath, _parameters);
    }

    /// <summary>
    /// Creates a new instance of <typeparamref name="TConfig"/>, registers all of its parameters,
    /// and loads persisted values from disk if the settings file exists.
    /// If no file exists, the defaults are saved immediately.
    /// </summary>
    /// <returns>
    /// A fully initialized <typeparamref name="TConfig"/> instance with values populated from disk or defaults.
    /// </returns>
    /// <remarks>
    /// <para>
    /// <typeparamref name="TConfig"/> must be decorated with
    /// <see cref="Attributes.UmbraAutoRegisterSettingsAttribute"/>; if the attribute is absent,
    /// no parameters are discovered and the returned instance will hold only its property default values.
    /// Nested settings group types exposed as <see cref="Attributes.UmbraSettingsParameterAttribute"/>
    /// properties must also carry the attribute.
    /// </para>
    /// <para>
    /// The built-in registration pipeline reflects only public instance properties marked with
    /// <see cref="Attributes.UmbraSettingsParameterAttribute"/>. Fields are ignored by
    /// <see cref="SettingsRegistrar"/> even though some metadata attributes permit field targets.
    /// </para>
    /// <para>
    /// Persisted values are matched by exact fully-qualified key during load. Changing a
    /// <see cref="Attributes.UmbraSettingsPrefixAttribute"/> value, or otherwise changing how a
    /// parameter key resolves, effectively renames those persisted keys. Old JSON entries are not
    /// migrated automatically and will no longer load unless the file is updated to the new names.
    /// </para>
    /// <para>
    /// If the existing JSON file is unreadable, Umbra attempts to move it aside to a timestamped
    /// <c>.invalid-*.json</c> backup and immediately rewrites a fresh defaults file at the original
    /// path. If the unreadable file cannot be backed up, the original file is left untouched and
    /// the returned config instance retains its in-memory defaults for the current session only.
    /// In that failure case, subsequent <see cref="Save"/> calls on the same store instance are
    /// suppressed so the original unreadable file is not overwritten later in the session.
    /// Any parameter values that may have been applied before the load failed are discarded by
    /// rebuilding the store from a fresh config instance, so the current session always continues
    /// from true declared defaults after an unreadable-file failure.
    /// </para>
    /// <para>
    /// On the first run, when no settings file exists yet, the default save path's parent
    /// directory is created automatically before defaults are written.
    /// </para>
    /// <para>
    /// This method must only be called once per <see cref="SettingsStore{TConfig}"/> instance.
    /// Calling it a second time would register duplicate <see cref="IParameter"/> instances and
    /// disconnect all previously registered event listeners from the returned config object.
    /// </para>
    /// <para>
    /// The store transitions to the loaded state only after parameter discovery succeeds. If
    /// discovery fails — for example due to duplicate fully-qualified keys — the instance remains
    /// unloaded so the failure does not leave behind a partially initialized store state.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="Load"/> has already been called on this instance, or when two
    /// discovered settings parameters resolve to the same fully-qualified key.
    /// </exception>
    public TConfig Load()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_loaded)
            throw new InvalidOperationException(
                $"SettingsStore<{typeof(TConfig).Name}>.Load() must only be called once per instance. " +
                "Create a new SettingsStore to load a fresh configuration.");

        var instance = new TConfig();
        var discovered = SettingsRegistrar.Register(instance);

        foreach (var (key, param) in discovered)
            _parameters[key] = param;

        _loaded = true;

        Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: discovered {_parameters.Count} parameter(s).");

        if (!File.Exists(_filePath))
        {
            Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: no existing config file found at '{_filePath}', saving defaults.");
            Save();
            return instance;
        }

        var loadResult = SettingsPersistence.Load(_filePath, _parameters);
        if (loadResult == SettingsPersistence.LoadResult.RecoveredToDefaults)
        {
            Logger.Warning(
                $"SettingsStore<{typeof(TConfig).Name}>: existing config was unreadable; rewriting defaults to '{_filePath}'.");

            // The previous load attempt may have partially mutated parameter values before failing.
            // Rebuild the config instance and parameter map from scratch to guarantee we persist true defaults.
            instance = RebuildDefaults();

            Save();
        }
        else if (loadResult == SettingsPersistence.LoadResult.Failed)
        {
            // Even when the unreadable file must be preserved in place, the current session should
            // still continue from true defaults rather than any values that may have been applied
            // before the failure occurred.
            instance = RebuildDefaults();
            _saveBlocked = true;
            Logger.Warning(
                $"SettingsStore<{typeof(TConfig).Name}>: preserving unreadable config at '{_filePath}'. " +
                "Saves are suppressed for this store instance because the file could not be backed up safely.");
        }

        return instance;
    }

    /// <summary>
    /// Copies all parameter values from this store into the corresponding parameters of
    /// <paramref name="target"/>, matched by key.
    /// </summary>
    /// <param name="target">The destination <see cref="SettingsStore{TConfig}"/> to copy values into.</param>
    /// <param name="setWithoutNotifying">
    /// When <see langword="true"/>, values are applied without raising <see cref="IParameter.ValueChanged"/> events.
    /// This uses <see cref="IParameter.SetValueWithoutNotify(object?)"/>, so metadata-based validation is also bypassed.
    /// When <see langword="false"/>, normal change notification is triggered.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="target"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">
    /// Thrown when this instance or <paramref name="target"/> has been disposed.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when this instance or <paramref name="target"/> has not completed <see cref="Load"/> yet.
    /// </exception>
    public void CopyValuesTo(SettingsStore<TConfig> target, bool setWithoutNotifying = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();
        ArgumentNullException.ThrowIfNull(target);
        ObjectDisposedException.ThrowIf(target._disposed, target);
        if (!target._loaded)
        {
            throw new InvalidOperationException(
                $"SettingsStore<{typeof(TConfig).Name}>.CopyValuesTo() requires a target store that has already completed Load().");
        }

        foreach (var (key, param) in _parameters)
        {
            if (!target._parameters.TryGetValue(key, out var dest)) continue;

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
        foreach (var p in _parameters.Values) p.ValueChanged += listener;
        RegisterCleanup(() =>
        {
            foreach (var p in _parameters.Values)
                p.ValueChanged -= listener;
        }, listener, null, null);
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
        foreach (var p in _parameters.Values)
            if (p is Parameter<T> typed) typed.ValueChanged += listener;

        RegisterCleanup(() =>
        {
            foreach (var p in _parameters.Values)
                if (p is Parameter<T> typed) typed.ValueChanged -= listener;
        }, listener, typeof(T), null);
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="predicate"/> or <paramref name="listener"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    public void AddListenerToAll(Func<IParameter, bool> predicate, Action listener)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(listener);
        ObjectDisposedException.ThrowIf(_disposed, this);
        ThrowIfNotLoaded();

        var matched = new List<IParameter>();
        foreach (var p in _parameters.Values)
        {
            if (!predicate(p)) continue;
            p.ValueChanged += listener;
            matched.Add(p);
        }

        RegisterCleanup(() =>
        {
            foreach (var p in matched)
                p.ValueChanged -= listener;
        }, listener, null, predicate);
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
        if (TryRemoveTrackedCleanup(listener, null, null))
            return;

        foreach (var p in _parameters.Values)
            p.ValueChanged -= listener;
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
        if (TryRemoveTrackedCleanup(listener, typeof(T), null))
            return;

        foreach (var p in _parameters.Values)
            if (p is Parameter<T> typed) typed.ValueChanged -= listener;
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
        if (TryRemoveTrackedCleanup(listener, null, predicate))
            return;

        foreach (var p in _parameters.Values)
            if (predicate(p)) p.ValueChanged -= listener;
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
        foreach (var p in _parameters.Values)
        {
            if (typeof(Delegate).IsAssignableFrom(p.ValueType)) continue;
            p.Reset();
            count++;
        }
        Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: reset {count} parameter(s) to defaults.");
    }

    /// <summary>
    /// Releases all resources used by this <see cref="SettingsStore{TConfig}"/>,
    /// including removing all remaining tracked event listeners.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var registration in _cleanupRegistrations)
            registration.Cleanup();
        _cleanupRegistrations.Clear();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Records one listener cleanup action so it can be undone on <see cref="Dispose"/> or by a
    /// matching manual remove call.
    /// </summary>
    /// <param name="cleanup">The unsubscription action to execute later.</param>
    /// <param name="listener">The listener delegate associated with the cleanup action.</param>
    /// <param name="valueType">The typed parameter value type when the listener is type-filtered; otherwise <see langword="null"/>.</param>
    /// <param name="predicate">The predicate associated with the listener when predicate-filtered; otherwise <see langword="null"/>.</param>
    private void RegisterCleanup(Action cleanup, Delegate listener, Type? valueType, Func<IParameter, bool>? predicate)
        => _cleanupRegistrations.Add(new ListenerCleanupRegistration(cleanup, listener, valueType, predicate));

    /// <summary>
    /// Removes one tracked cleanup registration that matches the supplied listener shape and executes it immediately.
    /// </summary>
    /// <param name="listener">The listener delegate being removed.</param>
    /// <param name="valueType">The type filter associated with the listener, if any.</param>
    /// <param name="predicate">The predicate filter associated with the listener, if any.</param>
    /// <returns><see langword="true"/> when a matching tracked cleanup registration was found; otherwise <see langword="false"/>.</returns>
    private bool TryRemoveTrackedCleanup(Delegate listener, Type? valueType, Func<IParameter, bool>? predicate)
    {
        for (var i = _cleanupRegistrations.Count - 1; i >= 0; i--)
        {
            var registration = _cleanupRegistrations[i];
            if (!Equals(registration.Listener, listener))
                continue;
            if (registration.ValueType != valueType)
                continue;
            if (!Equals(registration.Predicate, predicate))
                continue;

            _cleanupRegistrations.RemoveAt(i);
            registration.Cleanup();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Throws when this store has not completed <see cref="Load"/> yet.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has not yet been called.</exception>
    private void ThrowIfNotLoaded()
    {
        if (_loaded)
            return;

        throw new InvalidOperationException(
            $"SettingsStore<{typeof(TConfig).Name}> requires Load() to complete before this operation can be used.");
    }

    /// <summary>
    /// Logs a warning once when saves are being suppressed after an unrecoverable load failure.
    /// </summary>
    private void WarnSaveBlockedOnce()
    {
        if (_saveBlockedWarningLogged)
            return;

        _saveBlockedWarningLogged = true;
        Logger.Warning(
            $"SettingsStore<{typeof(TConfig).Name}>: Save() ignored because the original config file at '{_filePath}' " +
            "was unreadable and could not be backed up during Load().");
    }

    /// <summary>
    /// Rebuilds the store from a fresh <typeparamref name="TConfig"/> instance so all parameter
    /// values and metadata return to their declared defaults.
    /// </summary>
    /// <returns>A newly created config instance registered into this store.</returns>
    private TConfig RebuildDefaults()
    {
        var instance = new TConfig();
        _parameters.Clear();

        var discovered = SettingsRegistrar.Register(instance);
        foreach (var (key, param) in discovered)
            _parameters[key] = param;

        return instance;
    }
}
