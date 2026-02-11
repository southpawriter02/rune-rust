// ═══════════════════════════════════════════════════════════════════════════════
// IBerserkrCorruptionService.cs
// Interface for evaluating and applying Corruption risk specific to the
// Berserkr specialization. Triggered by Rage levels rather than light.
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Interfaces;

using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Service interface for evaluating Corruption risk associated with
/// Berserkr abilities.
/// </summary>
/// <remarks>
/// <para>
/// Unlike the Myrk-gengr's <see cref="IShadowCorruptionService"/> (which evaluates
/// Corruption based on light conditions), Berserkr Corruption is triggered by
/// high Rage levels. Abilities used at 80+ Rage carry Corruption risk.
/// </para>
/// <para>
/// Tier 1 risk table:
/// </para>
/// <list type="bullet">
///   <item><description>Fury Strike at 80+ Rage: +1 Corruption</description></item>
///   <item><description>Blood Scent: No risk (passive)</description></item>
///   <item><description>Pain is Fuel: No risk (passive)</description></item>
///   <item><description>Entering combat at 80+ Rage: +1 Corruption</description></item>
/// </list>
/// </remarks>
/// <seealso cref="BerserkrCorruptionRiskResult"/>
/// <seealso cref="BerserkrCorruptionTrigger"/>
/// <seealso cref="BerserkrAbilityId"/>
public interface IBerserkrCorruptionService
{
    /// <summary>
    /// Evaluates the Corruption risk for using a specific ability at
    /// the given Rage level.
    /// </summary>
    /// <param name="abilityId">The Berserkr ability being used.</param>
    /// <param name="currentRage">Character's current Rage value.</param>
    /// <param name="targetIsCoherent">Whether the target is a Coherent creature.</param>
    /// <returns>A result indicating whether Corruption was triggered and the amount.</returns>
    BerserkrCorruptionRiskResult EvaluateRisk(
        BerserkrAbilityId abilityId,
        int currentRage,
        bool targetIsCoherent = false);

    /// <summary>
    /// Applies the Corruption from a triggered risk result.
    /// </summary>
    /// <param name="characterId">Character to apply Corruption to.</param>
    /// <param name="risk">The triggered risk result.</param>
    void ApplyCorruption(Guid characterId, BerserkrCorruptionRiskResult risk);

    /// <summary>
    /// Evaluates Corruption risk for entering combat while Enraged.
    /// </summary>
    /// <param name="currentRage">Character's current Rage value.</param>
    /// <returns>A result indicating whether Corruption was triggered.</returns>
    BerserkrCorruptionRiskResult CheckCombatEntryRisk(int currentRage);

    /// <summary>
    /// Gets a human-readable description for a specific Corruption trigger.
    /// </summary>
    /// <param name="trigger">The trigger to describe.</param>
    /// <returns>Formatted description string.</returns>
    string GetTriggerDescription(BerserkrCorruptionTrigger trigger);
}
