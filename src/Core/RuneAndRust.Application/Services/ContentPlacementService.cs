using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service for placing content in rooms based on template slots and difficulty.
/// </summary>
/// <remarks>
/// Uses SeededRandomService for deterministic content placement, ensuring
/// the same seed produces identical room contents.
/// </remarks>
public class ContentPlacementService : IContentPlacementService
{
    private readonly ISeededRandomService _random;
    private readonly ILogger<ContentPlacementService> _logger;

    /// <summary>
    /// Base tier weights for monster selection.
    /// </summary>
    private static readonly Dictionary<string, int> BaseTierWeights = new()
    {
        ["common"] = 70,
        ["named"] = 20,
        ["elite"] = 8,
        ["boss"] = 2
    };

    /// <summary>
    /// Sort order for tiers (used to boost higher tiers at higher difficulty).
    /// </summary>
    private static readonly Dictionary<string, int> TierSortOrder = new()
    {
        ["common"] = 0,
        ["named"] = 1,
        ["elite"] = 2,
        ["boss"] = 3
    };

    /// <summary>
    /// Base rarity weights for item selection.
    /// </summary>
    private static readonly Dictionary<string, int> BaseRarityWeights = new()
    {
        ["common"] = 60,
        ["uncommon"] = 25,
        ["rare"] = 10,
        ["epic"] = 4,
        ["legendary"] = 1
    };

    /// <summary>
    /// Sort order for rarities (used to boost higher rarities at higher difficulty).
    /// </summary>
    private static readonly Dictionary<string, int> RaritySortOrder = new()
    {
        ["common"] = 0,
        ["uncommon"] = 1,
        ["rare"] = 2,
        ["epic"] = 3,
        ["legendary"] = 4
    };

    /// <summary>
    /// Creates a new content placement service.
    /// </summary>
    public ContentPlacementService(
        ISeededRandomService random,
        ILogger<ContentPlacementService> logger)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("ContentPlacementService initialized");
    }

    /// <inheritdoc/>
    public bool ShouldFillSlot(TemplateSlot slot, Position3D position)
    {
        // Required slots always fill
        if (slot.IsRequired)
            return true;

        var roll = _random.NextFloatForPosition(position, $"slot_fill_{slot.SlotId}");
        var shouldFill = roll < slot.EffectiveFillProbability;

        _logger.LogDebug(
            "ShouldFillSlot({SlotId}): roll={Roll:F2}, probability={Prob:F2}, result={Result}",
            slot.SlotId, roll, slot.EffectiveFillProbability, shouldFill);

        return shouldFill;
    }

    /// <inheritdoc/>
    public string SelectMonsterTier(DifficultyRating difficulty, TemplateSlot slot, Position3D position)
    {
        if (!difficulty.HasCombat)
        {
            _logger.LogDebug("No combat in room, returning 'none' tier");
            return "none";
        }

        var effectiveLevel = difficulty.EffectiveLevel;
        var adjustedWeights = new List<(string tier, int weight)>();

        // Get min/max tier constraints from slot
        var minTier = slot.GetConstraint("minTier");
        var maxTier = slot.GetConstraint("maxTier");
        var minOrder = minTier != null && TierSortOrder.TryGetValue(minTier, out var min) ? min : 0;
        var maxOrder = maxTier != null && TierSortOrder.TryGetValue(maxTier, out var max) ? max : int.MaxValue;

        foreach (var (tier, baseWeight) in BaseTierWeights)
        {
            if (!TierSortOrder.TryGetValue(tier, out var sortOrder))
                continue;

            // Apply min/max constraints
            if (sortOrder < minOrder || sortOrder > maxOrder)
                continue;

            // Adjust weight based on difficulty
            // Higher difficulty boosts higher-tier weights
            var adjustedWeight = (int)(baseWeight * (1 + effectiveLevel * 0.01f * sortOrder));
            adjustedWeights.Add((tier, adjustedWeight));
        }

        if (adjustedWeights.Count == 0)
        {
            _logger.LogWarning("No valid tiers after filtering, using default 'common'");
            return "common";
        }

        var selected = _random.SelectWeighted(position, adjustedWeights, "monster_tier");

        _logger.LogDebug(
            "SelectMonsterTier: effectiveLevel={Level}, selected={Tier}",
            effectiveLevel, selected);

        return selected;
    }

    /// <inheritdoc/>
    public int DetermineMonsterQuantity(TemplateSlot slot, Position3D position)
    {
        if (slot.MinQuantity == slot.MaxQuantity)
            return slot.MinQuantity;

        var quantity = _random.NextForPosition(
            position,
            slot.MinQuantity,
            slot.MaxQuantity + 1,
            $"monster_quantity_{slot.SlotId}");

        _logger.LogDebug(
            "DetermineMonsterQuantity({SlotId}): range=[{Min},{Max}], result={Quantity}",
            slot.SlotId, slot.MinQuantity, slot.MaxQuantity, quantity);

        return quantity;
    }

    /// <inheritdoc/>
    public string SelectItemRarity(DifficultyRating difficulty, TemplateSlot slot, Position3D position)
    {
        var effectiveLevel = difficulty.EffectiveLevel;
        var adjustedWeights = new List<(string rarity, int weight)>();

        // Get min/max rarity constraints from slot
        var minRarity = slot.GetConstraint("minRarity");
        var maxRarity = slot.GetConstraint("maxRarity");
        var minOrder = minRarity != null && RaritySortOrder.TryGetValue(minRarity, out var min) ? min : 0;
        var maxOrder = maxRarity != null && RaritySortOrder.TryGetValue(maxRarity, out var max) ? max : int.MaxValue;

        foreach (var (rarity, baseWeight) in BaseRarityWeights)
        {
            if (!RaritySortOrder.TryGetValue(rarity, out var sortOrder))
                continue;

            // Apply min/max constraints
            if (sortOrder < minOrder || sortOrder > maxOrder)
                continue;

            // Apply loot quality multiplier to boost rarer items at higher difficulty
            var qualityMultiplier = difficulty.LootQualityMultiplier;
            var adjustedWeight = (int)(baseWeight * (1 + (qualityMultiplier - 1) * sortOrder * 0.5f));
            adjustedWeights.Add((rarity, adjustedWeight));
        }

        if (adjustedWeights.Count == 0)
        {
            _logger.LogWarning("No valid rarities after filtering, using default 'common'");
            return "common";
        }

        var selected = _random.SelectWeighted(position, adjustedWeights, "item_rarity");

        _logger.LogDebug(
            "SelectItemRarity: effectiveLevel={Level}, lootMultiplier={Mult:F2}, selected={Rarity}",
            effectiveLevel, difficulty.LootQualityMultiplier, selected);

        return selected;
    }

    /// <inheritdoc/>
    public int DetermineItemQuantity(TemplateSlot slot, Position3D position)
    {
        if (slot.MinQuantity == slot.MaxQuantity)
            return slot.MinQuantity;

        var quantity = _random.NextForPosition(
            position,
            slot.MinQuantity,
            slot.MaxQuantity + 1,
            $"item_quantity_{slot.SlotId}");

        _logger.LogDebug(
            "DetermineItemQuantity({SlotId}): range=[{Min},{Max}], result={Quantity}",
            slot.SlotId, slot.MinQuantity, slot.MaxQuantity, quantity);

        return quantity;
    }

    /// <inheritdoc/>
    public int GetMonsterLevelBonus(DifficultyRating difficulty)
    {
        return difficulty.MonsterLevelBonus;
    }
}
