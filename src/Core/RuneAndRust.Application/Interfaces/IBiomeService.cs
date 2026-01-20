using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for biome management and selection.
/// </summary>
public interface IBiomeService
{
    /// <summary>
    /// Gets a biome by its string ID.
    /// </summary>
    BiomeDefinition? GetBiome(string biomeId);

    /// <summary>
    /// Gets all biomes valid for the specified depth.
    /// </summary>
    IReadOnlyList<BiomeDefinition> GetBiomesForDepth(int depth);

    /// <summary>
    /// Gets all biomes with the specified tags.
    /// </summary>
    /// <param name="tags">Tags to match.</param>
    /// <param name="matchAll">If true, biome must have all tags; if false, any tag.</param>
    IReadOnlyList<BiomeDefinition> GetBiomesByTags(IEnumerable<string> tags, bool matchAll = true);

    /// <summary>
    /// Selects a biome for a position using weighted random selection.
    /// </summary>
    BiomeDefinition SelectBiomeForPosition(Position3D position);

    /// <summary>
    /// Gets a random descriptor from a biome's descriptor pool.
    /// </summary>
    string? GetRandomDescriptor(string biomeId, string category, Position3D position);

    /// <summary>
    /// Registers a biome definition.
    /// </summary>
    void RegisterBiome(BiomeDefinition biome);

    /// <summary>
    /// Gets all registered biomes.
    /// </summary>
    IReadOnlyList<BiomeDefinition> GetAllBiomes();
}
