using System.Diagnostics;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Coalesces change-triggered saves for a loaded <see cref="IConfigStore{TConfig}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Non-numeric parameter changes are saved on the next <see cref="Tick"/> call when no numeric debounce is pending. Numeric parameter changes for <see cref="int"/>, <see cref="float"/>, and <see cref="double"/> are coalesced until <see cref="DebounceWindow"/> has elapsed since the last numeric change.
/// </para>
/// <para>
/// Construct this controller only after <see cref="IConfigStore{TConfig}.Load"/> has completed. Call <see cref="Tick"/> once per frame, and dispose the controller before or alongside the owning store so pending writes can still be flushed.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration type managed by the wrapped store.</typeparam>
public sealed class DeferredSaveController<TConfig> : IDisposable where TConfig : class, new()
{
    /// <summary>
    /// Gets the cooldown applied after the last numeric change before the controller writes to disk.
    /// </summary>
    /// <value>The debounce window used for numeric parameter changes.</value>
    public TimeSpan DebounceWindow { get; }

    private readonly IConfigStore<TConfig> _store;

    // Stored as fields so the exact delegate instances can be passed to RemoveListenerFromAll.
    private readonly Action _onAnyChanged;
    private readonly Action _onNumericChanged;

    private bool _anyPending;
    private bool _sliderPending;
    private long _sliderChangedAt;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="DeferredSaveController{TConfig}"/> and begins listening
    /// for parameter changes on <paramref name="store"/>.
    /// </summary>
    /// <param name="store">
    /// The settings store to drive saves for. <strong>Must have already been loaded via
    /// <see cref="IConfigStore{TConfig}.Load"/> before this constructor is called.</strong>
    /// </param>
    /// <param name="debounceWindow">
    /// How long to wait after the last numeric parameter change before writing to disk.
    /// The timer restarts on every subsequent numeric change, so the save only fires once
    /// the user stops interacting with sliders for this duration.
    /// Defaults to 1 second when <see langword="null"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when <paramref name="store"/> has already been disposed.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="store"/> has not yet been loaded via <see cref="IConfigStore{TConfig}.Load"/>.
    /// </exception>
    public DeferredSaveController(IConfigStore<TConfig> store, TimeSpan? debounceWindow = null)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (store.IsDisposed)
            throw new ObjectDisposedException(nameof(store),
                $"DeferredSaveController<{typeof(TConfig).Name}> cannot attach to a disposed settings store.");
        if (!store.IsLoaded)
            throw new InvalidOperationException(
                $"DeferredSaveController<{typeof(TConfig).Name}> requires a settings store that has already completed Load().");

        _store = store;
        DebounceWindow = debounceWindow ?? TimeSpan.FromSeconds(1);

        // _onNumericChanged fires only for int/float/double parameters and starts the debounce
        // timer. _onAnyChanged fires for every parameter and marks the pending flag. Both fire
        // synchronously within Parameter<T>.SetValue before the next Tick() call, so registration
        // order does not affect correctness.
        _onNumericChanged = MarkSliderDirty;
        _onAnyChanged = () => _anyPending = true;

        _store.AddListenerToAll(IsNumericParameter, _onNumericChanged);
        _store.AddListenerToAll(_onAnyChanged);
    }

    /// <summary>
    /// Evaluates pending changes and flushes to disk when the save policy allows it.
    /// </summary>
    /// <remarks>
    /// This method is a lightweight no-op when no changes are pending. After <see cref="Dispose"/> it becomes a permanent no-op.
    /// </remarks>
    public void Tick()
    {
        if (_disposed || !_anyPending) return;

        if (_sliderPending)
        {
            if (Stopwatch.GetElapsedTime(_sliderChangedAt) >= DebounceWindow)
                Flush();
        }
        else
        {
            Flush();
        }
    }

    /// <summary>
    /// Forces an immediate save attempt for the currently pending changes and clears the pending state.
    /// </summary>
    /// <remarks>
    /// If the wrapped store has already been disposed, pending changes are dropped because there is no live store left to persist through. After <see cref="Dispose"/> this method is a permanent no-op.
    /// </remarks>
    public void Flush()
    {
        if (_disposed) return;
        if (_store.IsDisposed)
        {
            if (_anyPending)
                Logger.Warning(
                    $"DeferredSaveController<{typeof(TConfig).Name}>: dropping pending changes because the settings store was already disposed.");
            ClearPendingState();
            return;
        }

        Logger.Info($"DeferredSaveController<{typeof(TConfig).Name}>: flushing pending changes to disk.");
        _store.Save();
        ClearPendingState();
    }

    /// <summary>
    /// Flushes pending changes, unregisters the controller's listeners, and marks the controller as disposed.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing. If the wrapped store has already been disposed, listener removal is skipped because the store has already torn down its subscriptions.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        Flush();
        _disposed = true;

        if (!_store.IsDisposed)
        {
            _store.RemoveListenerFromAll(_onAnyChanged);
            _store.RemoveListenerFromAll(IsNumericParameter, _onNumericChanged);
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Clears all tracked pending-save state after a successful flush or a forced drop.
    /// </summary>
    private void ClearPendingState()
    {
        _anyPending = false;
        _sliderPending = false;
    }

    /// <summary>
    /// Records the current timestamp as the moment of the last numeric parameter change
    /// and marks the slider save as pending, which starts (or restarts) the debounce timer.
    /// </summary>
    private void MarkSliderDirty()
    {
        _sliderChangedAt = Stopwatch.GetTimestamp();
        _sliderPending = true;
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="p"/> holds a numeric value type
    /// (<see cref="int"/>, <see cref="float"/>, or <see cref="double"/>) whose changes should
    /// be debounced rather than saved immediately.
    /// </summary>
    private static bool IsNumericParameter(IParameter p)
        => p.ValueType == typeof(int) || p.ValueType == typeof(float) || p.ValueType == typeof(double);
}
