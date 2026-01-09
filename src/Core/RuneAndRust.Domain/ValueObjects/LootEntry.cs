namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a possible item drop in a loot table.
/// </summary>
/// <remarks>
/// Each entry has a drop chance (0.0-1.0) and a weight for
/// selection when multiple items could drop.
/// </remarks>
public readonly record struct LootEntry
{
    /// <summary>
    /// Gets the ID of the item to drop.
    /// </summary>
    /// <remarks>
    /// References an Item entity or ItemDefinition.
    /// </remarks>
    public string ItemId { get; init; }

    /// <summary>
    /// Gets the relative weight for selection among entries.
    /// </summary>
    /// <remarks>
    /// Higher weights are more likely to be selected when
    /// determining which item drops.
    /// </remarks>
    public int Weight { get; init; }

    /// <summary>
    /// Gets the minimum quantity to drop.
    /// </summary>
    public int MinQuantity { get; init; }

    /// <summary>
    /// Gets the maximum quantity to drop.
    /// </summary>
    public int MaxQuantity { get; init; }

    /// <summary>
    /// Gets the chance (0.0-1.0) that this entry drops at all.
    /// </summary>
    /// <remarks>
    /// 1.0 = always drops (if selected), 0.25 = 25% chance.
    /// </remarks>
    public float DropChance { get; init; }

    /// <summary>
    /// Creates a validated LootEntry.
    /// </summary>
    /// <param name="itemId">The ID of the item to drop.</param>
    /// <param name="weight">The relative weight for selection (default: 100).</param>
    /// <param name="minQuantity">The minimum quantity to drop (default: 1).</param>
    /// <param name="maxQuantity">The maximum quantity to drop (default: 1).</param>
    /// <param name="dropChance">The chance (0.0-1.0) that this entry drops (default: 1.0).</param>
    /// <returns>A validated LootEntry.</returns>
    /// <exception cref="ArgumentException">Thrown when itemId is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when weight is not positive or quantity values are invalid.</exception>
    public static LootEntry Create(
        string itemId,
        int weight = 100,
        int minQuantity = 1,
        int maxQuantity = 1,
        float dropChance = 1.0f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(weight);
        ArgumentOutOfRangeException.ThrowIfNegative(minQuantity);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxQuantity, minQuantity);

        return new LootEntry
        {
            ItemId = itemId.ToLowerInvariant(),
            Weight = weight,
            MinQuantity = minQuantity,
            MaxQuantity = maxQuantity,
            DropChance = Math.Clamp(dropChance, 0f, 1f)
        };
    }
}
