using System.Text.Json;
using System.Text.Json.Serialization;
using Umbra.Runtime.Models;

namespace Umbra.Runtime;

/// <summary>
/// Provides functionality for loading game metadata from an embedded JSON resource.
/// </summary>
/// <remarks>
/// This static class is responsible for retrieving and deserializing the 'game-metadata.json' file
/// embedded within the assembly. It is intended for internal use to supply metadata about supported games to other
/// components.
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
    /// Loads game metadata from the embedded JSON resource.
    /// </summary>
    /// <remarks>The method retrieves the 'game-metadata.json' file embedded in the assembly and deserializes
    /// its contents. The returned array will contain all available game metadata entries.</remarks>
    /// <returns>An array of <see cref="GameMetadata"/> objects deserialized from the embedded resource.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the embedded game metadata resource cannot be loaded or deserialized.</exception>
    internal static GameMetadata[] Load()
    {
        using var stream = typeof(GameMetadataLoader).Assembly.GetManifestResourceStream("Umbra.Runtime.game-metadata.json")
                ?? throw new InvalidOperationException("Failed to load embedded game metadata resource.");
        return JsonSerializer.Deserialize<GameMetadata[]>(stream, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize game metadata.");
    }
}
