using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for prioritizing and selecting optimal abilities for enemies.
/// v0.42.2: Ability Usage & Behavior Patterns
/// </summary>
public interface IAbilityPrioritizationService
{
    /// <summary>
    /// Selects the optimal ability for an enemy to use against a target.
    /// </summary>
    /// <param name="enemy">The enemy using the ability.</param>
    /// <param name="target">The target of the ability.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>The selected ability (or basic attack if none available).</returns>
    Task<object> SelectOptimalAbilityAsync(
        Enemy enemy,
        object target,
        BattlefieldState state);

    /// <summary>
    /// Scores an individual ability for the current situation.
    /// </summary>
    /// <param name="ability">The ability to score.</param>
    /// <param name="enemy">The enemy using the ability.</param>
    /// <param name="target">The target of the ability.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>The ability score with component breakdown.</returns>
    Task<AbilityScore> ScoreAbilityAsync(
        object ability,
        Enemy enemy,
        object target,
        BattlefieldState state);

    /// <summary>
    /// Gets all available abilities for an enemy (not on cooldown, enough resources).
    /// </summary>
    /// <param name="enemy">The enemy to get abilities for.</param>
    /// <returns>List of available abilities.</returns>
    Task<List<object>> GetAvailableAbilitiesAsync(Enemy enemy);

    /// <summary>
    /// Calculates the damage score component for an ability.
    /// </summary>
    /// <param name="ability">The ability to score.</param>
    /// <param name="target">The target of the ability.</param>
    /// <returns>Damage score (0-100).</returns>
    float CalculateDamageScore(object ability, object target);

    /// <summary>
    /// Calculates the utility score component for an ability.
    /// Non-damage value (CC, buffs, debuffs).
    /// </summary>
    /// <param name="ability">The ability to score.</param>
    /// <param name="target">The target of the ability.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>Utility score (0-50).</returns>
    float CalculateUtilityScore(object ability, object target, BattlefieldState state);

    /// <summary>
    /// Calculates the efficiency score component for an ability.
    /// Resource cost vs benefit ratio.
    /// </summary>
    /// <param name="ability">The ability to score.</param>
    /// <param name="enemy">The enemy using the ability.</param>
    /// <returns>Efficiency score (0-30).</returns>
    float CalculateEfficiencyScore(object ability, Enemy enemy);

    /// <summary>
    /// Calculates the situational score component for an ability.
    /// Contextual value for current battlefield state.
    /// </summary>
    /// <param name="ability">The ability to score.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>Situation score (0-20).</returns>
    float CalculateSituationScore(object ability, BattlefieldState state);

    /// <summary>
    /// Gets the category of an ability for AI decision-making.
    /// </summary>
    /// <param name="ability">The ability to categorize.</param>
    /// <returns>The ability category.</returns>
    AbilityCategory GetAbilityCategory(object ability);

    /// <summary>
    /// Checks if an ability is on cooldown.
    /// </summary>
    /// <param name="ability">The ability to check.</param>
    /// <param name="enemy">The enemy that would use it.</param>
    /// <returns>True if the ability is on cooldown.</returns>
    bool IsAbilityOnCooldown(object ability, Enemy enemy);

    /// <summary>
    /// Checks if an enemy has enough resources to use an ability.
    /// </summary>
    /// <param name="ability">The ability to check.</param>
    /// <param name="enemy">The enemy that would use it.</param>
    /// <returns>True if the enemy has enough resources.</returns>
    bool HasSufficientResources(object ability, Enemy enemy);
}
