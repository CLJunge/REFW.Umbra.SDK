using Umbra.UI.Config.Transfer;

namespace Umbra.UI.Config;

/// <summary>
/// Stores optional configuration-drawer behavior flags.
/// </summary>
/// <remarks>
/// This options object exists so new drawer-level behaviors can be added without expanding constructor
/// parameter lists with unrelated Boolean flags. All options default to the current behavior so existing
/// call sites remain unchanged unless they opt in explicitly.
/// </remarks>
public sealed class ConfigDrawerOptions
{
    internal static ConfigDrawerOptions Default { get; } = new()
    {
        ShowSearchBar = false,
        SuppressRootNode = false,
        Transfer = null
    };

    /// <summary>
    /// Gets or sets a value indicating whether the built-in search bar is shown.
    /// </summary>
    /// <value><see langword="true"/> to show the search bar; otherwise, <see langword="false"/>.</value>
    public bool ShowSearchBar { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the root-node-attribute-driven root tree wrapper is suppressed.
    /// </summary>
    /// <value>
    /// <see langword="true"/> to suppress the root tree wrapper even when the configuration type would
    /// otherwise render one; otherwise, <see langword="false"/>.
    /// </value>
    public bool SuppressRootNode { get; init; }

    /// <summary>
    /// Gets or sets the optional built-in config transfer UI settings.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/> or when <see cref="ConfigTransferOptions.Enabled"/> is <see langword="false"/>,
    /// the wrapped drawer or section renders no built-in config transfer UI.
    /// </remarks>
    public ConfigTransferOptions? Transfer { get; init; }

    /// <summary>Initializes a new instance of <see cref="ConfigDrawerOptions"/> with all options set to their defaults.</summary>
    public ConfigDrawerOptions() { }

    private ConfigDrawerOptions(ConfigDrawerOptions source)
    {
        ShowSearchBar = source.ShowSearchBar;
        SuppressRootNode = source.SuppressRootNode;
        Transfer = source.Transfer;
    }

    internal ConfigDrawerOptions WithShowSearchBar(bool showSearchBar) => new(this) { ShowSearchBar = showSearchBar };

    internal ConfigDrawerOptions WithSuppressRootNode(bool suppressRootNode) => new(this) { SuppressRootNode = suppressRootNode };

    internal ConfigDrawerOptions WithTransfer(ConfigTransferOptions? transfer) => new(this) { Transfer = transfer };
}
