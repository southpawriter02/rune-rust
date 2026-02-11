// ═══════════════════════════════════════════════════════════════════════════════
// BerserkrCorruptionService.cs
// Application service evaluating Corruption risk from Berserkr abilities
// based on Rage level and ability type. Uses rage thresholds rather than
// light conditions (contrast with ShadowCorruptionService).
// Version: 0.20.5a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Evaluates Corruption risk for Berserkr abilities based on Rage levels.
/// </summary>
/// <remarks>
/// <para>
/// The Berserkr is a Heretical specialization where Corruption risk is tied
/// to Rage intensity. Unlike the Myrk-gengr (light-level-based), Berserkr
/// Corruption triggers when abilities are used at 80+ Rage.
/// </para>
/// <para>
/// <strong>Tier 1 Risk Table:</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Fury Strike at 80+ Rage: +1 Corruption</description></item>
///   <item><description>Blood Scent (passive): No risk</description></item>
///   <item><description>Pain is Fuel (passive): No risk</description></item>
///   <item><description>Entering combat at 80+ Rage: +1 Corruption</description></item>
/// </list>
/// </remarks>
/// <seealso cref="IBerserkrCorruptionService"/>
/// <seealso cref="BerserkrCorruptionRiskResult"/>
/// <seealso cref="BerserkrCorruptionTrigger"/>
public class BerserkrCorruptionService(ILogger<BerserkrCorruptionService> logger)
    : IBerserkrCorruptionService
{
    private readonly ILogger<BerserkrCorruptionService> _logger = logger;

    // ─────────────────────────────────────────────────────────────────────────
    // Risk Evaluation
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public BerserkrCorruptionRiskResult EvaluateRisk(
        BerserkrAbilityId abilityId,
        int currentRage,
        bool targetIsCoherent = false)
    {
        _logger.LogDebug(
            "Evaluating corruption risk for {AbilityId} at {CurrentRage} Rage, " +
            "TargetCoherent={TargetIsCoherent}",
            abilityId, currentRage, targetIsCoherent);

        // Passive abilities never trigger Corruption
        if (abilityId is BerserkrAbilityId.BloodScent or BerserkrAbilityId.PainIsFuel)
        {
            _logger.LogDebug(
                "No corruption risk for passive ability {AbilityId}",
                abilityId);

            return BerserkrCorruptionRiskResult.CreateSafe(
                $"{abilityId} is a passive ability with no Corruption risk");
        }

        // Below Enraged threshold — no Corruption
        if (currentRage < RageResource.EnragedThreshold)
        {
            _logger.LogDebug(
                "No corruption risk for {AbilityId} at {CurrentRage} Rage " +
                "(below {Threshold} threshold)",
                abilityId, currentRage, RageResource.EnragedThreshold);

            return BerserkrCorruptionRiskResult.CreateSafe(
                $"Rage ({currentRage}) below Enraged threshold ({RageResource.EnragedThreshold})");
        }

        // Determine trigger and amount based on ability
        var (trigger, amount) = abilityId switch
        {
            BerserkrAbilityId.FuryStrike =>
                (BerserkrCorruptionTrigger.FuryStrikeWhileEnraged, 1),
            BerserkrAbilityId.RecklessAssault =>
                (BerserkrCorruptionTrigger.AbilityWhileEnraged, 1),
            BerserkrAbilityId.Unstoppable =>
                (BerserkrCorruptionTrigger.UnstoppableWhileEnraged, 1),
            BerserkrAbilityId.IntimidatingPresence when targetIsCoherent =>
                (BerserkrCorruptionTrigger.IntimidatingCoherentTarget, 1),
            BerserkrAbilityId.FuryOfTheForlorn =>
                (BerserkrCorruptionTrigger.FuryOfTheForlornUsage, 1),
            BerserkrAbilityId.AvatarOfDestruction =>
                (BerserkrCorruptionTrigger.CapstoneActivation, 2),
            _ => (BerserkrCorruptionTrigger.AbilityWhileEnraged, 0)
        };

        // Non-Corruption active abilities at high Rage
        if (amount <= 0)
        {
            return BerserkrCorruptionRiskResult.CreateSafe(
                $"{abilityId} does not trigger Corruption at this Rage level");
        }

        var reason = $"{abilityId} used at {currentRage} Rage (Enraged)";

        _logger.LogWarning(
            "Corruption risk triggered: {AbilityId} at {CurrentRage} Rage → " +
            "+{CorruptionAmount} corruption. Trigger: {Trigger}",
            abilityId, currentRage, amount, trigger);

        return BerserkrCorruptionRiskResult.CreateTriggered(amount, trigger, reason);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Corruption Application
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public void ApplyCorruption(Guid characterId, BerserkrCorruptionRiskResult risk)
    {
        if (!risk.IsTriggered)
        {
            _logger.LogDebug(
                "No Corruption to apply for {CharacterId}: risk was not triggered",
                characterId);
            return;
        }

        // Log the application — actual corruption tracking is handled by
        // the ICorruptionService integration (deferred to game loop integration)
        _logger.LogWarning(
            "Applying +{CorruptionAmount} Corruption to {CharacterId}: " +
            "Trigger={Trigger}, Reason={Reason}",
            risk.CorruptionAmount, characterId, risk.Trigger, risk.Reason);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Combat Entry Risk
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public BerserkrCorruptionRiskResult CheckCombatEntryRisk(int currentRage)
    {
        _logger.LogDebug(
            "Checking combat entry risk at {CurrentRage} Rage",
            currentRage);

        if (currentRage < RageResource.EnragedThreshold)
        {
            return BerserkrCorruptionRiskResult.CreateSafe(
                $"Rage ({currentRage}) below Enraged threshold for combat entry");
        }

        _logger.LogWarning(
            "Combat entry at {CurrentRage} Rage triggers Corruption risk: +1",
            currentRage);

        return BerserkrCorruptionRiskResult.CreateTriggered(
            corruptionAmount: 1,
            trigger: BerserkrCorruptionTrigger.EnterCombatEnraged,
            reason: $"Entering combat at {currentRage} Rage (Enraged)");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Display
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public string GetTriggerDescription(BerserkrCorruptionTrigger trigger) => trigger switch
    {
        BerserkrCorruptionTrigger.EnterCombatEnraged =>
            "Entering combat while Enraged (80+ Rage): +1 Corruption",
        BerserkrCorruptionTrigger.AbilityWhileEnraged =>
            "Using an active ability while Enraged: +1 Corruption",
        BerserkrCorruptionTrigger.FuryStrikeWhileEnraged =>
            "Using Fury Strike while Enraged: +1 Corruption",
        BerserkrCorruptionTrigger.UnstoppableWhileEnraged =>
            "Using Unstoppable while Enraged: +1 Corruption",
        BerserkrCorruptionTrigger.IntimidatingCoherentTarget =>
            "Using Intimidating Presence against a Coherent target: +1 Corruption",
        BerserkrCorruptionTrigger.FuryOfTheForlornUsage =>
            "Activating Fury of the Forlorn: +1 Corruption",
        BerserkrCorruptionTrigger.CapstoneActivation =>
            "Activating Avatar of Destruction: +2 Corruption (always)",
        BerserkrCorruptionTrigger.KillCoherentEnraged =>
            "Killing a Coherent creature while Enraged: +1 Corruption",
        BerserkrCorruptionTrigger.SustainedBerserkRage =>
            "Sustaining Berserk (100 Rage) for extended periods",
        _ => $"Unknown trigger: {trigger}"
    };
}
