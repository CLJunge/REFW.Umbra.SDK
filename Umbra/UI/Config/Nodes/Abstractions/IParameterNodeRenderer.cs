using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config.Nodes;

/// <summary>
/// Defines the spacing, indentation, highlight, scroll, and keyboard-focus operations used by <see cref="ParameterNode"/>.
/// </summary>
/// <remarks>
/// This composed seam isolates parameter-node layout primitives from the shared ImGui render context so tests can verify visibility, spacing, indentation, highlight, scroll, and focus behavior without requiring an active ImGui frame.
/// </remarks>
internal interface IParameterNodeRenderer : ISpacingOps, IIndentationOps, IButtonStyleColorSink
{
    /// <summary>
    /// Scrolls the current item into view.
    /// </summary>
    /// <param name="centerYRatio">The target vertical position within the visible region.</param>
    void SetScrollHereY(float centerYRatio);

    /// <summary>
    /// Requests keyboard focus for the next interactive item submitted on the current row.
    /// </summary>
    void SetKeyboardFocusHere();
}
