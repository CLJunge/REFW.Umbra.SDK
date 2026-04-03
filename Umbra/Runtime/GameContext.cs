using System.Diagnostics;
using Umbra.Runtime.Models;

namespace Umbra.Runtime;

/// <summary>
/// Provides access to the current game context via compile-time preprocessor symbols.
/// </summary>
public static class GameContext
{
    private static readonly GameMetadata? _currentGameMetadata;

    static GameContext()
    {
        var metadata = GameMetadataLoader.Load();
        var processName = Process.GetCurrentProcess().ProcessName;
        foreach (var entry in metadata)
        {
            if (string.Compare(entry.ExecutableName, processName) == 0)
            {
                _currentGameMetadata = entry;
                return;
            }
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
