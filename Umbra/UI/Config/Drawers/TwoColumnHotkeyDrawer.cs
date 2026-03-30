using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// An <see cref="ITwoColumnParameterDrawer"/> implementation that renders a hotkey-capture
/// control for a <see cref="Parameter{T}"/> of type <see cref="int"/> in the two-column layout,
/// where the value represents an <see cref="Hexa.NET.ImGui.ImGuiKey"/> cast to <see cref="int"/>.
/// </summary>
/// <remarks>
/// The framework renders the parameter label in the left column before calling
/// <see cref="Draw"/>; this drawer renders only the key-name text and the Change/Cancel
/// button in the right column. The label is intentionally omitted from the widget text
/// to avoid duplicating what is already shown in the left column.
/// <para>
/// Mutual exclusion with <see cref="HotkeyDrawer"/> is enforced through the shared
/// <see cref="HotkeyCaptureState.WaitingCount"/> counter: at most one hotkey-capture
/// drawer (of either type) may be in capture mode per frame. <see cref="Dispose"/> must be
/// called (via the owning <see cref="ConfigDrawer{TConfig}"/>) on plugin unload so that
/// any in-progress capture does not permanently block future captures.
/// The default constructor renders through the shared ImGui context and captures keys through
/// <see cref="Umbra.Input.KeyboardInput"/>. Unit tests can replace those dependencies through the
/// internal constructor so the state machine can be verified without a live runtime host.
/// </para>
/// </remarks>
public sealed class TwoColumnHotkeyDrawer : ITwoColumnParameterDrawer
{
    private bool _waiting;
    private bool _disposed;
    private readonly IHotkeyDrawerRenderer _renderer;
    private readonly IHotkeyInputSource _inputSource;

    /// <summary>
    /// Initializes a new <see cref="TwoColumnHotkeyDrawer"/> that renders through the shared active ImGui
    /// frame and captures keys through <see cref="Umbra.Input.KeyboardInput"/>.
    /// </summary>
    public TwoColumnHotkeyDrawer()
        : this(ImGuiConfigRenderContext.Instance, new KeyboardHotkeyInputSource())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="TwoColumnHotkeyDrawer"/> with the specified renderer and
    /// keyboard input source.
    /// </summary>
    /// <param name="renderer">The renderer used for hotkey-drawer UI operations.</param>
    /// <param name="inputSource">The source used for key capture and key naming.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="renderer"/> or <paramref name="inputSource"/> is <see langword="null"/>.
    /// </exception>
    internal TwoColumnHotkeyDrawer(IHotkeyDrawerRenderer renderer, IHotkeyInputSource inputSource)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(inputSource);
        _renderer = renderer;
        _inputSource = inputSource;
    }

    /// <inheritdoc/>
    public void Draw(IParameter parameter)
    {
        if (_disposed) return;

        if (parameter is not Parameter<int> p)
        {
            _renderer.TextDisabled("(TwoColumnHotkeyDrawer requires Parameter<int>)");
            return;
        }

        var v = p.Value;
        var prev = v;
        var wasWaiting = _waiting;

        // Prevent multiple drawers from capturing input simultaneously.
        // HotkeyCaptureState.WaitingCount is shared with HotkeyDrawer.
        var otherWaiting = HotkeyCaptureState.WaitingCount > (wasWaiting ? 1 : 0);

        if (_waiting)
        {
            _renderer.Text("Press any key...");
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
            // Right-column widget: key name only, no label prefix.
            _renderer.Text(_inputSource.GetKeyName(v));
            _renderer.SameLine();
            if (_renderer.Button($"Change##{p.Key}") && !otherWaiting)
                _waiting = true;
        }

        // Keep the shared counter in sync when this drawer's capture state changes.
        if (_waiting != wasWaiting)
            HotkeyCaptureState.WaitingCount += _waiting ? 1 : -1;

        if (v != prev) p.Value = v;
    }

    /// <summary>
    /// Releases this drawer's contribution to the shared capture counter in
    /// <see cref="HotkeyCaptureState"/>. Must be called when the owning
    /// <see cref="ConfigDrawer{TConfig}"/> is disposed so that a mid-capture plugin unload
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
