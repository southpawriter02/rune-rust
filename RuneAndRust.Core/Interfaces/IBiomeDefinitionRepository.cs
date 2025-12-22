using RuneAndRust.Core.Entities;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Repository interface for BiomeDefinition entities with specialized query methods.
/// Part of the Dynamic Room Engine (v0.4.0).
/// </summary>
public interface IBiomeDefinitionRepository : IRepository<BiomeDefinition>
{
    /// <summary>
    /// Retrieves a single biome definition by its unique biome ID.
    /// </summary>
    /// <param name="biomeId">The biome ID (e.g., "the_roots").</param>
    /// <returns>The biome definition if found; otherwise, null.</returns>
    Task<BiomeDefinition?> GetByBiomeIdAsync(string biomeId);

    /// <summary>
    /// Retrieves all biome elements associated with a specific biome.
    /// </summary>
    /// <param name="biomeId">The biome ID.</param>
    /// <returns>Collection of biome elements for the specified biome.</returns>
    Task<IEnumerable<BiomeElement>> GetElementsForBiomeAsync(string biomeId);

    /// <summary>
    /// Upserts a biome definition (inserts if new, updates if existing based on BiomeId).
    /// </summary>
    /// <param name="biomeDefinition">The biome definition to upsert.</param>
    Task UpsertAsync(BiomeDefinition biomeDefinition);
}
