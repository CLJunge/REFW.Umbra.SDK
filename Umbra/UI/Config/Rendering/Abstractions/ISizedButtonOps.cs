using System.Numerics;

namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines button operations that accept an explicit size.
/// </summary>
internal interface ISizedButtonOps
{
    /// <summary>
    /// Renders a button and reports whether it was clicked.
    /// </summary>
    /// <param name="label">The button label.</param>
    /// <param name="size">The requested button size.</param>
    /// <returns><see langword="true"/> if the button was clicked; otherwise, <see langword="false"/>.</returns>
    bool Button(string label, Vector2 size);
}
