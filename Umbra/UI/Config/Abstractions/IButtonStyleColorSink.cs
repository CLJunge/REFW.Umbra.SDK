using System.Numerics;
using Hexa.NET.ImGui;

namespace Umbra.UI.Config;

/// <summary>
/// Defines the low-level color-stack operations used by <see cref="ButtonStyleColors"/>.
/// </summary>
/// <remarks>
/// This seam isolates button-style color push and pop behavior from native ImGui calls so tests can verify style-selection logic without requiring an active ImGui frame.
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
    /// Pops the specified number of style colors from the current ImGui color stack.
    /// </summary>
    /// <param name="count">The number of style-color entries to pop.</param>
    void PopStyleColor(int count);
}
