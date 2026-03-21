using System.Diagnostics;
using Umbra.SDK.Logging;

namespace Umbra.SDK.Config;

/// <summary>
/// Drives automatic, change-triggered persistence for a <see cref="SettingsStore{TConfig}"/>.
/// </summary>
/// <remarks>
/// <para>
/// Changes to non-numeric parameters (booleans, strings, enums) are saved on the very next
/// <see cref="Tick"/> call — effectively immediate from the user's perspective.
/// </para>
/// <para>
/// Changes to numeric parameters (<see cref="int"/>, <see cref="float"/>, <see cref="double"/>)
/// are coalesced: the save is deferred until <see cref="DebounceWindow"/> has elapsed since
/// the last change, so rapid slider interaction produces only one disk write.
/// </para>
/// <para>
/// Call <see cref="Tick"/> once per frame from an <c>ImGuiDrawUI</c> callback to drive the
/// save logic. Call <see cref="Flush"/> before plugin unload to guarantee any pending
/// changes are written. Dispose alongside the owning <see cref="SettingsStore{TConfig}"/>
/// to unregister all event subscriptions.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">
/// The configuration class type, matching the wrapped <see cref="SettingsStore{TConfig}"/>.
/// </typeparam>
public sealed class DeferredSaveController<TConfig> : IDisposable where TConfig : class, new()
{
    /// <summary>Gets the cooldown after the last numeric change before writing to disk.</summary>
    public TimeSpan DebounceWindow { get; }

    private readonly SettingsStore<TConfig> _store;

    // Stored as fields so the exact delegate instances can be passed to RemoveListenerFromAll.
    private readonly Action _onAnyChanged;
    private readonly Action<int?, int?> _onIntChanged;
    private readonly Action<float?, float?> _onFloatChanged;
    private readonly Action<double?, double?> _onDoubleChanged;

    private bool _anyPending;
    private bool _sliderPending;
    private long _sliderChangedAt;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="DeferredSaveController{TConfig}"/> and begins listening
    /// for parameter changes on <paramref name="store"/>.
    /// </summary>
    /// <param name="store">The settings store to drive saves for.</param>
    /// <param name="debounceWindow">
    /// How long to wait after the last numeric parameter change before writing to disk.
    /// Defaults to 1 second when <see langword="null"/>.
    /// </param>
    public DeferredSaveController(SettingsStore<TConfig> store, TimeSpan? debounceWindow = null)
    {
        _store = store;
        DebounceWindow = debounceWindow ?? TimeSpan.FromSeconds(1);

        // Typed numeric listeners set _sliderPending; the untyped listener sets _anyPending.
        // Both fire synchronously within Parameter<T>.SetValue and both complete before the
        // next Tick() call, so listener registration order does not affect correctness.
        _onIntChanged = (_, _) => MarkSliderDirty();
        _onFloatChanged = (_, _) => MarkSliderDirty();
        _onDoubleChanged = (_, _) => MarkSliderDirty();
        _onAnyChanged = () => _anyPending = true;

        _store.AddListenerToAll(_onIntChanged);
        _store.AddListenerToAll(_onFloatChanged);
        _store.AddListenerToAll(_onDoubleChanged);
        _store.AddListenerToAll(_onAnyChanged);
    }

    /// <summary>
    /// Evaluates pending saves and flushes to disk when appropriate.
    /// Must be called once per frame, typically from an <c>ImGuiDrawUI</c> callback.
    /// </summary>
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
    /// Call this before plugin unload to ensure no changes are lost.
    /// </summary>
    public void Flush()
    {
        if (_disposed) return;
        Logger.Info($"DeferredSaveController<{typeof(TConfig).Name}>: flushing pending changes to disk.");
        _store.Save();
        _anyPending = false;
        _sliderPending = false;
    }

    /// <summary>
    /// Unregisters all parameter-change listeners attached at construction time and marks
    /// this instance as disposed. After disposal, <see cref="Tick"/> and <see cref="Flush"/>
    /// become no-ops.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Always dispose this instance before or alongside the owning <see cref="SettingsStore{TConfig}"/>
    /// to ensure listener cleanup is coordinated correctly.
    /// </para>
    /// <para>
    /// Calls <see cref="Flush"/> before releasing resources so any debounced write that is
    /// still pending at unload time is not silently dropped.
    /// </para>
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        Flush();  // guarantee no pending write is dropped before listeners are removed
        _disposed = true;

        _store.RemoveListenerFromAll(_onAnyChanged);
        _store.RemoveListenerFromAll(_onIntChanged);
        _store.RemoveListenerFromAll(_onFloatChanged);
        _store.RemoveListenerFromAll(_onDoubleChanged);

        GC.SuppressFinalize(this);
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
}
