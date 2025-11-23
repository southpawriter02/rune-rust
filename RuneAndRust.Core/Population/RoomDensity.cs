namespace RuneAndRust.Core.Population;

/// <summary>
/// v0.39.3: Room density classification for content distribution
/// Determines how many threats (enemies + hazards) are allocated to each room
/// Creates pacing variety - not every room needs to be combat-heavy
/// </summary>
public enum RoomDensity
{
    /// <summary>
    /// Empty room - 0 threats
    /// Purpose: Breather rooms, exploration, loot focus
    /// Target: 10-15% of rooms
    /// </summary>
    Empty,

    /// <summary>
    /// Light encounter - 1-2 threats
    /// Purpose: Minor encounters, pacing, safe travel
    /// Target: 40-50% of rooms
    /// </summary>
    Light,

    /// <summary>
    /// Medium encounter - 3-4 threats
    /// Purpose: Standard combat, balanced challenge
    /// Target: 25-35% of rooms
    /// </summary>
    Medium,

    /// <summary>
    /// Heavy encounter - 5-7 threats
    /// Purpose: Difficult battles, preparation required
    /// Target: 10-15% of rooms
    /// </summary>
    Heavy,

    /// <summary>
    /// Boss encounter - 8+ threats
    /// Purpose: Climactic battles, sector finale
    /// Target: ~5% of rooms (typically 1 per sector)
    /// </summary>
    Boss
}
