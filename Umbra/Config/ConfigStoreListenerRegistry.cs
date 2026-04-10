using System.Diagnostics;

namespace Umbra.Config;

/// <summary>
/// Tracks listener subscriptions attached to a store's registered parameter set and the cleanup actions required to remove them.
/// </summary>
/// <remarks>
/// This registry isolates listener bookkeeping from <see cref="ConfigStore{TConfig}"/> so the store can remain focused on lifecycle and persistence. Each add call records one cleanup action, so repeated registrations of the same listener are tracked independently.
/// </remarks>
[DebuggerDisplay("Tracked listener registrations: {_cleanupRegistrations.Count}")]
internal sealed class ConfigStoreListenerRegistry : IDisposable
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

    private readonly List<ListenerCleanupRegistration> _cleanupRegistrations = [];
    private bool _disposed;

    /// <summary>
    /// Subscribes <paramref name="listener"/> to every registered parameter.
    /// </summary>
    /// <param name="parameters">The stable registered parameter set.</param>
    /// <param name="listener">The callback to invoke when any registered parameter changes.</param>
    internal void AddToAll(IReadOnlyDictionary<string, IParameter> parameters, Action listener)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(listener);

        foreach (var parameter in parameters.Values)
            parameter.ValueChanged += listener;

        RegisterCleanup(() =>
        {
            foreach (var parameter in parameters.Values)
                parameter.ValueChanged -= listener;
        }, listener, null, null);
    }

    /// <summary>
    /// Subscribes <paramref name="listener"/> to every registered parameter whose value type matches <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The parameter value type to match.</typeparam>
    /// <param name="parameters">The stable registered parameter set.</param>
    /// <param name="listener">The typed callback to invoke when a matching parameter changes.</param>
    internal void AddToAll<T>(IReadOnlyDictionary<string, IParameter> parameters, Action<T?, T?> listener)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(listener);

        foreach (var parameter in parameters.Values)
            if (parameter is Parameter<T> typed)
                typed.ValueChanged += listener;

        RegisterCleanup(() =>
        {
            foreach (var parameter in parameters.Values)
                if (parameter is Parameter<T> typed)
                    typed.ValueChanged -= listener;
        }, listener, typeof(T), null);
    }

    /// <summary>
    /// Subscribes <paramref name="listener"/> to the subset of registered parameters selected by <paramref name="predicate"/>.
    /// </summary>
    /// <param name="parameters">The stable registered parameter set.</param>
    /// <param name="predicate">The filter evaluated once at subscription time.</param>
    /// <param name="listener">The callback to invoke when a matching parameter changes.</param>
    internal void AddToAll(IReadOnlyDictionary<string, IParameter> parameters, Func<IParameter, bool> predicate, Action listener)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(listener);

        var matched = new List<IParameter>();
        foreach (var parameter in parameters.Values)
        {
            if (!predicate(parameter))
                continue;

            parameter.ValueChanged += listener;
            matched.Add(parameter);
        }

        RegisterCleanup(() =>
        {
            foreach (var parameter in matched)
                parameter.ValueChanged -= listener;
        }, listener, null, predicate);
    }

    /// <summary>
    /// Removes one previously added untyped listener registration from every registered parameter.
    /// </summary>
    /// <param name="parameters">The stable registered parameter set.</param>
    /// <param name="listener">The listener callback to remove.</param>
    internal void RemoveFromAll(IReadOnlyDictionary<string, IParameter> parameters, Action listener)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(listener);

        if (TryRemoveTrackedCleanup(listener, null, null))
            return;

        foreach (var parameter in parameters.Values)
            parameter.ValueChanged -= listener;
    }

    /// <summary>
    /// Removes one previously added typed listener registration from every matching registered parameter.
    /// </summary>
    /// <typeparam name="T">The parameter value type to match.</typeparam>
    /// <param name="parameters">The stable registered parameter set.</param>
    /// <param name="listener">The typed listener callback to remove.</param>
    internal void RemoveFromAll<T>(IReadOnlyDictionary<string, IParameter> parameters, Action<T?, T?> listener)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(listener);

        if (TryRemoveTrackedCleanup(listener, typeof(T), null))
            return;

        foreach (var parameter in parameters.Values)
            if (parameter is Parameter<T> typed)
                typed.ValueChanged -= listener;
    }

    /// <summary>
    /// Removes one previously added predicate-based listener registration from the matching registered parameters.
    /// </summary>
    /// <param name="parameters">The stable registered parameter set.</param>
    /// <param name="predicate">The predicate used to identify matching parameters.</param>
    /// <param name="listener">The listener callback to remove.</param>
    internal void RemoveFromAll(IReadOnlyDictionary<string, IParameter> parameters, Func<IParameter, bool> predicate, Action listener)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(listener);

        if (TryRemoveTrackedCleanup(listener, null, predicate))
            return;

        foreach (var parameter in parameters.Values)
        {
            if (!predicate(parameter))
                continue;

            parameter.ValueChanged -= listener;
        }
    }

    /// <summary>
    /// Executes every tracked cleanup action once and clears the registry.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        foreach (var registration in _cleanupRegistrations)
            registration.Cleanup();

        _cleanupRegistrations.Clear();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Records one cleanup action so it can be executed later or matched by a manual remove call.
    /// </summary>
    /// <param name="cleanup">The unsubscription action to execute later.</param>
    /// <param name="listener">The listener delegate associated with the cleanup.</param>
    /// <param name="valueType">The typed value filter associated with the listener, if any.</param>
    /// <param name="predicate">The predicate filter associated with the listener, if any.</param>
    private void RegisterCleanup(Action cleanup, Delegate listener, Type? valueType, Func<IParameter, bool>? predicate)
        => _cleanupRegistrations.Add(new ListenerCleanupRegistration(cleanup, listener, valueType, predicate));

    /// <summary>
    /// Removes one tracked cleanup registration matching the supplied listener shape and executes it immediately.
    /// </summary>
    /// <param name="listener">The listener delegate being removed.</param>
    /// <param name="valueType">The typed value filter associated with the listener, if any.</param>
    /// <param name="predicate">The predicate filter associated with the listener, if any.</param>
    /// <returns><see langword="true"/> when a tracked registration was found; otherwise <see langword="false"/>.</returns>
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
}
