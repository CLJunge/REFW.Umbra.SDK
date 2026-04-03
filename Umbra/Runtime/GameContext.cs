using System.Diagnostics;
using Umbra.Runtime.Models;

namespace Umbra.Runtime;

/// <summary>
/// Provides access to the current game context via compile-time preprocessor symbols.
/// </summary>
public static class GameContext
{
    private static readonly Dictionary<REGame, GameMetadata> _gameMetadata = [];
    private static REGame _currentGame = REGame.Unknown;

    static GameContext()
    {
        var metadata = GameMetadataLoader.Load();
        foreach (var entry in metadata)
        {
            _gameMetadata[entry.CompatibleTarget] = entry;
        }

        DetectCurrentGame();
    }

    /// <summary>
    /// Gets the current game being run based on compile-time preprocessor symbols.
    /// </summary>
    /// <returns>A value of the <see cref="REGame"/> enumeration corresponding to the defined game symbol.</returns>
    /// <remarks>
    /// This method uses preprocessor directives to determine which game is being compiled for.
    /// Exactly one game symbol must be defined at compile time (e.g., RE9, DMC5, DD2).
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when no game preprocessor symbol is defined.</exception>
    public static REGame GetCurrentGame() => _currentGame;

    private static void DetectCurrentGame()
    {
        var processName = Process.GetCurrentProcess().ProcessName;
        foreach (var entry in _gameMetadata.Values)
        {
            if (string.Compare(entry.ExecutableName, processName) == 0)
            {
                _currentGame = entry.CompatibleTarget;
                return;
            }
        }
    }
}
