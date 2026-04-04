namespace Umbra.UI.Config;

/// <summary>
/// Stores optional configuration-drawer feature flags.
/// </summary>
/// <remarks>
/// This options object exists so new drawer-level features can be added without expanding constructor
/// parameter lists with unrelated Boolean flags. All options default to the current no-search
/// behavior so existing call sites remain unchanged unless they opt in explicitly.
/// </remarks>
public sealed class ConfigDrawerOptions
{
    internal static ConfigDrawerOptions Default { get; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the built-in search bar is shown.
    /// </summary>
    /// <value><see langword="true"/> to show the search bar; otherwise, <see langword="false"/>.</value>
    public bool ShowSearchBar { get; init; }
}
