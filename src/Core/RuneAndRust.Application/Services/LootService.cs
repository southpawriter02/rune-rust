using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implementation of loot generation and collection.
/// </summary>
public class LootService : ILootService
{
    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<LootService> _logger;
    private readonly IGameEventLogger? _eventLogger;
    private readonly Random _random;

    /// <summary>
    /// Creates a new loot service.
    /// </summary>
    /// <param name="configProvider">Configuration provider for loot and currency definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="eventLogger">Optional event logger for comprehensive tracking.</param>
    public LootService(
        IGameConfigurationProvider configProvider,
        ILogger<LootService> logger,
        IGameEventLogger? eventLogger = null)
        : this(configProvider, logger, eventLogger, Random.Shared)
    {
    }

    /// <summary>
    /// Creates a new loot service with explicit random source (for testing).
    /// </summary>
    /// <param name="configProvider">Configuration provider for loot and currency definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="eventLogger">Optional event logger for comprehensive tracking.</param>
    /// <param name="random">Random number generator.</param>
    internal LootService(
        IGameConfigurationProvider configProvider,
        ILogger<LootService> logger,
        IGameEventLogger? eventLogger,
        Random random)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
        _random = random ?? throw new ArgumentNullException(nameof(random));
    }

    /// <inheritdoc/>
    public LootDrop GenerateLoot(Monster monster)
    {
        if (monster == null)
        {
            _logger.LogWarning("Attempted to generate loot for null monster");
            return LootDrop.Empty;
        }

        // Get the monster definition to access the loot table
        if (string.IsNullOrEmpty(monster.MonsterDefinitionId))
        {
            _logger.LogDebug("Monster has no definition ID: {MonsterName}", monster.Name);
            return LootDrop.Empty;
        }

        var definition = _configProvider.GetMonsterById(monster.MonsterDefinitionId);
        if (definition?.LootTable == null)
        {
            _logger.LogDebug("No loot table found for monster: {MonsterName} (ID: {MonsterId})",
                monster.Name, monster.MonsterDefinitionId);
            return LootDrop.Empty;
        }

        return GenerateLoot(definition, monster.LootMultiplier);
    }

    /// <inheritdoc/>
    public LootDrop GenerateLoot(MonsterDefinition definition, float lootMultiplier = 1.0f)
    {
        if (definition?.LootTable == null || !definition.LootTable.HasEntries)
        {
            return LootDrop.Empty;
        }

        var lootTable = definition.LootTable;
        var items = GenerateItemDrops(lootTable.Entries, lootMultiplier);
        var currency = GenerateCurrencyDrops(lootTable.CurrencyDrops, lootMultiplier);

        var loot = LootDrop.Create(items, currency);

        _logger.LogDebug("Generated loot for {MonsterName}: {ItemCount} items, {CurrencyTypes} currency types",
            definition.Name, items.Count, currency.Count);

        _eventLogger?.LogInventory("LootGenerated", $"Loot dropped from {definition.Name}",
            data: new Dictionary<string, object>
            {
                ["monsterName"] = definition.Name,
                ["itemCount"] = items.Count,
                ["currencyTypes"] = currency.Count,
                ["items"] = items.Select(i => $"{i.Quantity}x {i.Name}").ToList(),
                ["currency"] = currency.Select(c => $"{c.Value} {c.Key}").ToList(),
                ["lootMultiplier"] = lootMultiplier
            });

        return loot;
    }

    /// <inheritdoc/>
    public LootDrop CollectLoot(Player player, Room room)
    {
        if (player == null)
            throw new ArgumentNullException(nameof(player));
        if (room == null)
            throw new ArgumentNullException(nameof(room));

        if (!room.HasDroppedLoot)
        {
            return LootDrop.Empty;
        }

        var collectedLoot = room.CollectAllLoot();

        // Add currency to player
        if (collectedLoot.HasCurrency)
        {
            foreach (var kvp in collectedLoot.Currency)
            {
                player.AddCurrency(kvp.Key, kvp.Value);
                _logger.LogDebug("Player collected {Amount} {Currency}", kvp.Value, kvp.Key);
            }
        }

        // Note: Items are returned in the LootDrop for display
        // The caller can add them to player inventory if desired
        _logger.LogInformation("Player collected loot: {ItemCount} items, {CurrencyCount} currency types",
            collectedLoot.Items?.Count ?? 0,
            collectedLoot.Currency?.Count ?? 0);

        _eventLogger?.LogInventory("LootCollected", $"Player collected loot",
            data: new Dictionary<string, object>
            {
                ["playerName"] = player.Name,
                ["itemCount"] = collectedLoot.Items?.Count ?? 0,
                ["currencyCount"] = collectedLoot.Currency?.Count ?? 0
            });

        return collectedLoot;
    }

    /// <inheritdoc/>
    public CurrencyDefinition? GetCurrency(string currencyId)
    {
        if (string.IsNullOrWhiteSpace(currencyId))
            return null;

        return _configProvider.GetCurrencyById(currencyId.ToLowerInvariant());
    }

    /// <inheritdoc/>
    public IReadOnlyList<CurrencyDefinition> GetAllCurrencies()
    {
        return _configProvider.GetCurrencies();
    }

    private List<DroppedItem> GenerateItemDrops(IReadOnlyList<LootEntry> entries, float lootMultiplier)
    {
        var items = new List<DroppedItem>();

        foreach (var entry in entries)
        {
            // Apply multiplier to drop chance (capped at 100%)
            var adjustedChance = Math.Min(entry.DropChance * lootMultiplier, 1.0f);

            if (_random.NextDouble() > adjustedChance)
                continue;

            // Roll quantity
            var quantity = entry.MinQuantity == entry.MaxQuantity
                ? entry.MinQuantity
                : _random.Next(entry.MinQuantity, entry.MaxQuantity + 1);

            if (quantity <= 0)
                continue;

            // Get item display name (for now, use the item ID as name)
            var itemName = GetItemDisplayName(entry.ItemId);
            items.Add(DroppedItem.Create(entry.ItemId, itemName, quantity));

            _logger.LogDebug("Generated item drop: {Quantity}x {ItemName}", quantity, itemName);
        }

        return items;
    }

    private Dictionary<string, int> GenerateCurrencyDrops(IReadOnlyList<CurrencyDrop> currencyDrops, float lootMultiplier)
    {
        var currency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var drop in currencyDrops)
        {
            // Apply multiplier to drop chance (capped at 100%)
            var adjustedChance = Math.Min(drop.DropChance * lootMultiplier, 1.0f);

            if (_random.NextDouble() > adjustedChance)
                continue;

            // Roll amount with multiplier applied
            var baseAmount = drop.MinAmount == drop.MaxAmount
                ? drop.MinAmount
                : _random.Next(drop.MinAmount, drop.MaxAmount + 1);

            var adjustedAmount = (int)Math.Ceiling(baseAmount * lootMultiplier);

            if (adjustedAmount <= 0)
                continue;

            if (currency.TryGetValue(drop.CurrencyId, out var existing))
            {
                currency[drop.CurrencyId] = existing + adjustedAmount;
            }
            else
            {
                currency[drop.CurrencyId] = adjustedAmount;
            }

            _logger.LogDebug("Generated currency drop: {Amount} {Currency}", adjustedAmount, drop.CurrencyId);
        }

        return currency;
    }

    private string GetItemDisplayName(string itemId)
    {
        // TODO: Look up item definition to get proper display name
        // For now, convert ID to title case (e.g., "health_potion" -> "Health Potion")
        if (string.IsNullOrWhiteSpace(itemId))
            return "Unknown Item";

        return string.Join(" ",
            itemId.Split('_', '-')
                .Select(word => char.ToUpper(word[0]) + word[1..].ToLower()));
    }
}
