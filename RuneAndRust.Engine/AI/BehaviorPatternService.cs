using Microsoft.Extensions.Logging;
using RuneAndRust.Core;
using RuneAndRust.Core.AI;
using System.Threading.Tasks;

namespace RuneAndRust.Engine.AI;

/// <summary>
/// Service for managing AI behavior patterns and archetypes.
/// v0.42.1: Basic implementation for target selection
/// v0.42.2: Full implementation with ability usage patterns
/// </summary>
public class BehaviorPatternService : IBehaviorPatternService
{
    private readonly ILogger<BehaviorPatternService> _logger;

    public BehaviorPatternService(ILogger<BehaviorPatternService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<AIArchetype> GetArchetypeAsync(Enemy enemy)
    {
        // Check for archetype override first
        // (v0.42.3: Boss phase-based overrides)

        // Return the enemy's assigned archetype
        return Task.FromResult(enemy.AIArchetype);
    }

    /// <inheritdoc/>
    public AIArchetype GetDefaultArchetype(EnemyType enemyType)
    {
        // Assign default archetypes based on enemy type
        // This provides sensible defaults for existing enemies

        return enemyType switch
        {
            // v0.1-v0.3 enemies
            EnemyType.CorruptedServitor => AIArchetype.Reckless, // Mindless automaton
            EnemyType.BlightDrone => AIArchetype.Cautious, // Hovering ranged unit
            EnemyType.RuinWarden => AIArchetype.Defensive, // Guardian pattern

            // v0.4 enemies
            EnemyType.ScrapHound => AIArchetype.Aggressive, // Fast harasser
            EnemyType.TestSubject => AIArchetype.Reckless, // Glass cannon
            EnemyType.WarFrame => AIArchetype.Tactical, // Elite
            EnemyType.ForlornScholar => AIArchetype.Control, // Caster
            EnemyType.AethericAberration => AIArchetype.Tactical, // Boss

            // v0.6 enemies
            EnemyType.MaintenanceConstruct => AIArchetype.Defensive, // Self-healing balanced
            EnemyType.SludgeCrawler => AIArchetype.Aggressive, // Swarm enemy
            EnemyType.CorruptedEngineer => AIArchetype.Support, // Buffer/support
            EnemyType.VaultCustodian => AIArchetype.Defensive, // Mini-boss guardian
            EnemyType.ForlornArchivist => AIArchetype.Control, // Psychic/summoner boss
            EnemyType.OmegaSentinel => AIArchetype.Aggressive, // Physical tank boss

            // v0.16 enemies
            EnemyType.CorrodedSentry => AIArchetype.Cautious, // Draugr-Pattern drone
            EnemyType.HuskEnforcer => AIArchetype.Reckless, // Reanimated corpse
            EnemyType.ArcWelderUnit => AIArchetype.Aggressive, // Industrial robot
            EnemyType.Shrieker => AIArchetype.Control, // Psychic scream
            EnemyType.JotunReaderFragment => AIArchetype.Tactical, // AI fragment
            EnemyType.ServitorSwarm => AIArchetype.Aggressive, // Drone collective
            EnemyType.BoneKeeper => AIArchetype.Defensive, // Skeletal construct
            EnemyType.FailureColossus => AIArchetype.Aggressive, // Construction automaton
            EnemyType.RustWitch => AIArchetype.Control, // Advanced colony
            EnemyType.SentinelPrime => AIArchetype.Tactical, // Elite military automaton

            _ => AIArchetype.Tactical // Safe default
        };
    }

    /// <inheritdoc/>
    public AIArchetype? GetArchetypeOverride(Enemy enemy, SituationalContext situation)
    {
        // v0.42.1: Basic override logic
        // v0.42.3: Full boss phase-based overrides

        // Override to Reckless if critically wounded and not already Cautious
        if (situation.IsCriticalHP && enemy.AIArchetype != AIArchetype.Cautious)
        {
            _logger.LogDebug(
                "Archetype override: {EnemyId} → Reckless (critical HP: {HP:P0})",
                enemy.Id, situation.SelfHPPercent);
            return AIArchetype.Reckless; // Desperate final assault
        }

        // Override to Cautious if heavily disadvantaged
        if (situation.Advantage == TacticalAdvantage.Disadvantaged &&
            enemy.AIArchetype == AIArchetype.Aggressive)
        {
            _logger.LogDebug(
                "Archetype override: {EnemyId} → Cautious (disadvantaged)",
                enemy.Id);
            return AIArchetype.Cautious; // Retreat/defensive play
        }

        return null; // No override
    }
}
