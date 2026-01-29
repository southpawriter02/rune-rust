// ═══════════════════════════════════════════════════════════════════════════════
// RUNE & RUST — v0.16.3c Weighted Item Selection
// ═══════════════════════════════════════════════════════════════════════════════
// File: WeightedItem.cs
// Purpose: Value object representing an item with an associated selection weight
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents an item with an associated selection weight for weighted random selection.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="WeightedItem"/> wraps a <see cref="LootEntry"/> with a weight value that
/// determines its relative probability of being selected from a <see cref="Entities.WeightedItemPool"/>.
/// </para>
/// <para>
/// Higher weights mean higher selection probability. For example, a weight of 100
/// is twice as likely to be selected as a weight of 50 when both items are in the same pool.
/// </para>
/// <para>
/// Items with <see cref="Weight"/> = 0 are stored but never selected, effectively
/// disabling them without removal from the pool.
/// </para>
/// <para>
/// Standard rarity weights follow an approximate exponential drop-off pattern:
/// <list type="table">
/// <listheader>
/// <term>Rarity</term>
/// <description>Weight / Drop Rate (in 5-item pool)</description>
/// </listheader>
/// <item><term>Common</term><description>100 (~52.6%)</description></item>
/// <item><term>Uncommon</term><description>50 (~26.3%)</description></item>
/// <item><term>Rare</term><description>25 (~13.2%)</description></item>
/// <item><term>Very Rare</term><description>10 (~5.3%)</description></item>
/// <item><term>Unique</term><description>5 (~2.6%)</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="Item">The underlying loot entry containing item data.</param>
/// <param name="Weight">Selection weight (higher = more likely). Must be >= 0.</param>
/// <param name="Rarity">Optional display label for rarity tier (e.g., "Common", "Rare").</param>
public readonly record struct WeightedItem(
    LootEntry Item,
    int Weight,
    string Rarity = "")
{
    // ═══════════════════════════════════════════════════════════════════════════
    // STANDARD WEIGHT CONSTANTS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Standard weight for Common items (basic, frequently-dropped gear).
    /// </summary>
    /// <remarks>
    /// In a pool with one item of each rarity tier (Common through Unique),
    /// Common items have approximately 52.6% selection probability.
    /// </remarks>
    public const int CommonWeight = 100;

    /// <summary>
    /// Standard weight for Uncommon items (slightly better stats than common).
    /// </summary>
    /// <remarks>
    /// In a pool with one item of each rarity tier (Common through Unique),
    /// Uncommon items have approximately 26.3% selection probability.
    /// </remarks>
    public const int UncommonWeight = 50;

    /// <summary>
    /// Standard weight for Rare items (notable upgrades with special properties).
    /// </summary>
    /// <remarks>
    /// In a pool with one item of each rarity tier (Common through Unique),
    /// Rare items have approximately 13.2% selection probability.
    /// </remarks>
    public const int RareWeight = 25;

    /// <summary>
    /// Standard weight for Very Rare items (significant finds with powerful effects).
    /// </summary>
    /// <remarks>
    /// In a pool with one item of each rarity tier (Common through Unique),
    /// Very Rare items have approximately 5.3% selection probability.
    /// </remarks>
    public const int VeryRareWeight = 10;

    /// <summary>
    /// Standard weight for Unique items (legendary, run-defining equipment).
    /// </summary>
    /// <remarks>
    /// In a pool with one item of each rarity tier (Common through Unique),
    /// Unique items have approximately 2.6% selection probability.
    /// </remarks>
    public const int UniqueWeight = 5;

    // ═══════════════════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a value indicating whether this item can be selected (weight greater than 0).
    /// </summary>
    /// <remarks>
    /// Items with zero weight are effectively disabled but remain in the pool.
    /// This allows temporary exclusion without removal/re-addition overhead.
    /// </remarks>
    public bool IsSelectable => Weight > 0;

    /// <summary>
    /// Gets the item's unique identifier from the underlying <see cref="LootEntry"/>.
    /// </summary>
    /// <remarks>
    /// Convenience property to avoid nested access to <c>Item.ItemId</c>.
    /// </remarks>
    public string ItemId => Item.ItemId;

    // ═══════════════════════════════════════════════════════════════════════════
    // PROBABILITY CALCULATIONS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates this item's selection probability given a pool's total weight.
    /// </summary>
    /// <param name="totalWeight">The sum of all weights in the containing pool.</param>
    /// <returns>
    /// Probability as a value between 0.0 and 1.0. Returns 0 if <paramref name="totalWeight"/> is zero.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Selection probability is calculated as: <c>Weight / TotalWeight</c>
    /// </para>
    /// <para>
    /// Example: An item with weight 100 in a pool with total weight 200 has 50% probability.
    /// </para>
    /// </remarks>
    public double GetSelectionProbability(int totalWeight) =>
        totalWeight > 0 ? (double)Weight / totalWeight : 0;

    /// <summary>
    /// Formats the selection probability as a human-readable percentage string.
    /// </summary>
    /// <param name="totalWeight">The sum of all weights in the containing pool.</param>
    /// <returns>Formatted percentage with two decimal places (e.g., "52.63%").</returns>
    /// <remarks>
    /// Useful for debug output, logging, and UI display of drop rates.
    /// </remarks>
    public string FormatProbability(int totalWeight) =>
        $"{GetSelectionProbability(totalWeight) * 100:F2}%";

    // ═══════════════════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a <see cref="WeightedItem"/> with explicit validation.
    /// </summary>
    /// <param name="item">The loot entry to wrap. Cannot be null.</param>
    /// <param name="weight">Selection weight. Must be non-negative (0 disables selection).</param>
    /// <param name="rarity">Optional display label for rarity tier.</param>
    /// <returns>A validated <see cref="WeightedItem"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="item"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="weight"/> is negative.</exception>
    public static WeightedItem Create(
        LootEntry item,
        int weight,
        string rarity = "")
    {
        // Validate item is not default (record structs cannot be null but can be default)
        if (string.IsNullOrWhiteSpace(item.ItemId))
        {
            throw new ArgumentException("Item must have a valid ItemId.", nameof(item));
        }

        // Weight can be 0 (disabled) but not negative
        ArgumentOutOfRangeException.ThrowIfNegative(weight);

        return new WeightedItem(item, weight, rarity ?? string.Empty);
    }

    /// <summary>
    /// Creates a Common-rarity weighted item with standard weight of 100.
    /// </summary>
    /// <param name="item">The loot entry to wrap.</param>
    /// <returns>A <see cref="WeightedItem"/> with Common weight and rarity label.</returns>
    public static WeightedItem CreateCommon(LootEntry item) =>
        new(item, CommonWeight, "Common");

    /// <summary>
    /// Creates an Uncommon-rarity weighted item with standard weight of 50.
    /// </summary>
    /// <param name="item">The loot entry to wrap.</param>
    /// <returns>A <see cref="WeightedItem"/> with Uncommon weight and rarity label.</returns>
    public static WeightedItem CreateUncommon(LootEntry item) =>
        new(item, UncommonWeight, "Uncommon");

    /// <summary>
    /// Creates a Rare-rarity weighted item with standard weight of 25.
    /// </summary>
    /// <param name="item">The loot entry to wrap.</param>
    /// <returns>A <see cref="WeightedItem"/> with Rare weight and rarity label.</returns>
    public static WeightedItem CreateRare(LootEntry item) =>
        new(item, RareWeight, "Rare");

    /// <summary>
    /// Creates a Very Rare-rarity weighted item with standard weight of 10.
    /// </summary>
    /// <param name="item">The loot entry to wrap.</param>
    /// <returns>A <see cref="WeightedItem"/> with Very Rare weight and rarity label.</returns>
    public static WeightedItem CreateVeryRare(LootEntry item) =>
        new(item, VeryRareWeight, "Very Rare");

    /// <summary>
    /// Creates a Unique-rarity weighted item with standard weight of 5.
    /// </summary>
    /// <param name="item">The loot entry to wrap.</param>
    /// <returns>A <see cref="WeightedItem"/> with Unique weight and rarity label.</returns>
    public static WeightedItem CreateUnique(LootEntry item) =>
        new(item, UniqueWeight, "Unique");

    // ═══════════════════════════════════════════════════════════════════════════
    // DISPLAY / DEBUGGING
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns a string representation for debugging and logging.
    /// </summary>
    /// <returns>A formatted string showing item ID, weight, and rarity.</returns>
    public override string ToString() =>
        string.IsNullOrEmpty(Rarity)
            ? $"{Item.ItemId} (Weight={Weight})"
            : $"{Item.ItemId} (Weight={Weight}, Rarity={Rarity})";
}
