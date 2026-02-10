// ═══════════════════════════════════════════════════════════════════════════════
// ShadowCorruptionService.cs
// Application service for evaluating Corruption risk from Myrk-gengr
// shadow abilities. Uses static risk tables mapping ability × light level
// to corruption amounts.
// Version: 0.20.4a
// ═══════════════════════════════════════════════════════════════════════════════

namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Evaluates Corruption risk for Myrk-gengr shadow abilities based on
/// ability type, light conditions, and target characteristics.
/// </summary>
/// <remarks>
/// <para>
/// Corruption risk is determined by static lookup tables. The Myrk-gengr
/// is a Heretical specialization, meaning shadow ability usage in bright
/// conditions or against Coherent targets incurs Corruption.
/// </para>
/// <para>
/// <strong>Risk Table (Tier 1):</strong>
/// </para>
/// <list type="bullet">
///   <item><description>Shadow Step in BrightLight/Sunlight: +1 Corruption</description></item>
///   <item><description>Cloak of Night in BrightLight/Sunlight: +1 Corruption/turn</description></item>
///   <item><description>Dark-Adapted: Never triggers Corruption</description></item>
///   <item><description>Any ability targeting Coherent creature: +1 additional Corruption</description></item>
/// </list>
/// </remarks>
/// <seealso cref="IShadowCorruptionService"/>
/// <seealso cref="CorruptionRiskResult"/>
public class ShadowCorruptionService(ILogger<ShadowCorruptionService> logger)
    : IShadowCorruptionService
{
    private readonly ILogger<ShadowCorruptionService> _logger = logger;

    // ─────────────────────────────────────────────────────────────────────────
    // Corruption Risk Tables
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Static corruption risk table: maps (ability, lightLevel) → corruption amount.
    /// </summary>
    private static readonly Dictionary<MyrkgengrAbilityId, Dictionary<LightLevelType, int>>
        CorruptionRisks = new()
    {
        {
            MyrkgengrAbilityId.ShadowStep, new Dictionary<LightLevelType, int>
            {
                { LightLevelType.BrightLight, 1 },
                { LightLevelType.Sunlight, 1 }
            }
        },
        {
            MyrkgengrAbilityId.CloakOfNight, new Dictionary<LightLevelType, int>
            {
                { LightLevelType.BrightLight, 1 },
                { LightLevelType.Sunlight, 1 }
            }
        }
        // Dark-Adapted never triggers corruption — intentionally absent
    };

    // ─────────────────────────────────────────────────────────────────────────
    // Risk Evaluation
    // ─────────────────────────────────────────────────────────────────────────

    /// <inheritdoc />
    public CorruptionRiskResult EvaluateRisk(
        MyrkgengrAbilityId ability,
        LightLevelType lightLevel,
        bool targetIsCoherent = false)
    {
        _logger.LogDebug(
            "Evaluating corruption risk for {Ability} at {LightLevel}, " +
            "TargetCoherent={TargetIsCoherent}",
            ability, lightLevel, targetIsCoherent);

        var baseCorruption = GetCorruptionAmount(ability, lightLevel);
        var coherentBonus = targetIsCoherent ? 1 : 0;
        var totalCorruption = baseCorruption + coherentBonus;

        if (totalCorruption <= 0)
        {
            _logger.LogDebug(
                "No corruption risk for {Ability} at {LightLevel}",
                ability, lightLevel);

            return CorruptionRiskResult.CreateSafe(
                ability.ToString().ToLowerInvariant(),
                lightLevel);
        }

        var reason = BuildCorruptionReason(ability, lightLevel, targetIsCoherent);

        _logger.LogWarning(
            "Corruption risk triggered: {Ability} at {LightLevel} → " +
            "+{CorruptionAmount} corruption. Reason: {Reason}",
            ability, lightLevel, totalCorruption, reason);

        return CorruptionRiskResult.CreateTriggered(
            corruptionGained: totalCorruption,
            reason: reason,
            abilityUsed: ability.ToString().ToLowerInvariant(),
            lightCondition: lightLevel,
            targetIsCoherent: targetIsCoherent);
    }

    /// <inheritdoc />
    public int GetCorruptionAmount(MyrkgengrAbilityId ability, LightLevelType lightLevel)
    {
        if (CorruptionRisks.TryGetValue(ability, out var lightRisks) &&
            lightRisks.TryGetValue(lightLevel, out var amount))
        {
            return amount;
        }

        return 0;
    }

    /// <inheritdoc />
    public IReadOnlyList<CorruptionRiskResult> GetCorruptionTriggers(MyrkgengrAbilityId ability)
    {
        var triggers = new List<CorruptionRiskResult>();

        if (!CorruptionRisks.TryGetValue(ability, out var lightRisks))
        {
            _logger.LogDebug(
                "No corruption triggers registered for ability {Ability}",
                ability);
            return triggers;
        }

        foreach (var (lightLevel, amount) in lightRisks)
        {
            triggers.Add(CorruptionRiskResult.CreateTriggered(
                corruptionGained: amount,
                reason: $"{ability} used in {lightLevel}",
                abilityUsed: ability.ToString().ToLowerInvariant(),
                lightCondition: lightLevel));
        }

        _logger.LogDebug(
            "Retrieved {TriggerCount} corruption triggers for {Ability}",
            triggers.Count, ability);

        return triggers;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private Helpers
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a human-readable reason string for corruption trigger.
    /// </summary>
    private static string BuildCorruptionReason(
        MyrkgengrAbilityId ability,
        LightLevelType lightLevel,
        bool targetIsCoherent)
    {
        var parts = new List<string>();

        if (lightLevel >= LightLevelType.BrightLight)
            parts.Add($"{ability} used in {lightLevel}");

        if (targetIsCoherent)
            parts.Add("target is a Coherent creature");

        return parts.Count > 0
            ? string.Join("; ", parts)
            : "No corruption risk";
    }
}
