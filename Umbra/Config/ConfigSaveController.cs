using Umbra.Logging;
using Umbra.UI.Config;

namespace Umbra.Config;

/// <summary>
/// Persists parameter changes for a loaded <see cref="IConfigStore{TConfig}"/> using instant,
/// event-driven saves instead of time-based debouncing.
/// </summary>
/// <remarks>
/// <para>
/// Non-numeric, non-text parameter changes trigger an immediate <see cref="IConfigStore{TConfig}.Save"/>
/// call from within the change handler. Numeric parameter changes during an active slider or
/// drag interaction are deferred until <see cref="INumericEditSink.EndNumericEdit"/> fires,
/// then saved instantly. Text parameter changes during an active text input are deferred until
/// <see cref="ITextEditSink.EndTextEdit"/> fires, then saved instantly. Changes that occur
/// outside any active interaction (e.g. programmatic value assignments) are saved immediately.
/// </para>
/// <para>
/// Construct this controller only after <see cref="IConfigStore{TConfig}.Load"/> has completed.
/// Call <see cref="Dispose"/> before or alongside the owning store so pending writes can still
/// be flushed.
/// </para>
/// </remarks>
/// <typeparam name="TConfig">The configuration type managed by the wrapped store.</typeparam>
public sealed class ConfigSaveController<TConfig> : IDisposable, INumericEditSink, ITextEditSink
    where TConfig : class, new()
{
    private readonly IConfigStore<TConfig> _store;

    // Stored as a field so the exact delegate instance can be passed to RemoveListenerFromAll.
    private readonly Action _onParameterChanged;

    private bool _numericEditActive;
    private bool _textEditActive;
    private bool _pendingSave;
    private bool _disposed;

    /// <summary>
    /// Initializes a new <see cref="ConfigSaveController{TConfig}"/> and begins listening
    /// for parameter changes on <paramref name="store"/>.
    /// </summary>
    /// <param name="store">
    /// The config store to drive saves for. <strong>Must have already been loaded via
    /// <see cref="IConfigStore{TConfig}.Load"/> before this constructor is called.</strong>
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="store"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when <paramref name="store"/> has already been disposed.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <paramref name="store"/> has not yet been loaded via <see cref="IConfigStore{TConfig}.Load"/>.
    /// </exception>
    public ConfigSaveController(IConfigStore<TConfig> store)
    {
        ArgumentNullException.ThrowIfNull(store);
        if (store.IsDisposed)
            throw new ObjectDisposedException(nameof(store),
                $"ConfigSaveController<{typeof(TConfig).Name}> cannot attach to a disposed config store.");
        if (!store.IsLoaded)
            throw new InvalidOperationException(
                $"ConfigSaveController<{typeof(TConfig).Name}> requires a config store that has already completed Load().");

        _store = store;
        _onParameterChanged = OnParameterChanged;
        _store.AddListenerToAll(_onParameterChanged);
    }

    /// <summary>
    /// Forces an immediate save when changes are pending, regardless of numeric edit state.
    /// </summary>
    /// <remarks>
    /// When no changes are pending, this method is a no-op. After <see cref="Dispose"/> it
    /// becomes a permanent no-op.
    /// </remarks>
    public void Flush()
    {
        if (_disposed || !_pendingSave) return;
        Save();
    }

    /// <summary>
    /// Flushes pending changes, unregisters the controller's listener, and marks the controller
    /// as disposed.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing. If the wrapped store has already been
    /// disposed, listener removal is skipped because the store has already torn down its
    /// subscriptions.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        Flush();
        _disposed = true;

        if (!_store.IsDisposed)
            _store.RemoveListenerFromAll(_onParameterChanged);

        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    void INumericEditSink.BeginNumericEdit(IParameter parameter)
    {
        if (_disposed) return;
        _numericEditActive = true;
    }

    /// <inheritdoc/>
    void INumericEditSink.EndNumericEdit(IParameter parameter)
    {
        if (_disposed) return;
        _numericEditActive = false;
        if (_pendingSave && !_textEditActive)
            Save();
    }

    /// <inheritdoc/>
    void ITextEditSink.BeginTextEdit(IParameter parameter)
    {
        if (_disposed) return;
        _textEditActive = true;
    }

    /// <inheritdoc/>
    void ITextEditSink.EndTextEdit(IParameter parameter)
    {
        if (_disposed) return;
        _textEditActive = false;
        if (_pendingSave && !_numericEditActive)
            Save();
    }

    /// <summary>
    /// Handles a parameter change notification from the wrapped store.
    /// </summary>
    private void OnParameterChanged()
    {
        _pendingSave = true;
        if (!_numericEditActive && !_textEditActive)
            Save();
    }

    /// <summary>
    /// Persists current parameter values to disk and clears the pending-save flag.
    /// </summary>
    private void Save()
    {
        if (_disposed) return;
        if (_store.IsDisposed)
        {
            if (_pendingSave)
                Logger.Warning(
                    $"ConfigSaveController<{typeof(TConfig).Name}>: dropping pending changes because the config store was already disposed.");
            _pendingSave = false;
            return;
        }

#if !RELEASE
        Logger.Debug($"ConfigSaveController<{typeof(TConfig).Name}>: saving changes to disk.");
#endif
        _store.Save();
        _pendingSave = false;
    }
}
