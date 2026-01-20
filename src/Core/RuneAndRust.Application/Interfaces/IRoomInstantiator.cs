using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Instantiates Room entities from DungeonNodes using templates.
/// </summary>
public interface IRoomInstantiator
{
    /// <summary>
    /// Creates a Room from a DungeonNode by applying a matching template.
    /// </summary>
    /// <param name="node">The dungeon node to instantiate.</param>
    /// <param name="biome">The biome for template selection.</param>
    /// <param name="random">Random instance for deterministic generation.</param>
    /// <returns>A fully configured Room entity.</returns>
    Room InstantiateRoom(DungeonNode node, Biome biome, Random random);

    /// <summary>
    /// Creates all Rooms for a sector.
    /// </summary>
    /// <param name="sector">The sector containing nodes to instantiate.</param>
    /// <returns>Dictionary mapping node IDs to instantiated Rooms.</returns>
    IReadOnlyDictionary<Guid, Room> InstantiateSector(Sector sector);

    /// <summary>
    /// Creates all Rooms for a sector with a specific random seed.
    /// </summary>
    /// <param name="sector">The sector containing nodes to instantiate.</param>
    /// <param name="seed">Random seed for reproducible generation.</param>
    /// <returns>Dictionary mapping node IDs to instantiated Rooms.</returns>
    IReadOnlyDictionary<Guid, Room> InstantiateSector(Sector sector, int seed);
}
