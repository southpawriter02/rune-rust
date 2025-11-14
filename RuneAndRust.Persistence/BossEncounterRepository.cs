using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.23.1: Repository for managing boss encounter data including phases, add waves, and enrage mechanics.
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

            // Create indexes for efficient querying
            var createIndexCommands = new[]
            {
                "CREATE INDEX IF NOT EXISTS idx_boss_encounters_encounter ON Boss_Encounters(encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_encounters_phase ON Boss_Encounters(current_phase)",
                "CREATE INDEX IF NOT EXISTS idx_boss_phases_boss ON Boss_Phase_Definitions(boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_combat_state_boss ON Boss_Combat_State(boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_boss_combat_state_enemy ON Boss_Combat_State(enemy_id)"
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
