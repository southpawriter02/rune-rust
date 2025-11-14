using RuneAndRust.Core;
using RuneAndRust.Persistence;
using Serilog;
using System.Text.Json;

namespace RuneAndRust.Engine;

/// <summary>
/// v0.23.1: Boss Encounter Service - Database-backed implementation
/// Manages boss encounter mechanics including multi-phase transitions,
/// add wave spawning, and enrage timers with full persistence.
/// </summary>
public class BossEncounterService
{
    private static readonly ILogger _log = Log.ForContext<BossEncounterService>();
    private readonly BossEncounterRepository _repository;
    private readonly DiceService _diceService;

    public BossEncounterService(BossEncounterRepository repository, DiceService diceService)
    {
        _repository = repository;
        _diceService = diceService;
    }

    // ═════════════════════════════════════════════════════════════
    // BOSS ENCOUNTER INITIALIZATION
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Initialize boss encounter for a boss enemy
    /// </summary>
    public void InitializeBossEncounter(Enemy boss, int encounterId)
    {
        if (!boss.IsBoss)
        {
            _log.Warning("Attempted to initialize boss encounter for non-boss enemy: {EnemyId}", boss.Id);
            return;
        }

        _log.Information("Initializing boss encounter: EnemyId={EnemyId}, EncounterId={EncounterId}",
            boss.Id, encounterId);

        // Get boss configuration from database
        var bossConfig = _repository.GetBossEncounterByEncounterId(encounterId);
        if (bossConfig == null)
        {
            _log.Warning("No boss configuration found for EncounterId={EncounterId}", encounterId);
            return;
        }

        // Initialize combat state
        _repository.InitializeBossCombatState(bossConfig.BossEncounterId, boss.Id);

        _log.Information("Boss encounter initialized: Boss={BossName}, Phases={TotalPhases}",
            bossConfig.BossName, bossConfig.TotalPhases);
    }

    // ═════════════════════════════════════════════════════════════
    // PHASE TRANSITION SYSTEM
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Check if boss should transition to next phase based on HP percentage
    /// Returns transition description if phase transition occurs, null otherwise
    /// </summary>
    public string? CheckPhaseTransitions(Enemy boss, CombatState combatState)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        if (bossState == null || bossState.IsTransitioning)
        {
            return null; // Not a boss fight or already transitioning
        }

        var bossConfig = _repository.GetBossEncounter(bossState.BossEncounterId);
        if (bossConfig == null)
        {
            return null;
        }

        // Calculate current HP percentage
        float hpPercentage = (float)boss.HP / boss.MaxHP;

        _log.Debug("Checking phase transitions: Boss={BossName}, HP={HP}/{MaxHP} ({HPPercent}%), CurrentPhase={Phase}",
            bossConfig.BossName, boss.HP, boss.MaxHP, (hpPercentage * 100).ToString("F1"), bossState.CurrentPhase);

        // Check for phase 2 transition
        if (bossState.CurrentPhase == 1 &&
            !bossState.Phase2Triggered &&
            hpPercentage <= bossConfig.Phase2HpThreshold)
        {
            return TriggerPhaseTransition(boss, bossState, bossConfig, 2, combatState);
        }

        // Check for phase 3 transition
        if (bossState.CurrentPhase == 2 &&
            !bossState.Phase3Triggered &&
            bossConfig.TotalPhases >= 3 &&
            hpPercentage <= bossConfig.Phase3HpThreshold)
        {
            return TriggerPhaseTransition(boss, bossState, bossConfig, 3, combatState);
        }

        // Check for phase 4 transition
        if (bossState.CurrentPhase == 3 &&
            !bossState.Phase4Triggered &&
            bossConfig.TotalPhases >= 4 &&
            hpPercentage <= bossConfig.Phase4HpThreshold)
        {
            return TriggerPhaseTransition(boss, bossState, bossConfig, 4, combatState);
        }

