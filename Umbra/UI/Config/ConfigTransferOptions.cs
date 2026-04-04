namespace Umbra.UI.Config;

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
}
