using Microsoft.Extensions.Logging;
using RuneAndRust.Application.DTOs;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Implementation of loot generation and collection.
/// </summary>
/// <remarks>
/// <para>
/// The LootService handles all aspects of loot generation including:
/// <list type="bullet">
///   <item><description>Item drops from monster loot tables</description></item>
///   <item><description>Currency drops with configurable amounts</description></item>
///   <item><description>Recipe scroll generation based on dungeon level and source (v0.11.1c)</description></item>
///   <item><description>Loot collection and inventory management</description></item>
/// </list>
/// </para>
/// <para>
/// Recipe scroll generation is an optional feature that requires an <see cref="IRecipeScrollProvider"/>
/// and <see cref="IRecipeProvider"/> to be injected. If not available, scroll generation
/// methods will return null or 0 without error.
/// </para>
/// </remarks>
public class LootService : ILootService
{
    // ═══════════════════════════════════════════════════════════════
    // FIELDS
    // ═══════════════════════════════════════════════════════════════

    private readonly IGameConfigurationProvider _configProvider;
    private readonly ILogger<LootService> _logger;
    private readonly IGameEventLogger? _eventLogger;
    private readonly Random _random;

    /// <summary>
    /// Optional provider for recipe scroll configurations (v0.11.1c).
    /// </summary>
    private readonly IRecipeScrollProvider? _scrollProvider;

    /// <summary>
    /// Optional recipe provider for recipe lookups (v0.11.1c).
    /// </summary>
    private readonly IRecipeProvider? _recipeProvider;

    /// <summary>
    /// Optional recipe service for checking known recipes (v0.11.1c).
    /// </summary>
    private readonly IRecipeService? _recipeService;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new loot service.
    /// </summary>
    /// <param name="configProvider">Configuration provider for loot and currency definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="eventLogger">Optional event logger for comprehensive tracking.</param>
    /// <param name="scrollProvider">Optional provider for recipe scroll configurations (v0.11.1c).</param>
    /// <param name="recipeProvider">Optional recipe provider for recipe lookups (v0.11.1c).</param>
    /// <param name="recipeService">Optional recipe service for checking known recipes (v0.11.1c).</param>
    /// <remarks>
    /// The scroll-related dependencies are optional. If not provided, recipe scroll
    /// generation will be disabled but the service will function normally otherwise.
    /// </remarks>
    public LootService(
        IGameConfigurationProvider configProvider,
        ILogger<LootService> logger,
        IGameEventLogger? eventLogger = null,
        IRecipeScrollProvider? scrollProvider = null,
        IRecipeProvider? recipeProvider = null,
        IRecipeService? recipeService = null)
        : this(configProvider, logger, eventLogger, Random.Shared, scrollProvider, recipeProvider, recipeService)
    {
    }

