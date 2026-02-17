using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Services;

/// <summary>
/// Service handling Seiðkona-specific Corruption risk evaluation.
/// Performs probability-based d100 Corruption checks that scale with Aether Resonance level.
/// </summary>
/// <remarks>
/// <para>Unlike the Berserkr's deterministic Corruption system (always triggers at 80+ Rage),
/// the Seiðkona uses probability-based d100 checks against a percentage threshold:</para>
/// <list type="bullet">
/// <item>Resonance 0–4 (Safe): 0% — no check performed</item>
/// <item>Resonance 5–7 (Risky): 5% — d100 ≤ 5 triggers +1 Corruption</item>
/// <item>Resonance 8–9 (Dangerous): 15% — d100 ≤ 15 triggers +1 Corruption</item>
/// <item>Resonance 10 (Critical): 25% — d100 ≤ 25 triggers +1 Corruption</item>
/// </list>
/// <para>Corruption evaluation is stateless — each call independently assesses risk
/// based on the provided Resonance level and a fresh d100 roll.</para>
/// <para>The d100 roll method is marked <c>internal virtual</c> for unit test overriding,
/// following the <see cref="BerserkrAbilityService"/> dice pattern.</para>
/// </remarks>
public class SeidkonaCorruptionService : ISeidkonaCorruptionService
{
    /// <summary>
    /// Standard Corruption amount for most triggers (+1).
    /// </summary>
    private const int StandardCorruption = 1;

    /// <summary>
    /// Corruption risk percentage for the Risky tier (Resonance 5–7).
    /// </summary>
    private const int RiskyRiskPercent = 5;

    /// <summary>
    /// Corruption risk percentage for the Dangerous tier (Resonance 8–9).
    /// </summary>
    private const int DangerousRiskPercent = 15;

    /// <summary>
    /// Corruption risk percentage for the Critical tier (Resonance 10).
    /// </summary>
    private const int CriticalRiskPercent = 25;

    /// <summary>
    /// Resonance threshold for Corruption risk to begin (5+).
    /// </summary>
    private const int CorruptionRiskThreshold = 5;

    /// <summary>
    /// Resonance threshold for the Dangerous tier (8+).
    /// </summary>
    private const int DangerousThreshold = 8;

    /// <summary>
    /// Maximum Resonance value (Critical tier).
    /// </summary>
    private const int MaxResonance = 10;

