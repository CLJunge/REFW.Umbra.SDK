namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the low-level rendering operations required by <see cref="HotkeyDrawer"/>.
/// </summary>
/// <remarks>
/// This seam isolates hotkey-drawer state transitions from native ImGui calls so unit tests can
/// verify behavior without requiring an active ImGui frame.
/// </remarks>
internal interface IHotkeyDrawerRenderer
{
    /// <summary>
    /// Renders disabled explanatory text.
    /// </summary>
    /// <param name="text">The text to display.</param>
    void TextDisabled(string text);

    /// <summary>
    /// Renders plain text.
    /// </summary>
    /// <param name="text">The text to display.</param>
    void Text(string text);

    /// <summary>
    /// Places the next widget on the current line.
    /// </summary>
    void SameLine();

    /// <summary>
    /// Renders a button and reports whether it was clicked.
    /// </summary>
    /// <param name="label">The button label.</param>
    /// <returns><see langword="true"/> when the button was clicked; otherwise <see langword="false"/>.</returns>
    bool Button(string label);

    /// <summary>
    /// Draws the inline help marker for the supplied description text.
    /// </summary>
    /// <param name="description">The description displayed by the help marker.</param>
    void DrawHelpMarker(string description);
}
