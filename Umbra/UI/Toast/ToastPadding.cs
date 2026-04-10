namespace Umbra.UI.Toast;

/// <summary>
/// Defines four-sided spacing used by <see cref="ImGuiToastRenderer"/> to inset the
/// toast stack from the viewport edges and to pad content within each toast window.
/// Follows the same <c>Top, Right, Bottom, Left</c> convention as WPF <c>Thickness</c>.
/// </summary>
internal readonly record struct ToastPadding(float Top, float Right, float Bottom, float Left)
{
    /// <summary>
    /// Creates uniform padding on all four sides.
    /// </summary>
    /// <param name="uniform">The value applied to every side.</param>
    internal ToastPadding(float uniform) : this(uniform, uniform, uniform, uniform) { }

    /// <summary>
    /// Creates symmetric padding with separate vertical and horizontal values.
    /// </summary>
    /// <param name="vertical">The value applied to <see cref="Top"/> and <see cref="Bottom"/>.</param>
    /// <param name="horizontal">The value applied to <see cref="Left"/> and <see cref="Right"/>.</param>
    internal ToastPadding(float vertical, float horizontal) : this(vertical, horizontal, vertical, horizontal) { }
}