    /// <summary>
    /// Creates a new loot service with explicit random source (for testing).
    /// </summary>
    /// <param name="configProvider">Configuration provider for loot and currency definitions.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <param name="eventLogger">Optional event logger for comprehensive tracking.</param>
    /// <param name="random">Random number generator.</param>
    /// <param name="scrollProvider">Optional provider for recipe scroll configurations (v0.11.1c).</param>
    /// <param name="recipeProvider">Optional recipe provider for recipe lookups (v0.11.1c).</param>
    /// <param name="recipeService">Optional recipe service for checking known recipes (v0.11.1c).</param>
    internal LootService(
        IGameConfigurationProvider configProvider,
        ILogger<LootService> logger,
        IGameEventLogger? eventLogger,
        Random random,
        IRecipeScrollProvider? scrollProvider = null,
        IRecipeProvider? recipeProvider = null,
        IRecipeService? recipeService = null)
    {
        _configProvider = configProvider ?? throw new ArgumentNullException(nameof(configProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventLogger = eventLogger;
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _scrollProvider = scrollProvider;
        _recipeProvider = recipeProvider;
        _recipeService = recipeService;

        // Log initialization status for scroll generation
        if (_scrollProvider != null)
        {
            _logger.LogDebug(
                "LootService initialized with recipe scroll support: " +
                "ScrollProvider={ScrollProviderType}, RecipeProvider={RecipeProviderAvailable}, " +
                "RecipeService={RecipeServiceAvailable}",
                _scrollProvider.GetType().Name,
                _recipeProvider != null,
                _recipeService != null);
        }
        else
        {
            _logger.LogDebug("LootService initialized without recipe scroll support");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // LOOT GENERATION
    // ═══════════════════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════════════════
    // RECIPE SCROLL GENERATION (v0.11.1c)
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// This method implements the recipe scroll drop system introduced in v0.11.1c.
    /// Recipe scrolls are special items that teach players new crafting recipes when used.
    /// </para>
    /// <para>
    /// The generation process:
    /// <list type="number">
    ///   <item><description>Check if scroll provider is available (returns null if not)</description></item>
    ///   <item><description>Roll against the source-specific drop chance</description></item>
    ///   <item><description>Get eligible scrolls based on level and source filters</description></item>
    ///   <item><description>Optionally filter out known recipes if ExcludeKnownRecipes is true</description></item>
    ///   <item><description>Perform weighted selection from eligible scrolls</description></item>
    ///   <item><description>Create and return the scroll item</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public Item? TryGenerateRecipeScroll(LootContext context)
    {
        // ───────────────────────────────────────────────────────────
        // Validate scroll provider is available
        // ───────────────────────────────────────────────────────────
        if (_scrollProvider == null)
        {
            _logger.LogDebug(
                "Recipe scroll generation skipped: No scroll provider configured");
            return null;
        }

        if (_recipeProvider == null)
        {
            _logger.LogDebug(
                "Recipe scroll generation skipped: No recipe provider configured");
            return null;
        }

        _logger.LogDebug(
            "Attempting recipe scroll generation: Source={Source}, DungeonLevel={Level}, " +
            "PlayerProvided={PlayerProvided}, ExcludeKnown={ExcludeKnown}",
            context.Source,
            context.DungeonLevel,
            context.Player != null,
            context.ExcludeKnownRecipes);

        // ───────────────────────────────────────────────────────────
        // Roll against drop chance for this source
        // ───────────────────────────────────────────────────────────
        var dropChance = _scrollProvider.GetDropChance(context.Source);
        if (dropChance <= 0)
        {
            _logger.LogDebug(
                "Recipe scroll generation skipped: No drop chance configured for source {Source}",
                context.Source);
            return null;
        }

        var roll = (decimal)_random.NextDouble();
        if (roll >= dropChance)
        {
            _logger.LogDebug(
                "Recipe scroll drop check failed: Roll={Roll:F4}, DropChance={DropChance:F4}, Source={Source}",
                roll,
                dropChance,
                context.Source);
            return null;
        }

        _logger.LogDebug(
            "Recipe scroll drop check passed: Roll={Roll:F4}, DropChance={DropChance:F4}, Source={Source}",
            roll,
            dropChance,
            context.Source);

        // ───────────────────────────────────────────────────────────
        // Get eligible scrolls based on level and source
        // ───────────────────────────────────────────────────────────
        var eligibleScrolls = GetEligibleScrolls(context);
        if (eligibleScrolls.Count == 0)
        {
            _logger.LogDebug(
                "Recipe scroll generation skipped: No eligible scrolls for level {Level} from {Source}",
                context.DungeonLevel,
                context.Source);
            return null;
        }

        _logger.LogDebug(
            "Found {EligibleCount} eligible recipe scrolls for level {Level} from {Source}",
            eligibleScrolls.Count,
            context.DungeonLevel,
            context.Source);

        // ───────────────────────────────────────────────────────────
        // Select scroll using weighted random selection
        // ───────────────────────────────────────────────────────────
        var selectedConfig = SelectWeightedScroll(eligibleScrolls);
        if (selectedConfig == null)
        {
            _logger.LogWarning(
                "Weighted scroll selection returned null despite {Count} eligible scrolls",
                eligibleScrolls.Count);
            return null;
        }

        // ───────────────────────────────────────────────────────────
        // Look up recipe to get display name
        // ───────────────────────────────────────────────────────────
        var recipe = _recipeProvider.GetRecipe(selectedConfig.RecipeId);
        if (recipe == null)
        {
            _logger.LogWarning(
                "Scroll config references non-existent recipe: RecipeId={RecipeId}",
                selectedConfig.RecipeId);
            return null;
        }

        // ───────────────────────────────────────────────────────────
        // Create the scroll item
        // ───────────────────────────────────────────────────────────
        var scrollItem = Item.CreateRecipeScroll(
            recipeId: selectedConfig.RecipeId,
            recipeName: recipe.Name,
            baseValue: selectedConfig.BaseValue);

        _logger.LogInformation(
            "Generated recipe scroll: RecipeId={RecipeId}, RecipeName={RecipeName}, " +
            "Source={Source}, DungeonLevel={Level}, BaseValue={BaseValue}",
            selectedConfig.RecipeId,
            recipe.Name,
            context.Source,
            context.DungeonLevel,
            selectedConfig.BaseValue);

        _eventLogger?.LogInventory(
            eventType: "RecipeScrollGenerated",
            message: $"Recipe scroll generated: {recipe.Name}",
            playerId: context.Player?.Id,
            data: new Dictionary<string, object>
            {
                ["recipeId"] = selectedConfig.RecipeId,
                ["recipeName"] = recipe.Name,
                ["source"] = context.Source.ToString(),
                ["dungeonLevel"] = context.DungeonLevel,
                ["dropWeight"] = selectedConfig.DropWeight,
                ["baseValue"] = selectedConfig.BaseValue
            });

        return scrollItem;
    }

    /// <inheritdoc/>
    public decimal GetScrollDropChance(LootSourceType source)
    {
        if (_scrollProvider == null)
        {
            return 0;
        }

        return _scrollProvider.GetDropChance(source);
    }

    /// <summary>
    /// Gets eligible scroll configurations based on the loot context.
    /// </summary>
    /// <param name="context">The loot context containing filtering criteria.</param>
    /// <returns>A list of eligible scroll configurations.</returns>
    /// <remarks>
    /// <para>
    /// Filtering process:
    /// <list type="number">
    ///   <item><description>Get scrolls matching both level and source criteria</description></item>
    ///   <item><description>If ExcludeKnownRecipes is true and player is provided, filter out known recipes</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    private IReadOnlyList<RecipeScrollConfig> GetEligibleScrolls(LootContext context)
    {
        // Get scrolls eligible at this level from this source
        var baseEligible = _scrollProvider!.GetEligibleScrolls(context.DungeonLevel, context.Source);

        // If not filtering known recipes, return as-is
        if (!context.ExcludeKnownRecipes || context.Player == null || _recipeService == null)
        {
            _logger.LogDebug(
                "Returning {Count} eligible scrolls (known recipe filtering disabled)",
                baseEligible.Count);
            return baseEligible;
        }

        // Filter out recipes the player already knows
        var filtered = baseEligible
            .Where(config => !_recipeService.IsRecipeKnown(context.Player, config.RecipeId))
            .ToList();

        _logger.LogDebug(
            "Filtered eligible scrolls: {OriginalCount} -> {FilteredCount} (excluded known recipes)",
            baseEligible.Count,
            filtered.Count);

        return filtered;
    }

    /// <summary>
    /// Selects a scroll from a list using weighted random selection.
    /// </summary>
    /// <param name="scrolls">The list of scroll configurations to select from.</param>
    /// <returns>The selected scroll configuration, or null if the list is empty.</returns>
    /// <remarks>
    /// <para>
    /// Selection is weighted by each scroll's DropWeight property. Higher weights
    /// mean higher probability of selection.
    /// </para>
    /// <para>
    /// Example: If scroll A has weight 10 and scroll B has weight 5, scroll A
    /// is twice as likely to be selected.
    /// </para>
    /// </remarks>
    private RecipeScrollConfig? SelectWeightedScroll(IReadOnlyList<RecipeScrollConfig> scrolls)
    {
        if (scrolls.Count == 0)
        {
            return null;
        }

        if (scrolls.Count == 1)
        {
            _logger.LogDebug(
                "Single eligible scroll, selecting: RecipeId={RecipeId}",
                scrolls[0].RecipeId);
            return scrolls[0];
        }

        // Calculate total weight
        var totalWeight = scrolls.Sum(s => s.DropWeight);
        if (totalWeight <= 0)
        {
            _logger.LogWarning(
                "Total scroll weight is zero or negative: {TotalWeight}",
                totalWeight);
            return scrolls[0];
        }

        // Roll and select
        var roll = _random.Next(totalWeight);
        var cumulative = 0;

        foreach (var scroll in scrolls)
        {
            cumulative += scroll.DropWeight;
            if (roll < cumulative)
            {
                _logger.LogDebug(
                    "Weighted selection: Roll={Roll}, TotalWeight={TotalWeight}, " +
                    "Selected={RecipeId} (weight={Weight})",
                    roll,
                    totalWeight,
                    scroll.RecipeId,
                    scroll.DropWeight);
                return scroll;
            }
        }

        // Fallback (shouldn't happen due to math, but defensive)
        _logger.LogDebug(
            "Weighted selection fallback to last item: RecipeId={RecipeId}",
            scrolls[^1].RecipeId);
        return scrolls[^1];
    }

    // ═══════════════════════════════════════════════════════════════
    // CURRENCY
    // ═══════════════════════════════════════════════════════════════

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

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE HELPERS
    // ═══════════════════════════════════════════════════════════════

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
