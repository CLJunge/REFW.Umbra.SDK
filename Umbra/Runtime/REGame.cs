namespace Umbra.Runtime;

/// <summary>
/// Specifies the supported games for REFramework integration.
/// </summary>
/// <remarks>
/// This enumeration is used to identify the target game when working with REFramework-based plugins or
/// tools. Each value corresponds to a specific game title supported by REFramework.
/// <para>
/// The "Unknown" value serves as a default for cases where the game cannot be determined or is not supported./
/// </para>
/// </remarks>
public enum REGame
{
    /// <summary>
    /// Represents an unknown or unspecified value.
    /// </summary>
    Unknown,

    /// <summary>
    /// Resident Evil 2 (2019)
    /// </summary>
    RE2,

    /// <summary>
    /// Resident Evil 3 (2020)
    /// </summary>
    RE3,

    /// <summary>
    /// Resident Evil 4 (2023)
    /// </summary>
    RE4,

    /// <summary>
    /// Resident Evil 7
    /// </summary>
    RE7,

    /// <summary>
    /// Resident Evil Village
    /// </summary>
    RE8,

    /// <summary>
    /// Resident Evil Requiem
    /// </summary>
    RE9,

    /// <summary>
    /// Devil May Cry 5
    /// </summary>
    DMC5,

    /// <summary>
    /// Street Fighter 6
    /// </summary>
    SF6,

    /// <summary>
    /// Monster Hunter Rise
    /// </summary>
    MHRISE,

    /// <summary>
    /// Monster Hunter Wilds
    /// </summary>
    MHWILDS,

    /// <summary>
    /// Monster Hunter Stories 3: Twisted Reflection
    /// </summary>
    MHSTORIES3,

    /// <summary>
    /// Dragon's Dogma 2
    /// </summary>
    DD2,

    /// <summary>
    /// PRAGMATA
    /// </summary>
    PRAGMATA,

    /// <summary>
    /// Mega Man Star Force Legacy Collection
    /// </summary>
    STARFORCE,
}
