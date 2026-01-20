namespace RuneAndRust.Application.Services;

using Microsoft.Extensions.Logging;
using RuneAndRust.Application.Events;
using RuneAndRust.Application.Interfaces;
using RuneAndRust.Application.Tracking;
using RuneAndRust.Domain.Definitions;
using RuneAndRust.Domain.Entities;
using RuneAndRust.Domain.Enums;
using RuneAndRust.Domain.ValueObjects;

/// <summary>
/// Manages boss encounter mechanics including phase transitions,
/// transition effects, vulnerability windows, and summon tracking.
/// </summary>
/// <remarks>
/// <para>
/// BossMechanicsService is the runtime manager for boss encounters, handling:
/// </para>
/// <list type="bullet">
///   <item><description>Boss spawning from definitions with state initialization</description></item>
///   <item><description>Phase transition detection based on health thresholds</description></item>
///   <item><description>Transition effect execution (knockback, heal minions, create zones)</description></item>
///   <item><description>Vulnerability window management with damage multipliers</description></item>
///   <item><description>Per-turn state updates including summon intervals</description></item>
///   <item><description>Boss defeat handling with loot event publishing</description></item>
/// </list>
/// <para>
/// The service maintains a dictionary of <see cref="ActiveBossState"/> keyed by monster ID,
/// separating runtime state from static <see cref="BossDefinition"/> data.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Spawn a boss
/// var boss = bossMechanicsService.SpawnBoss("skeleton-king", new GridPosition(5, 5));
///
/// // After damage is applied
/// bossMechanicsService.OnBossDamaged(boss, damageDealt);
///
/// // Get damage multiplier for vulnerability
/// var multiplier = bossMechanicsService.GetVulnerabilityMultiplier(boss);
/// </code>
/// </example>
public class BossMechanicsService : IBossMechanicsService
{
    // ═══════════════════════════════════════════════════════════════
    // CONSTANTS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Default damage multiplier when boss is vulnerable.
    /// </summary>
    /// <remarks>
    /// <para>When <see cref="ActiveBossState.IsVulnerable"/> is true, damage dealt to
    /// the boss is multiplied by this value (1.5x = 50% bonus damage).</para>
    /// </remarks>
    private const float VulnerabilityMultiplier = 1.5f;

    // ═══════════════════════════════════════════════════════════════
    // DEPENDENCIES
    // ═══════════════════════════════════════════════════════════════

    private readonly IBossProvider _bossProvider;
    private readonly IGameEventLogger _eventLogger;
    private readonly ILogger<BossMechanicsService> _logger;

    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Tracks active boss states keyed by monster instance ID.
    /// </summary>
    private readonly Dictionary<Guid, ActiveBossState> _activeBosses = new();

