using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Generates dungeon topology graphs using procedural algorithms.
/// </summary>
public interface ITopologyGenerator
{
    /// <summary>
    /// Generates a sector with connected dungeon nodes.
    /// </summary>
    /// <param name="biome">The biome type for the sector.</param>
    /// <param name="targetRoomCount">Target number of rooms to generate.</param>
    /// <param name="depth">Floor depth for difficulty scaling.</param>
    /// <param name="seed">Optional seed for reproducible generation.</param>
    /// <returns>A sector containing connected dungeon nodes.</returns>
    Sector GenerateSector(
        Biome biome,
        int targetRoomCount,
        int depth,
        int? seed = null);
}
