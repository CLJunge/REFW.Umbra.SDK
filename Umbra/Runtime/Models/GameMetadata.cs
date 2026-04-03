using System.Text.Json.Serialization;

namespace Umbra.Runtime.Models;

/// <summary>
/// Represents a game's metadata and identification information.
/// </summary>
internal sealed record GameMetadata
{
    /// <summary>
    /// Gets or sets the human-readable display name for the game.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the REFramework target with which this configuration or plugin is compatible.
    /// </summary>
    [JsonPropertyName("compatibleTarget")]
    public REGame CompatibleTarget { get; set; } = REGame.Unknown;

    /// <summary>
    /// Gets or sets the name of the game's executable file (without extension).
    /// </summary>
    /// <remarks>
    /// Used for process identification and game detection.
    /// </remarks>
    [JsonPropertyName("executableName")]
    public string ExecutableName { get; set; } = string.Empty;
}
