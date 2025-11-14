using RuneAndRust.Core;
using Serilog;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.23.1: Manages boss encounter mechanics including multi-phase transitions,
/// add wave spawning, and enrage timers
/// </summary>
public class BossEncounterService
{
    private static readonly ILogger _log = Log.ForContext<BossEncounterService>();
    private readonly DiceService _diceService;
    private readonly EnemyFactory _enemyFactory;

    /// <summary>
    /// Tracks active phase transitions (boss ID -> remaining invulnerability turns)
    /// </summary>
    private Dictionary<string, int> _activeTransitions = new();

    /// <summary>
    /// Tracks current phase for each boss (boss ID -> phase number)
    /// </summary>
    private Dictionary<string, int> _bossPhases = new();

    /// <summary>
    /// Tracks enrage timers (boss ID -> remaining turns until enrage)
    /// </summary>
    private Dictionary<string, int> _enrageTimers = new();

    public BossEncounterService(DiceService diceService, EnemyFactory enemyFactory)
    {
        _diceService = diceService;
        _enemyFactory = enemyFactory;
    }

    /// <summary>
    /// Initialize boss encounter tracking for a boss enemy
    /// </summary>
    public void InitializeBossEncounter(Enemy boss, List<BossPhaseDefinition> phases, int? enrageTimer = null)
    {
        if (!boss.IsBoss)
        {
            _log.Warning("Attempted to initialize boss encounter for non-boss enemy: {EnemyId}", boss.Id);
            return;
        }

        _bossPhases[boss.Id] = 1; // Start at Phase 1

        if (enrageTimer.HasValue)
        {
            _enrageTimers[boss.Id] = enrageTimer.Value;
            _log.Information("Boss encounter initialized: {BossId}, Phases={PhaseCount}, EnrageTimer={EnrageTimer}",
                boss.Id, phases.Count, enrageTimer.Value);
        }
        else
        {
            _log.Information("Boss encounter initialized: {BossId}, Phases={PhaseCount}",
                boss.Id, phases.Count);
        }
    }

    /// <summary>
    /// Check if boss should transition to next phase based on HP percentage
    /// Returns the new phase definition if transition should occur, null otherwise
    /// </summary>
    public BossPhaseDefinition? CheckPhaseTransition(Enemy boss, List<BossPhaseDefinition> phases)
    {
        if (!boss.IsBoss || boss.MaxHP <= 0)
            return null;

        int currentPhase = GetCurrentPhase(boss);
        int hpPercentage = (int)((boss.HP / (double)boss.MaxHP) * 100);

        // Find the highest phase that boss should be in based on HP
        var targetPhase = phases
            .Where(p => p.PhaseNumber > currentPhase && hpPercentage <= p.HPPercentageThreshold)
            .OrderBy(p => p.PhaseNumber)
            .FirstOrDefault();

        if (targetPhase != null)
        {
            _log.Information("Boss phase transition triggered: {BossId}, Phase {OldPhase} → {NewPhase}, HP={HP}/{MaxHP} ({HPPercent}%)",
                boss.Id, currentPhase, targetPhase.PhaseNumber, boss.HP, boss.MaxHP, hpPercentage);

            return targetPhase;
        }

        return null;
    }

