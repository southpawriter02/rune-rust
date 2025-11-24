using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for scaling AI intelligence based on difficulty (NG+, Endless, Challenge Sectors).
/// v0.42.4: Integration & Difficulty Scaling
/// </summary>
public interface IDifficultyScalingService
{
    /// <summary>
    /// Gets the current AI intelligence level (0-5) based on game mode and difficulty.
    /// </summary>
    /// <returns>Intelligence level from 0 (basic) to 5 (perfect).</returns>
    Task<int> GetAIIntelligenceLevelAsync();

    /// <summary>
    /// Applies intelligence scaling to an enemy action.
    /// Low intelligence introduces intentional errors, high intelligence uses advanced tactics.
    /// </summary>
    /// <param name="action">The action to scale.</param>
    /// <param name="intelligenceLevel">Intelligence level (0-5).</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>Modified action with intelligence-appropriate decisions.</returns>
    Task<EnemyAction> ApplyIntelligenceScalingAsync(
        EnemyAction action,
        int intelligenceLevel,
        BattlefieldState state);

    /// <summary>
    /// Calculates error chance for a given intelligence level.
    /// </summary>
    /// <param name="intelligenceLevel">Intelligence level (0-5).</param>
    /// <returns>Error probability (0.0 to 1.0).</returns>
    double CalculateErrorChance(int intelligenceLevel);

    /// <summary>
    /// Checks if an action should be made suboptimal based on intelligence level.
    /// </summary>
    /// <param name="intelligenceLevel">Intelligence level (0-5).</param>
    /// <returns>True if should introduce an error.</returns>
    bool ShouldMakeError(int intelligenceLevel);
}
