using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Berserkr-specific Corruption risk evaluation.
/// Evaluates whether Berserkr actions trigger Corruption accumulation based on
/// current Rage level, ability type, and target alignment.
/// </summary>
/// <remarks>
/// <para>Key design principle: Corruption risk is evaluated BEFORE resources are spent.
/// This ensures players can see the risk assessment before committing to an action.</para>
/// <para>Corruption evaluation rules:</para>
/// <list type="bullet">
/// <item>Passive abilities (Blood Scent, Pain is Fuel): always safe, 0 Corruption</item>
/// <item>Fury Strike at 80+ Rage: +1 Corruption</item>
/// <item>Reckless Assault at 80+ Rage: +1 Corruption per turn (v0.20.5b)</item>
/// <item>Unstoppable at 80+ Rage: +1 Corruption (v0.20.5b)</item>
/// <item>Intimidating Presence vs Coherent target: +1 Corruption (v0.20.5b)</item>
/// <item>Capstone activation: +2 Corruption always (v0.20.5c)</item>
/// <item>Combat entry while Enraged: +1 Corruption</item>
/// <item>Kill Coherent target while Enraged: +1 Corruption</item>
/// <item>Sustained Berserk Rage (100 for 3+ turns): +1 Corruption/turn</item>
/// </list>
/// </remarks>
public interface IBerserkrCorruptionService
{
    /// <summary>
    /// Evaluates the Corruption risk for a specific ability use at the current Rage level.
    /// </summary>
    /// <param name="abilityId">The Berserkr ability being used.</param>
    /// <param name="currentRage">The player's current Rage value at time of evaluation.</param>
    /// <param name="targetIsCoherent">
    /// Whether the target is Coherent-aligned. Only relevant for
    /// <see cref="BerserkrAbilityId.IntimidatingPresence"/>.
    /// </param>
    /// <returns>
    /// A <see cref="BerserkrCorruptionRiskResult"/> indicating whether Corruption was triggered
    /// and the amount to apply.
    /// </returns>
    BerserkrCorruptionRiskResult EvaluateRisk(
        BerserkrAbilityId abilityId,
        int currentRage,
        bool targetIsCoherent = false);

    /// <summary>
    /// Applies a Corruption result to a character.
    /// Should be called after the ability has been successfully executed.
    /// </summary>
    /// <param name="characterId">The character accumulating Corruption.</param>
    /// <param name="result">The Corruption risk result to apply.</param>
    /// <remarks>
    /// Only applies Corruption if <see cref="BerserkrCorruptionRiskResult.IsTriggered"/> is true.
    /// Actual Corruption tracking is handled by the broader Corruption system;
    /// this method serves as the integration point.
    /// </remarks>
    void ApplyCorruption(Guid characterId, BerserkrCorruptionRiskResult result);

    /// <summary>
    /// Evaluates Corruption risk for entering combat while Enraged.
    /// </summary>
    /// <param name="currentRage">The player's Rage at combat start.</param>
    /// <returns>
    /// A Corruption risk result for the <see cref="BerserkrCorruptionTrigger.EnterCombatEnraged"/> trigger.
    /// </returns>
    BerserkrCorruptionRiskResult CheckCombatEntryRisk(int currentRage);

    /// <summary>
    /// Gets a human-readable description for a specific Corruption trigger.
    /// Used for UI tooltips and combat log entries.
    /// </summary>
    /// <param name="trigger">The Corruption trigger to describe.</param>
    /// <returns>A descriptive string explaining the trigger and its conditions.</returns>
    string GetTriggerDescription(BerserkrCorruptionTrigger trigger);
}
