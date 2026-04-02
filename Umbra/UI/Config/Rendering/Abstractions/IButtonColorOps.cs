using System.Numerics;
using Umbra.Config.Attributes;

namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines button color-stack operations shared by button-based controls.
/// </summary>
internal interface IButtonColorOps
{
    /// <summary>
    /// Applies the preset colors associated with the specified button style.
    /// </summary>
    /// <param name="style">The style whose colors should be applied.</param>
    /// <returns>
    /// <see langword="true"/> when colors were pushed and must later be popped; otherwise
    /// <see langword="false"/>.
    /// </returns>
    bool PushButtonColors(ButtonStyle style);

    /// <summary>
    /// Applies fully custom button colors.
    /// </summary>
    /// <param name="normal">The normal-state color.</param>
    /// <param name="hovered">The hovered-state color.</param>
    /// <param name="active">The active-state color.</param>
    /// <returns>Always <see langword="true"/>.</returns>
    bool PushButtonColors(Vector4 normal, Vector4 hovered, Vector4 active);

    /// <summary>
    /// Pops the previously pushed button colors.
    /// </summary>
    void PopButtonColors();
}
