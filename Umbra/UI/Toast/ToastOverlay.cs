namespace Umbra.UI.Toast;

/// <summary>
/// Static entry point for rendering all active toasts each frame.
/// </summary>
/// <remarks>
/// Call <see cref="Draw"/> from the host's <c>OnPreImGuiRenderer</c> callback.
/// When multiple <c>PluginHost&lt;TPlugin&gt;</c> instances share the same process,
/// each one calls <see cref="Draw"/>; the per-tick guard ensures the renderer only
/// runs once per frame. The renderer instance can be replaced via
/// <see cref="SetRenderer"/> for testing.
/// </remarks>
public static class ToastOverlay
{
    private static volatile IToastRenderer _renderer = new ImGuiToastRenderer();

    /// <summary>
    /// Tracks the last <see cref="Environment.TickCount64"/> value at which <see cref="Draw"/> rendered,
    /// used to deduplicate calls within the same millisecond (same frame from multiple plugin hosts).
    /// </summary>
    private static long _lastDrawTick;

    /// <summary>
    /// Draws all currently active toasts using the configured renderer.
    /// </summary>
    /// <remarks>
    /// Repeated calls within the same millisecond are deduplicated so that multiple plugin hosts
    /// sharing the same process do not render duplicate toast windows in a single ImGui frame.
    /// </remarks>
    public static void Draw()
    {
        var now = Environment.TickCount64;
        if (now == _lastDrawTick)
            return;

        _lastDrawTick = now;

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
    /// Resets the per-tick dedup guard so the next <see cref="Draw"/> call is not suppressed.
    /// </summary>
    /// <remarks>
    /// This is an internal test seam. Production code should never call this method.
    /// </remarks>
    internal static void ResetDrawTick() => _lastDrawTick = 0;
}
