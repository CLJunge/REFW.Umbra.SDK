using System.Numerics;

namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines the color control operations used by the built-in color config control.
/// </summary>
internal interface IColorControlOps
{
    /// <summary>
    /// Renders an RGBA color editor and returns <see langword="true"/> when the value changed.
    /// </summary>
    bool ColorEdit4(string label, ref Vector4 value);

    /// <summary>
    /// Returns <see langword="true"/> when the left mouse button is currently held down.
    /// </summary>
    bool IsMouseDown();
}
