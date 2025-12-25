using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;
using RuneAndRust.Core.Models;

namespace RuneAndRust.Core.Interfaces;

/// <summary>
/// Provides procedural loot generation based on biome, danger level, and quality tiers.
/// Implements weighted random item selection with WITS-based quality bonuses.
/// </summary>
/// <remarks>See: SPEC-INV-001 for Inventory & Equipment System design.</remarks>
public interface ILootService
{
    /// <summary>
    /// Generates loot for a container based on context.
    /// </summary>
    /// <param name="context">The loot generation context including biome and danger level.</param>
    /// <returns>A loot result containing generated items.</returns>
    LootResult GenerateLoot(LootGenerationContext context);

    /// <summary>
    /// Searches a container and generates loot if not already searched.
    /// </summary>
    /// <param name="container">The container object to search.</param>
    /// <param name="context">The loot generation context.</param>
    /// <returns>A loot result containing found items.</returns>
    Task<LootResult> SearchContainerAsync(InteractableObject container, LootGenerationContext context);

    /// <summary>
    /// Gets the quality tier weights for a given danger level.
    /// Higher danger levels increase chances of better quality.
    /// </summary>
    /// <param name="dangerLevel">The area danger level.</param>
    /// <returns>A dictionary of quality tiers to their weight percentages.</returns>
    Dictionary<QualityTier, int> GetQualityWeights(DangerLevel dangerLevel);

    /// <summary>
    /// Rolls for a quality tier based on danger level and WITS bonus.
    /// </summary>
    /// <param name="dangerLevel">The area danger level.</param>
    /// <param name="witsBonus">Bonus from character WITS attribute.</param>
    /// <returns>The rolled quality tier.</returns>
    QualityTier RollQualityTier(DangerLevel dangerLevel, int witsBonus = 0);

    /// <summary>
    /// Gets the number of items to generate based on danger level.
    /// </summary>
    /// <param name="dangerLevel">The area danger level.</param>
    /// <returns>The number of items to generate (1-5).</returns>
    int RollItemCount(DangerLevel dangerLevel);

    /// <summary>
    /// Generates a random item of the specified quality and type.
    /// </summary>
    /// <param name="quality">The quality tier for the item.</param>
    /// <param name="biome">The biome influencing item selection.</param>
    /// <param name="preferredType">Optional preferred item type.</param>
    /// <returns>A generated item instance.</returns>
    Item GenerateItem(QualityTier quality, BiomeType biome, ItemType? preferredType = null);
}
