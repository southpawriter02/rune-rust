using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for biome-specific spawn table management.
/// </summary>
public class BiomeSpawnTableService : IBiomeSpawnTableService
{
    private readonly Dictionary<string, BiomeSpawnTable> _tables = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISeededRandomService _random;
    private readonly ILogger<BiomeSpawnTableService> _logger;
    private readonly IGameEventLogger? _eventLogger;

    public BiomeSpawnTableService(
        ISeededRandomService random,
        ILogger<BiomeSpawnTableService>? logger = null,
        IGameEventLogger? eventLogger = null)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<BiomeSpawnTableService>.Instance;
        _eventLogger = eventLogger;

        RegisterDefaultTables();
        _logger.LogDebug("BiomeSpawnTableService initialized with {Count} tables", _tables.Count);
    }

    /// <inheritdoc/>
    public BiomeSpawnTable? GetSpawnTable(string biomeId) =>
        _tables.TryGetValue(biomeId, out var table) ? table : null;

    /// <inheritdoc/>
    public IReadOnlyList<SpawnEntry> GetValidMonsters(string biomeId, int depth)
    {
        var table = GetSpawnTable(biomeId);
        return table?.GetValidMonsters(depth) ?? [];
    }

    /// <inheritdoc/>
    public string? SelectMonster(string biomeId, Position3D position)
    {
        var table = GetSpawnTable(biomeId);
        if (table == null)
            return null;

        var depth = Math.Abs(position.Z);
        var validMonsters = table.GetValidMonsters(depth);

        if (validMonsters.Count == 0)
            return null;

        var weightedItems = validMonsters.Select(e => (e.EntityId, e.Weight)).ToList();
        var selected = _random.SelectWeighted(position, weightedItems, "monster_selection");

        _eventLogger?.LogAI("MonsterSelected", $"Selected {selected} for {biomeId}",
            data: new Dictionary<string, object>
            {
                ["monsterId"] = selected,
                ["biomeId"] = biomeId,
                ["depth"] = depth,
                ["position"] = position.ToString()
            });

        return selected;
    }

    /// <inheritdoc/>
    public string? SelectItem(string biomeId, Position3D position)
    {
        var table = GetSpawnTable(biomeId);
        if (table == null)
            return null;

        var depth = Math.Abs(position.Z);
        var validItems = table.GetValidItems(depth);

        if (validItems.Count == 0)
            return null;

        var weightedItems = validItems.Select(e => (e.EntityId, e.Weight)).ToList();
        var selected = _random.SelectWeighted(position, weightedItems, "item_selection");

        _eventLogger?.LogInventory("ItemSelected", $"Selected {selected} for {biomeId}",
            data: new Dictionary<string, object>
            {
                ["itemId"] = selected,
                ["biomeId"] = biomeId,
                ["depth"] = depth,
                ["position"] = position.ToString()
            });

        return selected;
    }

    /// <inheritdoc/>
    public LootModifiers GetLootModifiers(string biomeId)
    {
        var table = GetSpawnTable(biomeId);
        return table?.LootModifiers ?? LootModifiers.Default;
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetExclusiveItems(string biomeId)
    {
        var table = GetSpawnTable(biomeId);
        return table?.ExclusiveItems ?? [];
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetCraftingMaterials(string biomeId)
    {
        var table = GetSpawnTable(biomeId);
        return table?.CraftingMaterials ?? [];
    }

    /// <inheritdoc/>
    public void RegisterSpawnTable(BiomeSpawnTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        _tables[table.BiomeId] = table;
        _logger.LogDebug("Registered spawn table: {BiomeId}", table.BiomeId);
    }

    private void RegisterDefaultTables()
    {
        // Stone Corridors
        RegisterSpawnTable(BiomeSpawnTable.Create(
            "stone-corridors",
            monsterPool:
            [
                SpawnEntry.Create("giant-rat", weight: 100, minDepth: 0, maxDepth: 2),
                SpawnEntry.Create("skeleton", weight: 80, minDepth: 0),
                SpawnEntry.Create("zombie", weight: 60, minDepth: 1)
            ],
            itemPool:
            [
                SpawnEntry.Create("health-potion", weight: 100),
                SpawnEntry.Create("rusty-sword", weight: 50),
                SpawnEntry.Create("old-key", weight: 30)
            ],
            craftingMaterials: ["bone-fragment", "rusty-metal"],
            lootModifiers: LootModifiers.Default));

        // Fungal Caverns
        RegisterSpawnTable(BiomeSpawnTable.Create(
            "fungal-caverns",
            monsterPool:
            [
                SpawnEntry.Create("spore-crawler", weight: 100, minDepth: 2, maxDepth: 5),
                SpawnEntry.Create("fungal-beast", weight: 70, minDepth: 3),
                SpawnEntry.Create("myconid", weight: 50, minDepth: 4)
            ],
            itemPool:
            [
                SpawnEntry.Create("antidote", weight: 80),
                SpawnEntry.Create("glowing-mushroom", weight: 100),
                SpawnEntry.Create("spore-mask", weight: 30)
            ],
            exclusiveItems: ["bioluminescent-orb"],
            craftingMaterials: ["fungal-spore", "luminescent-cap"],
            lootModifiers: LootModifiers.Create(dropRateMultiplier: 1.2f)));

        // Flooded Depths
        RegisterSpawnTable(BiomeSpawnTable.Create(
            "flooded-depths",
            monsterPool:
            [
                SpawnEntry.Create("drowned-one", weight: 100, minDepth: 4),
                SpawnEntry.Create("deep-lurker", weight: 70, minDepth: 5),
                SpawnEntry.Create("water-elemental", weight: 40, minDepth: 6)
            ],
            itemPool:
            [
                SpawnEntry.Create("waterbreathing-potion", weight: 60),
                SpawnEntry.Create("corroded-coin", weight: 100),
                SpawnEntry.Create("pearl", weight: 20)
            ],
            exclusiveItems: ["trident-of-the-deep"],
            craftingMaterials: ["waterlogged-wood", "deep-pearl"],
            lootModifiers: LootModifiers.Create(goldMultiplier: 1.5f, rareChanceBonus: 0.1f)));
    }
}
