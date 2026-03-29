using System.Numerics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Config;

/// <summary>
/// Defines the low-level button-style color operations required by <see cref="ButtonStyleColors"/>.
/// </summary>
/// <remarks>
/// This seam isolates style-color push/pop behavior from native ImGui calls so unit tests can
/// verify color-selection logic without requiring an active ImGui frame.
/// </remarks>
internal interface IButtonStyleColorSink
{
    /// <summary>
    /// Pushes one style color onto the current ImGui color stack.
    /// </summary>
    /// <param name="color">The ImGui color slot to override.</param>
    /// <param name="value">The color value to push.</param>
    void PushStyleColor(ImGuiCol color, Vector4 value);

    /// <summary>
    /// Pops the specified number of colors from the current ImGui color stack.
    /// </summary>
    /// <param name="count">The number of style colors to pop.</param>
    void PopStyleColor(int count);
}
