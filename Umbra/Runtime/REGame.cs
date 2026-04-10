namespace Umbra.Runtime;

/// <summary>
/// Identifies the RE Engine games currently recognized by Umbra runtime metadata.
/// </summary>
/// <remarks>
/// <see cref="GameContext"/> returns these values after matching the current process name against Umbra's embedded game metadata. <see cref="Unknown"/> is the fallback when detection fails or the current process is not represented in the metadata.
/// </remarks>
public enum REGame
{
    /// <summary>
    /// Represents an unknown or unsupported game target.
    /// </summary>
    Unknown,

    /// <summary>
    /// Resident Evil 2 (2019).
    /// </summary>
    RE2,

    /// <summary>
    /// Resident Evil 3 (2020).
    /// </summary>
    RE3,

    /// <summary>
    /// Resident Evil 4 (2023).
    /// </summary>
    RE4,

    /// <summary>
    /// Resident Evil 7.
    /// </summary>
    RE7,

    /// <summary>
    /// Resident Evil Village.
    /// </summary>
    RE8,

    /// <summary>
    /// Resident Evil Requiem.
    /// </summary>
    RE9,

    /// <summary>
    /// Devil May Cry 5.
    /// </summary>
    DMC5,

    /// <summary>
    /// Street Fighter 6.
    /// </summary>
    SF6,

    /// <summary>
    /// Monster Hunter Rise.
    /// </summary>
    MHRISE,

    /// <summary>
    /// Monster Hunter Wilds.
    /// </summary>
    MHWILDS,

    /// <summary>
    /// Monster Hunter Stories 3: Twisted Reflection.
    /// </summary>
    MHSTORIES3,

    /// <summary>
    /// Dragon's Dogma 2.
    /// </summary>
    DD2,

    /// <summary>
    /// PRAGMATA.
    /// </summary>
    PRAGMATA,

    /// <summary>
    /// Mega Man Star Force Legacy Collection.
    /// </summary>
    STARFORCE,
}
