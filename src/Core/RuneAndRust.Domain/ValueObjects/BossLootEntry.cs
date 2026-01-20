namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents a potential loot drop from a boss encounter.
/// </summary>
/// <remarks>
/// <para>
/// BossLootEntry defines an item that may drop when a boss is defeated:
/// <list type="bullet">
///   <item><description><see cref="ItemId"/>: References the item definition</description></item>
///   <item><description><see cref="Amount"/>: Quantity of items dropped</description></item>
///   <item><description><see cref="Chance"/>: Probability of dropping (0.0-1.0)</description></item>
/// </list>
/// </para>
/// <para>
/// This is an immutable value object. Use <see cref="Create"/> factory method
/// for validated construction. Guaranteed drops use <see cref="Guaranteed"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // 50% chance to drop a rare sword
/// var rareDrop = BossLootEntry.Create("rare-sword", chance: 0.5);
///
/// // Guaranteed gold drop
/// var goldDrop = BossLootEntry.Guaranteed("gold", amount: 500);
/// </code>
/// </example>
public readonly record struct BossLootEntry
{
    // ═══════════════════════════════════════════════════════════════
    // PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the item definition ID for this loot entry.
    /// </summary>
    /// <remarks>
    /// References an item definition in the item configuration.
    /// Must match a valid item ID for the drop to be generated.
    /// </remarks>
    public string ItemId { get; init; }

    /// <summary>
    /// Gets the quantity of items to drop.
    /// </summary>
    /// <remarks>
    /// Defaults to 1. For stackable items like gold or potions,
    /// this specifies how many are received on a successful drop.
    /// </remarks>
    public int Amount { get; init; }

    /// <summary>
    /// Gets the drop chance as a decimal between 0.0 and 1.0.
    /// </summary>
    /// <remarks>
    /// <para>1.0 = guaranteed drop (100%)</para>
    /// <para>0.5 = 50% chance</para>
    /// <para>0.01 = 1% chance (rare drop)</para>
    /// <para>0.0 = never drops (should not be used)</para>
    /// </remarks>
    public double Chance { get; init; }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTORS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Initializes a new instance of the <see cref="BossLootEntry"/> struct.
    /// </summary>
    /// <remarks>
    /// Private constructor. Use factory methods for construction.
    /// </remarks>
    private BossLootEntry(string itemId, int amount, double chance)
    {
        ItemId = itemId;
        Amount = amount;
        Chance = chance;
    }

    // ═══════════════════════════════════════════════════════════════
    // FACTORY METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new loot entry with the specified parameters.
    /// </summary>
    /// <param name="itemId">The item definition ID.</param>
    /// <param name="chance">Drop probability between 0.0 and 1.0.</param>
    /// <param name="amount">Quantity to drop (default: 1).</param>
    /// <returns>A new <see cref="BossLootEntry"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="chance"/> is not between 0.0 and 1.0,
    /// or when <paramref name="amount"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// // 25% chance for a legendary weapon
    /// var loot = BossLootEntry.Create("legendary-axe", chance: 0.25);
    /// </code>
    /// </example>
    public static BossLootEntry Create(string itemId, double chance, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId))
        {
            throw new ArgumentException("Item ID cannot be null or empty.", nameof(itemId));
        }

        if (chance < 0.0 || chance > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(chance), chance, "Chance must be between 0.0 and 1.0.");
        }

        if (amount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be at least 1.");
        }

        return new BossLootEntry(itemId, amount, chance);
    }

    /// <summary>
    /// Creates a guaranteed loot drop (100% chance).
    /// </summary>
    /// <param name="itemId">The item definition ID.</param>
    /// <param name="amount">Quantity to drop (default: 1).</param>
    /// <returns>A new <see cref="BossLootEntry"/> with 100% drop chance.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="itemId"/> is null or whitespace.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="amount"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// // Boss always drops 1000 gold
    /// var gold = BossLootEntry.Guaranteed("gold", amount: 1000);
    /// </code>
    /// </example>
    public static BossLootEntry Guaranteed(string itemId, int amount = 1)
    {
        return Create(itemId, chance: 1.0, amount: amount);
    }

    // ═══════════════════════════════════════════════════════════════
    // FLUENT CONFIGURATION METHODS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a copy with a different amount.
    /// </summary>
    /// <param name="amount">The new quantity to drop.</param>
    /// <returns>A new <see cref="BossLootEntry"/> with the specified amount.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="amount"/> is less than 1.
    /// </exception>
    /// <example>
    /// <code>
    /// var loot = BossLootEntry.Create("potion", chance: 0.75)
    ///     .WithAmount(3); // Drop 3 potions
    /// </code>
    /// </example>
    public BossLootEntry WithAmount(int amount)
    {
        if (amount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be at least 1.");
        }

        return this with { Amount = amount };
    }

    /// <summary>
    /// Creates a copy with a different drop chance.
    /// </summary>
    /// <param name="chance">The new drop probability (0.0-1.0).</param>
    /// <returns>A new <see cref="BossLootEntry"/> with the specified chance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="chance"/> is not between 0.0 and 1.0.
    /// </exception>
    /// <example>
    /// <code>
    /// var loot = BossLootEntry.Create("rare-gem", chance: 0.10)
    ///     .WithChance(0.25); // Increase drop rate to 25%
    /// </code>
    /// </example>
    public BossLootEntry WithChance(double chance)
    {
        if (chance < 0.0 || chance > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(chance), chance, "Chance must be between 0.0 and 1.0.");
        }

        return this with { Chance = chance };
    }

    // ═══════════════════════════════════════════════════════════════
    // COMPUTED PROPERTIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets whether this is a guaranteed drop (100% chance).
    /// </summary>
    public bool IsGuaranteed => Math.Abs(Chance - 1.0) < 0.0001;

    /// <summary>
    /// Gets whether this loot entry is valid.
    /// </summary>
    /// <remarks>
    /// A valid entry has a non-empty item ID, positive amount,
    /// and chance between 0.0 and 1.0.
    /// </remarks>
    public bool IsValid =>
        !string.IsNullOrWhiteSpace(ItemId) &&
        Amount >= 1 &&
        Chance >= 0.0 &&
        Chance <= 1.0;

    /// <summary>
    /// Gets the chance as a percentage string.
    /// </summary>
    /// <example>
    /// <code>
    /// var loot = BossLootEntry.Create("sword", chance: 0.25);
    /// Console.WriteLine(loot.ChancePercent); // "25%"
    /// </code>
    /// </example>
    public string ChancePercent => $"{Chance * 100:F0}%";

    /// <summary>
    /// Returns an empty/invalid loot entry.
    /// </summary>
    /// <remarks>
    /// Used as a default. Check <see cref="IsValid"/> before using.
    /// </remarks>
    public static BossLootEntry Empty => default;
}
