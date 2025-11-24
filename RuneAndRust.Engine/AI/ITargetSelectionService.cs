using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for selecting optimal targets based on threat assessment and AI archetype.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public interface ITargetSelectionService
{
    /// <summary>
    /// Selects the optimal target for an enemy based on threat assessment and archetype behavior.
    /// </summary>
    /// <param name="enemy">The enemy selecting a target.</param>
    /// <param name="potentialTargets">List of potential targets (player characters or allies for healing).</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>The selected target, or null if no valid targets.</returns>
    Task<object?> SelectTargetAsync(
        Enemy enemy,
        List<object> potentialTargets,
        BattlefieldState state);

    /// <summary>
    /// Applies archetype-specific modifiers to threat scores.
    /// Different archetypes prioritize different factors.
    /// </summary>
    /// <param name="archetype">The AI archetype.</param>
    /// <param name="assessments">List of threat assessments.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>Dictionary of targets with modified scores.</returns>
    Task<Dictionary<object, float>> ApplyArchetypeModifiersAsync(
        AIArchetype archetype,
        List<ThreatAssessment> assessments,
        BattlefieldState state);

    /// <summary>
    /// Checks if a target is valid for targeting (alive, not untargetable, in range, etc.).
    /// </summary>
    /// <param name="enemy">The enemy attempting to target.</param>
    /// <param name="target">The potential target.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>True if the target is valid.</returns>
    bool IsValidTarget(Enemy enemy, object target, BattlefieldState state);

    /// <summary>
    /// Selects the best target for healing (for Support archetype).
    /// Prioritizes low-HP allies.
    /// </summary>
    /// <param name="healer">The healing enemy.</param>
    /// <param name="allies">List of allied enemies.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>The ally most in need of healing, or null.</returns>
    Task<Enemy?> SelectHealTargetAsync(
        Enemy healer,
        List<Enemy> allies,
        BattlefieldState state);
}
