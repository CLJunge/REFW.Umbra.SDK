using System.Numerics;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines the button color-stack operations shared by button-based configuration controls.
/// </summary>
/// <remarks>
/// <see cref="Drawers.ButtonDrawer"/> and other button-oriented renderers use this abstraction to apply either a built-in <see cref="ButtonStyle"/> palette or an explicit custom color triple before drawing the widget.
/// </remarks>
internal interface IButtonColorOps
{
    /// <summary>
    /// Pushes the preset colors associated with <paramref name="style"/>.
    /// </summary>
    /// <param name="style">The built-in button style whose colors should be applied.</param>
    /// <returns><see langword="true"/> if colors were pushed and must later be popped; otherwise, <see langword="false"/>.</returns>
    bool PushButtonColors(ButtonStyle style);

    /// <summary>
    /// Pushes a fully custom button color triple.
    /// </summary>
    /// <param name="normal">The normal-state color.</param>
    /// <param name="hovered">The hovered-state color.</param>
    /// <param name="active">The active-state color.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    bool PushButtonColors(Vector4 normal, Vector4 hovered, Vector4 active);

    /// <summary>
    /// Pops the button colors pushed by <see cref="PushButtonColors(ButtonStyle)"/> or <see cref="PushButtonColors(Vector4, Vector4, Vector4)"/>.
    /// </summary>
    void PopButtonColors();
}
