using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Defines the rendering operations required by <see cref="ConfigDrawer{TConfig}"/>, including optional drawer chrome such as the built-in search UI.
/// </summary>
/// <remarks>
/// This seam keeps <see cref="ConfigDrawer{TConfig}"/> testable without an active ImGui frame while remaining narrower than the full shared render context.
/// </remarks>
internal interface IConfigDrawerRenderer : IConfigDrawerScope, IButtonOps, ITextOps, IDisabledRegionOps
{
    /// <summary>
    /// Gets the horizontal width currently available for the search row.
    /// </summary>
    float GetAvailableWidth();

    /// <summary>
    /// Gets the horizontal spacing inserted between adjacent items on the current row.
    /// </summary>
    float GetItemSpacingX();

    /// <summary>
    /// Measures the width of a text label using the current ImGui style.
    /// </summary>
    /// <param name="text">The visible text to measure.</param>
    /// <returns>The rendered text width.</returns>
    float GetTextWidth(string text);

    /// <summary>
    /// Measures the width of a button with the supplied label using the current ImGui style.
    /// </summary>
    /// <param name="label">The button label.</param>
    /// <returns>The rendered button width.</returns>
    float GetButtonWidth(string label);

    /// <summary>
    /// Sets the width used by the next item submitted on the current row.
    /// </summary>
    /// <param name="width">The width to apply to the next item.</param>
    void SetNextItemWidth(float width);

    /// <summary>
    /// Renders a single-line text input and reports whether its value changed.
    /// </summary>
    /// <param name="label">The widget label.</param>
    /// <param name="value">The edited text value.</param>
    /// <param name="maxLength">The maximum input length.</param>
    /// <returns><see langword="true"/> when the input changed; otherwise, <see langword="false"/>.</returns>
    bool InputText(string label, ref string value, uint maxLength);
}
