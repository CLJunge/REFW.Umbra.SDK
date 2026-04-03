namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines the indentation operations used by grouped configuration UI elements.
/// </summary>
internal interface IIndentationOps
{
    /// <summary>
    /// Applies indentation before drawing a grouped region.
    /// </summary>
    /// <param name="amount">The indentation width in pixels, or <c>0f</c> to use the host default.</param>
    void Indent(float amount);

    /// <summary>
    /// Removes indentation after a grouped region has been drawn.
    /// </summary>
    /// <param name="amount">The indentation width in pixels, or <c>0f</c> to use the host default.</param>
    void Unindent(float amount);
}
