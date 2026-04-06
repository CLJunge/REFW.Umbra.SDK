namespace Umbra.UI.Config.Rendering;

/// <summary>
/// Defines the disabled-region operations used by conditionally disabled configuration UI elements.
/// </summary>
internal interface IDisabledRegionOps
{
    /// <summary>
    /// Begins a disabled region when <paramref name="disabled"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="disabled"><see langword="true"/> to disable submitted widgets until <see cref="EndDisabled"/>; otherwise, <see langword="false"/>.</param>
    void BeginDisabled(bool disabled);

    /// <summary>
    /// Ends the current disabled region.
    /// </summary>
    void EndDisabled();
}
