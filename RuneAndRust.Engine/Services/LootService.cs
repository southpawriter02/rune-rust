using Microsoft.Extensions.Logging;
using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Interfaces;
using RuneAndRust.Core.Models;
using CharacterAttribute = RuneAndRust.Core.Enums.Attribute;

namespace RuneAndRust.Engine.Services;

/// <summary>
/// Implements procedural loot generation based on biome, danger level, and quality tiers.
/// Uses weighted random selection with WITS-based quality bonuses.
/// </summary>
public class LootService : ILootService
{
    private readonly ILogger<LootService> _logger;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="LootService"/> class.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    public LootService(ILogger<LootService> logger)
    {
        _logger = logger;
        _random = new Random();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LootService"/> class with a seeded random.
    /// Used for testing to ensure deterministic results.
    /// </summary>
    /// <param name="logger">The logger for traceability.</param>
    /// <param name="seed">The random seed for deterministic generation.</param>
    public LootService(ILogger<LootService> logger, int seed)
    {
        _logger = logger;
        _random = new Random(seed);
    }

    /// <inheritdoc/>
    public LootResult GenerateLoot(LootGenerationContext context)
    {
        _logger.LogInformation("Generating loot for Biome={Biome}, Danger={Danger}, WitsBonus={Wits}",
            context.BiomeType, context.DangerLevel, context.WitsBonus);

        var itemCount = RollItemCount(context.DangerLevel);
        _logger.LogDebug("Rolling {ItemCount} items for loot", itemCount);

        var items = new List<Item>();
        for (int i = 0; i < itemCount; i++)
        {
            var quality = context.LootTier ?? RollQualityTier(context.DangerLevel, context.WitsBonus);
            var itemType = RollItemType(context.BiomeType);
            var item = GenerateItem(quality, context.BiomeType, itemType);

            items.Add(item);
            _logger.LogDebug("Generated item: {ItemName} ({Quality} {Type})",
                item.Name, item.Quality, item.ItemType);
        }

        var message = BuildLootMessage(items);
        var result = LootResult.Found(message, items.AsReadOnly());

        _logger.LogInformation("Loot generation complete: {ItemCount} items, {TotalValue} Scrip value",
            items.Count, result.TotalValue);

        return result;
    }

    /// <inheritdoc/>
    public async Task<LootResult> SearchContainerAsync(InteractableObject container, LootGenerationContext context)
    {
        _logger.LogInformation("Searching container '{ContainerName}' for loot", container.Name);

        if (container.HasBeenSearched)
        {
            _logger.LogDebug("Container '{ContainerName}' has already been searched", container.Name);
            return LootResult.Empty($"You've already searched the {container.Name}. Nothing else remains.");
        }

        if (!container.IsContainer)
        {
            _logger.LogDebug("Object '{ObjectName}' is not a container", container.Name);
            return LootResult.Failure($"The {container.Name} cannot be searched.");
        }

        if (!container.IsOpen)
        {
            _logger.LogDebug("Container '{ContainerName}' is closed", container.Name);
            return LootResult.Failure($"The {container.Name} is closed. Open it first.");
        }

        // Use the container's loot tier if specified
        var effectiveContext = container.LootTier.HasValue
            ? context with { LootTier = container.LootTier }
            : context;

        var result = GenerateLoot(effectiveContext);

        // Mark as searched - note: caller should persist this change
        container.HasBeenSearched = true;
        container.LastModified = DateTime.UtcNow;

        _logger.LogInformation("Container '{ContainerName}' searched, found {ItemCount} items",
            container.Name, result.Items.Count);

        // Await to satisfy async signature (for future DB persistence)
        await Task.CompletedTask;

        return result;
    }

    /// <inheritdoc/>
    public Dictionary<QualityTier, int> GetQualityWeights(DangerLevel dangerLevel)
    {
        if (LootTables.QualityWeightsByDanger.TryGetValue(dangerLevel, out var weights))
        {
            return new Dictionary<QualityTier, int>(weights);
        }

        _logger.LogWarning("No quality weights defined for danger level {DangerLevel}, using Safe defaults",
            dangerLevel);
        return new Dictionary<QualityTier, int>(LootTables.QualityWeightsByDanger[DangerLevel.Safe]);
    }

    /// <inheritdoc/>
    public QualityTier RollQualityTier(DangerLevel dangerLevel, int witsBonus = 0)
    {
        _logger.LogTrace("Rolling quality tier for Danger={Danger}, WitsBonus={Wits}",
            dangerLevel, witsBonus);

        var weights = GetQualityWeights(dangerLevel);

        // Apply WITS bonus: shifts weight toward higher quality
        // Each point of WITS bonus adds 2% chance to upgrade quality
        var upgradeChance = Math.Min(witsBonus * 2, 20); // Cap at 20%

        if (upgradeChance > 0 && _random.Next(100) < upgradeChance)
        {
            _logger.LogDebug("WITS bonus triggered quality upgrade");
            // Shift weights toward higher tiers
            ShiftWeightsUp(weights);
        }

        var roll = _random.Next(100);
        var cumulative = 0;

        foreach (var tier in Enum.GetValues<QualityTier>().OrderBy(t => (int)t))
        {
            if (weights.TryGetValue(tier, out var weight))
            {
                cumulative += weight;
                if (roll < cumulative)
                {
                    _logger.LogDebug("Quality roll {Roll} resulted in tier {Tier}", roll, tier);
                    return tier;
                }
            }
        }

        _logger.LogDebug("Quality roll defaulted to Scavenged");
        return QualityTier.Scavenged;
    }

    /// <inheritdoc/>
    public int RollItemCount(DangerLevel dangerLevel)
    {
        if (LootTables.ItemCountsByDanger.TryGetValue(dangerLevel, out var range))
        {
            var count = _random.Next(range.Min, range.Max + 1);
            _logger.LogTrace("Item count roll: {Count} (range {Min}-{Max})",
                count, range.Min, range.Max);
            return count;
        }

        _logger.LogWarning("No item count range defined for danger level {DangerLevel}", dangerLevel);
        return 1;
    }

    /// <inheritdoc/>
    public Item GenerateItem(QualityTier quality, BiomeType biome, ItemType? preferredType = null)
    {
        var itemType = preferredType ?? RollItemType(biome);

        _logger.LogTrace("Generating {Quality} {ItemType} item for {Biome} biome",
            quality, itemType, biome);

        return itemType switch
        {
            ItemType.Weapon => GenerateWeapon(quality),
            ItemType.Armor => GenerateArmor(quality),
            ItemType.Consumable => GenerateConsumable(quality),
            ItemType.Material => GenerateMaterial(quality),
            ItemType.Junk => GenerateJunk(),
            _ => GenerateJunk()
        };
    }

    #region Private Methods

    private ItemType RollItemType(BiomeType biome)
    {
        if (!LootTables.ItemTypeByBiome.TryGetValue(biome, out var weights))
        {
            _logger.LogWarning("No item type weights for biome {Biome}, using Ruin defaults", biome);
            weights = LootTables.ItemTypeByBiome[BiomeType.Ruin];
        }

        var roll = _random.Next(100);
        var cumulative = 0;

        foreach (var type in weights.OrderBy(kv => (int)kv.Key))
        {
            cumulative += type.Value;
            if (roll < cumulative)
            {
                return type.Key;
            }
        }

        return ItemType.Junk;
    }

    private void ShiftWeightsUp(Dictionary<QualityTier, int> weights)
    {
        // Take some weight from lower tiers and add to higher
        var juryRiggedReduction = Math.Min(weights.GetValueOrDefault(QualityTier.JuryRigged), 10);
        var scavengedReduction = Math.Min(weights.GetValueOrDefault(QualityTier.Scavenged), 5);

        weights[QualityTier.JuryRigged] = weights.GetValueOrDefault(QualityTier.JuryRigged) - juryRiggedReduction;
        weights[QualityTier.Scavenged] = weights.GetValueOrDefault(QualityTier.Scavenged) - scavengedReduction;
        weights[QualityTier.ClanForged] = weights.GetValueOrDefault(QualityTier.ClanForged) + juryRiggedReduction / 2;
        weights[QualityTier.Optimized] = weights.GetValueOrDefault(QualityTier.Optimized) + juryRiggedReduction / 2 + scavengedReduction;
    }

    private Equipment GenerateWeapon(QualityTier quality)
    {
        if (!LootTables.WeaponsByQuality.TryGetValue(quality, out var templates) || templates.Count == 0)
        {
            // Fall back to Scavenged if quality has no weapons
            templates = LootTables.WeaponsByQuality[QualityTier.Scavenged];
        }

        var template = templates[_random.Next(templates.Count)];

        return new Equipment
        {
            Name = template.Name,
            Description = template.Description,
            DetailedDescription = template.DetailedDescription,
            ItemType = ItemType.Weapon,
            Quality = quality,
            Slot = template.Slot,
            DamageDie = template.DamageDie,
            Weight = template.Weight,
            Value = ScaleValueByQuality(template.Value, quality)
        };
    }

    private Equipment GenerateArmor(QualityTier quality)
    {
        if (!LootTables.ArmorByQuality.TryGetValue(quality, out var templates) || templates.Count == 0)
        {
            templates = LootTables.ArmorByQuality[QualityTier.Scavenged];
        }

        var template = templates[_random.Next(templates.Count)];

        return new Equipment
        {
            Name = template.Name,
            Description = template.Description,
            DetailedDescription = template.DetailedDescription,
            ItemType = ItemType.Armor,
            Quality = quality,
            Slot = template.Slot,
            SoakBonus = template.SoakBonus,
            Weight = template.Weight,
            Value = ScaleValueByQuality(template.Value, quality)
        };
    }

    private Item GenerateConsumable(QualityTier quality)
    {
        if (!LootTables.ConsumablesByQuality.TryGetValue(quality, out var templates) || templates.Count == 0)
        {
            templates = LootTables.ConsumablesByQuality[QualityTier.Scavenged];
        }

        var template = templates[_random.Next(templates.Count)];

        return new Item
        {
            Name = template.Name,
            Description = template.Description,
            DetailedDescription = template.DetailedDescription,
            ItemType = ItemType.Consumable,
            Quality = quality,
            Weight = template.Weight,
            Value = ScaleValueByQuality(template.Value, quality),
            IsStackable = true,
            MaxStackSize = 10
        };
    }

    private Item GenerateMaterial(QualityTier quality)
    {
        if (!LootTables.MaterialsByQuality.TryGetValue(quality, out var templates) || templates.Count == 0)
        {
            templates = LootTables.MaterialsByQuality[QualityTier.Scavenged];
        }

        var template = templates[_random.Next(templates.Count)];

        return new Item
        {
            Name = template.Name,
            Description = template.Description,
            ItemType = ItemType.Material,
            Quality = quality,
            Weight = template.Weight,
            Value = template.Value,
            IsStackable = true,
            MaxStackSize = 99
        };
    }

    private Item GenerateJunk()
    {
        var template = LootTables.JunkItems[_random.Next(LootTables.JunkItems.Count)];

        return new Item
        {
            Name = template.Name,
            Description = template.Description,
            ItemType = ItemType.Junk,
            Quality = QualityTier.JuryRigged,
            Weight = template.Weight,
            Value = template.Value,
            IsStackable = true,
            MaxStackSize = 99
        };
    }

    private static int ScaleValueByQuality(int baseValue, QualityTier quality)
    {
        return quality switch
        {
            QualityTier.JuryRigged => (int)(baseValue * 0.5),
            QualityTier.Scavenged => baseValue,
            QualityTier.ClanForged => (int)(baseValue * 1.5),
            QualityTier.Optimized => baseValue * 2,
            QualityTier.MythForged => baseValue * 3,
            _ => baseValue
        };
    }

    private static string BuildLootMessage(List<Item> items)
    {
        if (items.Count == 0)
        {
            return "You find nothing of value.";
        }

        if (items.Count == 1)
        {
            return $"You find: {items[0].Name}.";
        }

        var names = items.Select(i => i.Name).ToList();
        var lastItem = names.Last();
        var otherItems = string.Join(", ", names.Take(names.Count - 1));
        return $"You find: {otherItems}, and {lastItem}.";
    }

    #endregion
}
