using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for managing boss ability rotations.
/// v0.42.3: Boss AI & Advanced Behaviors
/// </summary>
public interface IAbilityRotationService
{
    /// <summary>
    /// Gets the ability rotation for a boss phase.
    /// </summary>
    /// <param name="bossTypeId">Boss type ID.</param>
    /// <param name="phase">The phase to get rotation for.</param>
    /// <returns>The ability rotation, or null if none configured.</returns>
    Task<AbilityRotation?> GetPhaseRotationAsync(int bossTypeId, BossPhase phase);

    /// <summary>
    /// Selects the next ability in the boss's rotation.
    /// </summary>
    /// <param name="boss">The boss using the ability.</param>
    /// <param name="rotation">The ability rotation.</param>
    /// <param name="state">Current battlefield state.</param>
    /// <returns>The selected ability (or basic attack if rotation unavailable).</returns>
    Task<object> SelectNextAbilityInRotationAsync(
        Enemy boss,
        AbilityRotation rotation,
        BattlefieldState state);

    /// <summary>
    /// Checks if an ability is currently available (not on cooldown, enough resources).
    /// </summary>
    /// <param name="boss">The boss that would use it.</param>
    /// <param name="abilityId">The ability ID to check.</param>
    /// <returns>True if available.</returns>
    bool IsAbilityAvailable(Enemy boss, int abilityId);

    /// <summary>
    /// Resets the boss's rotation to the beginning.
    /// Called during phase transitions.
    /// </summary>
    /// <param name="boss">The boss to reset.</param>
    void ResetRotation(Enemy boss);
}
