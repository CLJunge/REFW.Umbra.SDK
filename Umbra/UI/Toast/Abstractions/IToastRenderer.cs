namespace Umbra.UI.Toast;

/// <summary>
/// Rendering seam for toast overlay display, allowing non-ImGui test implementations.
/// </summary>
internal interface IToastRenderer
{
    /// <summary>
    /// Draws the given active toast entries on screen.
    /// </summary>
    /// <param name="entries">The currently visible entries to render.</param>
    void Draw(List<ToastEntry> entries);
}
