using Umbra.Config;
using Umbra.Input;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Renders a hotkey-capture control for a <see cref="Parameter{T}"/> whose value type is <see cref="HotkeyBinding"/>.
/// </summary>
/// <remarks>
/// The binding value includes a primary <see cref="Hexa.NET.ImGui.ImGuiKey"/> plus Ctrl/Shift/Alt modifiers. Capture-mode coordination is delegated to a per-drawer <see cref="HotkeyCaptureController"/>, which synchronizes with <see cref="HotkeyCaptureState"/> so only one hotkey drawer across the assembly waits for input at a time.
/// </remarks>
public sealed class HotkeyDrawer : IParameterDrawer
{
    private readonly IHotkeyDrawerRenderer _renderer;
    private readonly IHotkeyInputSource _inputSource;
    private readonly HotkeyCaptureController _captureController;

    /// <summary>
    /// Initializes a new <see cref="HotkeyDrawer"/> that renders through the shared ImGui render context and captures keys through <see cref="Umbra.Input.KeyboardInput"/>.
    /// </summary>
    public HotkeyDrawer()
        : this(ImGuiConfigRenderContext.Instance, new KeyboardHotkeyInputSource())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="HotkeyDrawer"/> with the specified renderer seam and input source.
    /// </summary>
    /// <param name="renderer">The renderer used for hotkey-drawer UI operations.</param>
    /// <param name="inputSource">The source used for key capture and key naming.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> or <paramref name="inputSource"/> is <see langword="null"/>.</exception>
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

        if (parameter is not Parameter<HotkeyBinding> p)
        {
            _renderer.TextDisabled($"{label}: (HotkeyDrawer requires Parameter<HotkeyBinding>)");
            return;
        }

        _captureController.Draw(
            p,
            $"{label}: {_inputSource.GetBindingDisplayName(p.Value)}",
            $"{label}: Press any key...");

        var metadata = parameter.Metadata;
        if (metadata.Description is not null)
        {
            _renderer.SameLine();
            _renderer.DrawHelpMarker(metadata.Description);
        }
    }

    /// <summary>
    /// Releases this drawer's participation in the shared hotkey-capture workflow.
    /// </summary>
    public void Dispose()
    {
        _captureController.Dispose();
        GC.SuppressFinalize(this);
    }

}
