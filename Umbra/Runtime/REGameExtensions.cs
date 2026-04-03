namespace Umbra.Runtime;

/// <summary>
/// Provides extension methods for working with REFramework game enumeration values.
/// </summary>
/// <remarks>
/// This class contains utility methods that extend the REGame enumeration, enabling retrieval
/// of user-friendly display names and other game-specific information. All methods are static and intended for use with
/// REFramework game identifiers.
/// </remarks>
public static class REGameExtensions
{
    /// <summary>
    /// Gets the user-friendly display name for the specified game.
    /// </summary>
    /// <param name="game">The game for which to retrieve the display name.</param>
    /// <returns>A string containing the display name of the specified game.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the specified game value is not supported.</exception>
    public static string GetDisplayName(this REGame game)
    {
        return game switch
        {
            REGame.RE2 => "Resident Evil 2 (2019)",
            REGame.RE3 => "Resident Evil 3 (2020)",
            REGame.RE4 => "Resident Evil 4 (2023)",
            REGame.RE7 => "Resident Evil 7 (2017)",
            REGame.RE8 => "Resident Evil Village (2021)",
            REGame.RE9 => "Resident Evil Requiem (2026)",
            REGame.DMC5 => "Devil May Cry 5 (2019)",
            REGame.SF6 => "Street Fighter 6 (2023)",
            REGame.MHRISE => "Monster Hunter Rise (2021)",
            REGame.MHWILDS => "Monster Hunter Wilds (2025)",
            REGame.MHSTORIES3 => "Monster Hunter Stories 3: Twisted Reflection (2026)",
            REGame.DD2 => "Dragon's Dogma 2 (2024)",
            REGame.PRAGMATA => "PRAGMATA (2026)",
            REGame.STARFORCE => "Mega Man Star Force Legacy Collection (2026)",
            _ => throw new ArgumentOutOfRangeException(nameof(game), $"Unsupported game: {game}")
        };
    }
}
