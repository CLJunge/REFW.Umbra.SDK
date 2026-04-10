namespace Umbra.Runtime;

/// <summary>
/// Provides user-facing display-name helpers for <see cref="REGame"/> values.
/// </summary>
/// <remarks>
/// These helpers convert supported concrete game identifiers into display titles. Callers should handle <see cref="REGame.Unknown"/> before using them because runtime detection can legitimately return that fallback value.
/// </remarks>
public static class REGameExtensions
{
    /// <summary>
    /// Returns the user-facing title for a supported concrete <see cref="REGame"/> value.
    /// </summary>
    /// <param name="game">The game identifier to convert.</param>
    /// <returns>The display title associated with <paramref name="game"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="game"/> is <see cref="REGame.Unknown"/> or any other unmapped value.</exception>
    public static string GetDisplayName(this REGame game)
    {
        return game switch
        {
            REGame.RE2 => "Resident Evil 2 (2019)",
            REGame.RE3 => "Resident Evil 3 (2020)",
            REGame.RE4 => "Resident Evil 4 (2023)",
            REGame.RE7 => "Resident Evil 7",
            REGame.RE8 => "Resident Evil Village",
            REGame.RE9 => "Resident Evil Requiem",
            REGame.DMC5 => "Devil May Cry 5",
            REGame.SF6 => "Street Fighter 6",
            REGame.MHRISE => "Monster Hunter Rise",
            REGame.MHWILDS => "Monster Hunter Wilds",
            REGame.MHSTORIES3 => "Monster Hunter Stories 3: Twisted Reflection",
            REGame.DD2 => "Dragon's Dogma 2",
            REGame.PRAGMATA => "PRAGMATA",
            REGame.STARFORCE => "Mega Man Star Force Legacy Collection",
            _ => throw new ArgumentOutOfRangeException(nameof(game), $"Unsupported game: {game}")
        };
    }
}
