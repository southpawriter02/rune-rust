using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for managing AI behavior patterns and archetypes.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// Note: Full implementation in v0.42.2
/// </summary>
public interface IBehaviorPatternService
{
    /// <summary>
    /// Gets the AI archetype for an enemy.
    /// Can be overridden by boss-specific behavior or dynamic assignment.
    /// </summary>
    /// <param name="enemy">The enemy to get the archetype for.</param>
    /// <returns>The AI archetype.</returns>
    Task<AIArchetype> GetArchetypeAsync(Enemy enemy);

    /// <summary>
    /// Gets the default archetype for an enemy type.
    /// Used for initial archetype assignment.
    /// </summary>
    /// <param name="enemyType">The enemy type.</param>
    /// <returns>The default archetype for that enemy type.</returns>
    AIArchetype GetDefaultArchetype(EnemyType enemyType);

    /// <summary>
    /// Checks if an enemy should override its normal archetype due to special conditions.
    /// Examples: Boss entering final phase, low HP triggering desperate behavior, etc.
    /// </summary>
    /// <param name="enemy">The enemy to check.</param>
    /// <param name="situation">Current situational context.</param>
    /// <returns>The overridden archetype, or null if no override.</returns>
    AIArchetype? GetArchetypeOverride(Enemy enemy, SituationalContext situation);
}
