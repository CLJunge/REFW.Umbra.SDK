using System.Diagnostics;
using Umbra.Logging;

namespace Umbra.Config;

/// <summary>
/// Drives automatic, change-triggered persistence for a <see cref="SettingsStore{TConfig}"/>.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Behaviour</strong><br/>
/// Changes to non-numeric parameters (booleans, strings, enums) are saved on the next
/// <see cref="Tick"/> call when no numeric debounce is currently pending.<br/>
/// Changes to numeric parameters (<see cref="int"/>, <see cref="float"/>, <see cref="double"/>)
/// are coalesced: the save is deferred until <see cref="DebounceWindow"/> has elapsed since
/// the last change, so rapid slider interaction produces only one disk write instead of one
/// per frame. If a non-numeric change arrives while a numeric debounce is already pending,
/// both changes are flushed together when the debounced save fires.
/// </para>
/// <para>
/// <strong>Ordering requirement</strong><br/>
/// Construct <see cref="DeferredSaveController{TConfig}"/> <em>after</em> calling
    /// <see cref="SettingsStore{TConfig}.Load"/>. The constructor validates this requirement and
    /// throws <see cref="InvalidOperationException"/> when the supplied store is not yet loaded.
/// </para>
/// <para>
/// <strong>Per-frame driving</strong><br/>
/// Call <see cref="Tick"/> once per frame from an <c>ImGuiDrawUI</c> callback or equivalent
/// game-loop hook. <see cref="Tick"/> is a lightweight no-op when there is nothing pending,
/// so it is safe to call unconditionally every frame.
/// </para>
/// <para>
/// <strong>Unload sequence</strong><br/>
/// <see cref="Dispose"/> automatically calls <see cref="Flush"/> before unregistering
/// listeners, so pending changes are not lost if the plugin unloads while a debounce is active.
/// Call <see cref="Flush"/> explicitly only when you need the save to happen earlier in the
    /// unload sequence or before some other operation. The preferred order is still to dispose this
    /// instance <em>before</em> or <em>alongside</em> the owning <see cref="SettingsStore{TConfig}"/>
    /// so any pending debounced write can still be persisted. If the store has already been
    /// disposed, controller disposal becomes a safe no-op cleanup path and skips listener removal
    /// because the store has already torn those subscriptions down.
/// </para>
/// <para>
/// <strong>Typical lifecycle</strong>
/// <code>
/// // Entry point — always after Load():
/// _store          = new SettingsStore&lt;PluginConfig&gt;(configPath);
/// var config      = _store.Load();
/// _saveController = new DeferredSaveController&lt;PluginConfig&gt;(_store);
///
/// // ImGuiDrawUI callback — once per frame:
/// _saveController.Tick();
///
/// // Exit point — Dispose flushes any pending write before removing listeners:
/// _saveController.Dispose();
/// _saveController = null;
/// _store.Save();   // belt-and-suspenders final save
/// _store.Dispose();
/// _store = null;
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type, matching the wrapped <see cref="SettingsStore{TConfig}"/>.
/// </typeparam>
public sealed class DeferredSaveController<TConfig> : IDisposable where TConfig : class, new()
{
    /// <summary>Gets the cooldown after the last numeric change before writing to disk.</summary>
    /// <remarks>
    /// The timer is reset on every numeric parameter change, so the save only fires once
    /// the user stops interacting with a slider for this duration.
    /// Defaults to 1 second when not supplied at construction.
    /// </remarks>
    public TimeSpan DebounceWindow { get; }

    private readonly SettingsStore<TConfig> _store;

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
    /// <see cref="SettingsStore{TConfig}.Load"/> before this constructor is called.</strong>
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
    /// Thrown when <paramref name="store"/> has not yet been loaded via <see cref="SettingsStore{TConfig}.Load"/>.
    /// </exception>
    public DeferredSaveController(SettingsStore<TConfig> store, TimeSpan? debounceWindow = null)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (store.IsDisposed)
            throw new ObjectDisposedException(nameof(store),
                $"DeferredSaveController<{typeof(TConfig).Name}> cannot attach to a disposed SettingsStore.");
        if (!store.IsLoaded)
            throw new InvalidOperationException(
                $"DeferredSaveController<{typeof(TConfig).Name}> requires a SettingsStore that has already completed Load().");

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
    /// Evaluates pending saves and flushes to disk when appropriate.
    /// Must be called once per frame, typically from an <c>ImGuiDrawUI</c> callback.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is a lightweight no-op when there are no pending changes, so it is safe
    /// to call unconditionally every frame.
    /// </para>
    /// <para>
    /// Decision logic per call:
    /// <list type="bullet">
    /// <item>If nothing is pending, returns immediately.</item>
    /// <item>If a non-numeric change is pending (and no numeric change is also pending),
    ///   calls <see cref="Flush"/> immediately.</item>
    /// <item>If a numeric change is pending, waits until <see cref="DebounceWindow"/> has
    ///   elapsed since the last numeric change before calling <see cref="Flush"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// After disposal this method is a permanent no-op.
    /// </para>
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
            // Non-slider change (bool, string, enum, …) — save immediately.
            Flush();
        }
    }

    /// <summary>
    /// Forces an immediate save and clears all pending state, regardless of the debounce timer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this when you need a pending save to happen immediately rather than waiting for the
    /// debounce window or the eventual <see cref="Dispose"/> call.
    /// </para>
    /// <para>
    /// If the wrapped <see cref="SettingsStore{TConfig}"/> has already been disposed, any pending
    /// save is discarded because there is no live store left to persist through.
    /// </para>
    /// <para>
    /// After disposal this method is a permanent no-op.
    /// </para>
    /// </remarks>
    public void Flush()
    {
        if (_disposed) return;
        if (_store.IsDisposed)
        {
            if (_anyPending)
                Logger.Warning(
                    $"DeferredSaveController<{typeof(TConfig).Name}>: dropping pending changes because the SettingsStore was already disposed.");
            ClearPendingState();
            return;
        }

        Logger.Info($"DeferredSaveController<{typeof(TConfig).Name}>: flushing pending changes to disk.");
        _store.Save();
        ClearPendingState();
    }

    /// <summary>
    /// Unregisters all parameter-change listeners attached at construction time and marks
    /// this instance as disposed. After disposal, <see cref="Tick"/> and <see cref="Flush"/>
    /// become permanent no-ops.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Dispose"/> calls <see cref="Flush"/> before unregistering listeners, so any
    /// debounced write still pending at unload time is persisted automatically.
    /// </para>
    /// <para>
    /// The preferred unload order remains to dispose this instance before or alongside the owning
    /// <see cref="SettingsStore{TConfig}"/> so pending debounced writes can still be flushed. If the
    /// store has already been disposed, this method does not throw; it skips listener removal because
    /// the store has already removed all registered subscriptions during its own disposal.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        Flush();  // guarantee no pending write is dropped before listeners are removed
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
