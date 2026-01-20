namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Modifiers applied to loot drops within a biome.
/// </summary>
public readonly record struct LootModifiers
{
    /// <summary>
    /// Gold amount multiplier (1.0 = normal).
    /// </summary>
    public float GoldMultiplier { get; init; }

    /// <summary>
    /// Item drop rate multiplier (1.0 = normal).
    /// </summary>
    public float DropRateMultiplier { get; init; }

    /// <summary>
    /// Additive bonus to item quality tier.
    /// </summary>
    public int QualityBonus { get; init; }

    /// <summary>
    /// Additive bonus to rare item chance (0.0-1.0).
    /// </summary>
    public float RareChanceBonus { get; init; }

    /// <summary>
    /// Default modifiers (no changes).
    /// </summary>
    public static LootModifiers Default => new()
    {
        GoldMultiplier = 1.0f,
        DropRateMultiplier = 1.0f,
        QualityBonus = 0,
        RareChanceBonus = 0.0f
    };

    /// <summary>
    /// Creates loot modifiers.
    /// </summary>
    public static LootModifiers Create(
        float goldMultiplier = 1.0f,
        float dropRateMultiplier = 1.0f,
        int qualityBonus = 0,
        float rareChanceBonus = 0.0f) => new()
    {
        GoldMultiplier = goldMultiplier,
        DropRateMultiplier = dropRateMultiplier,
        QualityBonus = qualityBonus,
        RareChanceBonus = rareChanceBonus
    };

    /// <summary>
    /// Applies gold multiplier to a base value.
    /// </summary>
    public int ApplyGoldModifier(int baseGold) => (int)(baseGold * GoldMultiplier);

    /// <summary>
    /// Applies drop rate modifier to a base probability.
    /// </summary>
    public float ApplyDropRateModifier(float baseProbability) =>
        Math.Clamp(baseProbability * DropRateMultiplier, 0f, 1f);

    /// <summary>
    /// Applies quality bonus to a base tier.
    /// </summary>
    public int ApplyQualityModifier(int baseTier) => baseTier + QualityBonus;

    /// <summary>
    /// Applies rare chance bonus to a base probability.
    /// </summary>
    public float ApplyRareChanceModifier(float baseProbability) =>
        Math.Clamp(baseProbability + RareChanceBonus, 0f, 1f);
}
