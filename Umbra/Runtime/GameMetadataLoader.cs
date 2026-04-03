using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Umbra.Runtime.Models;

namespace Umbra.Runtime;

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

    public static GameMetadata[] Load()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Umbra.Runtime.game-metadata.json")
                ?? throw new InvalidOperationException("Failed to load embedded game metadata resource.");
        return JsonSerializer.Deserialize<GameMetadata[]>(stream, _jsonOptions)
            ?? throw new InvalidOperationException("Failed to deserialize game metadata.");
    }
}
