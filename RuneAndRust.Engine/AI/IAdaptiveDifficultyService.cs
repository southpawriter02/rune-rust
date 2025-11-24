using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for adaptive difficulty that responds to player strategies.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public interface IAdaptiveDifficultyService
{
    /// <summary>
    /// Analyzes player strategy patterns from combat history.
    /// </summary>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>Detected player strategy.</returns>
    PlayerStrategy AnalyzePlayerStrategy(BattlefieldState state);

    /// <summary>
    /// Applies counter-strategies based on detected player behavior.
    /// </summary>
    /// <param name="boss">The boss applying counter-strategies.</param>
    /// <param name="strategy">Detected player strategy.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>Modified target or ability selection.</returns>
    Task<object?> ApplyCounterStrategiesAsync(
        Enemy boss,
        PlayerStrategy strategy,
        BattlefieldState state);

    /// <summary>
    /// Checks if adaptive difficulty is enabled for a boss.
    /// </summary>
    /// <param name="boss">The boss to check.</param>
    /// <returns>True if adaptive difficulty is enabled.</returns>
    Task<bool> IsAdaptiveDifficultyEnabledAsync(Enemy boss);
}
