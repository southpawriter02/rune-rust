namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Field Dressing ability execution.
/// Encapsulates the full healing breakdown including base dice roll (2d6),
/// supply quality bonus, and Steady Hands passive bonus.
/// </summary>
/// <remarks>
/// <para>Field Dressing is the Bone-Setter's core Tier 1 healing ability:</para>
/// <list type="bullet">
/// <item>Cost: 2 AP + 1 Medical Supply</item>
/// <item>Healing: 2d6 + quality bonus + Steady Hands bonus</item>
/// <item>Quality bonus: supply quality - 1 (range: 0–4)</item>
/// <item>Steady Hands: +2 if passive is unlocked</item>
/// <item>Corruption: None (Coherent path)</item>
/// </list>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display and state updates.</para>
/// </remarks>
public sealed record FieldDressingResult
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
    /// Result of 2d6 dice roll for base healing.
    /// Range: 2–12.
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
    /// Total healing applied: HealingRoll + QualityBonus + SteadyHandsBonus.
    /// </summary>
    public int TotalHealing => HealingRoll + QualityBonus + SteadyHandsBonus;

    /// <summary>
    /// Target's HP value after healing was applied (capped at max HP).
    /// </summary>
    public int HpAfter { get; init; }

    /// <summary>
    /// Number of Medical Supplies remaining after spending.
    /// </summary>
    public int SuppliesRemaining { get; init; }

    /// <summary>
    /// Name of the supply type that was consumed for this healing.
    /// Example: "Bandage", "Salve".
    /// </summary>
    public string SupplyTypeUsed { get; init; } = string.Empty;

    /// <summary>
    /// Gets a detailed breakdown of the healing calculation for combat log display.
    /// Example: "Base (2d6): 7 + Quality (Bandage): +2 + Steady Hands: +2 = Total: 11"
    /// </summary>
    /// <returns>A formatted string showing each healing component.</returns>
    public string GetHealingBreakdown() =>
        $"Base (2d6): {HealingRoll} + Quality ({SupplyTypeUsed}): +{QualityBonus} + " +
        $"Steady Hands: +{SteadyHandsBonus} = Total: {TotalHealing}";

    /// <summary>
    /// Gets a concise status message suitable for combat log display.
    /// Example: "Fighter healed for 11 HP (48 → 59)"
    /// </summary>
    /// <returns>A formatted combat log entry for this Field Dressing result.</returns>
    public string GetStatusMessage() =>
        $"{TargetName} healed for {TotalHealing} HP ({HpBefore} → {HpAfter})";
}
