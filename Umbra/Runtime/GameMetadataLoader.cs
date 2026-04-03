using System.Text.Json;
using System.Text.Json.Serialization;
using Umbra.Runtime.Models;

namespace Umbra.Runtime;

/// <summary>
/// Loads supported-game metadata from Umbra's embedded runtime JSON resource.
/// </summary>
/// <remarks>
/// This helper reads the embedded <c>game-metadata.json</c> resource from the Umbra assembly and deserializes it into <see cref="GameMetadata"/> records used by <see cref="GameContext"/>.
/// </remarks>
internal static class GameMetadataLoader
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    /// <summary>
    /// Loads and deserializes the embedded game-metadata resource.
    /// </summary>
    /// <returns>An array containing every embedded <see cref="GameMetadata"/> entry.</returns>
    /// <exception cref="InvalidOperationException">The embedded metadata resource could not be opened or deserialized.</exception>
    internal static GameMetadata[] Load()
    {
        using var stream = typeof(GameMetadataLoader).Assembly.GetManifestResourceStream("Umbra.Runtime.game-metadata.json")
                ?? throw new InvalidOperationException("Failed to load embedded game metadata resource.");
        return JsonSerializer.Deserialize<GameMetadata[]>(stream, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize game metadata.");
    }
}
