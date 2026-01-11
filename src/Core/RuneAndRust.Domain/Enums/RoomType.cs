namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Categorizes rooms by their primary function and behavior.
/// </summary>
/// <remarks>
/// Room types affect monster spawning rules, entry behaviors,
/// available interactions, and how the room is described to players.
/// </remarks>
public enum RoomType
{
    /// <summary>
    /// Standard room with normal encounters and no special behavior.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Contains valuable loot. May be guarded by monsters.
    /// </summary>
    Treasure = 1,

    /// <summary>
    /// Contains traps that may trigger on entry or interaction.
    /// </summary>
    Trap = 2,

    /// <summary>
    /// Contains a powerful boss monster. Exits may lock during combat.
    /// </summary>
    Boss = 3,

    /// <summary>
    /// Safe haven where monsters never spawn. May offer rest mechanics.
    /// </summary>
    Safe = 4,

    /// <summary>
    /// Religious or magical site with special interactions available.
    /// </summary>
    Shrine = 5
}
