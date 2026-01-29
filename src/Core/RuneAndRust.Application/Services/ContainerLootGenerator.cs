// =============================================================================
// ContainerLootGenerator.cs
// Service for generating container loot with biome modifiers and smart loot.
// Version: 0.16.4d
// =============================================================================

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Generates container loot with biome modifiers and smart loot integration.
/// </summary>
/// <remarks>
/// <para>
/// ContainerLootGenerator orchestrates the complete loot generation pipeline:
/// <list type="number">
///   <item>Retrieve container type definition for base ranges</item>
///   <item>Apply biome modifiers from container location</item>
///   <item>Calculate adjusted item count, tier, and currency</item>
///   <item>Generate items using smart loot for class-appropriate selection</item>
///   <item>Create and store ContainerContents on container entity</item>
/// </list>
/// </para>
/// <para>
/// The service uses lazy generation - contents are only created on first
/// access and cached for consistent subsequent retrievals.
/// </para>
/// <para>
/// Thread safety: This implementation is thread-safe for concurrent
/// container access due to immutable lookups and atomic content setting.
/// </para>
/// </remarks>
public class ContainerLootGenerator : IContainerLootService
{
    // =========================================================================
    // Fields
    // =========================================================================

    /// <summary>
    /// Smart loot service for class-appropriate item selection.
    /// </summary>
    private readonly ISmartLootService _smartLootService;

    /// <summary>
    /// Logger for diagnostic output.
    /// </summary>
    private readonly ILogger<ContainerLootGenerator> _logger;

    /// <summary>
    /// Container type definitions indexed by type.
    /// </summary>
    private readonly IReadOnlyDictionary<ContainerType, ContainerTypeDefinition> _containerDefinitions;

    /// <summary>
    /// Biome modifiers indexed by normalized biome ID.
    /// </summary>
    private readonly IReadOnlyDictionary<string, BiomeLootModifiers> _biomeModifiers;

    /// <summary>
    /// Available loot entries for item generation, indexed by tier.
    /// </summary>
    private readonly IReadOnlyDictionary<QualityTier, IReadOnlyList<LootEntry>> _availableItems;

    /// <summary>
    /// Random number generator for loot calculations.
    /// </summary>
    private readonly Random _random;

    // =========================================================================
    // Constructor
    // =========================================================================

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerLootGenerator"/> class.
    /// </summary>
    /// <param name="smartLootService">Service for class-appropriate item selection.</param>
    /// <param name="logger">Logger for diagnostic output.</param>
    /// <param name="containerDefinitions">Container type definitions.</param>
    /// <param name="biomeModifiers">Biome loot modifiers.</param>
    /// <param name="availableItems">Available items by quality tier.</param>
    /// <param name="random">Optional random instance for deterministic testing.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when any required parameter is null.
    /// </exception>
    public ContainerLootGenerator(
        ISmartLootService smartLootService,
        ILogger<ContainerLootGenerator> logger,
        IReadOnlyDictionary<ContainerType, ContainerTypeDefinition> containerDefinitions,
        IReadOnlyDictionary<string, BiomeLootModifiers> biomeModifiers,
        IReadOnlyDictionary<QualityTier, IReadOnlyList<LootEntry>> availableItems,
        Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(smartLootService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(containerDefinitions);
        ArgumentNullException.ThrowIfNull(biomeModifiers);
        ArgumentNullException.ThrowIfNull(availableItems);

        _smartLootService = smartLootService;
        _logger = logger;
        _containerDefinitions = containerDefinitions;
        _biomeModifiers = biomeModifiers;
        _availableItems = availableItems;
        _random = random ?? new Random();

        _logger.LogDebug(
            "ContainerLootGenerator initialized with {DefinitionCount} container types, " +
            "{ModifierCount} biome modifiers, and {TierCount} loot tiers",
            containerDefinitions.Count,
            biomeModifiers.Count,
            availableItems.Count);
    }

    // =========================================================================
    // IContainerLootService Implementation
    // =========================================================================

    /// <inheritdoc />
    public ContainerContents GenerateContents(
        ContainerLootTable container,
        Guid playerId,
        string biomeId)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentException.ThrowIfNullOrWhiteSpace(biomeId);

        // Check if container is already looted
        if (container.IsLooted)
        {
            _logger.LogDebug(
                "Container {ContainerId} already looted, returning empty",
                container.Id);
            return ContainerContents.Empty;
        }

        // Check if contents already generated
        if (container.HasGeneratedContents)
        {
            _logger.LogDebug(
                "Container {ContainerId} already has contents, returning existing",
                container.Id);
            return container.Contents!.Value;
        }

        // Get container definition
        var definition = GetContainerDefinition(container.Type);
        var modifiers = GetBiomeModifiers(biomeId);

        _logger.LogInformation(
            "Generating contents for container {ContainerId} type {Type} in biome {BiomeId}",
            container.Id,
            container.Type,
            biomeId);

        // Calculate values with modifiers applied
        var itemCount = CalculateItemCount(definition, modifiers);
        var tier = SelectTier(definition, modifiers);
        var currency = CalculateCurrency(definition, modifiers);

        _logger.LogDebug(
            "Calculated values: ItemCount={ItemCount}, Tier={Tier}, Currency={Currency}",
            itemCount,
            tier,
            currency);

        // Generate items using smart loot
        var itemIds = GenerateItems(itemCount, tier, playerId);

        // Create contents
        var contents = ContainerContents.Create(itemIds, currency, (int)tier);
        container.SetContents(contents);

        _logger.LogInformation(
            "Generated {ItemCount} items and {Currency} currency at tier {Tier} for container {ContainerId}",
            itemIds.Count,
            currency,
            tier,
            container.Id);

        return contents;
    }

