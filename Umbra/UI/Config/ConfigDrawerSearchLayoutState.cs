namespace Umbra.UI.Config;

/// <summary>
/// Stores the cached search-row layout measurements for one <see cref="ConfigDrawer{TConfig}"/> instance.
/// </summary>
/// <remarks>
/// The drawer updates this state only when the available row width changes so the search-row input
/// width does not need to be recalculated every frame after reserving space for the visible search
/// label and the trailing navigation buttons.
/// </remarks>
internal sealed class ConfigDrawerSearchLayoutState
{
    /// <summary>
    /// Gets or sets a value indicating whether the cached layout values have been initialized.
    /// </summary>
    internal bool IsInitialized { get; set; }

    /// <summary>
    /// Gets or sets the last available row width used to compute the cached values.
    /// </summary>
    internal float LastAvailableWidth { get; set; }

    /// <summary>
    /// Gets or sets the cached previous-button width.
    /// </summary>
    internal float PreviousButtonWidth { get; set; }

    /// <summary>
    /// Gets or sets the cached next-button width.
    /// </summary>
    internal float NextButtonWidth { get; set; }

    /// <summary>
    /// Gets or sets the cached search-input width.
    /// </summary>
    internal float SearchInputWidth { get; set; }
}
