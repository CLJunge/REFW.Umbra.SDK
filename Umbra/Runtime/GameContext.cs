using System.Diagnostics;
using Umbra.Logging;
using Umbra.Runtime.Models;

namespace Umbra.Runtime;

/// <summary>
/// Exposes the RE Engine game detected for the current process.
/// </summary>
/// <remarks>
/// On first use, this type loads embedded metadata through <see cref="GameMetadataLoader"/>, compares each metadata entry's executable name against the current process name using an ordinal-ignore-case comparison, and caches the first match for the lifetime of the AppDomain. If loading or detection fails, the exception is logged and the context falls back to <see cref="REGame.Unknown"/>.
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
                if (string.IsNullOrEmpty(entry.ExecutableName))
                {
                    Logger.Info($"Skipping game metadata entry with missing executable name: {entry.DisplayName}");
                    continue;
                }

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
    /// Gets the detected RE Engine game target for the running process.
    /// </summary>
    /// <value>The compatible target from the cached metadata match, or <see cref="REGame.Unknown"/> when no match was found or detection failed.</value>
    /// <remarks>
    /// Callers that convert this value to a user-facing title through <see cref="REGameExtensions.GetDisplayName(REGame)"/> should guard against <see cref="REGame.Unknown"/>, because that extension accepts only supported concrete game identifiers.
    /// </remarks>
    public static REGame CurrentGame => _currentGameMetadata?.CompatibleTarget ?? REGame.Unknown;
}
