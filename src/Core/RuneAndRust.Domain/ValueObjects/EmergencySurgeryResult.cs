using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of an Emergency Surgery ability execution.
/// Encapsulates the full healing breakdown including base dice roll (4d6),
/// supply quality bonus, Steady Hands passive bonus, and recovery condition bonus.
/// </summary>
/// <remarks>
/// <para>Emergency Surgery is the Bone-Setter's Tier 2 high-impact healing ability:</para>
/// <list type="bullet">
/// <item>Cost: 3 AP + 1 Medical Supply (highest quality used)</item>
/// <item>Healing: 4d6 + quality bonus + Steady Hands bonus + recovery bonus</item>
/// <item>Quality bonus: supply quality - 1 (range: 0–4)</item>
/// <item>Steady Hands: +2 if passive is unlocked</item>
/// <item>Recovery bonus: +1 (Incapacitated), +3 (Recovering), +4 (Dying)</item>
/// <item>Corruption: None (Coherent path)</item>
/// </list>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display and state updates.</para>
/// </remarks>
public sealed record EmergencySurgeryResult
{
    /// <summary>
    /// Unique identifier of the character who was healed.
    /// </summary>
    public Guid TargetId { get; init; }

    /// <summary>
    /// Display name of the healed target.
    /// </summary>
    public string TargetName { get; init; } = string.Empty;

    /// <summary>
    /// Target's HP value before healing was applied.
    /// </summary>
    public int HpBefore { get; init; }

    /// <summary>
    /// Result of 4d6 dice roll for base healing.
    /// Range: 4–24 (double the Field Dressing base of 2d6).
    /// </summary>
    public int HealingRoll { get; init; }

    /// <summary>
    /// Bonus from Medical Supply quality (Quality - 1).
    /// Range: 0–4 based on supply quality level.
    /// </summary>
    public int QualityBonus { get; init; }

    /// <summary>
    /// Bonus from the Steady Hands passive ability.
    /// +2 if passive is unlocked, 0 otherwise.
    /// </summary>
    public int SteadyHandsBonus { get; init; }

    /// <summary>
    /// Bonus from the target's recovery condition.
    /// +1 (Incapacitated), +3 (Recovering), +4 (Dying), 0 (Active/Dead).
    /// </summary>
    public int RecoveryBonus { get; init; }

    /// <summary>
    /// Total healing applied: HealingRoll + QualityBonus + SteadyHandsBonus + RecoveryBonus.
    /// </summary>
    public int TotalHealing => HealingRoll + QualityBonus + SteadyHandsBonus + RecoveryBonus;

    /// <summary>
    /// Target's HP value after healing was applied (capped at max HP).
    /// </summary>
    public int HpAfter { get; init; }

    /// <summary>
    /// Number of Medical Supplies consumed (always 1 for Emergency Surgery).
    /// </summary>
    public int SuppliesUsed { get; init; }

    /// <summary>
    /// Number of Medical Supplies remaining after spending.
    /// </summary>
    public int SuppliesRemaining { get; init; }

    /// <summary>
    /// Name of the supply type that was consumed for this healing.
    /// Emergency Surgery uses the highest quality supply available.
    /// </summary>
    public string SupplyTypeUsed { get; init; } = string.Empty;

    /// <summary>
    /// Whether a recovery condition bonus was triggered.
    /// True when the target was in Recovering, Incapacitated, or Dying condition.
    /// </summary>
    public bool BonusTriggered { get; init; }

    /// <summary>
    /// The target's recovery condition that triggered the bonus (if any).
    /// Null when no bonus was applied (target was Active).
    /// </summary>
    public RecoveryCondition? TargetCondition { get; init; }

    /// <summary>
    /// Gets a detailed breakdown of the healing calculation for combat log display.
    /// Includes recovery bonus component when triggered.
    /// </summary>
    /// <returns>A formatted string showing each healing component.</returns>
    /// <example>
    /// "Base (4d6): 14 + Quality (Suture): +3 + Steady Hands: +2 + Recovery (Recovering): +3 = Total: 22"
    /// </example>
    public string GetHealingBreakdown()
    {
        var breakdown = $"Base (4d6): {HealingRoll} + Quality ({SupplyTypeUsed}): +{QualityBonus} + " +
                        $"Steady Hands: +{SteadyHandsBonus}";

        if (BonusTriggered && RecoveryBonus > 0)
            breakdown += $" + Recovery ({TargetCondition}): +{RecoveryBonus}";

        breakdown += $" = Total: {TotalHealing}";
        return breakdown;
    }

    /// <summary>
    /// Gets a concise status message suitable for combat log display.
    /// Includes recovery bonus indicator when triggered.
    /// </summary>
    /// <returns>A formatted combat log entry for this Emergency Surgery result.</returns>
    /// <example>
    /// "Fighter healed for 22 HP (0 → 22) [EMERGENCY INTERVENTION BONUS: +3]"
    /// </example>
    public string GetStatusMessage()
    {
        var bonus = BonusTriggered ? $" [EMERGENCY INTERVENTION BONUS: +{RecoveryBonus}]" : "";
        return $"{TargetName} healed for {TotalHealing} HP ({HpBefore} → {HpAfter}){bonus}";
    }
}
