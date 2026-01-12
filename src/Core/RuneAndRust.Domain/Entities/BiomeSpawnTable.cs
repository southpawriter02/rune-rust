using RuneAndRust.Domain.Interfaces;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Domain.Entities;

/// <summary>
/// Represents spawn tables and loot modifiers for a specific biome.
/// </summary>
public class BiomeSpawnTable : IEntity
{
    /// <summary>
    /// Gets the unique identifier.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the biome ID this table belongs to.
    /// </summary>
    public string BiomeId { get; private set; }

    /// <summary>
    /// Gets the monster spawn pool.
    /// </summary>
    public IReadOnlyList<SpawnEntry> MonsterPool { get; private set; }

    /// <summary>
    /// Gets the item spawn pool.
    /// </summary>
    public IReadOnlyList<SpawnEntry> ItemPool { get; private set; }

    /// <summary>
    /// Gets exclusive items that only drop in this biome.
    /// </summary>
    public IReadOnlyList<string> ExclusiveItems { get; private set; }

    /// <summary>
    /// Gets crafting materials available in this biome.
    /// </summary>
    public IReadOnlyList<string> CraftingMaterials { get; private set; }

    /// <summary>
    /// Gets the loot modifiers for this biome.
    /// </summary>
    public LootModifiers LootModifiers { get; private set; }

    /// <summary>
    /// Private constructor for EF Core.
    /// </summary>
    private BiomeSpawnTable()
    {
        BiomeId = null!;
        MonsterPool = [];
        ItemPool = [];
        ExclusiveItems = [];
        CraftingMaterials = [];
        LootModifiers = LootModifiers.Default;
    }

    /// <summary>
    /// Creates a new biome spawn table.
    /// </summary>
    public static BiomeSpawnTable Create(
        string biomeId,
        IReadOnlyList<SpawnEntry>? monsterPool = null,
        IReadOnlyList<SpawnEntry>? itemPool = null,
        IReadOnlyList<string>? exclusiveItems = null,
        IReadOnlyList<string>? craftingMaterials = null,
        LootModifiers? lootModifiers = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(biomeId);

        return new BiomeSpawnTable
        {
            Id = Guid.NewGuid(),
            BiomeId = biomeId.ToLowerInvariant(),
            MonsterPool = monsterPool ?? [],
            ItemPool = itemPool ?? [],
            ExclusiveItems = exclusiveItems ?? [],
            CraftingMaterials = craftingMaterials ?? [],
            LootModifiers = lootModifiers ?? LootModifiers.Default
        };
    }

    /// <summary>
    /// Gets monsters valid for the specified depth.
    /// </summary>
    public IReadOnlyList<SpawnEntry> GetValidMonsters(int depth) =>
        MonsterPool.Where(e => e.IsValidForDepth(depth)).ToList();

    /// <summary>
    /// Gets monsters valid for the specified depth and context tags.
    /// </summary>
    public IReadOnlyList<SpawnEntry> GetValidMonsters(int depth, IEnumerable<string> contextTags) =>
        MonsterPool.Where(e => e.IsValidForDepth(depth) && e.HasRequiredTags(contextTags)).ToList();

    /// <summary>
    /// Gets items valid for the specified depth.
    /// </summary>
    public IReadOnlyList<SpawnEntry> GetValidItems(int depth) =>
        ItemPool.Where(e => e.IsValidForDepth(depth)).ToList();

    /// <summary>
    /// Gets items valid for the specified depth and context tags.
    /// </summary>
    public IReadOnlyList<SpawnEntry> GetValidItems(int depth, IEnumerable<string> contextTags) =>
        ItemPool.Where(e => e.IsValidForDepth(depth) && e.HasRequiredTags(contextTags)).ToList();
}
