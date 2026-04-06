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
    private string? _configFilePath;
    private string? _browseFallbackDirectory;
    private string? _sectionLabel;
    private bool _expandedByDefault;
    private bool _showSeparatorBelowButtons = true;
    private TimeSpan _statusDisplayDuration = DefaultStatusVisibilityTimeout;

    /// <summary>
    /// The default visible section label used for the built-in transfer UI.
    /// </summary>
    public const string DefaultSectionLabel = "Import/Export";

    /// <summary>
    /// The default duration that a completed transfer status label remains visible.
    /// </summary>
    public static readonly TimeSpan DefaultStatusVisibilityTimeout = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets a value indicating whether the built-in config transfer UI is enabled.
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    /// Gets or sets an optional explicit file path override for the persisted transfer config file.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, Umbra derives the persisted transfer config file path from the main config-store file path.
    /// </remarks>
    public string? ConfigFilePath
    {
        get => _configFilePath;
        init => _configFilePath = value;
    }

    /// <summary>
    /// Gets or sets an optional fallback directory used by the native browse dialog when the current transfer path does not resolve to an existing directory.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/>, Umbra falls back to the directory that contains the main config-store file.
    /// </remarks>
    public string? BrowseFallbackDirectory
    {
        get => _browseFallbackDirectory;
        init => _browseFallbackDirectory = value;
    }

    /// <summary>
    /// Gets or sets the visible section label used to wrap the built-in transfer UI.
    /// </summary>
    /// <remarks>
    /// When <see langword="null"/> or whitespace, Umbra uses <see cref="DefaultSectionLabel"/>.
    /// </remarks>
    public string? SectionLabel
    {
        get => _sectionLabel;
        init => _sectionLabel = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the built-in transfer section starts expanded.
    /// </summary>
    public bool ExpandedByDefault
    {
        get => _expandedByDefault;
        init => _expandedByDefault = value;
    }

    /// <summary>
    /// Gets or sets where the built-in transfer UI is rendered relative to the main config UI.
    /// </summary>
    public ConfigTransferPlacement Placement { get; init; } = ConfigTransferPlacement.AfterConfig;

    /// <summary>
    /// Gets or sets a value indicating whether a separator is shown below the built-in transfer buttons.
    /// </summary>
    public bool ShowSeparatorBelowButtons
    {
        get => _showSeparatorBelowButtons;
        init => _showSeparatorBelowButtons = value;
    }

    /// <summary>
    /// Gets or sets how long the completed import or export status label remains visible.
    /// </summary>
    /// <remarks>
    /// When the configured value is zero or negative, Umbra falls back to <see cref="DefaultStatusVisibilityTimeout"/>.
    /// </remarks>
    public TimeSpan StatusDisplayDuration
    {
        get => _statusDisplayDuration;
        init => _statusDisplayDuration = value;
    }
}
