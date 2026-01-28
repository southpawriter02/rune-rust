namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Defines the stat scaling for weapons at a specific quality tier.
/// </summary>
/// <remarks>
/// <para>
/// Weapon scaling follows a progressive power curve where each tier
/// represents approximately 50-80% power increase. The damage dice
/// string uses standard notation (e.g., "2d6+4") that can be parsed
/// by the dice rolling system.
/// </para>
/// <para>
/// Attribute bonuses are only available for Tier 2 (ClanForged) and above.
/// Lower tiers are functional but don't provide stat enhancements.
/// </para>
/// </remarks>
/// <param name="Tier">The quality tier this scaling applies to.</param>
/// <param name="DamageDice">Base damage in dice notation (e.g., "1d6", "2d6+4").</param>
/// <param name="AccuracyModifier">Accuracy bonus or penalty (-1 to +2).</param>
/// <param name="AttributeBonusRange">Optional attribute bonus range for Tier 2+.</param>
public readonly record struct WeaponTierScaling(
    QualityTier Tier,
    string DamageDice,
    int AccuracyModifier,
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
    /// Gets whether this tier has an accuracy penalty (negative modifier).
    /// </summary>
    public bool HasAccuracyPenalty => AccuracyModifier < 0;

    /// <summary>
    /// Gets whether this tier has an accuracy bonus (positive modifier).
    /// </summary>
    public bool HasAccuracyBonus => AccuracyModifier > 0;

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
    /// Creates a new WeaponTierScaling with validation.
    /// </summary>
    /// <param name="tier">The quality tier.</param>
    /// <param name="damageDice">Damage dice notation.</param>
    /// <param name="accuracyModifier">Accuracy modifier.</param>
    /// <param name="attributeBonusRange">Optional attribute bonus range.</param>
    /// <returns>A new WeaponTierScaling instance.</returns>
    /// <exception cref="ArgumentException">Thrown when damageDice is null or empty.</exception>
    public static WeaponTierScaling Create(
        QualityTier tier,
        string damageDice,
        int accuracyModifier,
        AttributeBonusRange? attributeBonusRange = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(damageDice);

        return new WeaponTierScaling(
            tier,
            damageDice,
            accuracyModifier,
            attributeBonusRange);
    }

    /// <summary>
    /// Creates a display string for debug/logging purposes.
    /// </summary>
    /// <returns>A formatted string representation.</returns>
    public override string ToString()
    {
        var accuracyText = AccuracyModifier >= 0 ? $"+{AccuracyModifier}" : $"{AccuracyModifier}";
        var attrText = HasAttributeBonus ? $", Attr: {AttributeBonusRange}" : "";
        return $"Tier {TierValue}: {DamageDice} dmg, {accuracyText} acc{attrText}";
    }
}
