namespace Umbra.UI.Config.Transfer;

/// <summary>
/// Stores optional built-in config transfer UI settings.
/// </summary>
/// <remarks>
/// This options object keeps config import/export UI opt-in and configurable without forcing
/// plugins to define transfer-specific config groups or sidecar-state plumbing.
/// </remarks>
public sealed class ConfigTransferOptions
{
    /// <summary>
    /// The default visible tree-node label used for the built-in transfer UI.
    /// </summary>
    public const string DefaultTreeNodeLabel = "Import/Export";

    /// <summary>
    /// Gets or sets a value indicating whether the built-in config transfer UI is enabled.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets or sets an optional explicit sidecar file path override for the transfer-path state.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, Umbra derives the sidecar path from the main settings-store file path.
    /// </remarks>
    public string? SidecarFilePath { get; init; }

    /// <summary>
    /// Gets or sets an optional fallback directory used by the native browse dialog when the current transfer path does not resolve to an existing directory.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, Umbra falls back to the directory that contains the main settings-store file.
    /// </remarks>
    public string? BrowseInitialDirectory { get; init; }

    /// <summary>
    /// Gets or sets the visible tree-node label used to wrap the built-in transfer UI.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/> or whitespace, Umbra uses <see cref="DefaultTreeNodeLabel"/>.
    /// </remarks>
    public string? TreeNodeLabel { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the built-in transfer tree node starts expanded.
    /// </summary>
    public bool TreeNodeDefaultOpen { get; init; }

    /// <summary>
    /// Gets or sets where the built-in transfer UI is rendered relative to the main config UI.
    /// </summary>
    public ConfigTransferPlacement Placement { get; init; } = ConfigTransferPlacement.AfterConfig;

    /// <summary>
    /// Gets or sets a value indicating whether a separator is drawn below the built-in transfer buttons.
    /// </summary>
    public bool DrawSeparatorBelowButtons { get; init; } = true;
}