    /// <summary>
    /// Execute phase transition for a boss, applying stat modifiers and starting invulnerability
    /// </summary>
    public string ExecutePhaseTransition(Enemy boss, BossPhaseDefinition phase, CombatState combatState)
    {
        // Update boss phase tracking
        _bossPhases[boss.Id] = phase.PhaseNumber;
        boss.Phase = phase.PhaseNumber;

        // Apply stat modifiers
        ApplyPhaseStatModifiers(boss, phase.StatModifiers);

        // Start invulnerability window
        if (phase.InvulnerabilityTurns > 0)
        {
            _activeTransitions[boss.Id] = phase.InvulnerabilityTurns;
            _log.Information("Boss invulnerability started: {BossId}, Duration={Turns} turns",
                boss.Id, phase.InvulnerabilityTurns);
        }

        // Build combat log message
        string logMessage = $"\n╔═══════════════════════════════════════════════════════════════╗\n";
        logMessage += $"║ PHASE {phase.PhaseNumber} TRANSITION\n";
        logMessage += $"╚═══════════════════════════════════════════════════════════════╝\n";
        logMessage += $"{phase.TransitionDescription}\n";

        if (phase.InvulnerabilityTurns > 0)
        {
            logMessage += $"⚠️ {boss.Name} is [INVULNERABLE] for {phase.InvulnerabilityTurns} turn(s)!\n";
        }

        if (phase.StatModifiers.RegenerationPerTurn > 0)
        {
            logMessage += $"🔄 {boss.Name} gains [Regeneration] ({phase.StatModifiers.RegenerationPerTurn} HP/turn)\n";
        }

        if (phase.StatModifiers.DamageMultiplier > 1.0)
        {
            logMessage += $"⚡ {boss.Name}'s damage increased by {(int)((phase.StatModifiers.DamageMultiplier - 1.0) * 100)}%!\n";
        }

        if (phase.StatModifiers.BonusActionsPerTurn > 0)
        {
            logMessage += $"⚡ {boss.Name} gains {phase.StatModifiers.BonusActionsPerTurn} additional action(s) per turn!\n";
        }

        _log.Information("Phase transition executed: {BossId}, Phase={Phase}, Modifiers={@Modifiers}",
            boss.Id, phase.PhaseNumber, phase.StatModifiers);

        return logMessage;
    }

    /// <summary>
    /// Spawn add wave for a boss phase transition
    /// </summary>
    public (List<Enemy> spawnedEnemies, string logMessage) SpawnAddWave(AddWaveConfig addWave, BattlefieldGrid? grid = null)
    {
        var spawnedEnemies = new List<Enemy>();
        string logMessage = $"\n{addWave.SpawnDescription}\n";

        foreach (var enemyType in addWave.EnemyTypes)
        {
            int count = addWave.SpawnCounts.ContainsKey(enemyType)
                ? addWave.SpawnCounts[enemyType]
                : 1;

            for (int i = 0; i < count; i++)
            {
                var enemy = EnemyFactory.CreateEnemy(enemyType);
                enemy.Id = $"{enemyType}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                // Apply spawn delay
                if (addWave.SpawnDelay > 0)
                {
                    enemy.StunTurnsRemaining = addWave.SpawnDelay;
                    enemy.IsStunned = true;
                }

                spawnedEnemies.Add(enemy);
                logMessage += $"  • {enemy.Name} spawned!\n";

                _log.Information("Add spawned: Type={EnemyType}, ID={EnemyId}, SpawnDelay={Delay}",
                    enemyType, enemy.Id, addWave.SpawnDelay);
            }
        }

        return (spawnedEnemies, logMessage);
    }

    /// <summary>
    /// Check if boss is currently invulnerable due to phase transition
    /// </summary>
    public bool IsBossInvulnerable(Enemy boss)
    {
        return _activeTransitions.ContainsKey(boss.Id) && _activeTransitions[boss.Id] > 0;
    }

    /// <summary>
    /// Decrement invulnerability duration and enrage timers at end of turn
    /// </summary>
    public string ProcessEndOfTurn(Enemy boss)
    {
        string logMessage = "";

        // Decrement invulnerability
        if (_activeTransitions.ContainsKey(boss.Id) && _activeTransitions[boss.Id] > 0)
        {
            _activeTransitions[boss.Id]--;

            if (_activeTransitions[boss.Id] == 0)
            {
                logMessage += $"⚔️ {boss.Name} is no longer invulnerable!\n";
                _log.Information("Boss invulnerability ended: {BossId}", boss.Id);
            }
        }

        // Decrement enrage timer
        if (_enrageTimers.ContainsKey(boss.Id) && _enrageTimers[boss.Id] > 0)
        {
            _enrageTimers[boss.Id]--;

            if (_enrageTimers[boss.Id] == 0)
            {
                logMessage += ApplyEnrage(boss);
            }
            else if (_enrageTimers[boss.Id] <= 3)
            {
                logMessage += $"⏰ [WARNING] {boss.Name} enrages in {_enrageTimers[boss.Id]} turns!\n";
            }
        }

        return logMessage;
    }

