using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

namespace RuneAndRust.Application.Interfaces;

/// <summary>
/// Defines the contract for Seiðkona-specific Corruption risk evaluation.
/// Evaluates whether Seiðkona casting actions trigger Corruption accumulation
/// based on current Aether Resonance level and probability-based d100 checks.
/// </summary>
/// <remarks>
/// <para>Key design principle: Corruption risk is evaluated BEFORE resources are spent,
/// consistent with the Berserkr pattern (v0.20.5a). The evaluation uses the current
/// Resonance level (before any gain from the cast).</para>
/// <para>Critical difference from <see cref="IBerserkrCorruptionService"/>:
/// Seiðkona Corruption is <strong>probability-based</strong>, not deterministic.
/// A d100 is rolled against a percentage threshold that scales with Resonance:</para>
/// <list type="bullet">
/// <item>Resonance 0–4 (Safe): 0% risk — no check performed</item>
/// <item>Resonance 5–7 (Risky): 5% risk — d100 ≤ 5 triggers +1 Corruption</item>
/// <item>Resonance 8–9 (Dangerous): 15% risk — d100 ≤ 15 triggers +1 Corruption</item>
/// <item>Resonance 10 (Critical): 25% risk — d100 ≤ 25 triggers +1 Corruption</item>
/// </list>
/// <para>Abilities that do NOT channel Aether (WyrdSight, AetherAttunement) are always
/// safe and never trigger Corruption checks.</para>
/// </remarks>
public interface ISeidkonaCorruptionService
{
    /// <summary>
    /// Evaluates the Corruption risk for a specific ability use at the current Resonance level.
    /// Performs a probability-based d100 check if the ability channels Aether and
    /// Resonance is at 5 or above.
    /// </summary>
    /// <param name="abilityId">The Seiðkona ability being used.</param>
    /// <param name="currentResonance">The player's current Aether Resonance at time of evaluation.</param>
    /// <returns>
    /// A <see cref="SeidkonaCorruptionRiskResult"/> indicating whether Corruption was triggered,
    /// the amount to apply, and full roll context (d100 result and threshold).
    /// </returns>
    SeidkonaCorruptionRiskResult EvaluateRisk(
        SeidkonaAbilityId abilityId,
        int currentResonance);

    /// <summary>
    /// Applies a Corruption result to a character.
    /// Should be called after the ability has been successfully executed.
    /// </summary>
    /// <param name="characterId">The character accumulating Corruption.</param>
    /// <param name="result">The Corruption risk result to apply.</param>
    /// <remarks>
    /// Only applies Corruption if <see cref="SeidkonaCorruptionRiskResult.IsTriggered"/> is true.
    /// Actual Corruption tracking is handled by the broader Corruption system;
    /// this method serves as the integration point and logging boundary.
    /// </remarks>
    void ApplyCorruption(Guid characterId, SeidkonaCorruptionRiskResult result);

    /// <summary>
    /// Gets a human-readable description for a specific Corruption trigger.
    /// Used for UI tooltips and combat log entries.
    /// </summary>
    /// <param name="trigger">The Corruption trigger to describe.</param>
    /// <returns>A descriptive string explaining the trigger, its conditions, and risk percentages.</returns>
    string GetTriggerDescription(SeidkonaCorruptionTrigger trigger);
}
