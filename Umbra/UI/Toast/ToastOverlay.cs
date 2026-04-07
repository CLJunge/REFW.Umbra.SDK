namespace Umbra.UI.Toast;

/// <summary>
/// Static entry point for rendering all active toasts each frame.
/// </summary>
/// <remarks>
/// Call <see cref="Draw"/> from the host's <c>OnPreImGuiDrawUI</c> callback. The renderer
/// instance can be replaced via <see cref="SetRenderer"/> for testing.
/// </remarks>
public static class ToastOverlay
{
    private static volatile IToastRenderer _renderer = new ImGuiToastRenderer();

    /// <summary>
    /// Draws all currently active toasts using the configured renderer.
    /// </summary>
    public static void Draw()
    {
        var entries = ToastQueue.GetActiveEntries();
        if (entries.Count == 0) return;
        _renderer.Draw(entries);
    }

    /// <summary>
    /// Replaces the active renderer. Intended for testing.
    /// </summary>
    /// <param name="renderer">The renderer instance to use, or <see langword="null"/> to restore the default.</param>
    internal static void SetRenderer(IToastRenderer? renderer)
    {
        _renderer = renderer ?? new ImGuiToastRenderer();
    }
}
