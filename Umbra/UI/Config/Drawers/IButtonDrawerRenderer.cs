using System.Numerics;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the low-level rendering operations required by <see cref="ButtonDrawer"/>.
/// </summary>
/// <remarks>
/// This narrow seam isolates the drawer's button-style selection and click-handling logic from the
/// native ImGui host so unit tests can verify behavior without requiring an active ImGui frame.
/// </remarks>
internal interface IButtonDrawerRenderer
{
    /// <summary>
    /// Renders disabled explanatory text.
    /// </summary>
    /// <param name="text">The text to display.</param>
    void TextDisabled(string text);

    /// <summary>
    /// Applies the preset colors associated with the specified button style.
    /// </summary>
    /// <param name="style">The style whose colors should be applied.</param>
    /// <returns>
    /// <see langword="true"/> when colors were pushed and must later be popped; otherwise
    /// <see langword="false"/>.
    /// </returns>
    bool PushButtonColors(ButtonStyle style);

    /// <summary>
    /// Applies fully custom button colors.
    /// </summary>
    /// <param name="normal">The normal-state color.</param>
    /// <param name="hovered">The hovered-state color.</param>
    /// <param name="active">The active-state color.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    bool PushButtonColors(Vector4 normal, Vector4 hovered, Vector4 active);

    /// <summary>
    /// Pops the previously pushed button colors.
    /// </summary>
    void PopButtonColors();

    /// <summary>
    /// Renders the button and reports whether it was clicked.
    /// </summary>
    /// <param name="label">The button label.</param>
    /// <param name="size">The requested button size.</param>
    /// <returns><see langword="true"/> when the button was clicked; otherwise <see langword="false"/>.</returns>
    bool Button(string label, Vector2 size);

    /// <summary>
    /// Places the next widget on the current line.
    /// </summary>
    void SameLine();

    /// <summary>
    /// Draws the inline help marker for the supplied description text.
    /// </summary>
    /// <param name="description">The description displayed by the help marker.</param>
    void DrawHelpMarker(string description);
}
