using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Berserkr-specific Corruption risk evaluation.
/// Performs stateless evaluation of Corruption triggers based on ability type,
/// current Rage level, and target alignment.
/// </summary>
/// <remarks>
/// <para>Corruption evaluation is stateless — each call independently assesses
/// whether an action triggers Corruption based on the provided parameters.
/// The service does not maintain internal state between evaluations.</para>
/// <para>Corruption rules:</para>
/// <list type="bullet">
/// <item>Passive abilities (Blood Scent, Pain is Fuel): always safe, 0 Corruption</item>
/// <item>Fury Strike at 80+ Rage: +1 Corruption via <see cref="BerserkrCorruptionTrigger.FuryStrikeWhileEnraged"/></item>
/// <item>Reckless Assault at 80+ Rage: +1 Corruption/turn (v0.20.5b)</item>
/// <item>Unstoppable at 80+ Rage: +1 Corruption (v0.20.5b)</item>
/// <item>Intimidating Presence vs Coherent target: +1 Corruption (v0.20.5b)</item>
/// <item>Capstone activation: +2 Corruption always (v0.20.5c)</item>
/// <item>Combat entry while Enraged (80+): +1 Corruption</item>
/// <item>Sustained Berserk (100 Rage, 3+ turns): +1 Corruption/turn</item>
/// </list>
/// </remarks>
public class BerserkrCorruptionService : IBerserkrCorruptionService
{
    /// <summary>
    /// Standard Corruption amount for most triggers (+1).
    /// </summary>
    private const int StandardCorruption = 1;

    /// <summary>
    /// Elevated Corruption amount for capstone-level triggers (+2).
    /// </summary>
    private const int CapstoneCorruption = 2;

    /// <summary>
    /// Rage threshold at which active abilities start triggering Corruption.
    /// </summary>
    private const int EnragedThreshold = 80;

