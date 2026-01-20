using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Service for biome-specific spawn table management.
/// </summary>
public interface IBiomeSpawnTableService
{
    /// <summary>
    /// Gets a spawn table by biome ID.
    /// </summary>
    BiomeSpawnTable? GetSpawnTable(string biomeId);

    /// <summary>
    /// Gets monsters valid for the specified biome and depth.
    /// </summary>
    IReadOnlyList<SpawnEntry> GetValidMonsters(string biomeId, int depth);

    /// <summary>
    /// Selects a monster from the biome's pool using weighted selection.
    /// </summary>
    string? SelectMonster(string biomeId, Position3D position);

    /// <summary>
    /// Selects an item from the biome's pool using weighted selection.
    /// </summary>
    string? SelectItem(string biomeId, Position3D position);

    /// <summary>
    /// Gets loot modifiers for a biome.
    /// </summary>
    LootModifiers GetLootModifiers(string biomeId);

    /// <summary>
    /// Gets exclusive items for a biome.
    /// </summary>
    IReadOnlyList<string> GetExclusiveItems(string biomeId);

    /// <summary>
    /// Gets crafting materials for a biome.
    /// </summary>
    IReadOnlyList<string> GetCraftingMaterials(string biomeId);

    /// <summary>
    /// Registers a spawn table.
    /// </summary>
    void RegisterSpawnTable(BiomeSpawnTable table);
}
