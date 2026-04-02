namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines button operations that use the host's default sizing.
/// </summary>
internal interface IButtonOps
{
    /// <summary>
    /// Renders a button and reports whether it was clicked.
    /// </summary>
    /// <param name="label">The button label.</param>
    /// <returns><see langword="true"/> when the button was clicked; otherwise <see langword="false"/>.</returns>
    bool Button(string label);
}
