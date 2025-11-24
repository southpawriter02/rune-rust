using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for adapting AI behavior to Challenge Sector modifiers.
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public interface IChallengeSectorAIService
{
    /// <summary>
    /// Adapts AI tactics based on active Challenge Sector modifiers.
    /// </summary>
    /// <param name="enemy">The enemy adapting tactics.</param>
    /// <param name="action">The action to adapt.</param>
    /// <param name="modifiers">Active sector modifiers.</param>
    /// <param name="state">Current battlefield state.</param>
    Task AdaptToSectorModifiersAsync(
        Enemy enemy,
        EnemyAction action,
        List<ChallengeSectorModifier> modifiers,
        BattlefieldState state);

    /// <summary>
    /// Prioritizes burst damage for sectors where attrition isn't viable.
    /// </summary>
    Task PrioritizeBurstDamageAsync(Enemy enemy, EnemyAction action, BattlefieldState state);

    /// <summary>
    /// Prioritizes defensive abilities for high-risk sectors.
    /// </summary>
    Task PrioritizeDefenseAsync(Enemy enemy, EnemyAction action, BattlefieldState state);

    /// <summary>
    /// Conserves resources for long fights.
    /// </summary>
    Task ConserveResourcesAsync(Enemy enemy, EnemyAction action, BattlefieldState state);

    /// <summary>
    /// Increases aggression when players are handicapped.
    /// </summary>
    Task IncreaseAggressionAsync(Enemy enemy, EnemyAction action, decimal modifier);
}
