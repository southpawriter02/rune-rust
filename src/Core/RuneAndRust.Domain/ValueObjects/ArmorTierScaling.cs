namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the stat scaling for armor at a specific quality tier.
/// </summary>
/// <remarks>
/// <para>
/// Armor scaling provides defensive bonuses that increase with tier.
/// Higher-tier armor not only provides better HP and defense but may
/// also reduce the penalties associated with heavier armor types.
/// </para>
/// <para>
/// The PenaltyReduction property indicates how many tiers the armor's
/// penalties are reduced by. For example, a value of 1 means Heavy
/// armor penalties are calculated as Medium armor penalties.
/// </para>
/// </remarks>
/// <param name="Tier">The quality tier this scaling applies to.</param>
/// <param name="HpBonus">Maximum HP bonus provided by this tier.</param>
/// <param name="DefenseBonus">Defense stat bonus provided by this tier.</param>
/// <param name="PenaltyReduction">Armor penalty tier reduction (0 or 1).</param>
/// <param name="AttributeBonusRange">Optional attribute bonus range for Tier 2+.</param>
public readonly record struct ArmorTierScaling(
    QualityTier Tier,
    int HpBonus,
    int DefenseBonus,
    int PenaltyReduction,
    AttributeBonusRange? AttributeBonusRange)
{
    /// <summary>
    /// Gets the tier as an integer value.
    /// </summary>
    public int TierValue => (int)Tier;

    /// <summary>
    /// Gets whether this tier includes an attribute bonus.
    /// </summary>
    public bool HasAttributeBonus => AttributeBonusRange.HasValue;

    /// <summary>
    /// Gets whether this tier reduces armor penalties.
    /// </summary>
    public bool HasPenaltyReduction => PenaltyReduction > 0;

    /// <summary>
    /// Rolls an attribute bonus within the defined range.
    /// </summary>
    /// <param name="random">Random number generator.</param>
    /// <returns>The rolled bonus, or null if no bonus range is defined.</returns>
    public int? RollAttributeBonus(Random random)
    {
        return AttributeBonusRange?.RollBonus(random);
    }

    /// <summary>
    /// Creates a new ArmorTierScaling with validation.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <param name="hpBonus">HP bonus value.</param>
    /// <param name="defenseBonus">Defense bonus value.</param>
    /// <param name="penaltyReduction">Penalty tier reduction.</param>
    /// <param name="attributeBonusRange">Optional attribute bonus range.</param>
    /// <returns>A new ArmorTierScaling instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when values are negative or penalty reduction exceeds 2.</exception>
    public static ArmorTierScaling Create(
        QualityTier tier,
        int hpBonus,
        int defenseBonus,
        int penaltyReduction,
        AttributeBonusRange? attributeBonusRange = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(hpBonus);
        ArgumentOutOfRangeException.ThrowIfNegative(defenseBonus);
        ArgumentOutOfRangeException.ThrowIfNegative(penaltyReduction);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(penaltyReduction, 2);

        return new ArmorTierScaling(
            tier,
            hpBonus,
            defenseBonus,
            penaltyReduction,
            attributeBonusRange);
    }

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    /// <returns>A formatted string representation.</returns>
    public override string ToString()
    {
        var penaltyText = HasPenaltyReduction ? $", -{PenaltyReduction} penalty tier" : "";
        var attrText = HasAttributeBonus ? $", Attr: {AttributeBonusRange}" : "";
        return $"Tier {TierValue}: +{HpBonus} HP, +{DefenseBonus} Def{penaltyText}{attrText}";
    }
}
