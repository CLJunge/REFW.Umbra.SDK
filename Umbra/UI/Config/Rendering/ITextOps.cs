namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines text and inline layout operations shared by multiple config controls.
/// </summary>
internal interface ITextOps
{
    /// <summary>
    /// Renders plain text.
    /// </summary>
    /// <param name="text">The text to display.</param>
    void Text(string text);

    /// <summary>
    /// Renders disabled explanatory text.
    /// </summary>
    /// <param name="text">The text to display.</param>
    void TextDisabled(string text);

    /// <summary>
    /// Places the next widget on the current line.
    /// </summary>
    void SameLine();

    /// <summary>
    /// Draws an inline help marker for the supplied description text.
    /// </summary>
    /// <param name="description">The description displayed by the help marker.</param>
    void DrawHelpMarker(string description);
}
