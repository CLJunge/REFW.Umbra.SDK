namespace Umbra.Config;

/// <summary>
/// Represents the versioned document written by Umbra's config export pipeline.
/// </summary>
/// <remarks>
/// This envelope is used only for explicit import/export scenarios. Umbra's normal runtime persistence continues to use the existing flat `key -> value` JSON format.
/// </remarks>
public sealed class SettingsExchangeDocument
{
    /// <summary>
    /// Gets the current exchange-document format version supported by this Umbra build.
    /// </summary>
    public const int CurrentFormatVersion = 1;

    /// <summary>
    /// Gets or sets the exchange-document format version.
    /// </summary>
    public int FormatVersion { get; init; } = CurrentFormatVersion;

    /// <summary>
    /// Gets or sets the stable schema identifier for the exported config type.
    /// </summary>
    public string SchemaId { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the exported config schema version.
    /// </summary>
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Gets or sets the exported config values keyed by registered parameter name.
    /// </summary>
    public Dictionary<string, object?> Values { get; init; } = [];
}