    private readonly ILogger<BerserkrCorruptionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BerserkrCorruptionService"/> class.
    /// </summary>
    /// <param name="logger">Logger for Corruption evaluation results.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public BerserkrCorruptionService(ILogger<BerserkrCorruptionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public BerserkrCorruptionRiskResult EvaluateRisk(
        BerserkrAbilityId abilityId,
        int currentRage,
        bool targetIsCoherent = false)
    {
        var result = abilityId switch
        {
            // === Passive abilities: always safe ===
            BerserkrAbilityId.BloodScent => BerserkrCorruptionRiskResult.CreateSafe(
                "Blood Scent is a passive ability — no Corruption risk"),

            BerserkrAbilityId.PainIsFuel => BerserkrCorruptionRiskResult.CreateSafe(
                "Pain is Fuel is a passive ability — no Corruption risk"),

            // === Fury Strike: +1 at 80+ Rage ===
            BerserkrAbilityId.FuryStrike => EvaluateFuryStrikeRisk(currentRage),

            // === Tier 2: Reckless Assault at 80+ Rage ===
            BerserkrAbilityId.RecklessAssault => EvaluateRageThresholdRisk(
                currentRage,
                BerserkrCorruptionTrigger.RecklessAssaultEnraged,
                "Reckless Assault"),

            // === Tier 2: Unstoppable at 80+ Rage ===
            BerserkrAbilityId.Unstoppable => EvaluateRageThresholdRisk(
                currentRage,
                BerserkrCorruptionTrigger.UnstoppableWhileEnraged,
                "Unstoppable"),

            // === Tier 2: Intimidating Presence vs Coherent target ===
            BerserkrAbilityId.IntimidatingPresence => EvaluateIntimidationRisk(
                currentRage, targetIsCoherent),

            // === Tier 3: Fury of the Forlorn ===
            BerserkrAbilityId.FuryOfTheForlorn => EvaluateRageThresholdRisk(
                currentRage,
                BerserkrCorruptionTrigger.FuryOfTheForlornUsage,
                "Fury of the Forlorn"),

            // === Capstone: always +2 ===
            BerserkrAbilityId.AvatarOfDestruction => BerserkrCorruptionRiskResult.CreateTriggered(
                CapstoneCorruption,
                BerserkrCorruptionTrigger.CapstoneActivation,
                "Avatar of Destruction always generates Corruption — the price of ultimate power"),

            // === Tier 3: Death Defiance is a reaction — evaluated separately ===
            BerserkrAbilityId.DeathDefiance => BerserkrCorruptionRiskResult.CreateSafe(
                "Death Defiance is a reactive survival ability — no Corruption risk"),

            _ => BerserkrCorruptionRiskResult.CreateSafe(
                $"Unknown ability {abilityId} — defaulting to safe")
        };

        // Log the evaluation result
        if (result.IsTriggered)
        {
            _logger.LogWarning(
                "Corruption triggered: {Ability} at Rage {Rage} — +{Amount} Corruption. " +
                "Trigger: {Trigger}. Reason: {Reason}",
                abilityId, currentRage, result.CorruptionAmount,
                result.Trigger, result.Reason);
        }
        else
        {
            _logger.LogInformation(
                "Corruption evaluated: {Ability} at Rage {Rage} — safe. Reason: {Reason}",
                abilityId, currentRage, result.Reason);
        }

        return result;
    }

    /// <inheritdoc />
    public void ApplyCorruption(Guid characterId, BerserkrCorruptionRiskResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.IsTriggered)
        {
            _logger.LogInformation(
                "ApplyCorruption skipped: no Corruption to apply for character {CharacterId}",
                characterId);
            return;
        }

        // Actual Corruption application is handled by the broader Corruption system.
        // This method serves as the integration point and logging boundary.
        _logger.LogWarning(
            "Corruption applied: character {CharacterId} gained +{Amount} Corruption. " +
            "Trigger: {Trigger}. Reason: {Reason}",
            characterId, result.CorruptionAmount, result.Trigger, result.Reason);
    }

    /// <inheritdoc />
    public BerserkrCorruptionRiskResult CheckCombatEntryRisk(int currentRage)
    {
        if (currentRage < EnragedThreshold)
        {
            var safeResult = BerserkrCorruptionRiskResult.CreateSafe(
                $"Entering combat at Rage {currentRage} (below Enraged threshold {EnragedThreshold})");

            _logger.LogInformation(
                "Combat entry Corruption check: Rage {Rage} — safe (below {Threshold})",
                currentRage, EnragedThreshold);

            return safeResult;
        }

        var triggeredResult = BerserkrCorruptionRiskResult.CreateTriggered(
            StandardCorruption,
            BerserkrCorruptionTrigger.EnterCombatEnraged,
            $"Entering combat while Enraged (Rage {currentRage}) — the fury bleeds into the aether");

        _logger.LogWarning(
            "Combat entry Corruption triggered: Rage {Rage} ≥ {Threshold} — +{Amount} Corruption",
            currentRage, EnragedThreshold, StandardCorruption);

        return triggeredResult;
    }

    /// <inheritdoc />
    public string GetTriggerDescription(BerserkrCorruptionTrigger trigger)
    {
        return trigger switch
        {
            BerserkrCorruptionTrigger.EnterCombatEnraged =>
                "Entering combat while Enraged (Rage 80+): +1 Corruption. " +
                "Your seething fury draws unwanted attention from the aether.",

            BerserkrCorruptionTrigger.AbilityWhileEnraged =>
                "Using any ability while Enraged (Rage 80+): +1 Corruption. " +
                "Channeling power through unchecked rage corrupts the weave.",

            BerserkrCorruptionTrigger.FuryStrikeWhileEnraged =>
                "Using Fury Strike while Enraged (Rage 80+): +1 Corruption. " +
                "The devastating blow tears at reality's fabric.",

            BerserkrCorruptionTrigger.RecklessAssaultEnraged =>
                "Maintaining Reckless Assault while Enraged (Rage 80+): +1 Corruption per turn. " +
                "Sustained berserker fury erodes your spiritual defenses.",

            BerserkrCorruptionTrigger.UnstoppableWhileEnraged =>
                "Activating Unstoppable while Enraged (Rage 80+): +1 Corruption. " +
                "Pushing beyond mortal limits invites darker power.",

            BerserkrCorruptionTrigger.IntimidatingCoherentTarget =>
                "Using Intimidating Presence against a Coherent-aligned target: +1 Corruption. " +
                "Terrorizing the order-aligned stains your soul.",

            BerserkrCorruptionTrigger.FuryOfTheForlornUsage =>
                "Fury of the Forlorn passive triggering while Enraged: +1 Corruption. " +
                "Fighting alone in fury opens pathways to corruption.",

            BerserkrCorruptionTrigger.CapstoneActivation =>
                "Activating Avatar of Destruction: +2 Corruption. " +
                "Becoming a living weapon always demands a price.",

            BerserkrCorruptionTrigger.KillCoherentEnraged =>
                "Killing a Coherent-aligned target while Enraged: +1 Corruption. " +
                "Destroying order-aligned beings in fury deepens the taint.",

            BerserkrCorruptionTrigger.SustainedBerserkRage =>
                "Sustaining maximum Rage (100) for 3+ turns: +1 Corruption per turn. " +
                "The aether itself recoils from your sustained fury.",

            _ => $"Unknown trigger: {trigger}"
        };
    }

    /// <summary>
    /// Evaluates Fury Strike Corruption risk based on current Rage.
    /// </summary>
    /// <param name="currentRage">The player's current Rage value.</param>
    /// <returns>Triggered (+1) if Rage ≥ 80; safe otherwise.</returns>
    private static BerserkrCorruptionRiskResult EvaluateFuryStrikeRisk(int currentRage)
    {
        if (currentRage < EnragedThreshold)
        {
            return BerserkrCorruptionRiskResult.CreateSafe(
                $"Fury Strike at Rage {currentRage} (below Enraged threshold) — safe");
        }

        return BerserkrCorruptionRiskResult.CreateTriggered(
            StandardCorruption,
            BerserkrCorruptionTrigger.FuryStrikeWhileEnraged,
            $"Fury Strike used while Enraged (Rage {currentRage}) — fury tears at reality");
    }

    /// <summary>
    /// Evaluates Corruption risk for abilities that trigger based on Rage threshold.
    /// </summary>
    /// <param name="currentRage">The player's current Rage value.</param>
    /// <param name="trigger">The specific Corruption trigger for this ability.</param>
    /// <param name="abilityName">Display name of the ability for logging.</param>
    /// <returns>Triggered (+1) if Rage ≥ 80; safe otherwise.</returns>
    private static BerserkrCorruptionRiskResult EvaluateRageThresholdRisk(
        int currentRage,
        BerserkrCorruptionTrigger trigger,
        string abilityName)
    {
        if (currentRage < EnragedThreshold)
        {
            return BerserkrCorruptionRiskResult.CreateSafe(
                $"{abilityName} at Rage {currentRage} (below Enraged threshold) — safe");
        }

        return BerserkrCorruptionRiskResult.CreateTriggered(
            StandardCorruption,
            trigger,
            $"{abilityName} used while Enraged (Rage {currentRage})");
    }

    /// <summary>
    /// Evaluates Intimidating Presence Corruption risk.
    /// Triggers on Coherent-aligned targets regardless of Rage level.
    /// </summary>
    /// <param name="currentRage">The player's current Rage value.</param>
    /// <param name="targetIsCoherent">Whether the target is Coherent-aligned.</param>
    /// <returns>Triggered (+1) if target is Coherent; safe otherwise.</returns>
    private static BerserkrCorruptionRiskResult EvaluateIntimidationRisk(
        int currentRage,
        bool targetIsCoherent)
    {
        if (!targetIsCoherent)
        {
            return BerserkrCorruptionRiskResult.CreateSafe(
                "Intimidating Presence target is not Coherent-aligned — no Corruption risk");
        }

        return BerserkrCorruptionRiskResult.CreateTriggered(
            StandardCorruption,
            BerserkrCorruptionTrigger.IntimidatingCoherentTarget,
            $"Intimidating Presence used against Coherent-aligned target (Rage {currentRage})");
    }
}