    /// <summary>
    /// Tracks active boss monster instances keyed by monster instance ID.
    /// </summary>
    private readonly Dictionary<Guid, Monster> _bossMonsters = new();

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a new BossMechanicsService.
    /// </summary>
    /// <param name="bossProvider">Provider for boss definitions.</param>
    /// <param name="eventLogger">Logger for game events.</param>
    /// <param name="logger">Logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public BossMechanicsService(
        IBossProvider bossProvider,
        IGameEventLogger eventLogger,
        ILogger<BossMechanicsService> logger)
    {
        _bossProvider = bossProvider ?? throw new ArgumentNullException(nameof(bossProvider));
        _eventLogger = eventLogger ?? throw new ArgumentNullException(nameof(eventLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogDebug("BossMechanicsService initialized");
    }

    // ═══════════════════════════════════════════════════════════════
    // BOSS SPAWNING
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public Monster SpawnBoss(string bossId, GridPosition position)
    {
        _logger.LogDebug("SpawnBoss called for bossId={BossId} at position={Position}", bossId, position);

        var bossDef = _bossProvider.GetBoss(bossId);
        if (bossDef is null)
        {
            _logger.LogError("Boss definition not found: {BossId}", bossId);
            throw new ArgumentException($"Boss definition not found: {bossId}", nameof(bossId));
        }

        _logger.LogInformation(
            "Spawning boss {BossName} ({BossId}) at {Position}",
            bossDef.Name, bossId, position);

        // Create the boss monster instance
        // Note: In a full implementation, this would use a monster definition provider
        // to get base stats from the BaseMonsterDefinitionId. For now, we create
        // a boss with reasonable defaults based on the definition.
        var boss = new Monster(
            name: bossDef.Name,
            description: bossDef.Description,
            maxHealth: 1000, // Boss-tier health
            stats: new Stats(1000, 25, 15), // Boss-tier stats
            initiativeModifier: 5,
            monsterDefinitionId: bossDef.BaseMonsterDefinitionId,
            experienceValue: 500);

        // Set boss behavior from starting phase
        var startingPhase = bossDef.GetStartingPhase();
        if (startingPhase != null)
        {
            boss.SetBehavior(MapBossBehaviorToAIBehavior(startingPhase.Behavior));
            _logger.LogDebug(
                "Set boss behavior to {Behavior} from phase {Phase}",
                startingPhase.Behavior, startingPhase.Name);
        }

        // Initialize boss state
        var state = new ActiveBossState(bossId, startingPhase?.PhaseNumber ?? 1);
        _activeBosses[boss.Id] = state;
        _bossMonsters[boss.Id] = boss;

        _logger.LogInformation(
            "Boss {BossName} spawned with ID {MonsterId}, starting phase: {Phase}",
            bossDef.Name, boss.Id, startingPhase?.Name ?? "Unknown");

        // Log the spawn event
        _eventLogger.LogCombat(
            "BossSpawned",
            $"Boss '{bossDef.Name}' has entered the battle!",
            correlationId: boss.Id,
            data: new Dictionary<string, object>
            {
                ["BossId"] = bossId,
                ["TitleText"] = bossDef.TitleText ?? "",
                ["Position"] = position.ToString()
            });

        return boss;
    }

    // ═══════════════════════════════════════════════════════════════
    // DAMAGE NOTIFICATION
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void OnBossDamaged(Monster boss, int damage)
    {
        if (!_activeBosses.TryGetValue(boss.Id, out var state))
        {
            _logger.LogDebug("OnBossDamaged called for non-boss monster: {MonsterId}", boss.Id);
            return;
        }

        _logger.LogDebug(
            "OnBossDamaged: {BossId} took {Damage} damage, health={Health}/{MaxHealth}",
            state.BossId, damage, boss.Health, boss.MaxHealth);

        var bossDef = _bossProvider.GetBoss(state.BossId);
        if (bossDef is null)
        {
            _logger.LogWarning("Boss definition not found for active boss: {BossId}", state.BossId);
            return;
        }

        // Calculate current health percentage
        var healthPercent = CalculateHealthPercent(boss);
        _logger.LogDebug("Boss health percentage: {HealthPercent}%", healthPercent);

        // Get the phase for current health
        var newPhase = bossDef.GetPhaseForHealth(healthPercent);

        // Check for phase transition
        if (newPhase != null && newPhase.PhaseNumber != state.CurrentPhaseNumber)
        {
            _logger.LogInformation(
                "Phase transition detected: {OldPhase} -> {NewPhase}",
                state.CurrentPhaseNumber, newPhase.PhaseNumber);
            TransitionToPhase(boss, state, bossDef, newPhase);
        }

        // Check for defeat
        if (boss.Health <= 0)
        {
            _logger.LogInformation("Boss {BossName} defeated!", bossDef.Name);
            OnBossDefeated(boss, bossDef);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PHASE QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public BossPhase? GetCurrentPhase(Monster boss)
    {
        if (!_activeBosses.TryGetValue(boss.Id, out var state))
        {
            _logger.LogDebug("GetCurrentPhase: Monster {MonsterId} is not a tracked boss", boss.Id);
            return null;
        }

        var bossDef = _bossProvider.GetBoss(state.BossId);
        var phase = bossDef?.Phases.FirstOrDefault(p => p.PhaseNumber == state.CurrentPhaseNumber);

        _logger.LogDebug(
            "GetCurrentPhase: Boss {BossId} is in phase {PhaseNumber} ({PhaseName})",
            state.BossId, state.CurrentPhaseNumber, phase?.Name ?? "Unknown");

        return phase;
    }

    /// <inheritdoc />
    public ActiveBossState? GetBossState(Monster boss)
    {
        var state = _activeBosses.GetValueOrDefault(boss.Id);
        if (state == null)
        {
            _logger.LogDebug("GetBossState: Monster {MonsterId} is not a tracked boss", boss.Id);
        }
        return state;
    }

    // ═══════════════════════════════════════════════════════════════
    // VULNERABILITY MANAGEMENT
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsVulnerable(Monster boss)
    {
        var isVulnerable = _activeBosses.TryGetValue(boss.Id, out var state) && state.IsVulnerable;
        _logger.LogDebug("IsVulnerable: Boss {MonsterId} vulnerable={IsVulnerable}", boss.Id, isVulnerable);
        return isVulnerable;
    }

    /// <inheritdoc />
    public float GetVulnerabilityMultiplier(Monster boss)
    {
        var multiplier = IsVulnerable(boss) ? VulnerabilityMultiplier : 1.0f;
        _logger.LogDebug(
            "GetVulnerabilityMultiplier: Boss {MonsterId} multiplier={Multiplier}",
            boss.Id, multiplier);
        return multiplier;
    }

    /// <inheritdoc />
    public void SetVulnerable(Monster boss, int turns)
    {
        if (_activeBosses.TryGetValue(boss.Id, out var state))
        {
            state.VulnerableTurns = turns;

            _logger.LogInformation(
                "{BossId} is now vulnerable for {Turns} turns",
                state.BossId, turns);

            _eventLogger.LogCombat(
                "BossVulnerable",
                $"The boss is vulnerable! Deal extra damage for {turns} turns!",
                correlationId: boss.Id,
                data: new Dictionary<string, object>
                {
                    ["DurationTurns"] = turns,
                    ["Multiplier"] = VulnerabilityMultiplier
                });
        }
        else
        {
            _logger.LogWarning("SetVulnerable: Monster {MonsterId} is not a tracked boss", boss.Id);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // TURN PROCESSING
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void TickBoss(Monster boss)
    {
        if (!_activeBosses.TryGetValue(boss.Id, out var state))
        {
            _logger.LogDebug("TickBoss: Monster {MonsterId} is not a tracked boss", boss.Id);
            return;
        }

        _logger.LogDebug(
            "TickBoss: Processing turn for {BossId}, vulnerable={VulnTurns}, summonTimer={SummonTimer}",
            state.BossId, state.VulnerableTurns, state.TurnsSinceLastSummon);

        // Decrement vulnerability
        if (state.VulnerableTurns > 0)
        {
            state.VulnerableTurns--;
            _logger.LogDebug("Vulnerability countdown: {Remaining} turns remaining", state.VulnerableTurns);

            if (state.VulnerableTurns == 0)
            {
                _logger.LogInformation("{BossId} is no longer vulnerable", state.BossId);

                _eventLogger.LogCombat(
                    "BossVulnerabilityEnded",
                    "The boss recovers from vulnerability!",
                    correlationId: boss.Id);
            }
        }

        // Increment summon timer and check for summoning
        var bossDef = _bossProvider.GetBoss(state.BossId);
        var currentPhase = bossDef?.Phases.FirstOrDefault(p => p.PhaseNumber == state.CurrentPhaseNumber);

        if (currentPhase?.HasSummoning == true)
        {
            state.TurnsSinceLastSummon++;
            var summonConfig = currentPhase.SummonConfig;

            _logger.LogDebug(
                "Summon check: {TurnsSince}/{Interval} turns, active={Active}/{Max}",
                state.TurnsSinceLastSummon, summonConfig.IntervalTurns,
                state.ActiveSummonCount, summonConfig.MaxActive);

            if (state.TurnsSinceLastSummon >= summonConfig.IntervalTurns)
            {
                TrySummonMinions(boss, state, summonConfig);
                state.TurnsSinceLastSummon = 0;
            }
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // ABILITY QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public IReadOnlyList<string> GetAvailableAbilities(Monster boss)
    {
        var phase = GetCurrentPhase(boss);
        var abilities = phase?.AbilityIds ?? Array.Empty<string>();

        _logger.LogDebug(
            "GetAvailableAbilities: Boss {MonsterId} has {Count} abilities",
            boss.Id, abilities.Count);

        return abilities;
    }

    // ═══════════════════════════════════════════════════════════════
    // BOSS QUERIES
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public bool IsBoss(Monster monster)
    {
        var isBoss = _activeBosses.ContainsKey(monster.Id);
        _logger.LogDebug("IsBoss: Monster {MonsterId} isBoss={IsBoss}", monster.Id, isBoss);
        return isBoss;
    }

    /// <inheritdoc />
    public IReadOnlyList<Monster> GetActiveBosses()
    {
        var bosses = _bossMonsters.Values
            .Where(m => m.IsAlive)
            .ToList();

        _logger.LogDebug("GetActiveBosses: {Count} active bosses", bosses.Count);
        return bosses;
    }

    // ═══════════════════════════════════════════════════════════════
    // MINION TRACKING
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc />
    public void OnMinionDeath(Monster boss, Guid minionId)
    {
        if (_activeBosses.TryGetValue(boss.Id, out var state))
        {
            state.RemoveSummonedMinion(minionId);
            _logger.LogDebug(
                "Minion {MinionId} died, removed from {BossId} summons. Active: {Count}",
                minionId, state.BossId, state.ActiveSummonCount);
        }
        else
        {
            _logger.LogDebug("OnMinionDeath: Monster {MonsterId} is not a tracked boss", boss.Id);
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS - PHASE TRANSITIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Transitions the boss to a new phase.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <param name="state">The boss's active state.</param>
    /// <param name="bossDef">The boss definition.</param>
    /// <param name="newPhase">The phase to transition to.</param>
    private void TransitionToPhase(
        Monster boss,
        ActiveBossState state,
        BossDefinition bossDef,
        BossPhase newPhase)
    {
        var oldPhaseNumber = state.CurrentPhaseNumber;
        state.CurrentPhaseNumber = newPhase.PhaseNumber;

        _logger.LogInformation(
            "{BossName} transitioning from phase {OldPhase} to phase {NewPhase} ({PhaseName})",
            bossDef.Name, oldPhaseNumber, newPhase.PhaseNumber, newPhase.Name);

        // Execute transition effects
        if (!string.IsNullOrEmpty(newPhase.TransitionEffectId))
        {
            ExecuteTransitionEffect(boss, state, newPhase);
        }

        // Apply new phase stat modifiers (logged for future implementation)
        if (newPhase.StatModifiers.Count > 0)
        {
            _logger.LogDebug(
                "Phase {Phase} has {Count} stat modifiers to apply",
                newPhase.PhaseNumber, newPhase.StatModifiers.Count);
            // TODO: Apply stat modifiers via IBuffDebuffService when available
        }

        // Update AI behavior
        boss.SetBehavior(MapBossBehaviorToAIBehavior(newPhase.Behavior));
        _logger.LogDebug("Updated boss behavior to {Behavior}", newPhase.Behavior);

        // Log the phase change event
        _eventLogger.LogCombat(
            "BossPhaseChanged",
            newPhase.TransitionText ?? $"{bossDef.Name} enters phase {newPhase.PhaseNumber}: {newPhase.Name}!",
            correlationId: boss.Id,
            data: new Dictionary<string, object>
            {
                ["BossId"] = state.BossId,
                ["OldPhase"] = oldPhaseNumber,
                ["NewPhase"] = newPhase.PhaseNumber,
                ["PhaseName"] = newPhase.Name,
                ["Behavior"] = newPhase.Behavior.ToString()
            });
    }

    /// <summary>
    /// Executes a phase transition effect.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <param name="state">The boss's active state.</param>
    /// <param name="phase">The phase being entered.</param>
    private void ExecuteTransitionEffect(Monster boss, ActiveBossState state, BossPhase phase)
    {
        if (string.IsNullOrEmpty(phase.TransitionEffectId))
        {
            return;
        }

        _logger.LogDebug("Executing transition effect: {EffectId}", phase.TransitionEffectId);

        // Parse and execute effect based on ID
        switch (phase.TransitionEffectId)
        {
            case var s when s.StartsWith("knockback-all-"):
                var distance = ParseEffectParameter(s, 2);
                _logger.LogInformation(
                    "Transition effect: Knockback all players {Distance} cells from {Boss}",
                    distance, boss.Name);
                // TODO: Implement via IEnvironmentalCombatService when combat context available
                break;

            case "heal-minions":
                _logger.LogInformation(
                    "Transition effect: Healing all {Count} minions summoned by {Boss}",
                    state.ActiveSummonCount, boss.Name);
                // TODO: Implement minion healing when monster references available
                break;

            case "create-lava-zones":
                _logger.LogInformation(
                    "Transition effect: Creating lava zones around {Boss}",
                    boss.Name);
                // TODO: Implement via IZoneEffectService when combat grid available
                break;

            case var s when s.StartsWith("boss-"):
                // Generic boss effect (e.g., "boss-enrage-aura")
                _logger.LogInformation(
                    "Transition effect: Applying boss effect '{Effect}' to {Boss}",
                    phase.TransitionEffectId, boss.Name);
                break;

            default:
                _logger.LogWarning(
                    "Unknown transition effect: {EffectId} for phase {Phase}",
                    phase.TransitionEffectId, phase.Name);
                break;
        }
    }

    /// <summary>
    /// Attempts to summon minions for a boss.
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <param name="state">The boss's active state.</param>
    /// <param name="config">The summon configuration from the current phase.</param>
    private void TrySummonMinions(Monster boss, ActiveBossState state, SummonConfiguration config)
    {
        // Check summon limit
        if (!state.CanSummon(config.MaxActive))
        {
            _logger.LogDebug(
                "{BossId} at summon limit ({Count}/{Max})",
                state.BossId, state.ActiveSummonCount, config.MaxActive);
            return;
        }

        var toSummon = Math.Min(config.Count, config.MaxActive - state.ActiveSummonCount);
        _logger.LogInformation(
            "{BossId} summoning {Count} {Monster}(s)",
            state.BossId, toSummon, config.MonsterDefinitionId);

        // Log the summon attempt (actual spawning requires IMonsterSpawnService)
        _eventLogger.LogCombat(
            "BossSummon",
            $"The boss summons {toSummon} {config.MonsterDefinitionId}!",
            correlationId: boss.Id,
            data: new Dictionary<string, object>
            {
                ["MonsterType"] = config.MonsterDefinitionId,
                ["Count"] = toSummon,
                ["ActiveSummons"] = state.ActiveSummonCount
            });

        // TODO: When IMonsterSpawnService is available, spawn minions and track IDs:
        // foreach spawned minion: state.AddSummonedMinion(minion.Id);
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS - DEFEAT HANDLING
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Handles boss defeat.
    /// </summary>
    /// <param name="boss">The defeated boss monster.</param>
    /// <param name="bossDef">The boss definition.</param>
    private void OnBossDefeated(Monster boss, BossDefinition bossDef)
    {
        _activeBosses.Remove(boss.Id);
        _bossMonsters.Remove(boss.Id);

        _logger.LogInformation(
            "Boss {BossName} ({BossId}) defeated! Loot entries: {LootCount}",
            bossDef.Name, bossDef.BossId, bossDef.Loot.Count);

        // Log the defeat event with loot information
        _eventLogger.LogCombat(
            "BossDefeated",
            $"{bossDef.Name} has been defeated!",
            correlationId: boss.Id,
            data: new Dictionary<string, object>
            {
                ["BossId"] = bossDef.BossId,
                ["LootCount"] = bossDef.Loot.Count,
                ["GuaranteedLoot"] = bossDef.GuaranteedLoot.Select(l => l.ItemId).ToList(),
                ["ChanceLoot"] = bossDef.ChanceLoot.Select(l => new { l.ItemId, l.Chance }).ToList()
            });
    }

    // ═══════════════════════════════════════════════════════════════
    // PRIVATE METHODS - UTILITIES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates health as a percentage (0-100).
    /// </summary>
    /// <param name="boss">The boss monster.</param>
    /// <returns>Health percentage using ceiling for threshold calculations.</returns>
    private static int CalculateHealthPercent(Monster boss)
    {
        if (boss.MaxHealth <= 0)
        {
            return 0;
        }
        return (int)Math.Ceiling((boss.Health / (float)boss.MaxHealth) * 100);
    }

    /// <summary>
    /// Parses a numeric parameter from an effect ID like "knockback-all-2".
    /// </summary>
    /// <param name="effectId">The effect ID to parse.</param>
    /// <param name="defaultValue">Default value if parsing fails.</param>
    /// <returns>The parsed parameter value or default.</returns>
    private static int ParseEffectParameter(string effectId, int defaultValue)
    {
        var parts = effectId.Split('-');
        if (parts.Length > 0 && int.TryParse(parts[^1], out var value))
        {
            return value;
        }
        return defaultValue;
    }

    /// <summary>
    /// Maps <see cref="BossBehavior"/> to <see cref="AIBehavior"/> for monster AI.
    /// </summary>
    /// <param name="bossBehavior">The boss-specific behavior.</param>
    /// <returns>The corresponding AI behavior.</returns>
    private static AIBehavior MapBossBehaviorToAIBehavior(BossBehavior bossBehavior)
    {
        return bossBehavior switch
        {
            BossBehavior.Aggressive => AIBehavior.Aggressive,
            BossBehavior.Tactical => AIBehavior.Defensive, // Tactical maps to defensive for calculated play
            BossBehavior.Defensive => AIBehavior.Defensive,
            BossBehavior.Enraged => AIBehavior.Aggressive, // Enraged maps to aggressive
            BossBehavior.Summoner => AIBehavior.Support, // Summoner focuses on supporting minions
            _ => AIBehavior.Aggressive
        };
    }
}
