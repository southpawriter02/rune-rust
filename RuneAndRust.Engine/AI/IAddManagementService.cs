using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for managing boss add summoning and coordination.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public interface IAddManagementService
{
    /// <summary>
    /// Manages add summoning and coordination for a boss.
    /// </summary>
    /// <param name="boss">The boss managing adds.</param>
    /// <param name="state">Current battlefield state.</param>
    Task ManageAddsAsync(Enemy boss, BattlefieldState state);

    /// <summary>
    /// Checks if the boss should summon adds based on configuration.
    /// </summary>
    /// <param name="boss">The boss to check.</param>
    /// <param name="config">Add management configuration.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>True if should summon adds.</returns>
    bool ShouldSummonAdds(Enemy boss, AddManagementConfig config, BattlefieldState state);

    /// <summary>
    /// Summons adds for a boss.
    /// </summary>
    /// <param name="boss">The boss summoning adds.</param>
    /// <param name="config">Add management configuration.</param>
    /// <param name="state">Current battlefield state.</param>
    Task SummonAddsAsync(Enemy boss, AddManagementConfig config, BattlefieldState state);

    /// <summary>
    /// Coordinates targeting between boss and adds.
    /// </summary>
    /// <param name="boss">The boss coordinating.</param>
    /// <param name="state">Current battlefield state.</param>
    Task CoordinateWithAddsAsync(Enemy boss, BattlefieldState state);

    /// <summary>
    /// Gets the add management configuration for a boss phase.
    /// </summary>
    /// <param name="bossTypeId">Boss type ID.</param>
    /// <param name="phase">The phase to get configuration for.</param>
    /// <returns>The add management configuration, or null if none.</returns>
    Task<AddManagementConfig?> GetAddManagementConfigAsync(int bossTypeId, BossPhase phase);

    /// <summary>
    /// Gets all living adds summoned by a boss.
    /// </summary>
    /// <param name="boss">The boss.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>List of living adds.</returns>
    List<Enemy> GetLivingAdds(Enemy boss, BattlefieldState state);
}
