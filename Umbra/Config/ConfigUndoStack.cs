using System.Diagnostics;
using Umbra.UI.Config;
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
/// change and, when <see cref="ConfigUndoOptions.Toast"/> is non-<see langword="null"/>,
/// pushes a toast notification via <see cref="ToastQueue"/>.
/// </para>
/// <para>
/// Multi-parameter operations (e.g. reset-all, preset load) can be bracketed with
/// <see cref="BeginBatch"/> / <see cref="EndBatch"/> so that all changes within the batch
/// are recorded as a single composite entry and undone atomically by one <see cref="TryUndo"/> call.
/// </para>
/// <para>
/// Delegate-typed parameters (buttons) are excluded from tracking because they do not
/// represent persisted state.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration class managed by the store.</typeparam>
public sealed class ConfigUndoStack<TConfig> : IDisposable, INumericEditSink, IUndoStackHandle
    where TConfig : class, new()
{
    private sealed class NumericEditSession(object? initialValue)
    {
        internal object? InitialValue { get; } = initialValue;
    }

    /// <summary>
    /// The default maximum number of change records retained before the oldest is dropped.
    /// </summary>
    public const int DefaultCapacity = 32;

    private readonly IReadOnlyDictionary<string, IParameter> _parameters;
    private readonly Dictionary<string, object?> _snapshots;
    private readonly List<IUndoEntry> _stack;
    private readonly List<Action> _cleanupActions;
    private readonly int _capacity;
    private readonly ConfigToastOptions? _toast;
    private readonly Dictionary<string, NumericEditSession> _activeNumericEdits;
    private readonly string? _pluginName;
    private List<ConfigChangeRecord>? _pendingBatch;
    private string? _batchLabel;
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
    /// <remarks>
    /// Toast notifications are disabled when using this constructor. Supply a
    /// <see cref="ConfigUndoOptions"/> instance with a non-<see langword="null"/>
    /// <see cref="ConfigUndoOptions.Toast"/> to enable them.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="store"/> has not been loaded.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="store"/> has been disposed.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 1.</exception>
    public ConfigUndoStack(ConfigStore<TConfig> store, int capacity = DefaultCapacity)
        : this(store, capacity, toast: null)
    {
    }

    /// <summary>
    /// Initializes a new undo stack that tracks parameter changes on the specified store,
    /// using the supplied <see cref="ConfigUndoOptions"/>.
    /// </summary>
    /// <param name="store">
    /// A loaded <see cref="ConfigStore{TConfig}"/>. Must be loaded and not disposed.
    /// </param>
    /// <param name="options">The undo-stack options that control capacity and toast behavior.</param>
    /// <exception cref="ArgumentNullException"><paramref name="store"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException"><paramref name="store"/> has not been loaded.</exception>
    /// <exception cref="ObjectDisposedException"><paramref name="store"/> has been disposed.</exception>
    public ConfigUndoStack(ConfigStore<TConfig> store, ConfigUndoOptions options)
        : this(store,
              options is not null ? options.Capacity : throw new ArgumentNullException(nameof(options)),
              options.Toast,
              options.PluginName)
    {
    }

    private ConfigUndoStack(ConfigStore<TConfig> store, int capacity, ConfigToastOptions? toast, string? pluginName = null)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (!store.IsLoaded)
            throw new InvalidOperationException("The config store must be loaded before creating an undo stack.");

        if (store.IsDisposed)
            throw new ObjectDisposedException(nameof(store), "The config store has been disposed.");

        if (capacity < 1)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");

        _capacity = capacity;
        _toast = toast;
        _pluginName = string.IsNullOrWhiteSpace(pluginName) ? null : pluginName;
#pragma warning disable IDE0028
        _stack = new(capacity);
#pragma warning restore IDE0028
        _snapshots = [];
        _activeNumericEdits = [];
        _cleanupActions = [];

        var target = (IConfigStoreCopyTarget<TConfig>)store;
        _parameters = target.Parameters;

        SubscribeToParameters();
        ApplyAutoBatchWrapping();
        UndoShortcutCoordinator.Register(this);
    }

    /// <summary>
    /// Gets a value indicating whether the stack contains at least one record that can be undone.
    /// </summary>
    public bool CanUndo => !_disposed && _stack.Count > 0;

    /// <inheritdoc/>
    long IUndoStackHandle.TopEntryTimestamp => _stack.Count > 0 ? _stack[^1].Timestamp : 0;

    /// <summary>
    /// Gets the number of undo entries currently on the stack.
    /// </summary>
    /// <remarks>
    /// A batch entry counts as one regardless of how many individual parameter changes it contains.
    /// </remarks>
    public int Count => _stack.Count;

    /// <summary>
    /// Gets a value indicating whether a batch is currently active.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> between a <see cref="BeginBatch"/> and <see cref="EndBatch"/> call.
    /// </remarks>
    public bool IsBatchActive => _pendingBatch is not null;

    /// <summary>
    /// Returns the most recent single-parameter change record without removing it, or
    /// <see langword="null"/> if the stack is empty or the top entry is a batch.
    /// </summary>
    /// <returns>The topmost <see cref="ConfigChangeRecord"/>, or <see langword="null"/>.</returns>
    /// <remarks>
    /// For batch-aware inspection, use <see cref="PeekEntry"/> instead.
    /// </remarks>
    public ConfigChangeRecord? Peek()
    {
        if (_stack.Count == 0) return null;
        return _stack[^1] as ConfigChangeRecord;
    }

    /// <summary>
    /// Returns the most recent undo entry without removing it, or <see langword="null"/>
    /// if the stack is empty. The entry may be a single <see cref="ConfigChangeRecord"/>
    /// or a batch <see cref="ConfigBatchChangeRecord"/>.
    /// </summary>
    /// <returns>The topmost <see cref="IUndoEntry"/>, or <see langword="null"/>.</returns>
    internal IUndoEntry? PeekEntry()
    {
        if (_stack.Count == 0) return null;
        return _stack[^1];
    }

    /// <summary>
    /// Opens a batch scope so that subsequent parameter changes are collected into a single
    /// composite undo entry instead of being pushed individually.
    /// </summary>
    /// <param name="label">
    /// A human-readable label for the batch (e.g. <c>"Reset All"</c>, <c>"Preset: MyPreset"</c>).
    /// Used in toast notifications when the batch is undone.
    /// </param>
    /// <remarks>
    /// <para>
    /// While a batch is active, individual <see cref="IParameter.ValueChanged"/> events still
    /// fire normally — other subsystems such as the save controller are not affected. Only the
    /// undo stack's recording behavior changes.
    /// </para>
    /// <para>
    /// Call <see cref="EndBatch"/> to finalize the batch. If no parameter values actually changed
    /// during the batch, the batch is discarded silently.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">A batch is already active.</exception>
    /// <exception cref="ArgumentException"><paramref name="label"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="ObjectDisposedException">This undo stack has been disposed.</exception>
    public void BeginBatch(string label)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_pendingBatch is not null)
            throw new InvalidOperationException("A batch is already active. Nested batches are not supported.");
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Batch label cannot be null, empty, or whitespace.", nameof(label));

        _batchLabel = label;
        _pendingBatch = [];
    }

    /// <summary>
    /// Closes the current batch scope. If one or more parameter values changed during the
    /// batch, they are pushed as a single composite entry onto the undo stack. If no values
    /// changed, the batch is discarded silently.
    /// </summary>
    /// <exception cref="InvalidOperationException">No batch is currently active.</exception>
    /// <exception cref="ObjectDisposedException">This undo stack has been disposed.</exception>
    public void EndBatch()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_pendingBatch is null)
            throw new InvalidOperationException("No batch is currently active.");

        var records = _pendingBatch;
        var label = _batchLabel!;
        _pendingBatch = null;
        _batchLabel = null;

        if (records.Count == 0)
            return;

        var batchEntry = new ConfigBatchChangeRecord(label, records);

        if (_stack.Count >= _capacity)
            _stack.RemoveAt(0);

        _stack.Add(batchEntry);
        UndoShortcutCoordinator.SetActive(this);
    }

    /// <summary>
    /// Returns a new <see cref="Action"/> that wraps <paramref name="action"/> with
    /// <see cref="BeginBatch"/> / <see cref="EndBatch"/> boundaries.
    /// </summary>
    /// <param name="label">
    /// The batch label used when the wrapped action is undone (e.g. <c>"Reset General"</c>).
    /// </param>
    /// <param name="action">The action to wrap. Typically a per-category reset delegate.</param>
    /// <returns>
    /// A new delegate that, when invoked, opens a batch, executes <paramref name="action"/>,
    /// and closes the batch so all parameter changes within the action are recorded as one
    /// atomic undo entry.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the recommended pattern for adding batch-undo support to reset buttons and other
    /// multi-parameter operations without coupling the config type to the undo stack:
    /// </para>
    /// <code>
    /// // After the undo stack is available (e.g. from ConfigSection.UndoStack):
    /// config.General.ResetGeneral.Value = undoStack.WrapWithBatch("Reset General", config.General.ResetGeneral.Value);
    /// </code>
    /// <para>
    /// The <see cref="EndBatch"/> call is in a <see langword="finally"/> block so the batch
    /// scope is always closed even if <paramref name="action"/> throws.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException"><paramref name="label"/> is <see langword="null"/>, empty, or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <see langword="null"/>.</exception>
    public Action WrapWithBatch(string label, Action action)
    {
        if (string.IsNullOrWhiteSpace(label))
            throw new ArgumentException("Batch label cannot be null, empty, or whitespace.", nameof(label));
        ArgumentNullException.ThrowIfNull(action);

        return () =>
        {
            BeginBatch(label);
            try
            {
                action();
            }
            finally
            {
                EndBatch();
            }
        };
    }

    /// <summary>
    /// Undoes the most recent undo entry by restoring its parameter value(s).
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the top entry is a single <see cref="ConfigChangeRecord"/>, one parameter is restored.
    /// When the top entry is a <see cref="ConfigBatchChangeRecord"/>, all parameters in the batch
    /// are restored atomically.
    /// </para>
    /// <para>
    /// The undo operation suppresses change recording so the restoration itself does not
    /// push a new entry onto the stack. When toast notifications are enabled, a toast is
    /// displayed via <see cref="ToastQueue.Push(string, ToastLevel, TimeSpan?)"/>.
    /// </para>
    /// </remarks>
    /// <returns>
    /// <see langword="true"/> if a change was successfully undone;
    /// <see langword="false"/> if the stack is empty, disposed, or the parameter is no longer registered.
    /// </returns>
    public bool TryUndo()
    {
        if (_disposed || _stack.Count == 0) return false;

        var entry = _stack[^1];
        _stack.RemoveAt(_stack.Count - 1);

        if (entry is ConfigBatchChangeRecord batch)
            return TryUndoBatch(batch);

        if (entry is ConfigChangeRecord record)
            return TryUndoSingle(record);

        return false;
    }

    /// <summary>
    /// Removes all entries from the stack and discards any pending batch.
    /// </summary>
    public void Clear()
    {
        _stack.Clear();
        _pendingBatch = null;
        _batchLabel = null;
    }

    void INumericEditSink.BeginNumericEdit(IParameter parameter)
    {
        if (_disposed)
            return;

        ArgumentNullException.ThrowIfNull(parameter);
        var key = parameter.Key;
        if (_activeNumericEdits.ContainsKey(key))
            return;

        if (!_snapshots.TryGetValue(key, out var initialValue))
            initialValue = parameter.GetValue();

        _activeNumericEdits[key] = new NumericEditSession(initialValue);
    }

    void INumericEditSink.EndNumericEdit(IParameter parameter)
    {
        if (_disposed)
            return;

        ArgumentNullException.ThrowIfNull(parameter);
        var key = parameter.Key;
        if (!_activeNumericEdits.TryGetValue(key, out var session))
            return;

        _activeNumericEdits.Remove(key);
        var currentValue = parameter.GetValue();
        _snapshots[key] = currentValue;
        if (Equals(session.InitialValue, currentValue))
            return;

        AddRecord(key, parameter, session.InitialValue, currentValue);
    }

    /// <summary>
    /// Detaches all parameter event subscriptions and clears internal state.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        UndoShortcutCoordinator.Unregister(this);

        for (var i = 0; i < _cleanupActions.Count; i++)
            _cleanupActions[i]();

        _cleanupActions.Clear();
        _activeNumericEdits.Clear();
        _stack.Clear();
        _snapshots.Clear();
        _pendingBatch = null;
        _batchLabel = null;
    }

    private bool TryUndoSingle(ConfigChangeRecord record)
    {
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

        if (_toast is not null)
            ToastQueue.Push(BuildToastMessage($"Undo: {record.DisplayLabel}"), ToastLevel.Info, _toast.Duration);
        return true;
    }

    private bool TryUndoBatch(ConfigBatchChangeRecord batch)
    {
        _suppressRecording = true;
        try
        {
            var restored = 0;
            // Restore in reverse order so the earliest change is restored last,
            // leaving snapshots in the correct pre-batch state.
            for (var i = batch.Records.Count - 1; i >= 0; i--)
            {
                var record = batch.Records[i];
                if (!_parameters.TryGetValue(record.ParameterKey, out var param))
                    continue;

                param.SetValue(record.OldValue);
                _snapshots[record.ParameterKey] = record.OldValue;
                restored++;
            }

            if (restored == 0)
                return false;
        }
        finally
        {
            _suppressRecording = false;
        }

        if (_toast is not null)
            ToastQueue.Push(BuildToastMessage($"Undo: {batch.BatchLabel} ({batch.Records.Count} parameters)"), ToastLevel.Info, _toast.Duration);
        return true;
    }

    private string BuildToastMessage(string message) =>
        _pluginName is not null ? $"[{_pluginName}] {message}" : message;

    private void SubscribeToParameters()
    {
        foreach (var kvp in _parameters)
        {
            var param = kvp.Value;

            if (typeof(Delegate).IsAssignableFrom(param.ValueType))
                continue;

            var key = param.Key;
            _snapshots[key] = param.GetValue();

            void handler() => OnParameterChanged(key, param);
            param.ValueChanged += handler;
            _cleanupActions.Add(() => param.ValueChanged -= handler);
        }
    }

    /// <summary>
    /// Discovers delegate parameters marked with <see cref="Attributes.UmbraBatchUndoAttribute"/> and
    /// replaces their current <see cref="Action"/> value with a batch-wrapped version via
    /// <see cref="WrapWithBatch"/>. Uses <see cref="IParameter.SetValueWithoutNotify"/> so no
    /// <see cref="IParameter.ValueChanged"/> event is raised.
    /// </summary>
    private void ApplyAutoBatchWrapping()
    {
        foreach (var kvp in _parameters)
        {
            var param = kvp.Value;

            if (!typeof(Delegate).IsAssignableFrom(param.ValueType))
                continue;

            var label = param.Metadata.BatchUndoLabel;
            if (label is null)
                continue;

            if (param.GetValue() is not Action action)
                continue;

            param.SetValueWithoutNotify(WrapWithBatch(label, action));
        }
    }

    private void OnParameterChanged(string key, IParameter param)
    {
        if (_suppressRecording) return;

        var old = _snapshots[key];
        var current = param.GetValue();
        _snapshots[key] = current;

        if (_activeNumericEdits.ContainsKey(key))
            return;

        AddRecord(key, param, old, current);
    }

    private void AddRecord(string key, IParameter param, object? oldValue, object? newValue)
    {
        var label = param.Metadata.ResolvedLabel;
        if (string.IsNullOrEmpty(label))
            label = key;

        var record = new ConfigChangeRecord(key, label, oldValue, newValue, Stopwatch.GetTimestamp());

        if (_pendingBatch is not null)
        {
            _pendingBatch.Add(record);
            return;
        }

        if (_stack.Count >= _capacity)
            _stack.RemoveAt(0);

        _stack.Add(record);
        UndoShortcutCoordinator.SetActive(this);
    }
}
