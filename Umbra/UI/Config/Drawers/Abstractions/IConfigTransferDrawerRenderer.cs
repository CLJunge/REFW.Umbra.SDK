using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the low-level rendering operations used by <see cref="ConfigTransferDrawer"/>.
/// </summary>
/// <remarks>
/// This seam isolates the transfer drawer's text, inline layout, width, text-input, and button
/// behavior from the shared ImGui render context so the control can be tested without an active
/// ImGui frame.
/// </remarks>
internal interface IConfigTransferDrawerRenderer : ITextOps, IButtonOps, ISizedButtonOps
{
    /// <summary>
    /// Gets the width available for the current row.
    /// </summary>
    float GetAvailableWidth();

    /// <summary>
    /// Gets the horizontal item spacing configured by the current ImGui style.
    /// </summary>
    float GetItemSpacingX();

    /// <summary>
    /// Measures the rendered width of plain text.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    float GetTextWidth(string text);

    /// <summary>
    /// Measures the rendered width of a default-size button for the supplied label.
    /// </summary>
    /// <param name="label">The button label.</param>
    float GetButtonWidth(string label);

    /// <summary>
    /// Sets the width used by the next input control.
    /// </summary>
    /// <param name="width">The requested item width.</param>
    void SetNextItemWidth(float width);

    /// <summary>
    /// Renders a single-line text input.
    /// </summary>
    /// <param name="label">The hidden or visible input label.</param>
    /// <param name="value">The current text value, updated in place when the user edits it.</param>
    /// <param name="maxLength">The maximum text length accepted by the input widget.</param>
    /// <returns><see langword="true"/> when the value changed; otherwise, <see langword="false"/>.</returns>
    bool InputText(string label, ref string value, uint maxLength);

    /// <summary>
    /// Opens a popup with the supplied identifier.
    /// </summary>
    /// <param name="id">The popup identifier.</param>
    void OpenPopup(string id);

    /// <summary>
    /// Begins rendering a popup.
    /// </summary>
    /// <param name="id">The popup identifier.</param>
    /// <returns><see langword="true"/> when the popup is open and should be rendered.</returns>
    bool BeginPopup(string id);

    /// <summary>
    /// Ends the current popup.
    /// </summary>
    void EndPopup();

    /// <summary>
    /// Renders a selectable popup item.
    /// </summary>
    /// <param name="label">The visible item label.</param>
    /// <returns><see langword="true"/> when the item was selected.</returns>
    bool Selectable(string label);

    /// <summary>
    /// Draws a horizontal separator.
    /// </summary>
    void Separator();
}
