namespace RuneAndRust.Core.Population;

/// <summary>
/// v0.39.3: Complete population plan for a sector
/// Contains budget allocations for all rooms
/// </summary>
public class SectorPopulationPlan
{
    /// <summary>
    /// Budget allocations indexed by room ID
    /// </summary>
    public Dictionary<string, RoomAllocation> RoomAllocations { get; set; } = new();

    /// <summary>
    /// Total enemies allocated across all rooms
    /// </summary>
    public int TotalEnemiesAllocated => RoomAllocations.Values.Sum(a => a.AllocatedEnemies);

    /// <summary>
    /// Total hazards allocated across all rooms
    /// </summary>
    public int TotalHazardsAllocated => RoomAllocations.Values.Sum(a => a.AllocatedHazards);

    /// <summary>
    /// Total loot nodes allocated across all rooms
    /// </summary>
    public int TotalLootAllocated => RoomAllocations.Values.Sum(a => a.AllocatedLoot);

    /// <summary>
    /// Total threats allocated (enemies + hazards)
    /// </summary>
    public int TotalThreatsAllocated => TotalEnemiesAllocated + TotalHazardsAllocated;

    /// <summary>
    /// Gets allocation for a specific room, or null if not found
    /// </summary>
    public RoomAllocation? GetAllocation(string roomId)
    {
        return RoomAllocations.TryGetValue(roomId, out var allocation) ? allocation : null;
    }
}
