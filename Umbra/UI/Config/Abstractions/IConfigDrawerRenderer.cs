using Umbra.UI.Config.Rendering;

namespace Umbra.UI.Config;

/// <summary>
/// Defines the rendering operations required by <see cref="ConfigDrawer{TConfig}"/>, including optional drawer chrome such as the built-in search UI.
/// </summary>
/// <remarks>
/// This seam keeps `ConfigDrawer<TConfig>` testable without an active ImGui frame while remaining narrower than the full shared render context.
/// </remarks>
internal interface IConfigDrawerRenderer : IConfigDrawerScope, IButtonOps, ITextOps
{
    /// <summary>
    /// Renders a single-line text input and reports whether its value changed.
    /// </summary>
    /// <param name="label">The widget label.</param>
    /// <param name="value">The edited text value.</param>
    /// <param name="maxLength">The maximum input length.</param>
    /// <returns><see langword="true"/> when the input changed; otherwise, <see langword="false"/>.</returns>
    bool InputText(string label, ref string value, uint maxLength);
}
