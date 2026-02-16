using RuneAndRust.Domain.Enums;

namespace RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Represents the outcome of a Fury Strike ability execution.
/// Encapsulates the full damage breakdown including base damage, Fury bonus (3d6),
/// critical bonus (1d6), and Corruption evaluation results.
/// </summary>
/// <remarks>
/// <para>Fury Strike is the Berserkr's signature Tier 1 active ability:</para>
/// <list type="bullet">
/// <item>Cost: 2 AP + 20 Rage</item>
/// <item>Damage: weapon damage + 3d6 Fury damage</item>
/// <item>Critical (nat 20): additional +1d6 damage</item>
/// <item>Corruption: +1 if Rage was 80+ when used</item>
/// </list>
/// <para>This result object is returned by the ability service to provide
/// complete information for combat log display and state updates.</para>
/// </remarks>
public sealed record FuryStrikeResult
{
    /// <summary>
    /// The attack roll (d20) result before modifiers.
    /// A natural 20 triggers the critical bonus damage.
    /// </summary>
    public int AttackRoll { get; init; }

    /// <summary>
    /// Base weapon damage dealt by the attack.
    /// </summary>
    public int BaseDamage { get; init; }

    /// <summary>
    /// Fury bonus damage from the 3d6 roll.
    /// Always present on a successful Fury Strike.
    /// </summary>
    public int FuryDamage { get; init; }

    /// <summary>
    /// Critical bonus damage from the 1d6 roll.
    /// Only non-zero when <see cref="WasCritical"/> is true (natural 20).
    /// </summary>
    public int CriticalBonusDamage { get; init; }

    /// <summary>
    /// Total pre-mitigation damage (BaseDamage + FuryDamage + CriticalBonusDamage).
    /// </summary>
    public int TotalDamage => BaseDamage + FuryDamage + CriticalBonusDamage;

    /// <summary>
    /// Final damage after any reductions or modifications.
    /// May differ from <see cref="TotalDamage"/> if the target has damage reduction.
    /// </summary>
    public int FinalDamage { get; init; }

    /// <summary>
    /// Amount of Rage spent on this Fury Strike.
    /// </summary>
    public int RageSpent { get; init; }

    /// <summary>
    /// Whether this was a critical hit (natural 20 on the attack roll).
    /// </summary>
    public bool WasCritical { get; init; }

    /// <summary>
    /// Whether this Fury Strike triggered Corruption accumulation.
    /// True when Rage was at 80+ (Enraged) at the time of ability use.
    /// </summary>
    public bool CorruptionTriggered { get; init; }

    /// <summary>
    /// Descriptive reason for Corruption triggering (or why it didn't).
    /// Used for combat log display and player feedback.
    /// </summary>
    public string? CorruptionReason { get; init; }

    /// <summary>
    /// Gets a detailed breakdown of the damage components for combat log display.
    /// </summary>
    /// <returns>
    /// A formatted string showing each damage component, e.g.:
    /// "Base: 8 + Fury (3d6): 12 + Critical (1d6): 4 = Total: 24"
    /// </returns>
    public string GetDamageBreakdown()
    {
        var parts = $"Base: {BaseDamage} + Fury (3d6): {FuryDamage}";

        if (WasCritical)
        {
            parts += $" + Critical (1d6): {CriticalBonusDamage}";
        }

        parts += $" = Total: {TotalDamage}";

        if (FinalDamage != TotalDamage)
        {
            parts += $" (Final: {FinalDamage} after mitigation)";
        }

        return parts;
    }

    /// <summary>
    /// Gets a complete result string suitable for combat log display.
    /// </summary>
    /// <param name="targetName">Name of the target hit by Fury Strike.</param>
    /// <returns>A formatted combat log entry for this Fury Strike.</returns>
    public string GetResultString(string targetName)
    {
        var critTag = WasCritical ? " [CRITICAL]" : "";
        var corruptionTag = CorruptionTriggered ? " [CORRUPTION +1]" : "";
        return $"Fury Strike vs {targetName}{critTag}: {GetDamageBreakdown()} " +
               $"(Rage spent: {RageSpent}){corruptionTag}";
    }
}