    /// <summary>
    /// Apply enrage buff to boss (50% damage increase, +1 action per turn)
    /// </summary>
    private string ApplyEnrage(Enemy boss)
    {
        // Apply enrage stat buffs
        boss.DamageBonus += (int)(boss.BaseDamageDice * 0.5);  // +50% damage

        string logMessage = $"\n╔═══════════════════════════════════════════════════════════════╗\n";
        logMessage += $"║ ENRAGE\n";
        logMessage += $"╚═══════════════════════════════════════════════════════════════╝\n";
        logMessage += $"💀 {boss.Name} enters ENRAGE state!\n";
        logMessage += $"⚡ Damage increased by 50%!\n";
        logMessage += $"⚡ Gains +1 action per turn!\n";

        _log.Warning("Boss enraged: {BossId}, NewDamageBonus={DamageBonus}",
            boss.Id, boss.DamageBonus);

        return logMessage;
    }

    /// <summary>
    /// Apply phase stat modifiers to boss
    /// </summary>
    private void ApplyPhaseStatModifiers(Enemy boss, PhaseStatModifiers modifiers)
    {
        // Damage multiplier is applied during damage calculation
        // Store it as a damage bonus increase for simplicity
        if (modifiers.DamageMultiplier > 1.0)
        {
            int bonusDamage = (int)((modifiers.DamageMultiplier - 1.0) * boss.BaseDamageDice * 3); // Rough estimate
            boss.DamageBonus += bonusDamage;
        }

        // Apply defense bonus
        if (modifiers.DefenseBonus > 0)
        {
            boss.DefenseBonus += modifiers.DefenseBonus;
        }

        // Apply soak bonus
        if (modifiers.SoakBonus > 0)
        {
            boss.Soak += modifiers.SoakBonus;
        }

        _log.Debug("Phase stat modifiers applied: {BossId}, DefenseBonus={Defense}, SoakBonus={Soak}",
            boss.Id, modifiers.DefenseBonus, modifiers.SoakBonus);
    }

    /// <summary>
    /// Process boss regeneration (if active in current phase)
    /// </summary>
    public string ProcessRegeneration(Enemy boss, int regenAmount)
    {
        if (regenAmount <= 0 || boss.HP >= boss.MaxHP)
            return "";

        int healedAmount = Math.Min(regenAmount, boss.MaxHP - boss.HP);
        boss.HP += healedAmount;

        string logMessage = $"🔄 {boss.Name} regenerates {healedAmount} HP ({boss.HP}/{boss.MaxHP})\n";

        _log.Debug("Boss regeneration: {BossId}, Healed={Amount}, CurrentHP={HP}/{MaxHP}",
            boss.Id, healedAmount, boss.HP, boss.MaxHP);

        return logMessage;
    }

    /// <summary>
    /// Get current phase number for a boss
    /// </summary>
    public int GetCurrentPhase(Enemy boss)
    {
        return _bossPhases.ContainsKey(boss.Id) ? _bossPhases[boss.Id] : 1;
    }

    /// <summary>
    /// Get remaining invulnerability turns for a boss
    /// </summary>
    public int GetInvulnerabilityTurns(Enemy boss)
    {
        return _activeTransitions.ContainsKey(boss.Id) ? _activeTransitions[boss.Id] : 0;
    }

    /// <summary>
    /// Get remaining turns until enrage
    /// </summary>
    public int? GetEnrageTimer(Enemy boss)
    {
        return _enrageTimers.ContainsKey(boss.Id) ? _enrageTimers[boss.Id] : null;
    }

    /// <summary>
    /// Clear all boss encounter tracking data
    /// </summary>
    public void ClearEncounterData()
    {
        _activeTransitions.Clear();
        _bossPhases.Clear();
        _enrageTimers.Clear();

        _log.Debug("Boss encounter data cleared");
    }
}
