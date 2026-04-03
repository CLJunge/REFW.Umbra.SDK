using Umbra.Config;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Owns the shared hotkey-capture state machine used by the hotkey drawers.
/// </summary>
/// <remarks>
/// This controller centralizes capture-mode entry, cancellation, captured-key application, and synchronization with <see cref="HotkeyCaptureState"/> so the concrete drawers only decide how current and waiting text should be presented.
/// </remarks>
internal sealed class HotkeyCaptureController : IDisposable
{
    private bool _waiting;
    private bool _disposed;
    private readonly IHotkeyDrawerRenderer _renderer;
    private readonly IHotkeyInputSource _inputSource;

    /// <summary>
    /// Initializes a new <see cref="HotkeyCaptureController"/>.
    /// </summary>
    /// <param name="renderer">The renderer used for text, inline layout, and button operations.</param>
    /// <param name="inputSource">The input source used for key capture.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> or <paramref name="inputSource"/> is <see langword="null"/>.</exception>
    internal HotkeyCaptureController(IHotkeyDrawerRenderer renderer, IHotkeyInputSource inputSource)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(inputSource);
        _renderer = renderer;
        _inputSource = inputSource;
    }

    /// <summary>
    /// Gets a value indicating whether this controller has been disposed.
    /// </summary>
    internal bool IsDisposed => _disposed;

    /// <summary>
    /// Renders the hotkey-capture UI for one frame and applies any captured key value.
    /// </summary>
    /// <param name="parameter">The hotkey parameter being edited.</param>
    /// <param name="currentValueText">The text shown while capture mode is inactive.</param>
    /// <param name="waitingText">The text shown while capture mode is waiting for a key press.</param>
    internal void Draw(Parameter<int> parameter, string currentValueText, string waitingText)
    {
        var value = parameter.Value;
        var previousValue = value;
        var wasWaiting = _waiting;

        var otherWaiting = HotkeyCaptureState.WaitingCount > (wasWaiting ? 1 : 0);

        if (_waiting)
        {
            _renderer.Text(waitingText);
            _renderer.SameLine();
            if (_renderer.Button($"Cancel##{parameter.Key}"))
            {
                _waiting = false;
            }
            else if (_inputSource.TryCaptureKeyboardKey(out var captured))
            {
                value = captured;
                _waiting = false;
            }
        }
        else
        {
            _renderer.Text(currentValueText);
            _renderer.SameLine();
            if (_renderer.Button($"Change##{parameter.Key}") && !otherWaiting)
                _waiting = true;
        }

        if (_waiting != wasWaiting)
            HotkeyCaptureState.WaitingCount += _waiting ? 1 : -1;

        if (value != previousValue)
            parameter.Value = value;
    }

    /// <summary>
    /// Releases this controller's contribution to the shared capture counter.
    /// </summary>
    /// <remarks>
    /// Repeated calls after the first one do nothing.
    /// </remarks>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_waiting)
        {
            HotkeyCaptureState.WaitingCount--;
            _waiting = false;
        }

        GC.SuppressFinalize(this);
    }
}
