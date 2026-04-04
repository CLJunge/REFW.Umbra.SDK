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
internal interface IConfigTransferDrawerRenderer : ITextOps, IButtonOps
{
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
}
