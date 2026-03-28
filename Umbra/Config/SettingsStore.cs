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
    private readonly string _filePath;
    private readonly Dictionary<string, IParameter> _parameters = [];
    private readonly List<Action> _cleanupActions = [];
    private bool _loaded;
    private bool _disposed;

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
    /// Persists the current parameter values to the configured file path.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void Save()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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
    /// This method must only be called once per <see cref="SettingsStore{TConfig}"/> instance.
    /// Calling it a second time would register duplicate <see cref="IParameter"/> instances and
    /// disconnect all previously registered event listeners from the returned config object.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Load"/> has already been called on this instance.</exception>
    public TConfig Load()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_loaded)
            throw new InvalidOperationException(
                $"SettingsStore<{typeof(TConfig).Name}>.Load() must only be called once per instance. " +
                "Create a new SettingsStore to load a fresh configuration.");
        _loaded = true;

        var instance = new TConfig();
        var discovered = SettingsRegistrar.Register(instance);

        foreach (var (key, param) in discovered)
            _parameters[key] = param;

        Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: discovered {_parameters.Count} parameter(s).");

        if (!File.Exists(_filePath))
        {
            Logger.Info($"SettingsStore<{typeof(TConfig).Name}>: no existing config file found at '{_filePath}', saving defaults.");
            Save();
            return instance;
        }

        SettingsPersistence.Load(_filePath, _parameters);
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
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void CopyValuesTo(SettingsStore<TConfig> target, bool setWithoutNotifying = false)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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
    /// <param name="listener">The callback to invoke whenever any parameter value changes.</param>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void AddListenerToAll(Action listener)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        foreach (var p in _parameters.Values) p.ValueChanged += listener;
        _cleanupActions.Add(() => { foreach (var p in _parameters.Values) p.ValueChanged -= listener; });
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
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void AddListenerToAll<T>(Action<T?, T?> listener)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        foreach (var p in _parameters.Values)
            if (p is Parameter<T> typed) typed.ValueChanged += listener;

        _cleanupActions.Add(() =>
        {
            foreach (var p in _parameters.Values)
                if (p is Parameter<T> typed) typed.ValueChanged -= listener;
        });
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
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void AddListenerToAll(Func<IParameter, bool> predicate, Action listener)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var matched = new List<IParameter>();
        foreach (var p in _parameters.Values)
        {
            if (!predicate(p)) continue;
            p.ValueChanged += listener;
            matched.Add(p);
        }

        _cleanupActions.Add(() =>
        {
            foreach (var p in matched)
                p.ValueChanged -= listener;
        });
    }

    /// <summary>
    /// Removes a previously added callback from the <see cref="IParameter.ValueChanged"/> event of every registered parameter.
    /// </summary>
    /// <param name="listener">The callback to remove.</param>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void RemoveListenerFromAll(Action listener)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        foreach (var p in _parameters.Values) p.ValueChanged -= listener;
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
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void RemoveListenerFromAll<T>(Action<T?, T?> listener)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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
    /// </param>
    /// <param name="listener">The callback to remove.</param>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void RemoveListenerFromAll(Func<IParameter, bool> predicate, Action listener)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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
    /// persistent state.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when this instance has been disposed.</exception>
    public void ResetAll()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
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
    /// including removing all registered event listeners.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var cleanup in _cleanupActions) cleanup();
        _cleanupActions.Clear();

        GC.SuppressFinalize(this);
    }
}
