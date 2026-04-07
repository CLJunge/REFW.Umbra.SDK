using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Drawers;

/// <summary>
/// Defines the low-level rendering operations used by <see cref="ConfigPresetDrawer"/>.
/// </summary>
/// <remarks>
/// This seam isolates the preset drawer's text, inline layout, width, combo, and button
/// behavior from the shared ImGui render context so the control can be tested without an
/// active ImGui frame.
/// </remarks>
internal interface IConfigPresetDrawerRenderer : ITextOps, IButtonOps, ISizedButtonOps, IDisabledRegionOps
{
    /// <summary>
    /// Renders a combo box and updates the selected index when the user chooses a different item.
    /// </summary>
    /// <param name="label">The hidden or visible combo label.</param>
    /// <param name="selectedIndex">The current selected index, updated in place when the selection changes.</param>
    /// <param name="items">The visible item labels.</param>
    /// <param name="itemCount">The number of items available for selection.</param>
    /// <returns><see langword="true"/> when the selection changed; otherwise, <see langword="false"/>.</returns>
    bool Combo(string label, ref int selectedIndex, string[] items, int itemCount);

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
    /// Draws a horizontal separator.
    /// </summary>
    void Separator();
}
