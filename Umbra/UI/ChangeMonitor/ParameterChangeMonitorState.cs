using System.Diagnostics;
using Umbra.Config;
using Umbra.UI.LiveState;

namespace Umbra.UI.ChangeMonitor;

/// <summary>
/// Live-state type that tracks parameter changes on a loaded <see cref="ConfigStore{TConfig}"/>
/// and exposes them through a <see cref="ConfigChangeLog"/> for drawer rendering.
/// </summary>
/// <remarks>
/// <para>
/// On construction, the state subscribes to the untyped <see cref="IParameter.ValueChanged"/>
/// event of each non-delegate parameter and records changes into the log. Delegate-typed
/// parameters are excluded because they do not represent persisted state.
/// </para>
/// <para>
/// Dispose detaches all event subscriptions to prevent stale references.
/// </para>
/// </remarks>
[LiveStateSectionDrawer<ParameterChangeMonitorDrawer>]
public sealed class ParameterChangeMonitorState : IDisposable
{
    private readonly IReadOnlyDictionary<string, IParameter> _parameters;
    private readonly Dictionary<string, object?> _snapshots;
    private readonly List<Action> _cleanupActions;
    private bool _disposed;

    /// <summary>
    /// Initializes a new change monitor state that tracks parameter changes on the specified store.
    /// </summary>
    /// <param name="store">A loaded config store to monitor.</param>
    /// <param name="logCapacity">Maximum number of entries in the change log.</param>
    /// <typeparam name="TConfig">The configuration class managed by the store.</typeparam>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="store"/> has not been loaded.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="store"/> has been disposed.</exception>
    public static ParameterChangeMonitorState Create<TConfig>(
        ConfigStore<TConfig> store, int logCapacity = ConfigChangeLog.DefaultCapacity)
        where TConfig : class, new()
    {
        ArgumentNullException.ThrowIfNull(store);

        if (!store.IsLoaded)
            throw new InvalidOperationException("The config store must be loaded before creating a change monitor.");

        if (store.IsDisposed)
            throw new ObjectDisposedException(nameof(store), "The config store has been disposed.");

        var target = (IConfigStoreCopyTarget<TConfig>)store;
        return new ParameterChangeMonitorState(target.Parameters, logCapacity);
    }

    private ParameterChangeMonitorState(
        IReadOnlyDictionary<string, IParameter> parameters, int logCapacity)
    {
        _parameters = parameters;
        _snapshots = new Dictionary<string, object?>();
        _cleanupActions = new List<Action>();
        Log = new ConfigChangeLog(logCapacity);

        SubscribeToParameters();
    }

    /// <summary>
    /// Gets the change log populated by this monitor.
    /// </summary>
    public ConfigChangeLog Log { get; }

    /// <summary>
    /// Detaches all parameter event subscriptions and prevents further recording.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        for (int i = 0; i < _cleanupActions.Count; i++)
            _cleanupActions[i]();

        _cleanupActions.Clear();
        _snapshots.Clear();
    }

    private void SubscribeToParameters()
    {
        foreach (var kvp in _parameters)
        {
            var param = kvp.Value;

            if (typeof(Delegate).IsAssignableFrom(param.ValueType))
                continue;

            var key = param.Key;
            _snapshots[key] = param.GetValue();

            Action handler = () => OnParameterChanged(key, param);
            param.ValueChanged += handler;
            _cleanupActions.Add(() => param.ValueChanged -= handler);
        }
    }

    private void OnParameterChanged(string key, IParameter param)
    {
        if (_disposed) return;

        var old = _snapshots[key];
        var current = param.GetValue();
        _snapshots[key] = current;

        var label = param.Metadata.ResolvedLabel;
        if (string.IsNullOrEmpty(label))
            label = key;

        var record = new ConfigChangeRecord(key, label, old, current, Stopwatch.GetTimestamp());
        Log.Push(record);
    }
}
