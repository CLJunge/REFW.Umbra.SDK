using Umbra.Config;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// An <see cref="IParameterDrawer"/> implementation that renders a hotkey-capture control
/// for a <see cref="Parameter{T}"/> of type <see cref="int"/>, where the value represents
/// an <see cref="ImGuiKey"/> cast to <see cref="int"/>.
/// </summary>
/// <remarks>
/// At most one hotkey-capture drawer may be in capture mode at any given frame.
/// A shared static counter in <see cref="HotkeyCaptureState"/> enforces mutual exclusion
/// across all <see cref="HotkeyDrawer"/> and <see cref="TwoColumnHotkeyDrawer"/> instances
/// in the same assembly. <see cref="Dispose"/> must be called (via the owning
/// <see cref="Config.ConfigDrawer{TConfig}"/>) on plugin unload so that any in-progress capture
/// does not permanently block future captures.
/// The default constructor renders through ImGui and captures keys through
/// <see cref="Umbra.Input.KeyboardInput"/>. Unit tests can replace those dependencies through the
/// internal constructor so the state machine can be verified without a live runtime host.
/// </remarks>
public sealed class HotkeyDrawer : IParameterDrawer
{
    private bool _waiting;
    private bool _disposed;
    private readonly IHotkeyDrawerRenderer _renderer;
    private readonly IHotkeyInputSource _inputSource;

    /// <summary>
    /// Initializes a new <see cref="HotkeyDrawer"/> that renders through the active ImGui frame and
    /// captures keys through <see cref="Umbra.Input.KeyboardInput"/>.
    /// </summary>
    public HotkeyDrawer()
        : this(new ImGuiHotkeyDrawerRenderer(), new KeyboardHotkeyInputSource())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="HotkeyDrawer"/> with the specified renderer and keyboard input source.
    /// </summary>
    /// <param name="renderer">The renderer used for hotkey-drawer UI operations.</param>
    /// <param name="inputSource">The source used for key capture and key naming.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> or <paramref name="inputSource"/> is <see langword="null"/>.
    /// </exception>
    internal HotkeyDrawer(IHotkeyDrawerRenderer renderer, IHotkeyInputSource inputSource)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(inputSource);
        _renderer = renderer;
        _inputSource = inputSource;
    }

    /// <inheritdoc/>
    public void Draw(string label, IParameter parameter)
    {
        if (_disposed) return;

        if (parameter is not Parameter<int> p)
        {
            _renderer.TextDisabled($"{label}: (HotkeyDrawer requires Parameter<int>)");
            return;
        }

        var v = p.Value;
        var prev = v;
        var wasWaiting = _waiting;

        // Prevent multiple drawers from capturing input simultaneously.
        // HotkeyCaptureState.WaitingCount is shared with TwoColumnHotkeyDrawer.
        var otherWaiting = HotkeyCaptureState.WaitingCount > (wasWaiting ? 1 : 0);

        // Use the parameter key as the stable unique button ID so two parameters with the
        // same display label do not share an ImGui button ID within the same window.
        if (_waiting)
        {
            _renderer.Text($"{label}: Press any key...");
            _renderer.SameLine();
            if (_renderer.Button($"Cancel##{p.Key}"))
                _waiting = false;
            else if (_inputSource.TryCaptureKeyboardKey(out var captured))
            {
                v = captured;
                _waiting = false;
            }
        }
        else
        {
            _renderer.Text($"{label}: {_inputSource.GetKeyName(v)}");
            _renderer.SameLine();
            if (_renderer.Button($"Change##{p.Key}") && !otherWaiting)
                _waiting = true;
        }

        // Keep the shared counter in sync when this drawer's capture state changes.
        if (_waiting != wasWaiting)
            HotkeyCaptureState.WaitingCount += _waiting ? 1 : -1;

        if (v != prev) p.Value = v;

        var metadata = parameter.Metadata;
        if (metadata.Description is not null)
        {
            _renderer.SameLine();
            _renderer.DrawHelpMarker(metadata.Description);
        }
    }

    /// <summary>
    /// Releases this drawer's contribution to the shared capture counter in
    /// <see cref="HotkeyCaptureState"/>. Must be called when the owning
    /// <see cref="Config.ConfigDrawer{TConfig}"/> is disposed so that a mid-capture plugin unload
    /// does not permanently block future captures.
    /// </summary>
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