    /// <inheritdoc />
    public ContainerTypeDefinition GetContainerDefinition(ContainerType containerType)
    {
        if (!_containerDefinitions.TryGetValue(containerType, out var definition))
        {
            _logger.LogWarning(
                "No definition found for container type {ContainerType}",
                containerType);
            throw new KeyNotFoundException(
                $"No definition found for container type '{containerType}'.");
        }

        return definition;
    }

    /// <inheritdoc />
    public BiomeLootModifiers GetBiomeModifiers(string biomeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(biomeId);

        var normalizedId = biomeId.ToLowerInvariant();

        if (_biomeModifiers.TryGetValue(normalizedId, out var modifiers))
        {
            return modifiers;
        }

        _logger.LogDebug(
            "No modifiers found for biome {BiomeId}, using defaults",
            biomeId);
        return BiomeLootModifiers.Default;
    }

    /// <inheritdoc />
    public bool HasGeneratedContents(ContainerLootTable container)
    {
        ArgumentNullException.ThrowIfNull(container);
        return container.HasGeneratedContents;
    }

    // =========================================================================
    // Private Methods
    // =========================================================================

    /// <summary>
    /// Calculates the number of items to generate.
    /// </summary>
    /// <param name="definition">Container type definition.</param>
    /// <param name="modifiers">Biome modifiers to apply.</param>
    /// <returns>The adjusted item count.</returns>
    private int CalculateItemCount(
        ContainerTypeDefinition definition,
        BiomeLootModifiers modifiers)
    {
        // Random base count within definition range
        var baseCount = _random.Next(definition.MinItems, definition.MaxItems + 1);

        // Apply drop rate modifier from biome
        var adjustedCount = modifiers.ApplyToItemCount(baseCount, definition.MinItems);

        _logger.LogDebug(
            "Item count: base={BaseCount}, adjusted={AdjustedCount} (modifier={Modifier})",
            baseCount,
            adjustedCount,
            modifiers.DropRateMultiplier);

        return adjustedCount;
    }

    /// <summary>
    /// Selects a quality tier for generated items.
    /// </summary>
    /// <param name="definition">Container type definition.</param>
    /// <param name="modifiers">Biome modifiers to apply.</param>
    /// <returns>The selected quality tier.</returns>
    private QualityTier SelectTier(
        ContainerTypeDefinition definition,
        BiomeLootModifiers modifiers)
    {
        // Random base tier within definition range
        var baseTier = _random.Next(definition.MinTier, definition.MaxTier + 1);

        // Apply quality bonus from biome (capped at max tier)
        var adjustedTier = modifiers.ApplyToTier(baseTier, maxTier: 4);

        _logger.LogDebug(
            "Tier selection: base={BaseTier}, adjusted={AdjustedTier} (bonus={Bonus})",
            baseTier,
            adjustedTier,
            modifiers.QualityBonus);

        return (QualityTier)adjustedTier;
    }

    /// <summary>
    /// Calculates the currency amount to generate.
    /// </summary>
    /// <param name="definition">Container type definition.</param>
    /// <param name="modifiers">Biome modifiers to apply.</param>
    /// <returns>The adjusted currency amount.</returns>
    private int CalculateCurrency(
        ContainerTypeDefinition definition,
        BiomeLootModifiers modifiers)
    {
        // Check if container awards currency at all
        if (!definition.AwardsCurrency)
        {
            _logger.LogDebug("Container type {Type} does not award currency", definition.Type);
            return 0;
        }

        // Random base currency within definition range
        var baseCurrency = _random.Next(definition.MinCurrency!.Value, definition.MaxCurrency!.Value + 1);

        // Apply gold multiplier from biome
        var adjustedCurrency = modifiers.ApplyToGold(baseCurrency);

        _logger.LogDebug(
            "Currency: base={BaseCurrency}, adjusted={AdjustedCurrency} (multiplier={Multiplier})",
            baseCurrency,
            adjustedCurrency,
            modifiers.GoldMultiplier);

        return adjustedCurrency;
    }


    /// <summary>
    /// Generates items using smart loot selection.
    /// </summary>
    /// <param name="count">Number of items to generate.</param>
    /// <param name="tier">Quality tier for item selection.</param>
    /// <param name="playerId">Player ID for archetype lookup.</param>
    /// <returns>List of generated item IDs.</returns>
    private List<string> GenerateItems(int count, QualityTier tier, Guid playerId)
    {
        var items = new List<string>();

        // Get available items for this tier
        if (!_availableItems.TryGetValue(tier, out var availableForTier) ||
            availableForTier.Count == 0)
        {
            _logger.LogWarning(
                "No items available for tier {Tier}, returning empty list",
                tier);
            return items;
        }

        for (var i = 0; i < count; i++)
        {
            // Create context for smart loot selection
            // Note: Player archetype would normally be looked up from playerId,
            // but for container loot we use random-only selection to provide variety
            var context = SmartLootContext.CreateRandomOnly(
                tier,
                availableForTier);

            var result = _smartLootService.SelectItem(context);

            if (result.HasSelection)
            {
                items.Add(result.SelectedItem!.Value.ItemId);

                _logger.LogDebug(
                    "Selected item {ItemId} via {SelectionReason}",
                    result.SelectedItem.Value.ItemId,
                    result.SelectionReason);
            }
            else
            {
                _logger.LogWarning(
                    "Smart loot selection returned no item for tier {Tier}",
                    tier);
            }
        }

        return items;
    }
}
