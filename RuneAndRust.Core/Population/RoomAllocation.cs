namespace RuneAndRust.Core.Population;

/// <summary>
/// v0.39.3: Budget allocation for a single room
/// Replaces per-room budget calculation with pre-allocated limits
/// </summary>
public class RoomAllocation
{
    /// <summary>
    /// Room identifier this allocation applies to
    /// </summary>
    public string RoomId { get; set; } = string.Empty;

    /// <summary>
    /// Number of enemies allocated to this room
    /// Spawner must not exceed this limit
    /// </summary>
    public int AllocatedEnemies { get; set; } = 0;

    /// <summary>
    /// Number of hazards allocated to this room
    /// Spawner must not exceed this limit
    /// </summary>
    public int AllocatedHazards { get; set; } = 0;

    /// <summary>
    /// Number of loot nodes allocated to this room
    /// Spawner must not exceed this limit
    /// </summary>
    public int AllocatedLoot { get; set; } = 0;

    /// <summary>
    /// Density classification for this room
    /// Determines target threat count
    /// </summary>
    public RoomDensity Density { get; set; } = RoomDensity.Light;

    /// <summary>
    /// Total threats allocated (enemies + hazards)
    /// </summary>
    public int TotalThreats => AllocatedEnemies + AllocatedHazards;
}
