namespace RuneAndRust.Domain.Enums;

/// <summary>
/// Represents the type of content found in dead ends.
/// </summary>
public enum DeadEndContent
{
    /// <summary>
    /// Standard room with no special content.
    /// </summary>
    Empty = 0,

    /// <summary>
    /// Treasure cache (good loot, no combat).
    /// </summary>
    TreasureCache = 1,

    /// <summary>
    /// Monster lair (tough fight, great loot).
    /// </summary>
    MonsterLair = 2,

    /// <summary>
    /// Secret shrine (special interaction/buff).
    /// </summary>
    SecretShrine = 3,

    /// <summary>
    /// Trap room (dangerous, some loot).
    /// </summary>
    TrapRoom = 4
}
