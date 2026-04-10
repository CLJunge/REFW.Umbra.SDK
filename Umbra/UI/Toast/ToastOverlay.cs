using Hexa.NET.ImGui;

namespace Umbra.UI.Toast;

/// <summary>
/// Static entry point for rendering all active toasts each frame.
/// </summary>
/// <remarks>
/// Call <see cref="Draw"/> from the host's <c>OnPreImGuiRenderer</c> callback.
/// When multiple <c>PluginHost&lt;TPlugin&gt;</c> instances share the same process,
/// each one calls <see cref="Draw"/>; the per-frame guard ensures the renderer only
/// runs once per ImGui frame. The renderer and frame-ID provider can be replaced via
/// <see cref="SetRenderer"/> and <see cref="SetFrameIdProvider"/> for testing.
/// </remarks>
public static class ToastOverlay
{
    private static volatile IToastRenderer _renderer = new ImGuiToastRenderer();
    private static Func<long> _frameIdProvider = static () => ImGui.GetFrameCount();

    /// <summary>
    /// Tracks the last ImGui frame ID at which <see cref="Draw"/> rendered,
    /// used to deduplicate calls within the same frame from multiple plugin hosts.
    /// </summary>
    private static long _lastDrawFrame = -1;

    /// <summary>
    /// Draws all currently active toasts using the configured renderer.
    /// </summary>
    /// <remarks>
    /// Repeated calls within the same ImGui frame are deduplicated so that multiple plugin hosts
    /// sharing the same process do not render duplicate toast windows in a single ImGui frame.
    /// </remarks>
    public static void Draw()
    {
        var now = _frameIdProvider();
        if (now == _lastDrawFrame)
            return;

        _lastDrawFrame = now;

        var entries = ToastQueue.GetActiveEntries();
        if (entries.Count == 0) return;
        _renderer.Draw(entries);
    }

    /// <summary>
    /// Replaces the active renderer. Intended for testing.
    /// </summary>
    /// <param name="renderer">The renderer instance to use, or <see langword="null"/> to restore the default.</param>
    internal static void SetRenderer(IToastRenderer? renderer) => _renderer = renderer ?? new ImGuiToastRenderer();

    /// <summary>
    /// Replaces the frame-ID provider used by <see cref="Draw"/> to identify the current ImGui frame.
    /// </summary>
    /// <param name="provider">
    /// A delegate that returns the current frame ID, or <see langword="null"/> to restore
    /// <see cref="ImGui.GetFrameCount"/>.
    /// </param>
    /// <remarks>
    /// This is an internal test seam. Production code should never call this method.
    /// Supply a fixed-value delegate in tests to make the per-frame dedup guard deterministic.
    /// </remarks>
    internal static void SetFrameIdProvider(Func<long>? provider) =>
        _frameIdProvider = provider ?? (static () => ImGui.GetFrameCount());

    /// <summary>
    /// Resets the per-frame dedup guard so the next <see cref="Draw"/> call is not suppressed.
    /// </summary>
    /// <remarks>
    /// This is an internal test seam. Production code should never call this method.
    /// </remarks>
    internal static void ResetDrawFrame() => _lastDrawFrame = -1;
}
