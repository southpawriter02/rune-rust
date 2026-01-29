namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents biome-specific modifiers that affect container loot generation.
/// </summary>
/// <remarks>
/// <para>
/// Modifiers are applied during loot generation to scale gold amounts,
/// adjust item counts, shift quality tiers, and modify rare item chances.
/// All multipliers use 1.0 as the baseline (no modification).
/// </para>
/// <para>
/// Biome modifiers create environmental variety in loot rewards:
/// <list type="bullet">
///   <item><description>Safe areas: Normal loot rates</description></item>
///   <item><description>Dangerous areas: Higher gold, fewer items</description></item>
///   <item><description>Premium areas: Better quality, increased rare chances</description></item>
/// </list>
/// </para>
/// </remarks>
/// <param name="BiomeId">Unique identifier for the biome (kebab-case, e.g., "the-roots").</param>
/// <param name="GoldMultiplier">Multiplier for gold amounts (1.0 = normal, 2.0 = double).</param>
/// <param name="DropRateMultiplier">Multiplier for item counts (1.0 = normal, 0.5 = half).</param>
/// <param name="QualityBonus">Additive bonus to quality tier (0 = no bonus, max 4).</param>
/// <param name="RareChanceBonusPercent">Additive bonus to rare chance as percentage (0-100).</param>
public readonly record struct BiomeLootModifiers(
    string BiomeId,
    decimal GoldMultiplier,
    decimal DropRateMultiplier,
    int QualityBonus,
    int RareChanceBonusPercent)
{
    /// <summary>
    /// Gets the default modifiers with no adjustments.
    /// </summary>
    /// <remarks>
    /// Used when no biome-specific modifiers are defined or as a fallback.
    /// All values represent no modification to base loot values:
    /// <list type="bullet">
    ///   <item><description>GoldMultiplier: 1.0 (no change)</description></item>
    ///   <item><description>DropRateMultiplier: 1.0 (no change)</description></item>
    ///   <item><description>QualityBonus: 0 (no tier shift)</description></item>
    ///   <item><description>RareChanceBonusPercent: 0 (no bonus)</description></item>
    /// </list>
    /// </remarks>
    public static BiomeLootModifiers Default { get; } = new(
        BiomeId: "default",
        GoldMultiplier: 1.0m,
        DropRateMultiplier: 1.0m,
        QualityBonus: 0,
        RareChanceBonusPercent: 0);

    /// <summary>
    /// Gets whether this modifier increases gold amounts above baseline.
    /// </summary>
    /// <value><c>true</c> if <see cref="GoldMultiplier"/> is greater than 1.0; otherwise, <c>false</c>.</value>
    public bool IncreasesGold => GoldMultiplier > 1.0m;

    /// <summary>
    /// Gets whether this modifier reduces drop rates below baseline.
    /// </summary>
    /// <value><c>true</c> if <see cref="DropRateMultiplier"/> is less than 1.0; otherwise, <c>false</c>.</value>
    public bool ReducesDropRate => DropRateMultiplier < 1.0m;

    /// <summary>
    /// Gets whether this modifier improves item quality above baseline.
    /// </summary>
    /// <value><c>true</c> if <see cref="QualityBonus"/> is greater than 0; otherwise, <c>false</c>.</value>
    public bool ImprovesQuality => QualityBonus > 0;

    /// <summary>
    /// Gets whether this modifier increases rare item chances above baseline.
    /// </summary>
    /// <value><c>true</c> if <see cref="RareChanceBonusPercent"/> is greater than 0; otherwise, <c>false</c>.</value>
    public bool IncreasesRareChance => RareChanceBonusPercent > 0;

    /// <summary>
    /// Applies the gold multiplier to a base gold amount.
    /// </summary>
    /// <param name="baseGold">The base gold amount before modification. Must be non-negative.</param>
    /// <returns>
    /// The scaled gold amount, rounded down using <see cref="Math.Floor(decimal)"/>.
    /// Returns 0 if <paramref name="baseGold"/> is 0 or negative.
    /// </returns>
    /// <example>
    /// <code>
    /// var alfheim = BiomeLootModifiers.Create("alfheim", 1.5m, 1.0m, 0, 0);
    /// int scaled = alfheim.ApplyToGold(100); // Returns 150
    /// </code>
    /// </example>
    public int ApplyToGold(int baseGold)
    {
        if (baseGold <= 0)
        {
            return 0;
        }

        var scaled = (int)Math.Floor(baseGold * GoldMultiplier);
        return Math.Max(0, scaled);
    }

    /// <summary>
    /// Applies the drop rate multiplier to calculate adjusted item count.
    /// </summary>
    /// <param name="baseCount">The base item count before modification.</param>
    /// <param name="minCount">The minimum allowed item count (default: 0).</param>
    /// <returns>
    /// The adjusted item count, floored and clamped to at least <paramref name="minCount"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var alfheim = BiomeLootModifiers.Create("alfheim", 1.0m, 0.7m, 0, 0);
    /// int items = alfheim.ApplyToItemCount(5, 1); // Returns 3 (5 * 0.7 = 3.5 → 3)
    /// </code>
    /// </example>
    public int ApplyToItemCount(int baseCount, int minCount = 0)
    {
        if (baseCount <= 0)
        {
            return minCount;
        }

        var scaled = (int)Math.Floor(baseCount * DropRateMultiplier);
        return Math.Max(minCount, scaled);
    }

    /// <summary>
    /// Applies the quality bonus to a base tier.
    /// </summary>
    /// <param name="baseTier">The base quality tier (typically 0-4).</param>
    /// <param name="maxTier">The maximum allowed tier (default: 4 for Myth-Forged).</param>
    /// <returns>
    /// The adjusted tier, capped at <paramref name="maxTier"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var jotunheim = BiomeLootModifiers.Create("jotunheim", 1.0m, 1.0m, 1, 0);
    /// int tier = jotunheim.ApplyToTier(3, 4); // Returns 4 (3 + 1, capped at 4)
    /// </code>
    /// </example>
    public int ApplyToTier(int baseTier, int maxTier = 4)
    {
        var adjusted = baseTier + QualityBonus;
        return Math.Min(adjusted, maxTier);
    }

    /// <summary>
    /// Gets the rare chance bonus as a decimal multiplier for calculations.
    /// </summary>
    /// <returns>
    /// The bonus percentage as a decimal fraction (e.g., 15% returns 0.15).
    /// </returns>
    /// <example>
    /// <code>
    /// var alfheim = BiomeLootModifiers.Create("alfheim", 1.0m, 1.0m, 0, 15);
    /// decimal bonus = alfheim.GetRareChanceBonus(); // Returns 0.15m
    /// </code>
    /// </example>
    public decimal GetRareChanceBonus() => RareChanceBonusPercent / 100m;

    /// <summary>
    /// Creates a new <see cref="BiomeLootModifiers"/> instance with full validation.
    /// </summary>
    /// <param name="biomeId">Unique identifier for the biome (required, normalized to lowercase).</param>
    /// <param name="goldMultiplier">Multiplier for gold (must be greater than 0).</param>
    /// <param name="dropRateMultiplier">Multiplier for drop rates (must be greater than 0).</param>
    /// <param name="qualityBonus">Bonus to quality tier (0-4).</param>
    /// <param name="rareChanceBonusPercent">Bonus to rare chance (0-100).</param>
    /// <returns>A validated <see cref="BiomeLootModifiers"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="biomeId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when multipliers are not positive, or bonus values are out of range.
    /// </exception>
    /// <example>
    /// <code>
    /// var modifiers = BiomeLootModifiers.Create(
    ///     biomeId: "muspelheim",
    ///     goldMultiplier: 1.2m,
    ///     dropRateMultiplier: 0.9m,
    ///     qualityBonus: 0,
    ///     rareChanceBonusPercent: 5);
    /// </code>
    /// </example>
    public static BiomeLootModifiers Create(
        string biomeId,
        decimal goldMultiplier,
        decimal dropRateMultiplier,
        int qualityBonus,
        int rareChanceBonusPercent)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(biomeId, nameof(biomeId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(goldMultiplier, nameof(goldMultiplier));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dropRateMultiplier, nameof(dropRateMultiplier));
        ArgumentOutOfRangeException.ThrowIfNegative(qualityBonus, nameof(qualityBonus));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(qualityBonus, 4, nameof(qualityBonus));
        ArgumentOutOfRangeException.ThrowIfNegative(rareChanceBonusPercent, nameof(rareChanceBonusPercent));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(rareChanceBonusPercent, 100, nameof(rareChanceBonusPercent));

        return new BiomeLootModifiers(
            biomeId.ToLowerInvariant(),
            goldMultiplier,
            dropRateMultiplier,
            qualityBonus,
            rareChanceBonusPercent);
    }

    /// <inheritdoc />
    /// <summary>
    /// Returns a string representation of the biome modifiers for debugging.
    /// </summary>
    /// <returns>
    /// A formatted string showing all modifier values in a readable format.
    /// </returns>
    public override string ToString() =>
        $"[{BiomeId}] Gold: ×{GoldMultiplier:F1}, Drop: ×{DropRateMultiplier:F1}, " +
        $"Tier: +{QualityBonus}, Rare: +{RareChanceBonusPercent}%";
}