    private readonly ILogger<SeidkonaCorruptionService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeidkonaCorruptionService"/> class.
    /// </summary>
    /// <param name="logger">Logger for Corruption evaluation results.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null.</exception>
    public SeidkonaCorruptionService(ILogger<SeidkonaCorruptionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public SeidkonaCorruptionRiskResult EvaluateRisk(
        SeidkonaAbilityId abilityId,
        int currentResonance)
    {
        var result = abilityId switch
        {
            // === Seiðr Bolt: probability-based check at Resonance 5+ ===
            SeidkonaAbilityId.SeidrBolt => EvaluateSeidrBoltRisk(currentResonance),

            // === Wyrd Sight: always safe — no Aether channeled ===
            SeidkonaAbilityId.WyrdSight => SeidkonaCorruptionRiskResult.CreateSafe(
                "Wyrd Sight is a detection ability — no Aether channeled, no Corruption risk"),

            // === Aether Attunement: always safe — passive ability ===
            SeidkonaAbilityId.AetherAttunement => SeidkonaCorruptionRiskResult.CreateSafe(
                "Aether Attunement is a passive ability — no Corruption risk"),

            // === Tier 2 active abilities: generic high-resonance check (v0.20.8b) ===
            SeidkonaAbilityId.FatesThread or
            SeidkonaAbilityId.WeaveDisruption => EvaluateGenericCastingRisk(currentResonance, abilityId),

            // === Resonance Cascade: always safe — passive ability (v0.20.8b) ===
            SeidkonaAbilityId.ResonanceCascade => SeidkonaCorruptionRiskResult.CreateSafe(
                "Resonance Cascade is a passive ability — no Aether channeled, no Corruption risk"),

            // === Future T3 abilities: generic high-resonance check ===
            SeidkonaAbilityId.VolvasVision or
            SeidkonaAbilityId.AetherStorm => EvaluateGenericCastingRisk(currentResonance, abilityId),

            // === Capstone: handled separately in v0.20.8c ===
            SeidkonaAbilityId.Unraveling => SeidkonaCorruptionRiskResult.CreateSafe(
                "Unraveling capstone Corruption check deferred to v0.20.8c"),

            _ => SeidkonaCorruptionRiskResult.CreateSafe(
                $"Unknown ability {abilityId} — defaulting to safe")
        };

        // Log the evaluation result
        if (result.IsTriggered)
        {
            _logger.LogWarning(
                "Corruption triggered: {Ability} at Resonance {Resonance} — +{Amount} Corruption. " +
                "Roll: {Roll} vs {RiskPercent}% threshold. Trigger: {Trigger}. Reason: {Reason}",
                abilityId, currentResonance, result.CorruptionAmount,
                result.RollResult, result.RiskPercent, result.Trigger, result.Reason);
        }
        else if (result.RollResult > 0)
        {
            _logger.LogInformation(
                "Corruption evaluated: {Ability} at Resonance {Resonance} — safe " +
                "(d100: {Roll} vs {RiskPercent}% threshold). Reason: {Reason}",
                abilityId, currentResonance, result.RollResult, result.RiskPercent, result.Reason);
        }
        else
        {
            _logger.LogInformation(
                "Corruption evaluated: {Ability} at Resonance {Resonance} — safe (no check needed). " +
                "Reason: {Reason}",
                abilityId, currentResonance, result.Reason);
        }

        return result;
    }

    /// <inheritdoc />
    public void ApplyCorruption(Guid characterId, SeidkonaCorruptionRiskResult result)
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
            "Trigger: {Trigger}. Roll: {Roll} vs {RiskPercent}% threshold. Reason: {Reason}",
            characterId, result.CorruptionAmount, result.Trigger,
            result.RollResult, result.RiskPercent, result.Reason);
    }

    /// <inheritdoc />
    public string GetTriggerDescription(SeidkonaCorruptionTrigger trigger)
    {
        return trigger switch
        {
            SeidkonaCorruptionTrigger.SeidrBoltLowResonance =>
                "Casting Seiðr Bolt at Resonance 5–7 (Risky): 5% chance of +1 Corruption. " +
                "The Aether stirs faintly, drawn by your casting.",

            SeidkonaCorruptionTrigger.SeidrBoltHighResonance =>
                "Casting Seiðr Bolt at Resonance 8–9 (Dangerous): 15% chance of +1 Corruption. " +
                "Reality warps around you as the Aether responds to your attunement.",

            SeidkonaCorruptionTrigger.SeidrBoltMaxResonance =>
                "Casting Seiðr Bolt at Resonance 10 (Critical): 25% chance of +1 Corruption. " +
                "The Wyrd screams as your Aetheric attunement reaches its peak.",

            SeidkonaCorruptionTrigger.CastingAtHighResonance =>
                "Casting any Aether ability at elevated Resonance: chance of +1 Corruption. " +
                "Each spell further frays the boundary between you and the chaos beyond.",

            SeidkonaCorruptionTrigger.CapstoneActivation =>
                "Activating the Unraveling capstone: 20% chance of +2 Corruption. " +
                "Releasing all accumulated Aetheric energy tears at the fabric of reality.",

            _ => $"Unknown trigger: {trigger}"
        };
    }

    /// <summary>
    /// Rolls a d100 (1–100) for Corruption probability checks.
    /// Marked <c>internal virtual</c> for unit test overriding.
    /// </summary>
    /// <returns>A random integer between 1 and 100 inclusive.</returns>
    internal virtual int RollD100() => Random.Shared.Next(1, 101);

    /// <summary>
    /// Evaluates Seiðr Bolt Corruption risk based on current Aether Resonance.
    /// Performs a probability-based d100 check if Resonance is 5 or above.
    /// </summary>
    /// <param name="currentResonance">The player's current Aether Resonance value.</param>
    /// <returns>
    /// Safe (no check) if Resonance &lt; 5; safe-with-roll if d100 exceeds threshold;
    /// triggered (+1) if d100 falls within threshold.
    /// </returns>
    private SeidkonaCorruptionRiskResult EvaluateSeidrBoltRisk(int currentResonance)
    {
        // Below Corruption risk threshold — no check needed
        if (currentResonance < CorruptionRiskThreshold)
        {
            return SeidkonaCorruptionRiskResult.CreateSafe(
                $"Seiðr Bolt at Resonance {currentResonance} (below risk threshold {CorruptionRiskThreshold}) — safe");
        }

        // Determine risk percentage and trigger type based on Resonance tier
        var (riskPercent, trigger) = GetSeidrBoltRiskParameters(currentResonance);

        // Roll d100 against the threshold
        var roll = RollD100();

        if (roll <= riskPercent)
        {
            // Corruption triggered
            return SeidkonaCorruptionRiskResult.CreateTriggered(
                StandardCorruption,
                trigger,
                $"Seiðr Bolt at Resonance {currentResonance} — the Aether tears at your soul " +
                $"(d100: {roll} ≤ {riskPercent}%)",
                roll,
                riskPercent);
        }

        // Safe — roll exceeded threshold
        return SeidkonaCorruptionRiskResult.CreateSafeWithRoll(
            $"Seiðr Bolt at Resonance {currentResonance} — the Aether surges but your will holds " +
            $"(d100: {roll} > {riskPercent}%)",
            roll,
            riskPercent);
    }

    /// <summary>
    /// Evaluates generic casting Corruption risk for T2/T3 abilities.
    /// Uses the same probability-based d100 check as Seiðr Bolt but with
    /// the <see cref="SeidkonaCorruptionTrigger.CastingAtHighResonance"/> trigger.
    /// </summary>
    /// <param name="currentResonance">The player's current Aether Resonance value.</param>
    /// <param name="abilityId">The ability being evaluated (for logging).</param>
    /// <returns>
    /// Safe if Resonance &lt; 5; otherwise a probability-based check result.
    /// </returns>
    private SeidkonaCorruptionRiskResult EvaluateGenericCastingRisk(
        int currentResonance,
        SeidkonaAbilityId abilityId)
    {
        if (currentResonance < CorruptionRiskThreshold)
        {
            return SeidkonaCorruptionRiskResult.CreateSafe(
                $"{abilityId} at Resonance {currentResonance} (below risk threshold) — safe");
        }

        var riskPercent = GetRiskPercent(currentResonance);
        var roll = RollD100();

        if (roll <= riskPercent)
        {
            return SeidkonaCorruptionRiskResult.CreateTriggered(
                StandardCorruption,
                SeidkonaCorruptionTrigger.CastingAtHighResonance,
                $"{abilityId} at Resonance {currentResonance} — Corruption triggered " +
                $"(d100: {roll} ≤ {riskPercent}%)",
                roll,
                riskPercent);
        }

        return SeidkonaCorruptionRiskResult.CreateSafeWithRoll(
            $"{abilityId} at Resonance {currentResonance} — safe " +
            $"(d100: {roll} > {riskPercent}%)",
            roll,
            riskPercent);
    }

    /// <summary>
    /// Gets the Seiðr Bolt-specific risk parameters for the current Resonance tier.
    /// </summary>
    /// <param name="currentResonance">The current Aether Resonance value (must be ≥ 5).</param>
    /// <returns>A tuple of (risk percentage, specific trigger type).</returns>
    private static (int riskPercent, SeidkonaCorruptionTrigger trigger) GetSeidrBoltRiskParameters(
        int currentResonance)
    {
        return currentResonance switch
        {
            >= MaxResonance => (CriticalRiskPercent, SeidkonaCorruptionTrigger.SeidrBoltMaxResonance),
            >= DangerousThreshold => (DangerousRiskPercent, SeidkonaCorruptionTrigger.SeidrBoltHighResonance),
            _ => (RiskyRiskPercent, SeidkonaCorruptionTrigger.SeidrBoltLowResonance)
        };
    }

    /// <summary>
    /// Gets the generic Corruption risk percentage for the current Resonance level.
    /// </summary>
    /// <param name="currentResonance">The current Aether Resonance value.</param>
    /// <returns>The risk percentage (0, 5, 15, or 25).</returns>
    private static int GetRiskPercent(int currentResonance)
    {
        return currentResonance switch
        {
            >= MaxResonance => CriticalRiskPercent,
            >= DangerousThreshold => DangerousRiskPercent,
            >= CorruptionRiskThreshold => RiskyRiskPercent,
            _ => 0
        };
    }
}
