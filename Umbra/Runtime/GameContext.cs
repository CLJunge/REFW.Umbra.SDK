using System.Diagnostics;
using Umbra.Logging;
using Umbra.Runtime.Models;

namespace Umbra.Runtime;

/// <summary>
/// Provides information about the currently detected RE Engine game for the running process.
/// </summary>
/// <remarks
/// >Use this class to determine which RE Engine game is active, allowing plugins or mods to implement
/// game-specific logic or compatibility checks. The detection is based on the process name and available game
/// metadata.
/// </remarks>
public static class GameContext
{
    private static readonly GameMetadata? _currentGameMetadata;

    static GameContext()
    {
        try
        {
            var metadata = GameMetadataLoader.Load();
            var processName = Process.GetCurrentProcess().ProcessName;
            foreach (var entry in metadata)
            {
                if (string.Equals(entry.ExecutableName, processName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    _currentGameMetadata = entry;
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Exception(ex, "Failed to load game metadata or detect current game.");
        }
    }


    /// <summary>
    /// Gets the currently detected RE Engine game target for the running process.
    /// </summary>
    /// <remarks>
    /// Returns the compatible game target if detected; otherwise, returns <see cref="REGame.Unknown"/>.
    /// This property is typically used to determine which RE Engine game the plugin is running in,
    /// enabling game-specific logic or compatibility checks.
    /// </remarks>
    public static REGame CurrentGame => _currentGameMetadata?.CompatibleTarget ?? REGame.Unknown;
}
