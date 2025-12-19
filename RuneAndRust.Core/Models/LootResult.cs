using RuneAndRust.Core.Entities;
using RuneAndRust.Core.Enums;

namespace RuneAndRust.Core.Models;

/// <summary>
/// Represents the result of a loot generation operation.
/// Contains generated items and metadata about the loot roll.
/// </summary>
/// <param name="Success">Whether loot was successfully generated.</param>
/// <param name="Message">Descriptive message about the loot found.</param>
/// <param name="Items">The list of generated items.</param>
/// <param name="TotalValue">The combined Scrip value of all items.</param>
/// <param name="TotalWeight">The combined weight in grams of all items.</param>
public record LootResult(
    bool Success,
    string Message,
    IReadOnlyList<Item> Items,
    int TotalValue,
    int TotalWeight)
{
    /// <summary>
    /// Creates a successful loot result with items.
    /// </summary>
    /// <param name="message">The loot message.</param>
    /// <param name="items">The generated items.</param>
    /// <returns>A successful loot result.</returns>
    public static LootResult Found(string message, IReadOnlyList<Item> items)
    {
        var totalValue = items.Sum(i => i.Value);
        var totalWeight = items.Sum(i => i.Weight);
        return new LootResult(true, message, items, totalValue, totalWeight);
    }

    /// <summary>
    /// Creates an empty loot result when nothing was found.
    /// </summary>
    /// <param name="message">The empty loot message.</param>
    /// <returns>An empty loot result.</returns>
    public static LootResult Empty(string message)
    {
        return new LootResult(false, message, Array.Empty<Item>(), 0, 0);
    }

    /// <summary>
    /// Creates a failure result when loot generation fails.
    /// </summary>
    /// <param name="message">The failure message.</param>
    /// <returns>A failed loot result.</returns>
    public static LootResult Failure(string message)
    {
        return new LootResult(false, message, Array.Empty<Item>(), 0, 0);
    }
}

/// <summary>
/// Represents loot generation parameters for procedural generation.
/// </summary>
/// <param name="BiomeType">The biome influencing item types.</param>
/// <param name="DangerLevel">The danger level affecting quality tiers.</param>
/// <param name="LootTier">Optional quality tier override from the container.</param>
/// <param name="WitsBonus">Character WITS bonus affecting loot quality.</param>
public record LootGenerationContext(
    BiomeType BiomeType,
    DangerLevel DangerLevel,
    QualityTier? LootTier,
    int WitsBonus = 0);
