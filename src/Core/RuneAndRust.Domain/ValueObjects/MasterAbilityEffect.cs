namespace RuneAndRust.Domain.ValueObjects;

using RuneAndRust.Domain.Enums;

/// <summary>
/// Contains the effect parameters for a master ability.
/// </summary>
/// <remarks>
/// <para>
/// This value object holds the type-specific parameters for different
/// <see cref="MasterAbilityType"/> values:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="MasterAbilityType.AutoSucceed"/>: Uses <see cref="AutoSucceedDc"/></description></item>
///   <item><description><see cref="MasterAbilityType.DiceBonus"/>: Uses <see cref="DiceBonus"/></description></item>
///   <item><description><see cref="MasterAbilityType.DamageReduction"/>: Uses <see cref="DamageThreshold"/> and <see cref="ReductionAmount"/></description></item>
///   <item><description><see cref="MasterAbilityType.DistanceBonus"/>: Uses <see cref="DistanceBonus"/></description></item>
///   <item><description><see cref="MasterAbilityType.RerollFailure"/>: Uses <see cref="RerollPeriod"/></description></item>
///   <item><description><see cref="MasterAbilityType.SpecialAction"/>: Uses <see cref="SpecialEffect"/></description></item>
/// </list>
/// </remarks>
/// <param name="AutoSucceedDc">DC threshold for auto-succeed abilities.</param>
/// <param name="DiceBonus">Bonus dice for dice bonus abilities.</param>
/// <param name="DamageThreshold">Maximum damage height/amount for damage reduction.</param>
/// <param name="ReductionAmount">Amount to reduce damage by (null = negate entirely).</param>
/// <param name="DistanceBonus">Additional distance/time for distance bonus abilities.</param>
/// <param name="RerollPeriod">Period for re-roll ability refresh.</param>
/// <param name="SpecialEffect">Special effect description for skill subsystem parsing.</param>
public readonly record struct MasterAbilityEffect(
    int? AutoSucceedDc = null,
    int? DiceBonus = null,
    int? DamageThreshold = null,
    int? ReductionAmount = null,
    int? DistanceBonus = null,
    RerollPeriod? RerollPeriod = null,
    string? SpecialEffect = null)
{
    /// <summary>
    /// An empty effect with no parameters set.
    /// </summary>
    public static MasterAbilityEffect Empty => new();

    /// <summary>
    /// Creates an auto-succeed effect.
    /// </summary>
    /// <param name="autoSucceedDc">Maximum DC that auto-succeeds.</param>
    /// <returns>A new effect configured for auto-succeed.</returns>
    public static MasterAbilityEffect ForAutoSucceed(int autoSucceedDc)
        => new(AutoSucceedDc: autoSucceedDc);

    /// <summary>
    /// Creates a dice bonus effect.
    /// </summary>
    /// <param name="diceBonus">Number of bonus dice.</param>
    /// <returns>A new effect configured for dice bonus.</returns>
    public static MasterAbilityEffect ForDiceBonus(int diceBonus)
        => new(DiceBonus: diceBonus);

    /// <summary>
    /// Creates a damage reduction effect.
    /// </summary>
    /// <param name="damageThreshold">Maximum damage that can be reduced (e.g., 30ft fall).</param>
    /// <param name="reductionAmount">Amount to reduce by, or null to negate entirely.</param>
    /// <returns>A new effect configured for damage reduction.</returns>
    public static MasterAbilityEffect ForDamageReduction(int damageThreshold, int? reductionAmount = null)
        => new(DamageThreshold: damageThreshold, ReductionAmount: reductionAmount);

    /// <summary>
    /// Creates a distance bonus effect.
    /// </summary>
    /// <param name="distanceBonus">Additional distance/time.</param>
    /// <returns>A new effect configured for distance bonus.</returns>
    public static MasterAbilityEffect ForDistanceBonus(int distanceBonus)
        => new(DistanceBonus: distanceBonus);

    /// <summary>
    /// Creates a re-roll failure effect.
    /// </summary>
    /// <param name="rerollPeriod">How often the re-roll refreshes.</param>
    /// <returns>A new effect configured for re-roll.</returns>
    public static MasterAbilityEffect ForRerollFailure(RerollPeriod rerollPeriod)
        => new(RerollPeriod: rerollPeriod);

    /// <summary>
    /// Creates a special action effect.
    /// </summary>
    /// <param name="specialEffect">Description of the special effect.</param>
    /// <returns>A new effect configured for special action.</returns>
    public static MasterAbilityEffect ForSpecialAction(string specialEffect)
        => new(SpecialEffect: specialEffect);

    /// <summary>
    /// Validates that the effect has appropriate parameters for the given ability type.
    /// </summary>
    /// <param name="abilityType">The ability type to validate against.</param>
    /// <returns><c>true</c> if the effect has valid parameters; otherwise <c>false</c>.</returns>
    public bool IsValidForType(MasterAbilityType abilityType)
    {
        return abilityType switch
        {
            MasterAbilityType.AutoSucceed => AutoSucceedDc.HasValue,
            MasterAbilityType.DiceBonus => DiceBonus.HasValue,
            MasterAbilityType.DamageReduction => DamageThreshold.HasValue,
            MasterAbilityType.DistanceBonus => DistanceBonus.HasValue,
            MasterAbilityType.RerollFailure => RerollPeriod.HasValue,
            MasterAbilityType.SpecialAction => !string.IsNullOrWhiteSpace(SpecialEffect),
            _ => false
        };
    }

    /// <summary>
    /// Returns a human-readable description of the effect.
    /// </summary>
    public string ToDisplayString()
    {
        if (AutoSucceedDc.HasValue)
            return $"Auto-succeed DC ≤ {AutoSucceedDc}";
        if (DiceBonus.HasValue)
            return $"+{DiceBonus}d10 bonus";
        if (DamageThreshold.HasValue)
            return ReductionAmount.HasValue
                ? $"Reduce damage ≤ {DamageThreshold} by {ReductionAmount}"
                : $"Negate damage ≤ {DamageThreshold}";
        if (DistanceBonus.HasValue)
            return $"+{DistanceBonus} distance/time";
        if (RerollPeriod.HasValue)
            return $"Re-roll once per {RerollPeriod}";
        if (!string.IsNullOrWhiteSpace(SpecialEffect))
            return SpecialEffect;

        return "No effect";
    }
}
