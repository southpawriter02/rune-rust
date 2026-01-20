using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Populates rooms with entities (monsters) using the threat budget system.
/// </summary>
public interface IEntityPopulator
{
    /// <summary>
    /// Populates a single room with monsters based on threat budget.
    /// </summary>
    /// <param name="room">The room to populate.</param>
    /// <param name="node">The corresponding dungeon node.</param>
    /// <param name="threatBudget">Available threat budget for this room.</param>
    /// <param name="factionId">The faction to spawn from.</param>
    /// <param name="biome">The biome for entity selection.</param>
    /// <param name="random">Random instance for deterministic generation.</param>
    void PopulateRoom(
        Room room,
        DungeonNode node,
        int threatBudget,
        string factionId,
        Biome biome,
        Random random);

    /// <summary>
    /// Populates all rooms in a sector with monsters.
    /// </summary>
    /// <param name="sector">The sector containing the topology.</param>
    /// <param name="rooms">Dictionary mapping node IDs to rooms.</param>
    /// <param name="random">Random instance for deterministic generation.</param>
    void PopulateSector(
        Sector sector,
        IReadOnlyDictionary<Guid, Room> rooms,
        Random random);

    /// <summary>
    /// Gets the faction ID for a biome.
    /// </summary>
    string GetFactionForBiome(Biome biome);
}
