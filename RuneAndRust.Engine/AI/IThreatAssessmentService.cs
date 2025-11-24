using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for assessing the threat level of targets.
/// Calculates threat scores based on damage output, HP, positioning, abilities, and status effects.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public interface IThreatAssessmentService
{
    /// <summary>
    /// Assesses the threat level of a specific target from an enemy's perspective.
    /// </summary>
    /// <param name="assessor">The enemy performing the assessment.</param>
    /// <param name="target">The character being assessed (PlayerCharacter or Enemy for healing).</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>A threat assessment with scores and reasoning.</returns>
    Task<ThreatAssessment> AssessThreatAsync(
        Enemy assessor,
        object target,
        BattlefieldState state);

    /// <summary>
    /// Gets the threat weights configuration for a specific AI archetype.
    /// </summary>
    /// <param name="archetype">The AI archetype.</param>
    /// <returns>The threat weights configuration.</returns>
    Task<AIThreatWeights> GetThreatWeightsAsync(AIArchetype archetype);

    /// <summary>
    /// Evaluates the damage output threat factor for a target.
    /// Based on recent combat history (last 3 turns).
    /// </summary>
    /// <param name="target">The target to evaluate.</param>
    /// <param name="turns">Number of turns to look back.</param>
    /// <returns>Damage threat score (0-100).</returns>
    Task<float> EvaluateDamageThreatAsync(object target, int turns = 3);

    /// <summary>
    /// Evaluates the HP-based threat factor for a target.
    /// Lower HP = higher priority (easier to finish off).
    /// </summary>
    /// <param name="target">The target to evaluate.</param>
    /// <returns>HP threat score (0-20).</returns>
    float EvaluateHPThreat(object target);

    /// <summary>
    /// Evaluates the positioning threat factor for a target.
    /// Considers elevation, cover, isolation, flanking opportunities.
    /// </summary>
    /// <param name="target">The target to evaluate.</param>
    /// <param name="grid">The battlefield grid.</param>
    /// <returns>Position threat score (-10 to +20).</returns>
    float EvaluatePositionThreat(object target, BattlefieldGrid? grid);

    /// <summary>
    /// Evaluates the ability-based threat factor for a target.
    /// Higher threat for dangerous abilities that are off cooldown.
    /// </summary>
    /// <param name="target">The target to evaluate.</param>
    /// <returns>Ability threat score (0-30).</returns>
    Task<float> EvaluateAbilityThreatAsync(object target);

    /// <summary>
    /// Evaluates the status effect threat factor for a target.
    /// Buffs increase threat, debuffs decrease threat.
    /// </summary>
    /// <param name="target">The target to evaluate.</param>
    /// <returns>Status effect threat score (-10 to +10).</returns>
    float EvaluateStatusEffectThreat(object target);
}
