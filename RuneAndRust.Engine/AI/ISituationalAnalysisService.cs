using RuneAndRust.Core;
using RuneAndRust.Core.AI;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for analyzing the current battlefield situation.
/// Provides context for AI tactical decision-making.
/// v0.42.1: Tactical Decision-Making & Target Selection
/// </summary>
public interface ISituationalAnalysisService
{
    /// <summary>
    /// Analyzes the current battlefield situation from an enemy's perspective.
    /// </summary>
    /// <param name="actor">The enemy performing the analysis.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>Situational context with advantage assessment.</returns>
    SituationalContext AnalyzeSituation(Enemy actor, BattlefieldState state);

    /// <summary>
    /// Calculates the overall tactical advantage.
    /// Considers numerical superiority, HP advantage, and positioning.
    /// </summary>
    /// <param name="context">The situational context.</param>
    /// <returns>The calculated tactical advantage.</returns>
    TacticalAdvantage CalculateAdvantage(SituationalContext context);

    /// <summary>
    /// Checks if an enemy is being flanked (enemies on multiple sides).
    /// </summary>
    /// <param name="actor">The enemy to check.</param>
    /// <param name="grid">The battlefield grid.</param>
    /// <param name="playerCharacters">List of player characters.</param>
    /// <returns>True if the enemy is flanked.</returns>
    bool IsFlanked(Enemy actor, BattlefieldGrid? grid, List<PlayerCharacter> playerCharacters);

    /// <summary>
    /// Checks if an enemy has elevation advantage over all enemies.
    /// </summary>
    /// <param name="actor">The enemy to check.</param>
    /// <param name="grid">The battlefield grid.</param>
    /// <param name="playerCharacters">List of player characters.</param>
    /// <returns>True if the enemy has high ground.</returns>
    bool HasHighGround(Enemy actor, BattlefieldGrid? grid, List<PlayerCharacter> playerCharacters);

    /// <summary>
    /// Checks if an enemy is isolated from allies (no allies within 2 tiles).
    /// </summary>
    /// <param name="actor">The enemy to check.</param>
    /// <param name="grid">The battlefield grid.</param>
    /// <param name="allies">List of allied enemies.</param>
    /// <returns>True if the enemy is isolated.</returns>
    bool IsIsolated(Enemy actor, BattlefieldGrid? grid, List<Enemy> allies);
}
