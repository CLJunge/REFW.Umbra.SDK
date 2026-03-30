using Umbra.Config;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// An <see cref="IParameterDrawer"/> implementation that renders a hotkey-capture control
/// for a <see cref="Parameter{T}"/> of type <see cref="int"/>, where the value represents
/// an <see cref="Hexa.NET.ImGui.ImGuiKey"/> cast to <see cref="int"/>.
/// </summary>
/// <remarks>
/// At most one hotkey-capture drawer may be in capture mode at any given frame.
/// A shared <see cref="HotkeyCaptureController"/> instance per drawer coordinates capture-mode UI
/// and keeps the static counter in <see cref="HotkeyCaptureState"/> accurate across all
/// <see cref="HotkeyDrawer"/> and <see cref="TwoColumnHotkeyDrawer"/> instances in the same
/// assembly. <see cref="Dispose"/> must be called (via the owning
/// <see cref="Config.ConfigDrawer{TConfig}"/>) on plugin unload so that any in-progress capture
/// does not permanently block future captures.
/// The default constructor renders through the shared ImGui context and captures keys through
/// <see cref="Umbra.Input.KeyboardInput"/>. Unit tests can replace those dependencies through the
/// internal constructor so the state machine can be verified without a live runtime host.
/// </remarks>
public sealed class HotkeyDrawer : IParameterDrawer
{
    private readonly IHotkeyDrawerRenderer _renderer;
    private readonly IHotkeyInputSource _inputSource;
    private readonly HotkeyCaptureController _captureController;

    /// <summary>
    /// Initializes a new <see cref="HotkeyDrawer"/> that renders through the shared active ImGui context and
    /// captures keys through <see cref="Umbra.Input.KeyboardInput"/>.
    /// </summary>
    public HotkeyDrawer()
        : this(ImGuiConfigRenderContext.Instance, new KeyboardHotkeyInputSource())
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
        _captureController = new HotkeyCaptureController(renderer, inputSource);
    }

    /// <inheritdoc/>
    public void Draw(string label, IParameter parameter)
    {
        if (_captureController.IsDisposed) return;

        if (parameter is not Parameter<int> p)
        {
            _renderer.TextDisabled($"{label}: (HotkeyDrawer requires Parameter<int>)");
            return;
        }

        _captureController.Draw(
            p,
            $"{label}: {_inputSource.GetKeyName(p.Value)}",
            $"{label}: Press any key...");

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
        _captureController.Dispose();
        GC.SuppressFinalize(this);
    }

}
