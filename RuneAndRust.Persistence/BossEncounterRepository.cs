using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.23.1-v0.23.2: Repository for managing boss encounter data including phases, add waves, enrage mechanics,
/// telegraphed abilities, AI patterns, and vulnerability windows.
/// Implements database persistence for boss encounter configurations and combat state.
/// </summary>
public class BossEncounterRepository
{
    private static readonly ILogger _log = Log.ForContext<BossEncounterRepository>();
    private readonly string _connectionString;
    private const string DatabaseName = "runeandrust.db";

    public BossEncounterRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, DatabaseName);
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("BossEncounterRepository initialized with database path: {DbPath}", dbPath);

        InitializeBossEncounterTables();
    }

    /// <summary>
    /// Create boss encounter tables if they don't exist
    /// </summary>
    private void InitializeBossEncounterTables()
    {
        _log.Debug("Initializing boss encounter tables");

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            // Boss_Encounters table
            var createBossEncountersTable = connection.CreateCommand();
            createBossEncountersTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Boss_Encounters (
                    boss_encounter_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    encounter_id INTEGER NOT NULL UNIQUE,
                    boss_name TEXT NOT NULL,
                    boss_type TEXT NOT NULL,

                    -- Phase configuration
                    total_phases INTEGER NOT NULL DEFAULT 2,
                    phase_2_hp_threshold REAL DEFAULT 0.75,
                    phase_3_hp_threshold REAL DEFAULT 0.50,
                    phase_4_hp_threshold REAL DEFAULT 0.25,

                    -- Phase transition settings
                    transition_invulnerability_turns INTEGER DEFAULT 1,

                    -- Enrage configuration
                    enrage_turn_threshold INTEGER,
                    enrage_hp_threshold REAL DEFAULT 0.25,
                    enrage_damage_multiplier REAL DEFAULT 1.5,
                    enrage_speed_bonus INTEGER DEFAULT 1,

                    -- Current state (tracked during combat)
                    current_phase INTEGER DEFAULT 1,
                    is_enraged INTEGER DEFAULT 0,
                    phase_transition_active INTEGER DEFAULT 0
                )
            ";
            createBossEncountersTable.ExecuteNonQuery();

            // Boss_Phase_Definitions table
            var createBossPhaseDefinitionsTable = connection.CreateCommand();
            createBossPhaseDefinitionsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Boss_Phase_Definitions (
                    phase_definition_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    boss_encounter_id INTEGER NOT NULL,
                    phase_number INTEGER NOT NULL,

                    -- Phase identity
                    phase_name TEXT,
                    phase_description TEXT,

                    -- Add wave configuration
                    spawns_adds INTEGER DEFAULT 0,
                    add_wave_composition TEXT,

                    -- Environmental changes
                    activates_hazards TEXT,
                    modifies_terrain TEXT,

                    -- AI behavior changes
                    ai_behavior_pattern TEXT,
                    ability_unlock_list TEXT,
                    ability_disable_list TEXT,

                    -- Stat modifiers for this phase
                    damage_modifier REAL DEFAULT 1.0,
                    defense_modifier INTEGER DEFAULT 0,
                    soak_modifier INTEGER DEFAULT 0,
                    regeneration_per_turn INTEGER DEFAULT 0,
                    bonus_actions_per_turn INTEGER DEFAULT 0,

                    FOREIGN KEY (boss_encounter_id) REFERENCES Boss_Encounters(boss_encounter_id) ON DELETE CASCADE,
                    UNIQUE(boss_encounter_id, phase_number)
                )
            ";
            createBossPhaseDefinitionsTable.ExecuteNonQuery();

            // Boss_Combat_State table
            var createBossCombatStateTable = connection.CreateCommand();
            createBossCombatStateTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Boss_Combat_State (
                    combat_state_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    boss_encounter_id INTEGER NOT NULL,
                    enemy_id TEXT NOT NULL UNIQUE,

                    -- Current phase tracking
                    current_phase INTEGER DEFAULT 1,
                    phase_2_triggered INTEGER DEFAULT 0,
                    phase_3_triggered INTEGER DEFAULT 0,
                    phase_4_triggered INTEGER DEFAULT 0,

                    -- Enrage tracking
                    is_enraged INTEGER DEFAULT 0,
                    enrage_triggered_turn INTEGER,

                    -- Add wave tracking
                    total_adds_spawned INTEGER DEFAULT 0,
                    current_adds_alive INTEGER DEFAULT 0,

                    -- Transition state
                    is_transitioning INTEGER DEFAULT 0,
                    transition_start_turn INTEGER,
                    transition_end_turn INTEGER,
                    invulnerability_turns_remaining INTEGER DEFAULT 0,

                    started_at TEXT NOT NULL,

                    FOREIGN KEY (boss_encounter_id) REFERENCES Boss_Encounters(boss_encounter_id)
                )
            ";
            createBossCombatStateTable.ExecuteNonQuery();

            // v0.23.2: Boss_Abilities table (telegraphed attacks, damage formulas, interrupts)
            var createBossAbilitiesTable = connection.CreateCommand();
            createBossAbilitiesTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Boss_Abilities (
                    boss_ability_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    boss_encounter_id INTEGER NOT NULL,
                    ability_name TEXT NOT NULL,
                    ability_description TEXT,
                    ability_type TEXT NOT NULL,

                    -- Telegraph configuration
                    is_telegraphed INTEGER DEFAULT 0,
                    telegraph_charge_turns INTEGER DEFAULT 1,
                    telegraph_warning_message TEXT,

                    -- Ultimate configuration
                    is_ultimate INTEGER DEFAULT 0,
                    vulnerability_duration_turns INTEGER DEFAULT 0,
                    vulnerability_damage_multiplier REAL DEFAULT 1.5,

                    -- Damage configuration
                    base_damage_dice INTEGER NOT NULL,
                    damage_bonus INTEGER DEFAULT 0,
                    damage_type TEXT DEFAULT 'Physical',
                    damage_formula TEXT,

                    -- Target configuration
                    target_type TEXT DEFAULT 'Single',
                    aoe_radius INTEGER DEFAULT 0,

                    -- Status effects (JSON)
                    applies_status_effects TEXT,

                    -- Interrupt mechanics
                    interrupt_damage_threshold INTEGER DEFAULT 0,
                    interrupt_stagger_duration INTEGER DEFAULT 2,

                    -- Cooldown
                    cooldown_turns INTEGER DEFAULT 0,

                    -- Special effects (JSON)
                    special_effects TEXT,

                    FOREIGN KEY (boss_encounter_id) REFERENCES Boss_Encounters(boss_encounter_id) ON DELETE CASCADE
                )
            ";
            createBossAbilitiesTable.ExecuteNonQuery();

            // v0.23.2: Boss_AI_Patterns table (phase-specific AI behavior)
            var createBossAIPatternsTable = connection.CreateCommand();
            createBossAIPatternsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Boss_AI_Patterns (
                    ai_pattern_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    boss_encounter_id INTEGER NOT NULL,
                    phase_number INTEGER NOT NULL,

                    -- AI behavior configuration
                    pattern_name TEXT,
                    pattern_description TEXT,

                    -- Target priority (JSON array)
                    target_priority_list TEXT,

                    -- Ability usage patterns
                    preferred_abilities TEXT,
                    ability_rotation TEXT,
                    telegraph_frequency REAL DEFAULT 0.3,
                    ultimate_hp_threshold REAL DEFAULT 0.5,

                    -- Positioning behavior
                    prefers_melee INTEGER DEFAULT 1,
                    prefers_range INTEGER DEFAULT 0,
                    positioning_strategy TEXT,

                    -- Defensive behavior
                    heal_threshold REAL DEFAULT 0.25,
                    retreat_threshold REAL DEFAULT 0.15,
                    summon_adds_threshold REAL DEFAULT 0.75,

                    FOREIGN KEY (boss_encounter_id) REFERENCES Boss_Encounters(boss_encounter_id) ON DELETE CASCADE,
                    UNIQUE(boss_encounter_id, phase_number)
                )
            ";
            createBossAIPatternsTable.ExecuteNonQuery();

            // v0.23.2: Boss_Telegraph_State table (active telegraph tracking)
            var createBossTelegraphStateTable = connection.CreateCommand();
            createBossTelegraphStateTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Boss_Telegraph_State (
                    telegraph_state_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    enemy_id TEXT NOT NULL,
                    boss_ability_id INTEGER NOT NULL,

                    -- Charge tracking
                    charge_started_turn INTEGER NOT NULL,
                    charge_complete_turn INTEGER NOT NULL,
                    current_turn INTEGER NOT NULL,
                    is_charging INTEGER DEFAULT 1,
                    is_completed INTEGER DEFAULT 0,
                    is_interrupted INTEGER DEFAULT 0,

                    -- Interrupt tracking
                    interrupt_damage_threshold INTEGER DEFAULT 0,
                    accumulated_interrupt_damage INTEGER DEFAULT 0,

                    -- Target tracking (JSON)
                    target_character_ids TEXT,

                    -- Timestamps
                    started_at TEXT NOT NULL,
                    completed_at TEXT,

                    FOREIGN KEY (boss_ability_id) REFERENCES Boss_Abilities(boss_ability_id) ON DELETE CASCADE
                )
            ";
            createBossTelegraphStateTable.ExecuteNonQuery();

            // Create indexes for efficient querying
            var createIndexCommands = new[]
            {
                "CREATE INDEX IF NOT EXISTS idx_boss_encounters_encounter ON Boss_Encounters(encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_encounters_phase ON Boss_Encounters(current_phase)",
                "CREATE INDEX IF NOT EXISTS idx_boss_phases_boss ON Boss_Phase_Definitions(boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_combat_state_boss ON Boss_Combat_State(boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_combat_state_enemy ON Boss_Combat_State(enemy_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_abilities_boss ON Boss_Abilities(boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_abilities_type ON Boss_Abilities(ability_type)",
                "CREATE INDEX IF NOT EXISTS idx_boss_ai_patterns_boss ON Boss_AI_Patterns(boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_ai_patterns_phase ON Boss_AI_Patterns(phase_number)",
                "CREATE INDEX IF NOT EXISTS idx_boss_telegraph_state_enemy ON Boss_Telegraph_State(enemy_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_telegraph_state_ability ON Boss_Telegraph_State(boss_ability_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_telegraph_state_charging ON Boss_Telegraph_State(is_charging)"
            };

            foreach (var indexSql in createIndexCommands)
            {
                var indexCommand = connection.CreateCommand();
                indexCommand.CommandText = indexSql;
                indexCommand.ExecuteNonQuery();
            }

            _log.Information("Boss encounter tables initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize boss encounter tables");
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // BOSS ENCOUNTER OPERATIONS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a new boss encounter configuration
    /// </summary>
    public int CreateBossEncounter(BossEncounterConfig config)
    {
        _log.Information("Creating boss encounter: Boss={BossName}, Type={BossType}, Phases={TotalPhases}",
            config.BossName, config.BossType, config.TotalPhases);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Boss_Encounters (
                    encounter_id, boss_name, boss_type, total_phases,
                    phase_2_hp_threshold, phase_3_hp_threshold, phase_4_hp_threshold,
                    transition_invulnerability_turns,
                    enrage_turn_threshold, enrage_hp_threshold,
                    enrage_damage_multiplier, enrage_speed_bonus
                ) VALUES (
                    $encounterId, $bossName, $bossType, $totalPhases,
                    $phase2, $phase3, $phase4,
                    $invulnTurns,
                    $enrageTurn, $enrageHp,
                    $enrageDmg, $enrageSpeed
                )
            ";

            command.Parameters.AddWithValue("$encounterId", config.EncounterId);
            command.Parameters.AddWithValue("$bossName", config.BossName);
            command.Parameters.AddWithValue("$bossType", config.BossType);
            command.Parameters.AddWithValue("$totalPhases", config.TotalPhases);
            command.Parameters.AddWithValue("$phase2", config.Phase2HpThreshold);
            command.Parameters.AddWithValue("$phase3", config.Phase3HpThreshold);
            command.Parameters.AddWithValue("$phase4", config.Phase4HpThreshold);
            command.Parameters.AddWithValue("$invulnTurns", config.TransitionInvulnerabilityTurns);
            command.Parameters.AddWithValue("$enrageTurn", (object?)config.EnrageTurnThreshold ?? DBNull.Value);
            command.Parameters.AddWithValue("$enrageHp", config.EnrageHpThreshold);
            command.Parameters.AddWithValue("$enrageDmg", config.EnrageDamageMultiplier);
            command.Parameters.AddWithValue("$enrageSpeed", config.EnrageSpeedBonus);

            command.ExecuteNonQuery();

            // Get the auto-generated ID
            var idCommand = connection.CreateCommand();
            idCommand.CommandText = "SELECT last_insert_rowid()";
            var bossEncounterId = Convert.ToInt32(idCommand.ExecuteScalar());

            _log.Information("Boss encounter created: BossEncounterId={BossEncounterId}", bossEncounterId);

            return bossEncounterId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create boss encounter: Boss={BossName}", config.BossName);
            throw;
        }
    }

    /// <summary>
    /// Create a phase definition for a boss encounter
    /// </summary>
    public void CreatePhaseDefinition(BossPhaseDefinitionData phase)
    {
        _log.Debug("Creating phase definition: BossEncounterId={BossEncounterId}, Phase={PhaseNumber}",
            phase.BossEncounterId, phase.PhaseNumber);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Boss_Phase_Definitions (
                    boss_encounter_id, phase_number, phase_name, phase_description,
                    spawns_adds, add_wave_composition,
                    activates_hazards, modifies_terrain,
                    ai_behavior_pattern, ability_unlock_list, ability_disable_list,
                    damage_modifier, defense_modifier, soak_modifier,
                    regeneration_per_turn, bonus_actions_per_turn
                ) VALUES (
                    $bossId, $phaseNum, $phaseName, $phaseDesc,
                    $spawnsAdds, $addWave,
                    $hazards, $terrain,
                    $aiBehavior, $abilityUnlock, $abilityDisable,
                    $damageMod, $defenseMod, $soakMod,
                    $regen, $bonusActions
                )
            ";

            command.Parameters.AddWithValue("$bossId", phase.BossEncounterId);
            command.Parameters.AddWithValue("$phaseNum", phase.PhaseNumber);
            command.Parameters.AddWithValue("$phaseName", phase.PhaseName ?? "");
            command.Parameters.AddWithValue("$phaseDesc", phase.PhaseDescription ?? "");
            command.Parameters.AddWithValue("$spawnsAdds", phase.SpawnsAdds ? 1 : 0);
            command.Parameters.AddWithValue("$addWave", phase.AddWaveComposition ?? "");
            command.Parameters.AddWithValue("$hazards", phase.ActivatesHazards ?? "");
            command.Parameters.AddWithValue("$terrain", phase.ModifiesTerrain ?? "");
            command.Parameters.AddWithValue("$aiBehavior", phase.AIBehaviorPattern ?? "");
            command.Parameters.AddWithValue("$abilityUnlock", phase.AbilityUnlockList ?? "");
            command.Parameters.AddWithValue("$abilityDisable", phase.AbilityDisableList ?? "");
            command.Parameters.AddWithValue("$damageMod", phase.DamageModifier);
            command.Parameters.AddWithValue("$defenseMod", phase.DefenseModifier);
            command.Parameters.AddWithValue("$soakMod", phase.SoakModifier);
            command.Parameters.AddWithValue("$regen", phase.RegenerationPerTurn);
            command.Parameters.AddWithValue("$bonusActions", phase.BonusActionsPerTurn);

            command.ExecuteNonQuery();

            _log.Debug("Phase definition created successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create phase definition: BossEncounterId={BossEncounterId}, Phase={PhaseNumber}",
                phase.BossEncounterId, phase.PhaseNumber);
            throw;
        }
    }

    /// <summary>
    /// Get boss encounter configuration by encounter ID
    /// </summary>
    public BossEncounterConfig? GetBossEncounterByEncounterId(int encounterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Boss_Encounters WHERE encounter_id = $encounterId";
            command.Parameters.AddWithValue("$encounterId", encounterId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapBossEncounterConfig(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss encounter: EncounterId={EncounterId}", encounterId);
            throw;
        }
    }

    /// <summary>
    /// Get boss encounter configuration by boss encounter ID
    /// </summary>
    public BossEncounterConfig? GetBossEncounter(int bossEncounterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Boss_Encounters WHERE boss_encounter_id = $bossId";
            command.Parameters.AddWithValue("$bossId", bossEncounterId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapBossEncounterConfig(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss encounter: BossEncounterId={BossEncounterId}", bossEncounterId);
            throw;
        }
    }

    /// <summary>
    /// Get all phase definitions for a boss encounter
    /// </summary>
    public List<BossPhaseDefinitionData> GetPhaseDefinitions(int bossEncounterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Boss_Phase_Definitions
                WHERE boss_encounter_id = $bossId
                ORDER BY phase_number
            ";
            command.Parameters.AddWithValue("$bossId", bossEncounterId);

            var phases = new List<BossPhaseDefinitionData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                phases.Add(MapPhaseDefinition(reader));
            }

            return phases;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get phase definitions: BossEncounterId={BossEncounterId}", bossEncounterId);
            throw;
        }
    }

    /// <summary>
    /// Get specific phase definition
    /// </summary>
    public BossPhaseDefinitionData? GetPhaseDefinition(int bossEncounterId, int phaseNumber)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Boss_Phase_Definitions
                WHERE boss_encounter_id = $bossId AND phase_number = $phaseNum
            ";
            command.Parameters.AddWithValue("$bossId", bossEncounterId);
            command.Parameters.AddWithValue("$phaseNum", phaseNumber);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapPhaseDefinition(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get phase definition: BossEncounterId={BossEncounterId}, Phase={PhaseNumber}",
                bossEncounterId, phaseNumber);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // BOSS COMBAT STATE OPERATIONS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Initialize boss combat state for a new fight
    /// </summary>
    public void InitializeBossCombatState(int bossEncounterId, string enemyId)
    {
        _log.Information("Initializing boss combat state: BossEncounterId={BossEncounterId}, EnemyId={EnemyId}",
            bossEncounterId, enemyId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Boss_Combat_State (
                    boss_encounter_id, enemy_id, current_phase,
                    is_enraged, is_transitioning, started_at
                ) VALUES (
                    $bossId, $enemyId, 1, 0, 0, $startedAt
                )
            ";

            command.Parameters.AddWithValue("$bossId", bossEncounterId);
            command.Parameters.AddWithValue("$enemyId", enemyId);
            command.Parameters.AddWithValue("$startedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();

            _log.Information("Boss combat state initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize boss combat state: EnemyId={EnemyId}", enemyId);
            throw;
        }
    }

    /// <summary>
    /// Get boss combat state by enemy ID
    /// </summary>
    public BossCombatStateData? GetBossCombatState(string enemyId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Boss_Combat_State WHERE enemy_id = $enemyId";
            command.Parameters.AddWithValue("$enemyId", enemyId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapBossCombatState(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss combat state: EnemyId={EnemyId}", enemyId);
            throw;
        }
    }

    /// <summary>
    /// Update boss combat state current phase
    /// </summary>
    public void UpdatePhaseState(string enemyId, int newPhase)
    {
        _log.Information("Updating phase state: EnemyId={EnemyId}, NewPhase={NewPhase}", enemyId, newPhase);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Boss_Combat_State
                SET current_phase = $phase,
                    phase_2_triggered = CASE WHEN $phase >= 2 THEN 1 ELSE phase_2_triggered END,
                    phase_3_triggered = CASE WHEN $phase >= 3 THEN 1 ELSE phase_3_triggered END,
                    phase_4_triggered = CASE WHEN $phase >= 4 THEN 1 ELSE phase_4_triggered END
                WHERE enemy_id = $enemyId
            ";

            command.Parameters.AddWithValue("$phase", newPhase);
            command.Parameters.AddWithValue("$enemyId", enemyId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update phase state: EnemyId={EnemyId}", enemyId);
            throw;
        }
    }

    /// <summary>
    /// Update invulnerability state
    /// </summary>
    public void UpdateInvulnerability(string enemyId, int turnsRemaining)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Boss_Combat_State
                SET invulnerability_turns_remaining = $turns,
                    is_transitioning = CASE WHEN $turns > 0 THEN 1 ELSE 0 END
                WHERE enemy_id = $enemyId
            ";

            command.Parameters.AddWithValue("$turns", turnsRemaining);
            command.Parameters.AddWithValue("$enemyId", enemyId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update invulnerability: EnemyId={EnemyId}", enemyId);
            throw;
        }
    }

    /// <summary>
    /// Update enrage state
    /// </summary>
    public void UpdateEnrageState(string enemyId, bool isEnraged, int? enrageTurn = null)
    {
        _log.Information("Updating enrage state: EnemyId={EnemyId}, IsEnraged={IsEnraged}", enemyId, isEnraged);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Boss_Combat_State
                SET is_enraged = $enraged,
                    enrage_triggered_turn = $enrageTurn
                WHERE enemy_id = $enemyId
            ";

            command.Parameters.AddWithValue("$enraged", isEnraged ? 1 : 0);
            command.Parameters.AddWithValue("$enrageTurn", (object?)enrageTurn ?? DBNull.Value);
            command.Parameters.AddWithValue("$enemyId", enemyId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update enrage state: EnemyId={EnemyId}", enemyId);
            throw;
        }
    }

    /// <summary>
    /// Update add wave tracking
    /// </summary>
    public void UpdateAddTracking(string enemyId, int addsSpawned, int addsAliveChange = 0)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Boss_Combat_State
                SET total_adds_spawned = total_adds_spawned + $spawned,
                    current_adds_alive = current_adds_alive + $aliveChange
                WHERE enemy_id = $enemyId
            ";

            command.Parameters.AddWithValue("$spawned", addsSpawned);
            command.Parameters.AddWithValue("$aliveChange", addsAliveChange);
            command.Parameters.AddWithValue("$enemyId", enemyId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update add tracking: EnemyId={EnemyId}", enemyId);
            throw;
        }
    }

    /// <summary>
    /// Clear boss combat state (when combat ends)
    /// </summary>
    public void ClearBossCombatState(string enemyId)
    {
        _log.Information("Clearing boss combat state: EnemyId={EnemyId}", enemyId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Boss_Combat_State WHERE enemy_id = $enemyId";
            command.Parameters.AddWithValue("$enemyId", enemyId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to clear boss combat state: EnemyId={EnemyId}", enemyId);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // BOSS ABILITIES OPERATIONS (v0.23.2)
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a boss ability definition
    /// </summary>
    public int CreateBossAbility(BossAbilityData ability)
    {
        _log.Debug("Creating boss ability: Ability={AbilityName}, Boss={BossEncounterId}",
            ability.AbilityName, ability.BossEncounterId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Boss_Abilities (
                    boss_encounter_id, ability_name, ability_description, ability_type,
                    is_telegraphed, telegraph_charge_turns, telegraph_warning_message,
                    is_ultimate, vulnerability_duration_turns, vulnerability_damage_multiplier,
                    base_damage_dice, damage_bonus, damage_type, damage_formula,
                    target_type, aoe_radius, applies_status_effects,
                    interrupt_damage_threshold, interrupt_stagger_duration,
                    cooldown_turns, special_effects
                ) VALUES (
                    $bossId, $name, $desc, $type,
                    $isTelegraphed, $chargeTurns, $warningMsg,
                    $isUltimate, $vulnDuration, $vulnMultiplier,
                    $baseDice, $dmgBonus, $dmgType, $dmgFormula,
                    $targetType, $aoeRadius, $statusEffects,
                    $interruptThreshold, $staggerDuration,
                    $cooldown, $specialEffects
                )
            ";

            command.Parameters.AddWithValue("$bossId", ability.BossEncounterId);
            command.Parameters.AddWithValue("$name", ability.AbilityName);
            command.Parameters.AddWithValue("$desc", ability.AbilityDescription ?? "");
            command.Parameters.AddWithValue("$type", ability.AbilityType);
            command.Parameters.AddWithValue("$isTelegraphed", ability.IsTelegraphed ? 1 : 0);
            command.Parameters.AddWithValue("$chargeTurns", ability.TelegraphChargeTurns);
            command.Parameters.AddWithValue("$warningMsg", ability.TelegraphWarningMessage ?? "");
            command.Parameters.AddWithValue("$isUltimate", ability.IsUltimate ? 1 : 0);
            command.Parameters.AddWithValue("$vulnDuration", ability.VulnerabilityDurationTurns);
            command.Parameters.AddWithValue("$vulnMultiplier", ability.VulnerabilityDamageMultiplier);
            command.Parameters.AddWithValue("$baseDice", ability.BaseDamageDice);
            command.Parameters.AddWithValue("$dmgBonus", ability.DamageBonus);
            command.Parameters.AddWithValue("$dmgType", ability.DamageType ?? "Physical");
            command.Parameters.AddWithValue("$dmgFormula", ability.DamageFormula ?? "");
            command.Parameters.AddWithValue("$targetType", ability.TargetType ?? "Single");
            command.Parameters.AddWithValue("$aoeRadius", ability.AoeRadius);
            command.Parameters.AddWithValue("$statusEffects", ability.AppliesStatusEffects ?? "");
            command.Parameters.AddWithValue("$interruptThreshold", ability.InterruptDamageThreshold);
            command.Parameters.AddWithValue("$staggerDuration", ability.InterruptStaggerDuration);
            command.Parameters.AddWithValue("$cooldown", ability.CooldownTurns);
            command.Parameters.AddWithValue("$specialEffects", ability.SpecialEffects ?? "");

            command.ExecuteNonQuery();

            var idCommand = connection.CreateCommand();
            idCommand.CommandText = "SELECT last_insert_rowid()";
            var abilityId = Convert.ToInt32(idCommand.ExecuteScalar());

            _log.Debug("Boss ability created: AbilityId={AbilityId}", abilityId);
            return abilityId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create boss ability: Ability={AbilityName}", ability.AbilityName);
            throw;
        }
    }

    /// <summary>
    /// Get all abilities for a boss encounter
    /// </summary>
    public List<BossAbilityData> GetBossAbilities(int bossEncounterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Boss_Abilities WHERE boss_encounter_id = $bossId ORDER BY boss_ability_id";
            command.Parameters.AddWithValue("$bossId", bossEncounterId);

            var abilities = new List<BossAbilityData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                abilities.Add(MapBossAbility(reader));
            }

            return abilities;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss abilities: BossEncounterId={BossEncounterId}", bossEncounterId);
            throw;
        }
    }

    /// <summary>
    /// Get a specific boss ability by ID
    /// </summary>
    public BossAbilityData? GetBossAbility(int abilityId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Boss_Abilities WHERE boss_ability_id = $abilityId";
            command.Parameters.AddWithValue("$abilityId", abilityId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapBossAbility(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss ability: AbilityId={AbilityId}", abilityId);
            throw;
        }
    }

    /// <summary>
    /// Get telegraphed abilities for a boss encounter
    /// </summary>
    public List<BossAbilityData> GetTelegraphedAbilities(int bossEncounterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Boss_Abilities
                WHERE boss_encounter_id = $bossId AND is_telegraphed = 1
                ORDER BY boss_ability_id
            ";
            command.Parameters.AddWithValue("$bossId", bossEncounterId);

            var abilities = new List<BossAbilityData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                abilities.Add(MapBossAbility(reader));
            }

            return abilities;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get telegraphed abilities: BossEncounterId={BossEncounterId}", bossEncounterId);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // BOSS AI PATTERNS OPERATIONS (v0.23.2)
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a boss AI pattern for a specific phase
    /// </summary>
    public void CreateBossAIPattern(BossAIPatternData aiPattern)
    {
        _log.Debug("Creating boss AI pattern: Boss={BossEncounterId}, Phase={PhaseNumber}",
            aiPattern.BossEncounterId, aiPattern.PhaseNumber);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Boss_AI_Patterns (
                    boss_encounter_id, phase_number, pattern_name, pattern_description,
                    target_priority_list, preferred_abilities, ability_rotation,
                    telegraph_frequency, ultimate_hp_threshold,
                    prefers_melee, prefers_range, positioning_strategy,
                    heal_threshold, retreat_threshold, summon_adds_threshold
                ) VALUES (
                    $bossId, $phase, $name, $desc,
                    $targetPriority, $preferredAbilities, $abilityRotation,
                    $telegraphFreq, $ultimateHp,
                    $prefersMelee, $prefersRange, $positioning,
                    $healThreshold, $retreatThreshold, $summonThreshold
                )
            ";

            command.Parameters.AddWithValue("$bossId", aiPattern.BossEncounterId);
            command.Parameters.AddWithValue("$phase", aiPattern.PhaseNumber);
            command.Parameters.AddWithValue("$name", aiPattern.PatternName ?? "");
            command.Parameters.AddWithValue("$desc", aiPattern.PatternDescription ?? "");
            command.Parameters.AddWithValue("$targetPriority", aiPattern.TargetPriorityList ?? "");
            command.Parameters.AddWithValue("$preferredAbilities", aiPattern.PreferredAbilities ?? "");
            command.Parameters.AddWithValue("$abilityRotation", aiPattern.AbilityRotation ?? "");
            command.Parameters.AddWithValue("$telegraphFreq", aiPattern.TelegraphFrequency);
            command.Parameters.AddWithValue("$ultimateHp", aiPattern.UltimateHpThreshold);
            command.Parameters.AddWithValue("$prefersMelee", aiPattern.PrefersMelee ? 1 : 0);
            command.Parameters.AddWithValue("$prefersRange", aiPattern.PrefersRange ? 1 : 0);
            command.Parameters.AddWithValue("$positioning", aiPattern.PositioningStrategy ?? "");
            command.Parameters.AddWithValue("$healThreshold", aiPattern.HealThreshold);
            command.Parameters.AddWithValue("$retreatThreshold", aiPattern.RetreatThreshold);
            command.Parameters.AddWithValue("$summonThreshold", aiPattern.SummonAddsThreshold);

            command.ExecuteNonQuery();

            _log.Debug("Boss AI pattern created successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create boss AI pattern: Boss={BossEncounterId}, Phase={PhaseNumber}",
                aiPattern.BossEncounterId, aiPattern.PhaseNumber);
            throw;
        }
    }

    /// <summary>
    /// Get AI pattern for a specific boss phase
    /// </summary>
    public BossAIPatternData? GetBossAIPattern(int bossEncounterId, int phaseNumber)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Boss_AI_Patterns
                WHERE boss_encounter_id = $bossId AND phase_number = $phase
            ";
            command.Parameters.AddWithValue("$bossId", bossEncounterId);
            command.Parameters.AddWithValue("$phase", phaseNumber);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapBossAIPattern(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss AI pattern: Boss={BossEncounterId}, Phase={PhaseNumber}",
                bossEncounterId, phaseNumber);
            throw;
        }
    }

    /// <summary>
    /// Get all AI patterns for a boss encounter
    /// </summary>
    public List<BossAIPatternData> GetBossAIPatterns(int bossEncounterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Boss_AI_Patterns
                WHERE boss_encounter_id = $bossId
                ORDER BY phase_number
            ";
            command.Parameters.AddWithValue("$bossId", bossEncounterId);

            var patterns = new List<BossAIPatternData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                patterns.Add(MapBossAIPattern(reader));
            }

            return patterns;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss AI patterns: BossEncounterId={BossEncounterId}", bossEncounterId);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // BOSS TELEGRAPH STATE OPERATIONS (v0.23.2)
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Start a telegraphed ability charge
    /// </summary>
    public int StartTelegraph(string enemyId, int abilityId, int currentTurn, int chargeTurns, int interruptThreshold, List<string>? targetIds = null)
    {
        _log.Information("Starting telegraph: Enemy={EnemyId}, Ability={AbilityId}, Turn={Turn}",
            enemyId, abilityId, currentTurn);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Boss_Telegraph_State (
                    enemy_id, boss_ability_id,
                    charge_started_turn, charge_complete_turn, current_turn,
                    is_charging, interrupt_damage_threshold,
                    target_character_ids, started_at
                ) VALUES (
                    $enemyId, $abilityId,
                    $startTurn, $completeTurn, $currentTurn,
                    1, $interruptThreshold,
                    $targets, $startedAt
                )
            ";

            command.Parameters.AddWithValue("$enemyId", enemyId);
            command.Parameters.AddWithValue("$abilityId", abilityId);
            command.Parameters.AddWithValue("$startTurn", currentTurn);
            command.Parameters.AddWithValue("$completeTurn", currentTurn + chargeTurns);
            command.Parameters.AddWithValue("$currentTurn", currentTurn);
            command.Parameters.AddWithValue("$interruptThreshold", interruptThreshold);
            command.Parameters.AddWithValue("$targets", targetIds != null ? JsonSerializer.Serialize(targetIds) : "");
            command.Parameters.AddWithValue("$startedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));

            command.ExecuteNonQuery();

            var idCommand = connection.CreateCommand();
            idCommand.CommandText = "SELECT last_insert_rowid()";
            var telegraphId = Convert.ToInt32(idCommand.ExecuteScalar());

            _log.Information("Telegraph started: TelegraphId={TelegraphId}", telegraphId);
            return telegraphId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to start telegraph: Enemy={EnemyId}, Ability={AbilityId}", enemyId, abilityId);
            throw;
        }
    }

    /// <summary>
    /// Get all active telegraphs for a boss
    /// </summary>
    public List<BossTelegraphStateData> GetActiveTelegraphs(string enemyId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Boss_Telegraph_State
                WHERE enemy_id = $enemyId AND is_charging = 1 AND is_interrupted = 0
                ORDER BY charge_complete_turn
            ";
            command.Parameters.AddWithValue("$enemyId", enemyId);

            var telegraphs = new List<BossTelegraphStateData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                telegraphs.Add(MapBossTelegraphState(reader));
            }

            return telegraphs;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get active telegraphs: Enemy={EnemyId}", enemyId);
            throw;
        }
    }

    /// <summary>
    /// Update telegraph turn counter
    /// </summary>
    public void UpdateTelegraphTurn(int telegraphId, int currentTurn)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "UPDATE Boss_Telegraph_State SET current_turn = $turn WHERE telegraph_state_id = $id";
            command.Parameters.AddWithValue("$turn", currentTurn);
            command.Parameters.AddWithValue("$id", telegraphId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update telegraph turn: TelegraphId={TelegraphId}", telegraphId);
            throw;
        }
    }

    /// <summary>
    /// Mark telegraph as completed
    /// </summary>
    public void CompleteTelegraph(int telegraphId)
    {
        _log.Information("Completing telegraph: TelegraphId={TelegraphId}", telegraphId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Boss_Telegraph_State
                SET is_charging = 0, is_completed = 1, completed_at = $completedAt
                WHERE telegraph_state_id = $id
            ";
            command.Parameters.AddWithValue("$completedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("$id", telegraphId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to complete telegraph: TelegraphId={TelegraphId}", telegraphId);
            throw;
        }
    }

    /// <summary>
    /// Add interrupt damage to telegraph
    /// </summary>
    public bool AddInterruptDamage(int telegraphId, int damage)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Boss_Telegraph_State
                SET accumulated_interrupt_damage = accumulated_interrupt_damage + $damage
                WHERE telegraph_state_id = $id
            ";
            command.Parameters.AddWithValue("$damage", damage);
            command.Parameters.AddWithValue("$id", telegraphId);

            command.ExecuteNonQuery();

            // Check if interrupt threshold exceeded
            var checkCommand = connection.CreateCommand();
            checkCommand.CommandText = @"
                SELECT accumulated_interrupt_damage >= interrupt_damage_threshold
                FROM Boss_Telegraph_State
                WHERE telegraph_state_id = $id
            ";
            checkCommand.Parameters.AddWithValue("$id", telegraphId);

            var interrupted = Convert.ToBoolean(checkCommand.ExecuteScalar());
            return interrupted;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to add interrupt damage: TelegraphId={TelegraphId}", telegraphId);
            throw;
        }
    }

    /// <summary>
    /// Mark telegraph as interrupted
    /// </summary>
    public void InterruptTelegraph(int telegraphId)
    {
        _log.Information("Interrupting telegraph: TelegraphId={TelegraphId}", telegraphId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Boss_Telegraph_State
                SET is_charging = 0, is_interrupted = 1, completed_at = $completedAt
                WHERE telegraph_state_id = $id
            ";
            command.Parameters.AddWithValue("$completedAt", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("$id", telegraphId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to interrupt telegraph: TelegraphId={TelegraphId}", telegraphId);
            throw;
        }
    }

    /// <summary>
    /// Clear all telegraphs for a boss (when combat ends)
    /// </summary>
    public void ClearTelegraphs(string enemyId)
    {
        _log.Information("Clearing telegraphs: Enemy={EnemyId}", enemyId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Boss_Telegraph_State WHERE enemy_id = $enemyId";
            command.Parameters.AddWithValue("$enemyId", enemyId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to clear telegraphs: Enemy={EnemyId}", enemyId);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // HELPER METHODS
    // ═════════════════════════════════════════════════════════════

    private BossEncounterConfig MapBossEncounterConfig(SqliteDataReader reader)
    {
        return new BossEncounterConfig
        {
            BossEncounterId = reader.GetInt32(reader.GetOrdinal("boss_encounter_id")),
            EncounterId = reader.GetInt32(reader.GetOrdinal("encounter_id")),
            BossName = reader.GetString(reader.GetOrdinal("boss_name")),
            BossType = reader.GetString(reader.GetOrdinal("boss_type")),
            TotalPhases = reader.GetInt32(reader.GetOrdinal("total_phases")),
            Phase2HpThreshold = (float)reader.GetDouble(reader.GetOrdinal("phase_2_hp_threshold")),
            Phase3HpThreshold = (float)reader.GetDouble(reader.GetOrdinal("phase_3_hp_threshold")),
            Phase4HpThreshold = (float)reader.GetDouble(reader.GetOrdinal("phase_4_hp_threshold")),
            TransitionInvulnerabilityTurns = reader.GetInt32(reader.GetOrdinal("transition_invulnerability_turns")),
            EnrageTurnThreshold = reader.IsDBNull(reader.GetOrdinal("enrage_turn_threshold"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("enrage_turn_threshold")),
            EnrageHpThreshold = (float)reader.GetDouble(reader.GetOrdinal("enrage_hp_threshold")),
            EnrageDamageMultiplier = (float)reader.GetDouble(reader.GetOrdinal("enrage_damage_multiplier")),
            EnrageSpeedBonus = reader.GetInt32(reader.GetOrdinal("enrage_speed_bonus"))
        };
    }

    private BossPhaseDefinitionData MapPhaseDefinition(SqliteDataReader reader)
    {
        return new BossPhaseDefinitionData
        {
            PhaseDefinitionId = reader.GetInt32(reader.GetOrdinal("phase_definition_id")),
            BossEncounterId = reader.GetInt32(reader.GetOrdinal("boss_encounter_id")),
            PhaseNumber = reader.GetInt32(reader.GetOrdinal("phase_number")),
            PhaseName = reader.GetString(reader.GetOrdinal("phase_name")),
            PhaseDescription = reader.GetString(reader.GetOrdinal("phase_description")),
            SpawnsAdds = reader.GetInt32(reader.GetOrdinal("spawns_adds")) == 1,
            AddWaveComposition = reader.GetString(reader.GetOrdinal("add_wave_composition")),
            ActivatesHazards = reader.GetString(reader.GetOrdinal("activates_hazards")),
            ModifiesTerrain = reader.GetString(reader.GetOrdinal("modifies_terrain")),
            AIBehaviorPattern = reader.GetString(reader.GetOrdinal("ai_behavior_pattern")),
            AbilityUnlockList = reader.GetString(reader.GetOrdinal("ability_unlock_list")),
            AbilityDisableList = reader.GetString(reader.GetOrdinal("ability_disable_list")),
            DamageModifier = (float)reader.GetDouble(reader.GetOrdinal("damage_modifier")),
            DefenseModifier = reader.GetInt32(reader.GetOrdinal("defense_modifier")),
            SoakModifier = reader.GetInt32(reader.GetOrdinal("soak_modifier")),
            RegenerationPerTurn = reader.GetInt32(reader.GetOrdinal("regeneration_per_turn")),
            BonusActionsPerTurn = reader.GetInt32(reader.GetOrdinal("bonus_actions_per_turn"))
        };
    }

    private BossCombatStateData MapBossCombatState(SqliteDataReader reader)
    {
        return new BossCombatStateData
        {
            CombatStateId = reader.GetInt32(reader.GetOrdinal("combat_state_id")),
            BossEncounterId = reader.GetInt32(reader.GetOrdinal("boss_encounter_id")),
            EnemyId = reader.GetString(reader.GetOrdinal("enemy_id")),
            CurrentPhase = reader.GetInt32(reader.GetOrdinal("current_phase")),
            Phase2Triggered = reader.GetInt32(reader.GetOrdinal("phase_2_triggered")) == 1,
            Phase3Triggered = reader.GetInt32(reader.GetOrdinal("phase_3_triggered")) == 1,
            Phase4Triggered = reader.GetInt32(reader.GetOrdinal("phase_4_triggered")) == 1,
            IsEnraged = reader.GetInt32(reader.GetOrdinal("is_enraged")) == 1,
            EnrageTriggeredTurn = reader.IsDBNull(reader.GetOrdinal("enrage_triggered_turn"))
                ? null
                : reader.GetInt32(reader.GetOrdinal("enrage_triggered_turn")),
            TotalAddsSpawned = reader.GetInt32(reader.GetOrdinal("total_adds_spawned")),
            CurrentAddsAlive = reader.GetInt32(reader.GetOrdinal("current_adds_alive")),
            IsTransitioning = reader.GetInt32(reader.GetOrdinal("is_transitioning")) == 1,
            InvulnerabilityTurnsRemaining = reader.GetInt32(reader.GetOrdinal("invulnerability_turns_remaining"))
        };
    }

    private BossAbilityData MapBossAbility(SqliteDataReader reader)
    {
        return new BossAbilityData
        {
            BossAbilityId = reader.GetInt32(reader.GetOrdinal("boss_ability_id")),
            BossEncounterId = reader.GetInt32(reader.GetOrdinal("boss_encounter_id")),
            AbilityName = reader.GetString(reader.GetOrdinal("ability_name")),
            AbilityDescription = reader.GetString(reader.GetOrdinal("ability_description")),
            AbilityType = reader.GetString(reader.GetOrdinal("ability_type")),
            IsTelegraphed = reader.GetInt32(reader.GetOrdinal("is_telegraphed")) == 1,
            TelegraphChargeTurns = reader.GetInt32(reader.GetOrdinal("telegraph_charge_turns")),
            TelegraphWarningMessage = reader.GetString(reader.GetOrdinal("telegraph_warning_message")),
            IsUltimate = reader.GetInt32(reader.GetOrdinal("is_ultimate")) == 1,
            VulnerabilityDurationTurns = reader.GetInt32(reader.GetOrdinal("vulnerability_duration_turns")),
            VulnerabilityDamageMultiplier = (float)reader.GetDouble(reader.GetOrdinal("vulnerability_damage_multiplier")),
            BaseDamageDice = reader.GetInt32(reader.GetOrdinal("base_damage_dice")),
            DamageBonus = reader.GetInt32(reader.GetOrdinal("damage_bonus")),
            DamageType = reader.GetString(reader.GetOrdinal("damage_type")),
            DamageFormula = reader.GetString(reader.GetOrdinal("damage_formula")),
            TargetType = reader.GetString(reader.GetOrdinal("target_type")),
            AoeRadius = reader.GetInt32(reader.GetOrdinal("aoe_radius")),
            AppliesStatusEffects = reader.GetString(reader.GetOrdinal("applies_status_effects")),
            InterruptDamageThreshold = reader.GetInt32(reader.GetOrdinal("interrupt_damage_threshold")),
            InterruptStaggerDuration = reader.GetInt32(reader.GetOrdinal("interrupt_stagger_duration")),
            CooldownTurns = reader.GetInt32(reader.GetOrdinal("cooldown_turns")),
            SpecialEffects = reader.GetString(reader.GetOrdinal("special_effects"))
        };
    }

    private BossAIPatternData MapBossAIPattern(SqliteDataReader reader)
    {
        return new BossAIPatternData
        {
            AiPatternId = reader.GetInt32(reader.GetOrdinal("ai_pattern_id")),
            BossEncounterId = reader.GetInt32(reader.GetOrdinal("boss_encounter_id")),
            PhaseNumber = reader.GetInt32(reader.GetOrdinal("phase_number")),
            PatternName = reader.GetString(reader.GetOrdinal("pattern_name")),
            PatternDescription = reader.GetString(reader.GetOrdinal("pattern_description")),
            TargetPriorityList = reader.GetString(reader.GetOrdinal("target_priority_list")),
            PreferredAbilities = reader.GetString(reader.GetOrdinal("preferred_abilities")),
            AbilityRotation = reader.GetString(reader.GetOrdinal("ability_rotation")),
            TelegraphFrequency = (float)reader.GetDouble(reader.GetOrdinal("telegraph_frequency")),
            UltimateHpThreshold = (float)reader.GetDouble(reader.GetOrdinal("ultimate_hp_threshold")),
            PrefersMelee = reader.GetInt32(reader.GetOrdinal("prefers_melee")) == 1,
            PrefersRange = reader.GetInt32(reader.GetOrdinal("prefers_range")) == 1,
            PositioningStrategy = reader.GetString(reader.GetOrdinal("positioning_strategy")),
            HealThreshold = (float)reader.GetDouble(reader.GetOrdinal("heal_threshold")),
            RetreatThreshold = (float)reader.GetDouble(reader.GetOrdinal("retreat_threshold")),
            SummonAddsThreshold = (float)reader.GetDouble(reader.GetOrdinal("summon_adds_threshold"))
        };
    }

    private BossTelegraphStateData MapBossTelegraphState(SqliteDataReader reader)
    {
        return new BossTelegraphStateData
        {
            TelegraphStateId = reader.GetInt32(reader.GetOrdinal("telegraph_state_id")),
            EnemyId = reader.GetString(reader.GetOrdinal("enemy_id")),
            BossAbilityId = reader.GetInt32(reader.GetOrdinal("boss_ability_id")),
            ChargeStartedTurn = reader.GetInt32(reader.GetOrdinal("charge_started_turn")),
            ChargeCompleteTurn = reader.GetInt32(reader.GetOrdinal("charge_complete_turn")),
            CurrentTurn = reader.GetInt32(reader.GetOrdinal("current_turn")),
            IsCharging = reader.GetInt32(reader.GetOrdinal("is_charging")) == 1,
            IsCompleted = reader.GetInt32(reader.GetOrdinal("is_completed")) == 1,
            IsInterrupted = reader.GetInt32(reader.GetOrdinal("is_interrupted")) == 1,
            InterruptDamageThreshold = reader.GetInt32(reader.GetOrdinal("interrupt_damage_threshold")),
            AccumulatedInterruptDamage = reader.GetInt32(reader.GetOrdinal("accumulated_interrupt_damage")),
            TargetCharacterIds = reader.GetString(reader.GetOrdinal("target_character_ids"))
        };
    }
}

// ═════════════════════════════════════════════════════════════
// DATA MODELS
// ═════════════════════════════════════════════════════════════

/// <summary>
/// Boss encounter configuration (maps to Boss_Encounters table)
/// </summary>
public class BossEncounterConfig
{
    public int BossEncounterId { get; set; }
    public int EncounterId { get; set; }
    public string BossName { get; set; } = string.Empty;
    public string BossType { get; set; } = "Sector Boss";
    public int TotalPhases { get; set; } = 2;
    public float Phase2HpThreshold { get; set; } = 0.75f;
    public float Phase3HpThreshold { get; set; } = 0.50f;
    public float Phase4HpThreshold { get; set; } = 0.25f;
    public int TransitionInvulnerabilityTurns { get; set; } = 1;
    public int? EnrageTurnThreshold { get; set; } = null;
    public float EnrageHpThreshold { get; set; } = 0.25f;
    public float EnrageDamageMultiplier { get; set; } = 1.5f;
    public int EnrageSpeedBonus { get; set; } = 1;
}

/// <summary>
/// Boss phase definition data (maps to Boss_Phase_Definitions table)
/// </summary>
public class BossPhaseDefinitionData
{
    public int PhaseDefinitionId { get; set; }
    public int BossEncounterId { get; set; }
    public int PhaseNumber { get; set; }
    public string? PhaseName { get; set; }
    public string? PhaseDescription { get; set; }
    public bool SpawnsAdds { get; set; }
    public string? AddWaveComposition { get; set; }
    public string? ActivatesHazards { get; set; }
    public string? ModifiesTerrain { get; set; }
    public string? AIBehaviorPattern { get; set; }
    public string? AbilityUnlockList { get; set; }
    public string? AbilityDisableList { get; set; }
    public float DamageModifier { get; set; } = 1.0f;
    public int DefenseModifier { get; set; } = 0;
    public int SoakModifier { get; set; } = 0;
    public int RegenerationPerTurn { get; set; } = 0;
    public int BonusActionsPerTurn { get; set; } = 0;
}

/// <summary>
/// Boss combat state (maps to Boss_Combat_State table)
/// </summary>
public class BossCombatStateData
{
    public int CombatStateId { get; set; }
    public int BossEncounterId { get; set; }
    public string EnemyId { get; set; } = string.Empty;
    public int CurrentPhase { get; set; } = 1;
    public bool Phase2Triggered { get; set; }
    public bool Phase3Triggered { get; set; }
    public bool Phase4Triggered { get; set; }
    public bool IsEnraged { get; set; }
    public int? EnrageTriggeredTurn { get; set; }
    public int TotalAddsSpawned { get; set; }
    public int CurrentAddsAlive { get; set; }
    public bool IsTransitioning { get; set; }
    public int InvulnerabilityTurnsRemaining { get; set; }
}

/// <summary>
/// v0.23.2: Boss ability data (maps to Boss_Abilities table)
/// </summary>
public class BossAbilityData
{
    public int BossAbilityId { get; set; }
    public int BossEncounterId { get; set; }
    public string AbilityName { get; set; } = string.Empty;
    public string? AbilityDescription { get; set; }
    public string AbilityType { get; set; } = "Standard";

    // Telegraph configuration
    public bool IsTelegraphed { get; set; }
    public int TelegraphChargeTurns { get; set; } = 1;
    public string? TelegraphWarningMessage { get; set; }

    // Ultimate configuration
    public bool IsUltimate { get; set; }
    public int VulnerabilityDurationTurns { get; set; } = 0;
    public float VulnerabilityDamageMultiplier { get; set; } = 1.5f;

    // Damage configuration
    public int BaseDamageDice { get; set; }
    public int DamageBonus { get; set; } = 0;
    public string? DamageType { get; set; } = "Physical";
    public string? DamageFormula { get; set; }

    // Target configuration
    public string? TargetType { get; set; } = "Single";
    public int AoeRadius { get; set; } = 0;

    // Status effects (JSON)
    public string? AppliesStatusEffects { get; set; }

    // Interrupt mechanics
    public int InterruptDamageThreshold { get; set; } = 0;
    public int InterruptStaggerDuration { get; set; } = 2;

    // Cooldown
    public int CooldownTurns { get; set; } = 0;

    // Special effects (JSON)
    public string? SpecialEffects { get; set; }
}

/// <summary>
/// v0.23.2: Boss AI pattern data (maps to Boss_AI_Patterns table)
/// </summary>
public class BossAIPatternData
{
    public int AiPatternId { get; set; }
    public int BossEncounterId { get; set; }
    public int PhaseNumber { get; set; }

    // AI behavior configuration
    public string? PatternName { get; set; }
    public string? PatternDescription { get; set; }

    // Target priority (JSON array)
    public string? TargetPriorityList { get; set; }

    // Ability usage patterns
    public string? PreferredAbilities { get; set; }
    public string? AbilityRotation { get; set; }
    public float TelegraphFrequency { get; set; } = 0.3f;
    public float UltimateHpThreshold { get; set; } = 0.5f;

    // Positioning behavior
    public bool PrefersMelee { get; set; } = true;
    public bool PrefersRange { get; set; } = false;
    public string? PositioningStrategy { get; set; }

    // Defensive behavior
    public float HealThreshold { get; set; } = 0.25f;
    public float RetreatThreshold { get; set; } = 0.15f;
    public float SummonAddsThreshold { get; set; } = 0.75f;
}

/// <summary>
/// v0.23.2: Boss telegraph state data (maps to Boss_Telegraph_State table)
/// </summary>
public class BossTelegraphStateData
{
    public int TelegraphStateId { get; set; }
    public string EnemyId { get; set; } = string.Empty;
    public int BossAbilityId { get; set; }

    // Charge tracking
    public int ChargeStartedTurn { get; set; }
    public int ChargeCompleteTurn { get; set; }
    public int CurrentTurn { get; set; }
    public bool IsCharging { get; set; } = true;
    public bool IsCompleted { get; set; }
    public bool IsInterrupted { get; set; }

    // Interrupt tracking
    public int InterruptDamageThreshold { get; set; }
    public int AccumulatedInterruptDamage { get; set; }

    // Target tracking (JSON)
    public string? TargetCharacterIds { get; set; }
}
