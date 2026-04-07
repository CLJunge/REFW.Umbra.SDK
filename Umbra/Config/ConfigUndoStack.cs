using System.Diagnostics;
using Umbra.UI.Toast;

namespace Umbra.Config;

/// <summary>
/// Tracks parameter changes on a loaded <see cref="ConfigStore{TConfig}"/> and provides
/// single-step undo capability backed by a fixed-capacity stack.
/// </summary>
/// <remarks>
/// <para>
/// On construction the stack snapshots every non-delegate parameter value and subscribes to
/// the untyped <see cref="IParameter.ValueChanged"/> event. When a parameter changes, the
/// old snapshot value and the new current value are recorded as a
/// <see cref="ConfigChangeRecord"/>. Calling <see cref="TryUndo"/> restores the most recent
/// change and pushes a toast notification via <see cref="ToastQueue"/>.
/// </para>
/// <para>
/// Delegate-typed parameters (buttons) are excluded from tracking because they do not
/// represent persisted state.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration class managed by the store.</typeparam>
public sealed class ConfigUndoStack<TConfig> : IDisposable
    where TConfig : class, new()
{
    /// <summary>
    /// The default maximum number of change records retained before the oldest is dropped.
    /// </summary>
    public const int DefaultCapacity = 32;

    private readonly IReadOnlyDictionary<string, IParameter> _parameters;
    private readonly Dictionary<string, object?> _snapshots;
    private readonly List<ConfigChangeRecord> _stack;
    private readonly List<Action> _cleanupActions;
    private readonly int _capacity;
    private bool _suppressRecording;
    private bool _disposed;

    /// <summary>
    /// Initializes a new undo stack that tracks parameter changes on the specified store.
    /// </summary>
    /// <param name="store">
    /// A loaded <see cref="ConfigStore{TConfig}"/>. Must be loaded and not disposed.
    /// </param>
    /// <param name="capacity">
    /// Maximum number of change records to retain. When exceeded, the oldest record is dropped.
    /// Defaults to <see cref="DefaultCapacity"/>.
    /// </param>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="store"/> has not been loaded.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="store"/> has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 1.</exception>
    public ConfigUndoStack(ConfigStore<TConfig> store, int capacity = DefaultCapacity)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (!store.IsLoaded)
            throw new InvalidOperationException("The config store must be loaded before creating an undo stack.");

        if (store.IsDisposed)
            throw new ObjectDisposedException(nameof(store), "The config store has been disposed.");

        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");

        _capacity = capacity;
        _stack = new List<ConfigChangeRecord>(capacity);
        _snapshots = new Dictionary<string, object?>();
        _cleanupActions = new List<Action>();

        var target = (IConfigStoreCopyTarget<TConfig>)store;
        _parameters = target.Parameters;

        SubscribeToParameters();
    }

    /// <summary>
    /// Gets a value indicating whether the stack contains at least one record that can be undone.
    /// </summary>
    public bool CanUndo => !_disposed && _stack.Count > 0;

    /// <summary>
    /// Gets the number of change records currently on the stack.
    /// </summary>
    public int Count => _stack.Count;

    /// <summary>
    /// Returns the most recent change record without removing it, or <see langword="null"/>
    /// if the stack is empty.
    /// </summary>
    /// <returns>The topmost <see cref="ConfigChangeRecord"/>, or <see langword="null"/>.</returns>
    public ConfigChangeRecord? Peek()
    {
        if (_stack.Count == 0) return null;
        return _stack[_stack.Count - 1];
    }

    /// <summary>
    /// Undoes the most recent parameter change by restoring its previous value.
    /// </summary>
    /// <remarks>
    /// The undo operation suppresses change recording so the restoration itself does not
    /// push a new entry onto the stack. On success a toast notification is displayed via
    /// <see cref="ToastQueue.Push(string, ToastLevel, TimeSpan?)"/>.
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if a change was successfully undone;
    /// <see langword="false"/> if the stack is empty, disposed, or the parameter is no longer registered.
    /// </returns>
    public bool TryUndo()
    {
        if (_disposed || _stack.Count == 0) return false;

        var record = _stack[_stack.Count - 1];
        _stack.RemoveAt(_stack.Count - 1);

        if (!_parameters.TryGetValue(record.ParameterKey, out var param))
            return false;

        _suppressRecording = true;
        try
        {
            param.SetValue(record.OldValue);
            _snapshots[record.ParameterKey] = record.OldValue;
        }
        finally
        {
            _suppressRecording = false;
        }

        ToastQueue.Push($"Undo: {record.DisplayLabel}", ToastLevel.Info);
        return true;
    }

    /// <summary>
    /// Removes all change records from the stack.
    /// </summary>
    public void Clear() => _stack.Clear();

    /// <summary>
    /// Detaches all parameter event subscriptions and clears internal state.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        for (int i = 0; i < _cleanupActions.Count; i++)
            _cleanupActions[i]();

        _cleanupActions.Clear();
        _stack.Clear();
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
        if (_suppressRecording) return;

        var old = _snapshots[key];
        var current = param.GetValue();
        _snapshots[key] = current;

        var label = param.Metadata.ResolvedLabel;
        if (string.IsNullOrEmpty(label))
            label = key;

        var record = new ConfigChangeRecord(key, label, old, current, Stopwatch.GetTimestamp());

        if (_stack.Count >= _capacity)
            _stack.RemoveAt(0);

        _stack.Add(record);
    }
}
