using System.Text.Json.Serialization;

namespace Umbra.Runtime.Models;

/// <summary>
/// Stores one embedded supported-game metadata entry used for process detection.
/// </summary>
/// <remarks>
/// <see cref="GameMetadataLoader"/> deserializes these records from Umbra's embedded runtime metadata JSON, and <see cref="GameContext"/> uses them to match the current process name to a supported <see cref="REGame"/> target.
/// </remarks>
internal sealed record GameMetadata
{
    /// <summary>
    /// Gets or sets the human-readable display name for the game.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the supported Umbra runtime target associated with this metadata entry.
    /// </summary>
    [JsonPropertyName("compatibleTarget")]
    public REGame CompatibleTarget { get; set; } = REGame.Unknown;

    /// <summary>
    /// Gets or sets the executable name used to identify the game process.
    /// </summary>
    /// <remarks>
    /// The value is stored without the file extension so it can be compared directly against <see cref="System.Diagnostics.Process.ProcessName"/>.
    /// </remarks>
    [JsonPropertyName("executableName")]
    public string ExecutableName { get; set; } = string.Empty;
}
