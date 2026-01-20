// ═══════════════════════════════════════════════════════════════════════════════
// QualityModifier.cs
// Value object representing multipliers applied to crafted items based on their
// quality tier. Provides stat and value multipliers for quality-based scaling.
// Version: 0.11.2c
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the multipliers applied to a crafted item based on its quality tier.
/// </summary>
/// <remarks>
/// <para>
/// This immutable value object encapsulates the stat and value multipliers that
/// determine how a crafted item's properties are scaled based on its quality.
/// </para>
/// <para>
/// Default multiplier values by quality tier (configurable via JSON):
/// </para>
/// <list type="bullet">
///   <item><description>Standard: 1.0x stat, 1.0x value (base quality)</description></item>
///   <item><description>Fine: 1.10x stat, 1.5x value (+10% stats, +50% value)</description></item>
///   <item><description>Masterwork: 1.25x stat, 2.5x value (+25% stats, +150% value)</description></item>
///   <item><description>Legendary: 1.50x stat, 5.0x value (+50% stats, +400% value)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // Create a modifier for Fine quality
/// var fineModifier = QualityModifier.Create(1.10m, 1.5m);
///
/// // Apply to base stats
/// int baseAttack = 10;
/// int scaledAttack = fineModifier.ApplyToStat(baseAttack); // Returns 11
///
/// // Apply to base value
/// int baseValue = 100;
/// int scaledValue = fineModifier.ApplyToValue(baseValue); // Returns 150
/// </code>
/// </example>
public readonly record struct QualityModifier
{
    // ═══════════════════════════════════════════════════════════════════════════
    // Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets the multiplier applied to item stats (attack, defense, etc.).
    /// </summary>
    /// <value>
    /// A decimal value where 1.0 represents no change, values greater than 1.0
    /// increase stats, and values less than 1.0 decrease stats.
    /// </value>
    /// <remarks>
    /// Typical values range from 1.0 (Standard) to 1.50 (Legendary).
    /// </remarks>
    public decimal StatMultiplier { get; init; }

    /// <summary>
    /// Gets the multiplier applied to item gold/currency value.
    /// </summary>
    /// <value>
    /// A decimal value where 1.0 represents base value, with higher quality
    /// tiers having progressively higher value multipliers.
    /// </value>
    /// <remarks>
    /// Value multipliers are typically higher than stat multipliers to reflect
    /// the rarity and desirability of higher quality items. Typical values
    /// range from 1.0 (Standard) to 5.0 (Legendary).
    /// </remarks>
    public decimal ValueMultiplier { get; init; }

    // ═══════════════════════════════════════════════════════════════════════════
    // Static Factory Properties
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Gets a modifier representing no modification (1.0x multipliers).
    /// </summary>
    /// <value>
    /// A <see cref="QualityModifier"/> with both multipliers set to 1.0,
    /// representing Standard quality or no modification.
    /// </value>
    /// <remarks>
    /// Use this property when you need a neutral modifier that does not
    /// change the base stats or value of an item.
    /// </remarks>
    public static QualityModifier None => new() { StatMultiplier = 1.0m, ValueMultiplier = 1.0m };

    // ═══════════════════════════════════════════════════════════════════════════
    // Factory Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new <see cref="QualityModifier"/> with the specified multipliers.
    /// </summary>
    /// <param name="statMultiplier">
    /// The multiplier to apply to item stats. Must be a positive value.
    /// A value of 1.0 represents no change.
    /// </param>
    /// <param name="valueMultiplier">
    /// The multiplier to apply to item value. Must be a positive value.
    /// A value of 1.0 represents no change.
    /// </param>
    /// <returns>
    /// A new <see cref="QualityModifier"/> instance with the specified multipliers.
    /// </returns>
    /// <example>
    /// <code>
    /// // Create a Masterwork modifier: +25% stats, +150% value
    /// var masterworkModifier = QualityModifier.Create(1.25m, 2.5m);
    /// </code>
    /// </example>
    public static QualityModifier Create(decimal statMultiplier, decimal valueMultiplier)
        => new() { StatMultiplier = statMultiplier, ValueMultiplier = valueMultiplier };

    // ═══════════════════════════════════════════════════════════════════════════
    // Application Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Applies the stat multiplier to a base stat value.
    /// </summary>
    /// <param name="baseStat">The base stat value before quality modification.</param>
    /// <returns>
    /// The stat value after applying the quality multiplier, rounded to the
    /// nearest integer using standard rounding (MidpointRounding.AwayFromZero).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses <see cref="Math.Round(decimal)"/> with default rounding
    /// (banker's rounding / round-half-to-even) for consistent behavior.
    /// </para>
    /// <para>
    /// Examples with Fine quality (1.10x stat multiplier):
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base 10 → Scaled 11</description></item>
    ///   <item><description>Base 15 → Scaled 17 (16.5 rounds to 17)</description></item>
    ///   <item><description>Base 20 → Scaled 22</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifier = QualityModifier.Create(1.25m, 2.5m); // Masterwork
    /// int baseDefense = 20;
    /// int scaledDefense = modifier.ApplyToStat(baseDefense); // Returns 25
    /// </code>
    /// </example>
    public int ApplyToStat(int baseStat) => (int)Math.Round(baseStat * StatMultiplier);

    /// <summary>
    /// Applies the value multiplier to a base gold/currency value.
    /// </summary>
    /// <param name="baseValue">The base gold value before quality modification.</param>
    /// <returns>
    /// The gold value after applying the quality multiplier, rounded to the
    /// nearest integer using standard rounding (MidpointRounding.AwayFromZero).
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method uses <see cref="Math.Round(decimal)"/> with default rounding
    /// (banker's rounding / round-half-to-even) for consistent behavior.
    /// </para>
    /// <para>
    /// Examples with Legendary quality (5.0x value multiplier):
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Base 100 → Scaled 500</description></item>
    ///   <item><description>Base 250 → Scaled 1250</description></item>
    ///   <item><description>Base 1000 → Scaled 5000</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// var modifier = QualityModifier.Create(1.50m, 5.0m); // Legendary
    /// int baseGold = 200;
    /// int scaledGold = modifier.ApplyToValue(baseGold); // Returns 1000
    /// </code>
    /// </example>
    public int ApplyToValue(int baseValue) => (int)Math.Round(baseValue * ValueMultiplier);

    // ═══════════════════════════════════════════════════════════════════════════
    // Query Methods
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Determines whether this modifier has any effect (non-1.0 multipliers).
    /// </summary>
    /// <value>
    /// <c>true</c> if either the stat or value multiplier is not equal to 1.0;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// A modifier with no effect is equivalent to <see cref="None"/> and will
    /// not change any stat or value when applied.
    /// </remarks>
    public bool HasEffect => StatMultiplier != 1.0m || ValueMultiplier != 1.0m;

    /// <summary>
    /// Returns a string representation of this modifier for debugging purposes.
    /// </summary>
    /// <returns>
    /// A string showing the stat and value multipliers in a readable format.
    /// </returns>
    /// <example>
    /// <code>
    /// var modifier = QualityModifier.Create(1.25m, 2.5m);
    /// Console.WriteLine(modifier.ToDebugString());
    /// // Output: "QualityModifier { StatMultiplier: 1.25x, ValueMultiplier: 2.5x }"
    /// </code>
    /// </example>
    public string ToDebugString() =>
        $"QualityModifier {{ StatMultiplier: {StatMultiplier:F2}x, ValueMultiplier: {ValueMultiplier:F1}x }}";
}
