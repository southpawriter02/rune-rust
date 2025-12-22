using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository interface for BiomeElement entities with specialized query methods.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public interface IBiomeElementRepository : IRepository<BiomeElement>
{
    /// <summary>
    /// Retrieves all biome elements for a specific biome.
    /// </summary>
    /// <param name="biomeId">The biome ID (e.g., "the_roots").</param>
    /// <returns>Collection of biome elements for the specified biome.</returns>
    Task<IEnumerable<BiomeElement>> GetByBiomeIdAsync(string biomeId);

    /// <summary>
    /// Retrieves all biome elements of a specific type.
    /// </summary>
    /// <param name="elementType">The element type (e.g., "DynamicHazard", "DormantProcess", "StaticTerrain").</param>
    /// <returns>Collection of biome elements with the specified type.</returns>
    Task<IEnumerable<BiomeElement>> GetByElementTypeAsync(string elementType);

    /// <summary>
    /// Retrieves biome elements filtered by both biome ID and element type.
    /// </summary>
    /// <param name="biomeId">The biome ID.</param>
    /// <param name="elementType">The element type.</param>
    /// <returns>Collection of biome elements matching both criteria.</returns>
    Task<IEnumerable<BiomeElement>> GetByBiomeAndTypeAsync(string biomeId, string elementType);
}