        return null;
    }

    private string TriggerPhaseTransition(
        Enemy boss,
        BossCombatStateData bossState,
        BossEncounterConfig bossConfig,
        int newPhase,
        CombatState combatState)
    {
        var phaseDefinition = _repository.GetPhaseDefinition(bossState.BossEncounterId, newPhase);
        if (phaseDefinition == null)
        {
            _log.Warning("No phase definition found: BossEncounterId={BossEncounterId}, Phase={Phase}",
                bossState.BossEncounterId, newPhase);
            return string.Empty;
        }

        _log.Warning("PHASE TRANSITION: Boss={BossName}, Phase {OldPhase} → {NewPhase}",
            bossConfig.BossName, bossState.CurrentPhase, newPhase);

        // Step 1: Apply invulnerability
        _repository.UpdateInvulnerability(boss.Id, bossConfig.TransitionInvulnerabilityTurns);

        // Step 2: Update phase state
        _repository.UpdatePhaseState(boss.Id, newPhase);
        boss.Phase = newPhase;

        // Step 3: Execute phase transition events
        string eventLog = ExecutePhaseTransitionEvents(boss, phaseDefinition, combatState);

        // Step 4: Build cinematic description
        string logMessage = $"\n╔═══════════════════════════════════════════════════════════════╗\n";
        logMessage += $"║ PHASE {newPhase} TRANSITION\n";
        logMessage += $"╚═══════════════════════════════════════════════════════════════╝\n";
        logMessage += $"{phaseDefinition.PhaseDescription}\n";

        if (bossConfig.TransitionInvulnerabilityTurns > 0)
        {
            logMessage += $"⚠️ {boss.Name} is [INVULNERABLE] for {bossConfig.TransitionInvulnerabilityTurns} turn(s)!\n";
        }

        if (phaseDefinition.RegenerationPerTurn > 0)
        {
            logMessage += $"🔄 {boss.Name} gains [Regeneration] ({phaseDefinition.RegenerationPerTurn} HP/turn)\n";
        }

        if (phaseDefinition.DamageModifier > 1.0)
        {
            logMessage += $"⚡ {boss.Name}'s damage increased by {(int)((phaseDefinition.DamageModifier - 1.0) * 100)}%!\n";
        }

        if (phaseDefinition.BonusActionsPerTurn > 0)
        {
            logMessage += $"⚡ {boss.Name} gains {phaseDefinition.BonusActionsPerTurn} additional action(s) per turn!\n";
        }

        logMessage += eventLog;

        return logMessage;
    }

    private string ExecutePhaseTransitionEvents(
        Enemy boss,
        BossPhaseDefinitionData phaseDefinition,
        CombatState combatState)
    {
        string eventLog = "";

        _log.Information("Executing phase transition events: Phase={PhaseName}", phaseDefinition.PhaseName);

        // Spawn add waves
        if (phaseDefinition.SpawnsAdds && !string.IsNullOrEmpty(phaseDefinition.AddWaveComposition))
        {
            var (spawnedEnemies, addLog) = SpawnAddWave(boss.Id, phaseDefinition.AddWaveComposition);
            if (spawnedEnemies.Count > 0)
            {
                combatState.Enemies.AddRange(spawnedEnemies);
                eventLog += addLog;
            }
        }

        // Apply phase stat modifiers
        ApplyPhaseStatModifiers(boss, phaseDefinition);

        return eventLog;
    }

    // ═════════════════════════════════════════════════════════════
    // ADD WAVE SPAWNING
    // ═════════════════════════════════════════════════════════════

    private (List<Enemy> spawnedEnemies, string logMessage) SpawnAddWave(
        string bossId,
        string addWaveCompositionJSON)
    {
        var spawnedEnemies = new List<Enemy>();
        string logMessage = "";

        try
        {
            var addWave = JsonSerializer.Deserialize<List<AddSpawnDefinition>>(addWaveCompositionJSON);
            if (addWave == null || addWave.Count == 0)
            {
                return (spawnedEnemies, logMessage);
            }

            _log.Information("Spawning add wave: {AddCount} enemies", addWave.Sum(a => a.Count));

            logMessage += "\n⚠️ Reinforcements summoned!\n";

            foreach (var addDef in addWave)
            {
                for (int i = 0; i < addDef.Count; i++)
                {
                    var enemy = EnemyFactory.CreateEnemy(addDef.EnemyType);
                    enemy.Id = $"{addDef.EnemyType}_{Guid.NewGuid().ToString().Substring(0, 8)}";

                    spawnedEnemies.Add(enemy);
                    logMessage += $"  • {enemy.Name} spawned!\n";

                    _log.Debug("Add spawned: Type={EnemyType}, ID={EnemyId}",
                        addDef.EnemyType, enemy.Id);
                }
            }

            // Update add tracking
            _repository.UpdateAddTracking(bossId, spawnedEnemies.Count, spawnedEnemies.Count);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to spawn add wave: BossId={BossId}", bossId);
        }

        return (spawnedEnemies, logMessage);
    }

    /// <summary>
    /// Call when an add is killed to update tracking
    /// </summary>
    public void OnAddKilled(string bossId)
    {
        _repository.UpdateAddTracking(bossId, 0, -1);
        _log.Debug("Add killed in boss fight: BossId={BossId}", bossId);
    }

    // ═════════════════════════════════════════════════════════════
    // ENRAGE SYSTEM
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Check if boss should enrage based on HP/turn thresholds
    /// Returns enrage message if enrage triggers, null otherwise
    /// </summary>
    public string? CheckEnrageConditions(Enemy boss, int currentTurn)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        if (bossState == null || bossState.IsEnraged)
        {
            return null; // Not a boss fight or already enraged
        }

        var bossConfig = _repository.GetBossEncounter(bossState.BossEncounterId);
        if (bossConfig == null)
        {
            return null;
        }

        bool shouldEnrage = false;
        string enrageReason = "";

        // Check HP-based enrage
        float hpPercentage = (float)boss.HP / boss.MaxHP;
        if (hpPercentage <= bossConfig.EnrageHpThreshold)
        {
            shouldEnrage = true;
            enrageReason = $"System integrity critical ({(hpPercentage * 100):F0}%)";
        }

        // Check turn-based enrage (optional)
        if (bossConfig.EnrageTurnThreshold.HasValue &&
            currentTurn >= bossConfig.EnrageTurnThreshold.Value)
        {
            shouldEnrage = true;
            enrageReason = $"Combat duration exceeded ({currentTurn} turns)";
        }

        if (shouldEnrage)
        {
            return TriggerEnrage(boss, bossState, bossConfig, currentTurn, enrageReason);
        }

        return null;
    }

    private string TriggerEnrage(
        Enemy boss,
        BossCombatStateData bossState,
        BossEncounterConfig bossConfig,
        int currentTurn,
        string reason)
    {
        _log.Warning("BOSS ENRAGE: Boss={BossName}, Reason={Reason}, Turn={Turn}",
            bossConfig.BossName, reason, currentTurn);

        // Update enrage state
        _repository.UpdateEnrageState(boss.Id, true, currentTurn);

        // Apply enrage buffs
        int originalDamageBonus = boss.DamageBonus;
        boss.DamageBonus += (int)(boss.BaseDamageDice * (bossConfig.EnrageDamageMultiplier - 1.0) * 3);

        // Build dramatic enrage message
        string logMessage = $"\n╔═══════════════════════════════════════════════════════════════╗\n";
        logMessage += $"║ ENRAGE\n";
        logMessage += $"╚═══════════════════════════════════════════════════════════════╝\n";
        logMessage += $"💀 {boss.Name} enters ENRAGE state!\n";
        logMessage += $"⚡ Reason: {reason}\n";
        logMessage += $"⚡ Damage increased by {(int)((bossConfig.EnrageDamageMultiplier - 1.0) * 100)}%!\n";

        if (bossConfig.EnrageSpeedBonus > 0)
        {
            logMessage += $"⚡ Gains +{bossConfig.EnrageSpeedBonus} action(s) per turn!\n";
        }

        logMessage += $"⚡ [Control Immunity] - Cannot be stunned or disabled!\n";

        _log.Warning("Enrage buffs applied: DamageBonus {OldBonus} → {NewBonus}",
            originalDamageBonus, boss.DamageBonus);

        return logMessage;
    }

    // ═════════════════════════════════════════════════════════════
    // INVULNERABILITY SYSTEM
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Check if boss is currently invulnerable due to phase transition
    /// </summary>
    public bool IsBossInvulnerable(Enemy boss)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        return bossState != null && bossState.InvulnerabilityTurnsRemaining > 0;
    }

    /// <summary>
    /// Get remaining invulnerability turns
    /// </summary>
    public int GetInvulnerabilityTurns(Enemy boss)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        return bossState?.InvulnerabilityTurnsRemaining ?? 0;
    }

    // ═════════════════════════════════════════════════════════════
    // TURN PROCESSING
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Process end of turn for boss (decrement invulnerability, apply regeneration)
    /// </summary>
    public string ProcessEndOfTurn(Enemy boss)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        if (bossState == null)
        {
            return ""; // Not a boss fight
        }

        var bossConfig = _repository.GetBossEncounter(bossState.BossEncounterId);
        if (bossConfig == null)
        {
            return "";
        }

        string logMessage = "";

        // Decrement invulnerability
        if (bossState.InvulnerabilityTurnsRemaining > 0)
        {
            int newTurns = bossState.InvulnerabilityTurnsRemaining - 1;
            _repository.UpdateInvulnerability(boss.Id, newTurns);

            if (newTurns == 0)
            {
                logMessage += $"⚔️ {boss.Name} is no longer invulnerable!\n";
                _log.Information("Boss invulnerability ended: BossId={BossId}", boss.Id);
            }
        }

        // Apply regeneration if in regen phase
        var phaseDefinition = _repository.GetPhaseDefinition(bossState.BossEncounterId, bossState.CurrentPhase);
        if (phaseDefinition != null && phaseDefinition.RegenerationPerTurn > 0)
        {
            logMessage += ProcessRegeneration(boss, phaseDefinition.RegenerationPerTurn);
        }

        return logMessage;
    }

    /// <summary>
    /// Process boss regeneration
    /// </summary>
    public string ProcessRegeneration(Enemy boss, int regenAmount)
    {
        if (regenAmount <= 0 || boss.HP >= boss.MaxHP)
            return "";

        int healedAmount = Math.Min(regenAmount, boss.MaxHP - boss.HP);
        boss.HP += healedAmount;

        string logMessage = $"🔄 {boss.Name} regenerates {healedAmount} HP ({boss.HP}/{boss.MaxHP})\n";

        _log.Debug("Boss regeneration: BossId={BossId}, Healed={Amount}, CurrentHP={HP}/{MaxHP}",
            boss.Id, healedAmount, boss.HP, boss.MaxHP);

        return logMessage;
    }

    // ═════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Get current phase number for a boss
    /// </summary>
    public int GetCurrentPhase(Enemy boss)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        return bossState?.CurrentPhase ?? 1;
    }

    /// <summary>
    /// Check if boss is enraged
    /// </summary>
    public bool IsBossEnraged(Enemy boss)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        return bossState != null && bossState.IsEnraged;
    }

    /// <summary>
    /// Get boss encounter configuration
    /// </summary>
    public BossEncounterConfig? GetBossConfig(Enemy boss)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        if (bossState == null)
        {
            return null;
        }

        return _repository.GetBossEncounter(bossState.BossEncounterId);
    }

    /// <summary>
    /// Get current phase definition
    /// </summary>
    public BossPhaseDefinitionData? GetCurrentPhaseDefinition(Enemy boss)
    {
        var bossState = _repository.GetBossCombatState(boss.Id);
        if (bossState == null)
        {
            return null;
        }

        return _repository.GetPhaseDefinition(bossState.BossEncounterId, bossState.CurrentPhase);
    }

    /// <summary>
    /// Apply phase stat modifiers to boss
    /// </summary>
    private void ApplyPhaseStatModifiers(Enemy boss, BossPhaseDefinitionData phaseDefinition)
    {
        // Damage multiplier
        if (phaseDefinition.DamageModifier > 1.0)
        {
            int bonusDamage = (int)((phaseDefinition.DamageModifier - 1.0) * boss.BaseDamageDice * 3);
            boss.DamageBonus += bonusDamage;
        }

        // Defense bonus
        if (phaseDefinition.DefenseModifier != 0)
        {
            boss.DefenseBonus += phaseDefinition.DefenseModifier;
        }

        // Soak bonus
        if (phaseDefinition.SoakModifier != 0)
        {
            boss.Soak += phaseDefinition.SoakModifier;
        }

        _log.Debug("Phase stat modifiers applied: BossId={BossId}, DefenseBonus={Defense}, SoakBonus={Soak}",
            boss.Id, phaseDefinition.DefenseModifier, phaseDefinition.SoakModifier);
    }

    /// <summary>
    /// Clear boss combat state (when combat ends)
    /// </summary>
    public void ClearEncounterData(Enemy boss)
    {
        _repository.ClearBossCombatState(boss.Id);
        _log.Debug("Boss encounter data cleared: BossId={BossId}", boss.Id);
    }
}

/// <summary>
/// Add spawn definition for JSON deserialization
/// </summary>
public class AddSpawnDefinition
{
    public EnemyType EnemyType { get; set; }
    public int Count { get; set; }
    public string? Position { get; set; }
}
