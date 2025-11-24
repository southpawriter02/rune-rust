using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for managing boss AI behavior including phase transitions and special mechanics.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public interface IBossAIService
{
    /// <summary>
    /// Determines the current phase based on boss HP.
    /// </summary>
    /// <param name="boss">The boss to check.</param>
    /// <returns>The current boss phase.</returns>
    BossPhase DeterminePhase(Enemy boss);

    /// <summary>
    /// Checks if the boss should transition to a new phase.
    /// </summary>
    /// <param name="boss">The boss to check.</param>
    /// <param name="currentPhase">The current phase.</param>
    /// <returns>True if should transition.</returns>
    bool ShouldTransitionPhase(Enemy boss, BossPhase currentPhase);

    /// <summary>
    /// Executes a phase transition (dialogue, special abilities, buffs).
    /// </summary>
    /// <param name="boss">The boss transitioning.</param>
    /// <param name="newPhase">The new phase.</param>
    /// <param name="state">Current battlefield state.</param>
    Task ExecutePhaseTransitionAsync(Enemy boss, BossPhase newPhase, BattlefieldState state);

    /// <summary>
    /// Gets the phase transition configuration for a boss.
    /// </summary>
    /// <param name="bossTypeId">Boss type ID.</param>
    /// <param name="toPhase">The phase transitioning to.</param>
    /// <returns>The phase transition configuration, or null if none.</returns>
    Task<BossPhaseTransition?> GetPhaseTransitionConfigAsync(int bossTypeId, BossPhase toPhase);

    /// <summary>
    /// Gets the boss configuration for a boss type.
    /// </summary>
    /// <param name="bossTypeId">Boss type ID.</param>
    /// <returns>The boss configuration.</returns>
    Task<BossConfiguration?> GetBossConfigurationAsync(int bossTypeId);
}
