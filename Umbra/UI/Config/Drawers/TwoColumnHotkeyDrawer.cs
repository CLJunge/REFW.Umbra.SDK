using Umbra.Config;
using Umbra.Input;
using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Renders a two-column hotkey-capture widget for a <see cref="Parameter{T}"/> whose value type is <see cref="HotkeyBinding"/>.
/// </summary>
/// <remarks>
/// The binding value includes a primary <see cref="Umbra.Input.UmbraKey"/> plus Ctrl/Shift/Alt modifiers. The configuration-drawer pipeline renders the label in the left column before calling <see cref="Draw"/>, so this drawer renders only the binding text and Change or Cancel button in the right column. Capture-mode coordination is shared with <see cref="HotkeyDrawer"/> through <see cref="HotkeyCaptureController"/> and <see cref="HotkeyCaptureState"/>.
/// </remarks>
public sealed class TwoColumnHotkeyDrawer : ITwoColumnParameterDrawer
{
    private readonly IHotkeyDrawerRenderer _renderer;
    private readonly IHotkeyInputSource _inputSource;
    private readonly HotkeyCaptureController _captureController;

    /// <summary>
    /// Initializes a new <see cref="TwoColumnHotkeyDrawer"/> that renders through the shared ImGui render context and captures keys through <see cref="Umbra.Input.KeyboardInput"/>.
    /// </summary>
    public TwoColumnHotkeyDrawer()
        : this(ImGuiConfigRenderContext.Instance, new KeyboardHotkeyInputSource())
    {
    }

    /// <summary>
    /// Initializes a new <see cref="TwoColumnHotkeyDrawer"/> with the specified renderer seam and input source.
    /// </summary>
    /// <param name="renderer">The renderer used for hotkey-drawer UI operations.</param>
    /// <param name="inputSource">The source used for key capture and key naming.</param>
    /// <exception cref="ArgumentNullException"><paramref name="renderer"/> or <paramref name="inputSource"/> is <see langword="null"/>.</exception>
    internal TwoColumnHotkeyDrawer(IHotkeyDrawerRenderer renderer, IHotkeyInputSource inputSource)
    {
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(inputSource);
        _renderer = renderer;
        _inputSource = inputSource;
        _captureController = new HotkeyCaptureController(renderer, inputSource);
    }

    /// <inheritdoc/>
    public void Draw(IParameter parameter)
    {
        if (_captureController.IsDisposed) return;

        if (parameter is not Parameter<HotkeyBinding> p)
        {
            _renderer.TextDisabled("(TwoColumnHotkeyDrawer requires Parameter<HotkeyBinding>)");
            return;
        }

        _captureController.Draw(
            p,
            _inputSource.GetBindingDisplayName(p.Value),
            "Press any key...");
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
