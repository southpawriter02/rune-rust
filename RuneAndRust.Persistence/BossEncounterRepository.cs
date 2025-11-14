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

            // v0.23.3: Boss_Loot_Tables table (guaranteed drops, quality distribution, rewards)
            var createBossLootTablesTable = connection.CreateCommand();
            createBossLootTablesTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Boss_Loot_Tables (
                    boss_loot_table_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    boss_encounter_id INTEGER NOT NULL UNIQUE,

                    -- Guaranteed drops
                    guaranteed_drop_count INTEGER DEFAULT 1,
                    minimum_quality_tier TEXT DEFAULT 'Clan-Forged',

                    -- Quality distribution (percentages)
                    clan_forged_chance INTEGER DEFAULT 40,
                    rune_carved_chance INTEGER DEFAULT 45,
                    artifact_chance INTEGER DEFAULT 15,

                    -- Currency rewards
                    silver_marks_min INTEGER DEFAULT 100,
                    silver_marks_max INTEGER DEFAULT 200,

                    -- Special drops
                    drops_unique_item INTEGER DEFAULT 1,
                    unique_item_pool TEXT,

                    -- Crafting materials
                    drops_crafting_materials INTEGER DEFAULT 1,
                    crafting_material_pool TEXT,

                    FOREIGN KEY (boss_encounter_id) REFERENCES Boss_Encounters(boss_encounter_id) ON DELETE CASCADE
                )
            ";
            createBossLootTablesTable.ExecuteNonQuery();

            // v0.23.3: Artifacts table (legendary items with unique effects)
            var createArtifactsTable = connection.CreateCommand();
            createArtifactsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Artifacts (
                    artifact_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    artifact_name TEXT NOT NULL UNIQUE,
                    artifact_type TEXT NOT NULL,

                    -- Item properties
                    description TEXT,
                    flavor_text TEXT,

                    -- Stats
                    might_bonus INTEGER DEFAULT 0,
                    finesse_bonus INTEGER DEFAULT 0,
                    wits_bonus INTEGER DEFAULT 0,
                    will_bonus INTEGER DEFAULT 0,
                    sturdiness_bonus INTEGER DEFAULT 0,

                    max_hp_bonus INTEGER DEFAULT 0,
                    max_stamina_bonus INTEGER DEFAULT 0,
                    max_aether_bonus INTEGER DEFAULT 0,

                    defense_bonus INTEGER DEFAULT 0,
                    soak_bonus INTEGER DEFAULT 0,
                    accuracy_bonus INTEGER DEFAULT 0,

                    -- Unique properties
                    unique_effect_name TEXT,
                    unique_effect_description TEXT,
                    unique_effect_script TEXT,

                    -- Set membership (optional)
                    set_name TEXT,
                    set_piece_count INTEGER,

                    -- Drop restrictions
                    drops_from_boss_encounter_id INTEGER,
                    minimum_tdr INTEGER DEFAULT 60,

                    -- Rarity
                    is_unique INTEGER DEFAULT 1,

                    FOREIGN KEY (drops_from_boss_encounter_id) REFERENCES Boss_Encounters(boss_encounter_id)
                )
            ";
            createArtifactsTable.ExecuteNonQuery();

            // v0.23.3: Boss_Unique_Items table (boss-specific unique drops)
            var createBossUniqueItemsTable = connection.CreateCommand();
            createBossUniqueItemsTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Boss_Unique_Items (
                    unique_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    boss_encounter_id INTEGER NOT NULL,
                    artifact_id INTEGER NOT NULL,

                    -- Drop configuration
                    drop_chance INTEGER DEFAULT 100,
                    drop_count_min INTEGER DEFAULT 1,
                    drop_count_max INTEGER DEFAULT 1,

                    -- Restrictions
                    drops_once_per_character INTEGER DEFAULT 0,

                    FOREIGN KEY (boss_encounter_id) REFERENCES Boss_Encounters(boss_encounter_id) ON DELETE CASCADE,
                    FOREIGN KEY (artifact_id) REFERENCES Artifacts(artifact_id)
                )
            ";
            createBossUniqueItemsTable.ExecuteNonQuery();

            // v0.23.3: Artifact_Set_Bonuses table (set bonus effects)
            var createArtifactSetBonusesTable = connection.CreateCommand();
            createArtifactSetBonusesTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Artifact_Set_Bonuses (
                    set_bonus_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    set_name TEXT NOT NULL,
                    pieces_required INTEGER NOT NULL,

                    -- Bonus effects
                    bonus_name TEXT NOT NULL,
                    bonus_description TEXT NOT NULL,
                    bonus_effect_script TEXT,

                    -- Display
                    bonus_icon TEXT,

                    UNIQUE(set_name, pieces_required)
                )
            ";
            createArtifactSetBonusesTable.ExecuteNonQuery();

            // v0.23.3: Character_Unique_Items_Received table (tracking once-per-character drops)
            var createCharacterUniqueItemsReceivedTable = connection.CreateCommand();
            createCharacterUniqueItemsReceivedTable.CommandText = @"
                CREATE TABLE IF NOT EXISTS Character_Unique_Items_Received (
                    character_unique_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    character_id TEXT NOT NULL,
                    artifact_id INTEGER NOT NULL,
                    received_at TEXT NOT NULL,

                    UNIQUE(character_id, artifact_id)
                )
            ";
            createCharacterUniqueItemsReceivedTable.ExecuteNonQuery();

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
                "CREATE INDEX IF NOT EXISTS idx_boss_telegraph_state_charging ON Boss_Telegraph_State(is_charging)",
                "CREATE INDEX IF NOT EXISTS idx_boss_loot_tables_boss ON Boss_Loot_Tables(boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_artifacts_boss ON Artifacts(drops_from_boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_artifacts_set ON Artifacts(set_name)",
                "CREATE INDEX IF NOT EXISTS idx_artifacts_tdr ON Artifacts(minimum_tdr)",
                "CREATE INDEX IF NOT EXISTS idx_boss_unique_items_boss ON Boss_Unique_Items(boss_encounter_id)",
                "CREATE INDEX IF NOT EXISTS idx_artifact_set_bonuses_set ON Artifact_Set_Bonuses(set_name)",
                "CREATE INDEX IF NOT EXISTS idx_character_unique_items_char ON Character_Unique_Items_Received(character_id)"
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
    // v0.23.3: BOSS LOOT TABLES
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Create boss loot table configuration
    /// </summary>
    public int CreateBossLootTable(BossLootTableData lootTable)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Boss_Loot_Tables (
                    boss_encounter_id, guaranteed_drop_count, minimum_quality_tier,
                    clan_forged_chance, rune_carved_chance, artifact_chance,
                    silver_marks_min, silver_marks_max,
                    drops_unique_item, unique_item_pool,
                    drops_crafting_materials, crafting_material_pool
                )
                VALUES (
                    $bossEncounterId, $guaranteedDropCount, $minimumQualityTier,
                    $clanForgedChance, $runeCarvedChance, $artifactChance,
                    $silverMarksMin, $silverMarksMax,
                    $dropsUniqueItem, $uniqueItemPool,
                    $dropsCraftingMaterials, $craftingMaterialPool
                )
            ";

            command.Parameters.AddWithValue("$bossEncounterId", lootTable.BossEncounterId);
            command.Parameters.AddWithValue("$guaranteedDropCount", lootTable.GuaranteedDropCount);
            command.Parameters.AddWithValue("$minimumQualityTier", lootTable.MinimumQualityTier);
            command.Parameters.AddWithValue("$clanForgedChance", lootTable.ClanForgedChance);
            command.Parameters.AddWithValue("$runeCarvedChance", lootTable.RuneCarvedChance);
            command.Parameters.AddWithValue("$artifactChance", lootTable.ArtifactChance);
            command.Parameters.AddWithValue("$silverMarksMin", lootTable.SilverMarksMin);
            command.Parameters.AddWithValue("$silverMarksMax", lootTable.SilverMarksMax);
            command.Parameters.AddWithValue("$dropsUniqueItem", lootTable.DropsUniqueItem ? 1 : 0);
            command.Parameters.AddWithValue("$uniqueItemPool", lootTable.UniqueItemPool ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$dropsCraftingMaterials", lootTable.DropsCraftingMaterials ? 1 : 0);
            command.Parameters.AddWithValue("$craftingMaterialPool", lootTable.CraftingMaterialPool ?? (object)DBNull.Value);

            command.ExecuteNonQuery();

            return (int)connection.LastInsertRowId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create boss loot table: BossEncounterId={BossEncounterId}", lootTable.BossEncounterId);
            throw;
        }
    }

    /// <summary>
    /// Get boss loot table configuration
    /// </summary>
    public BossLootTableData? GetBossLootTable(int bossEncounterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Boss_Loot_Tables
                WHERE boss_encounter_id = $bossEncounterId
            ";
            command.Parameters.AddWithValue("$bossEncounterId", bossEncounterId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapBossLootTable(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss loot table: BossEncounterId={BossEncounterId}", bossEncounterId);
            throw;
        }
    }

    /// <summary>
    /// Update boss loot table configuration
    /// </summary>
    public void UpdateBossLootTable(BossLootTableData lootTable)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Boss_Loot_Tables SET
                    guaranteed_drop_count = $guaranteedDropCount,
                    minimum_quality_tier = $minimumQualityTier,
                    clan_forged_chance = $clanForgedChance,
                    rune_carved_chance = $runeCarvedChance,
                    artifact_chance = $artifactChance,
                    silver_marks_min = $silverMarksMin,
                    silver_marks_max = $silverMarksMax,
                    drops_unique_item = $dropsUniqueItem,
                    unique_item_pool = $uniqueItemPool,
                    drops_crafting_materials = $dropsCraftingMaterials,
                    crafting_material_pool = $craftingMaterialPool
                WHERE boss_loot_table_id = $lootTableId
            ";

            command.Parameters.AddWithValue("$lootTableId", lootTable.BossLootTableId);
            command.Parameters.AddWithValue("$guaranteedDropCount", lootTable.GuaranteedDropCount);
            command.Parameters.AddWithValue("$minimumQualityTier", lootTable.MinimumQualityTier);
            command.Parameters.AddWithValue("$clanForgedChance", lootTable.ClanForgedChance);
            command.Parameters.AddWithValue("$runeCarvedChance", lootTable.RuneCarvedChance);
            command.Parameters.AddWithValue("$artifactChance", lootTable.ArtifactChance);
            command.Parameters.AddWithValue("$silverMarksMin", lootTable.SilverMarksMin);
            command.Parameters.AddWithValue("$silverMarksMax", lootTable.SilverMarksMax);
            command.Parameters.AddWithValue("$dropsUniqueItem", lootTable.DropsUniqueItem ? 1 : 0);
            command.Parameters.AddWithValue("$uniqueItemPool", lootTable.UniqueItemPool ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$dropsCraftingMaterials", lootTable.DropsCraftingMaterials ? 1 : 0);
            command.Parameters.AddWithValue("$craftingMaterialPool", lootTable.CraftingMaterialPool ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to update boss loot table: LootTableId={LootTableId}", lootTable.BossLootTableId);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // v0.23.3: ARTIFACTS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Create artifact definition
    /// </summary>
    public int CreateArtifact(ArtifactData artifact)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Artifacts (
                    artifact_name, artifact_type, description, flavor_text,
                    might_bonus, finesse_bonus, wits_bonus, will_bonus, sturdiness_bonus,
                    max_hp_bonus, max_stamina_bonus, max_aether_bonus,
                    defense_bonus, soak_bonus, accuracy_bonus,
                    unique_effect_name, unique_effect_description, unique_effect_script,
                    set_name, set_piece_count,
                    drops_from_boss_encounter_id, minimum_tdr, is_unique
                )
                VALUES (
                    $artifactName, $artifactType, $description, $flavorText,
                    $mightBonus, $finesseBonus, $witsBonus, $willBonus, $sturdinessBonus,
                    $maxHpBonus, $maxStaminaBonus, $maxAetherBonus,
                    $defenseBonus, $soakBonus, $accuracyBonus,
                    $uniqueEffectName, $uniqueEffectDescription, $uniqueEffectScript,
                    $setName, $setPieceCount,
                    $dropsFromBossEncounterId, $minimumTdr, $isUnique
                )
            ";

            command.Parameters.AddWithValue("$artifactName", artifact.ArtifactName);
            command.Parameters.AddWithValue("$artifactType", artifact.ArtifactType);
            command.Parameters.AddWithValue("$description", artifact.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$flavorText", artifact.FlavorText ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$mightBonus", artifact.MightBonus);
            command.Parameters.AddWithValue("$finesseBonus", artifact.FinesseBonus);
            command.Parameters.AddWithValue("$witsBonus", artifact.WitsBonus);
            command.Parameters.AddWithValue("$willBonus", artifact.WillBonus);
            command.Parameters.AddWithValue("$sturdinessBonus", artifact.SturdinessBonus);
            command.Parameters.AddWithValue("$maxHpBonus", artifact.MaxHpBonus);
            command.Parameters.AddWithValue("$maxStaminaBonus", artifact.MaxStaminaBonus);
            command.Parameters.AddWithValue("$maxAetherBonus", artifact.MaxAetherBonus);
            command.Parameters.AddWithValue("$defenseBonus", artifact.DefenseBonus);
            command.Parameters.AddWithValue("$soakBonus", artifact.SoakBonus);
            command.Parameters.AddWithValue("$accuracyBonus", artifact.AccuracyBonus);
            command.Parameters.AddWithValue("$uniqueEffectName", artifact.UniqueEffectName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$uniqueEffectDescription", artifact.UniqueEffectDescription ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$uniqueEffectScript", artifact.UniqueEffectScript ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$setName", artifact.SetName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$setPieceCount", artifact.SetPieceCount ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$dropsFromBossEncounterId", artifact.DropsFromBossEncounterId ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$minimumTdr", artifact.MinimumTdr);
            command.Parameters.AddWithValue("$isUnique", artifact.IsUnique ? 1 : 0);

            command.ExecuteNonQuery();

            return (int)connection.LastInsertRowId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create artifact: ArtifactName={ArtifactName}", artifact.ArtifactName);
            throw;
        }
    }

    /// <summary>
    /// Get artifact by ID
    /// </summary>
    public ArtifactData? GetArtifact(int artifactId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Artifacts WHERE artifact_id = $artifactId";
            command.Parameters.AddWithValue("$artifactId", artifactId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return MapArtifact(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get artifact: ArtifactId={ArtifactId}", artifactId);
            throw;
        }
    }

    /// <summary>
    /// Get all artifacts that can drop from enemies with given TDR
    /// </summary>
    public List<ArtifactData> GetArtifactsByTDR(int minimumTdr)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Artifacts
                WHERE minimum_tdr <= $minimumTdr
                ORDER BY minimum_tdr DESC
            ";
            command.Parameters.AddWithValue("$minimumTdr", minimumTdr);

            var artifacts = new List<ArtifactData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                artifacts.Add(MapArtifact(reader));
            }

            return artifacts;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get artifacts by TDR: MinimumTdr={MinimumTdr}", minimumTdr);
            throw;
        }
    }

    /// <summary>
    /// Get all artifacts from a specific set
    /// </summary>
    public List<ArtifactData> GetArtifactsBySet(string setName)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Artifacts WHERE set_name = $setName";
            command.Parameters.AddWithValue("$setName", setName);

            var artifacts = new List<ArtifactData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                artifacts.Add(MapArtifact(reader));
            }

            return artifacts;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get artifacts by set: SetName={SetName}", setName);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // v0.23.3: BOSS UNIQUE ITEMS
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Create boss-specific unique item drop
    /// </summary>
    public int CreateBossUniqueItem(BossUniqueItemData uniqueItem)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Boss_Unique_Items (
                    boss_encounter_id, artifact_id, drop_chance,
                    drop_count_min, drop_count_max, drops_once_per_character
                )
                VALUES (
                    $bossEncounterId, $artifactId, $dropChance,
                    $dropCountMin, $dropCountMax, $dropsOncePerCharacter
                )
            ";

            command.Parameters.AddWithValue("$bossEncounterId", uniqueItem.BossEncounterId);
            command.Parameters.AddWithValue("$artifactId", uniqueItem.ArtifactId);
            command.Parameters.AddWithValue("$dropChance", uniqueItem.DropChance);
            command.Parameters.AddWithValue("$dropCountMin", uniqueItem.DropCountMin);
            command.Parameters.AddWithValue("$dropCountMax", uniqueItem.DropCountMax);
            command.Parameters.AddWithValue("$dropsOncePerCharacter", uniqueItem.DropsOncePerCharacter ? 1 : 0);

            command.ExecuteNonQuery();

            return (int)connection.LastInsertRowId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create boss unique item: BossEncounterId={BossEncounterId}, ArtifactId={ArtifactId}",
                uniqueItem.BossEncounterId, uniqueItem.ArtifactId);
            throw;
        }
    }

    /// <summary>
    /// Get all unique items for a boss encounter
    /// </summary>
    public List<BossUniqueItemData> GetBossUniqueItems(int bossEncounterId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Boss_Unique_Items
                WHERE boss_encounter_id = $bossEncounterId
            ";
            command.Parameters.AddWithValue("$bossEncounterId", bossEncounterId);

            var uniqueItems = new List<BossUniqueItemData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                uniqueItems.Add(MapBossUniqueItem(reader));
            }

            return uniqueItems;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get boss unique items: BossEncounterId={BossEncounterId}", bossEncounterId);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // v0.23.3: ONCE-PER-CHARACTER TRACKING
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Check if character has already received a unique item
    /// </summary>
    public bool HasReceivedUniqueItem(string characterId, int artifactId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM Character_Unique_Items_Received
                WHERE character_id = $characterId AND artifact_id = $artifactId
            ";
            command.Parameters.AddWithValue("$characterId", characterId);
            command.Parameters.AddWithValue("$artifactId", artifactId);

            var count = (long)command.ExecuteScalar()!;
            return count > 0;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to check unique item receipt: CharacterId={CharacterId}, ArtifactId={ArtifactId}",
                characterId, artifactId);
            throw;
        }
    }

    /// <summary>
    /// Record that character received a unique item
    /// </summary>
    public void RecordUniqueItemDrop(string characterId, int artifactId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Character_Unique_Items_Received (character_id, artifact_id, received_at)
                VALUES ($characterId, $artifactId, $receivedAt)
            ";
            command.Parameters.AddWithValue("$characterId", characterId);
            command.Parameters.AddWithValue("$artifactId", artifactId);
            command.Parameters.AddWithValue("$receivedAt", DateTime.UtcNow.ToString("o"));

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to record unique item drop: CharacterId={CharacterId}, ArtifactId={ArtifactId}",
                characterId, artifactId);
            throw;
        }
    }

    // ═════════════════════════════════════════════════════════════
    // v0.23.3: SET BONUSES
    // ═════════════════════════════════════════════════════════════

    /// <summary>
    /// Create set bonus definition
    /// </summary>
    public int CreateSetBonus(SetBonusData setBonus)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Artifact_Set_Bonuses (
                    set_name, pieces_required, bonus_name, bonus_description,
                    bonus_effect_script, bonus_icon
                )
                VALUES (
                    $setName, $piecesRequired, $bonusName, $bonusDescription,
                    $bonusEffectScript, $bonusIcon
                )
            ";

            command.Parameters.AddWithValue("$setName", setBonus.SetName);
            command.Parameters.AddWithValue("$piecesRequired", setBonus.PiecesRequired);
            command.Parameters.AddWithValue("$bonusName", setBonus.BonusName);
            command.Parameters.AddWithValue("$bonusDescription", setBonus.BonusDescription);
            command.Parameters.AddWithValue("$bonusEffectScript", setBonus.BonusEffectScript ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("$bonusIcon", setBonus.BonusIcon ?? (object)DBNull.Value);

            command.ExecuteNonQuery();

            return (int)connection.LastInsertRowId;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to create set bonus: SetName={SetName}, PiecesRequired={PiecesRequired}",
                setBonus.SetName, setBonus.PiecesRequired);
            throw;
        }
    }

    /// <summary>
    /// Get all set bonuses for a set
    /// </summary>
    public List<SetBonusData> GetSetBonuses(string setName)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Artifact_Set_Bonuses
                WHERE set_name = $setName
                ORDER BY pieces_required ASC
            ";
            command.Parameters.AddWithValue("$setName", setName);

            var setBonuses = new List<SetBonusData>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                setBonuses.Add(MapSetBonus(reader));
            }

            return setBonuses;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get set bonuses: SetName={SetName}", setName);
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

    // v0.23.3 Mappers

    private BossLootTableData MapBossLootTable(SqliteDataReader reader)
    {
        return new BossLootTableData
        {
            BossLootTableId = reader.GetInt32(reader.GetOrdinal("boss_loot_table_id")),
            BossEncounterId = reader.GetInt32(reader.GetOrdinal("boss_encounter_id")),
            GuaranteedDropCount = reader.GetInt32(reader.GetOrdinal("guaranteed_drop_count")),
            MinimumQualityTier = reader.GetString(reader.GetOrdinal("minimum_quality_tier")),
            ClanForgedChance = reader.GetInt32(reader.GetOrdinal("clan_forged_chance")),
            RuneCarvedChance = reader.GetInt32(reader.GetOrdinal("rune_carved_chance")),
            ArtifactChance = reader.GetInt32(reader.GetOrdinal("artifact_chance")),
            SilverMarksMin = reader.GetInt32(reader.GetOrdinal("silver_marks_min")),
            SilverMarksMax = reader.GetInt32(reader.GetOrdinal("silver_marks_max")),
            DropsUniqueItem = reader.GetInt32(reader.GetOrdinal("drops_unique_item")) == 1,
            UniqueItemPool = reader.IsDBNull(reader.GetOrdinal("unique_item_pool")) ? null : reader.GetString(reader.GetOrdinal("unique_item_pool")),
            DropsCraftingMaterials = reader.GetInt32(reader.GetOrdinal("drops_crafting_materials")) == 1,
            CraftingMaterialPool = reader.IsDBNull(reader.GetOrdinal("crafting_material_pool")) ? null : reader.GetString(reader.GetOrdinal("crafting_material_pool"))
        };
    }

    private ArtifactData MapArtifact(SqliteDataReader reader)
    {
        return new ArtifactData
        {
            ArtifactId = reader.GetInt32(reader.GetOrdinal("artifact_id")),
            ArtifactName = reader.GetString(reader.GetOrdinal("artifact_name")),
            ArtifactType = reader.GetString(reader.GetOrdinal("artifact_type")),
            Description = reader.IsDBNull(reader.GetOrdinal("description")) ? null : reader.GetString(reader.GetOrdinal("description")),
            FlavorText = reader.IsDBNull(reader.GetOrdinal("flavor_text")) ? null : reader.GetString(reader.GetOrdinal("flavor_text")),
            MightBonus = reader.GetInt32(reader.GetOrdinal("might_bonus")),
            FinesseBonus = reader.GetInt32(reader.GetOrdinal("finesse_bonus")),
            WitsBonus = reader.GetInt32(reader.GetOrdinal("wits_bonus")),
            WillBonus = reader.GetInt32(reader.GetOrdinal("will_bonus")),
            SturdinessBonus = reader.GetInt32(reader.GetOrdinal("sturdiness_bonus")),
            MaxHpBonus = reader.GetInt32(reader.GetOrdinal("max_hp_bonus")),
            MaxStaminaBonus = reader.GetInt32(reader.GetOrdinal("max_stamina_bonus")),
            MaxAetherBonus = reader.GetInt32(reader.GetOrdinal("max_aether_bonus")),
            DefenseBonus = reader.GetInt32(reader.GetOrdinal("defense_bonus")),
            SoakBonus = reader.GetInt32(reader.GetOrdinal("soak_bonus")),
            AccuracyBonus = reader.GetInt32(reader.GetOrdinal("accuracy_bonus")),
            UniqueEffectName = reader.IsDBNull(reader.GetOrdinal("unique_effect_name")) ? null : reader.GetString(reader.GetOrdinal("unique_effect_name")),
            UniqueEffectDescription = reader.IsDBNull(reader.GetOrdinal("unique_effect_description")) ? null : reader.GetString(reader.GetOrdinal("unique_effect_description")),
            UniqueEffectScript = reader.IsDBNull(reader.GetOrdinal("unique_effect_script")) ? null : reader.GetString(reader.GetOrdinal("unique_effect_script")),
            SetName = reader.IsDBNull(reader.GetOrdinal("set_name")) ? null : reader.GetString(reader.GetOrdinal("set_name")),
            SetPieceCount = reader.IsDBNull(reader.GetOrdinal("set_piece_count")) ? null : reader.GetInt32(reader.GetOrdinal("set_piece_count")),
            DropsFromBossEncounterId = reader.IsDBNull(reader.GetOrdinal("drops_from_boss_encounter_id")) ? null : reader.GetInt32(reader.GetOrdinal("drops_from_boss_encounter_id")),
            MinimumTdr = reader.GetInt32(reader.GetOrdinal("minimum_tdr")),
            IsUnique = reader.GetInt32(reader.GetOrdinal("is_unique")) == 1
        };
    }

    private BossUniqueItemData MapBossUniqueItem(SqliteDataReader reader)
    {
        return new BossUniqueItemData
        {
            UniqueItemId = reader.GetInt32(reader.GetOrdinal("unique_item_id")),
            BossEncounterId = reader.GetInt32(reader.GetOrdinal("boss_encounter_id")),
            ArtifactId = reader.GetInt32(reader.GetOrdinal("artifact_id")),
            DropChance = reader.GetInt32(reader.GetOrdinal("drop_chance")),
            DropCountMin = reader.GetInt32(reader.GetOrdinal("drop_count_min")),
            DropCountMax = reader.GetInt32(reader.GetOrdinal("drop_count_max")),
            DropsOncePerCharacter = reader.GetInt32(reader.GetOrdinal("drops_once_per_character")) == 1
        };
    }

    private SetBonusData MapSetBonus(SqliteDataReader reader)
    {
        return new SetBonusData
        {
            SetBonusId = reader.GetInt32(reader.GetOrdinal("set_bonus_id")),
            SetName = reader.GetString(reader.GetOrdinal("set_name")),
            PiecesRequired = reader.GetInt32(reader.GetOrdinal("pieces_required")),
            BonusName = reader.GetString(reader.GetOrdinal("bonus_name")),
            BonusDescription = reader.GetString(reader.GetOrdinal("bonus_description")),
            BonusEffectScript = reader.IsDBNull(reader.GetOrdinal("bonus_effect_script")) ? null : reader.GetString(reader.GetOrdinal("bonus_effect_script")),
            BonusIcon = reader.IsDBNull(reader.GetOrdinal("bonus_icon")) ? null : reader.GetString(reader.GetOrdinal("bonus_icon"))
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

// ═════════════════════════════════════════════════════════════
// v0.23.3: BOSS LOOT DATA MODELS
// ═════════════════════════════════════════════════════════════

/// <summary>
/// v0.23.3: Boss loot table configuration (maps to Boss_Loot_Tables table)
/// </summary>
public class BossLootTableData
{
    public int BossLootTableId { get; set; }
    public int BossEncounterId { get; set; }

    // Guaranteed drops
    public int GuaranteedDropCount { get; set; } = 1;
    public string MinimumQualityTier { get; set; } = "Clan-Forged";

    // Quality distribution (percentages)
    public int ClanForgedChance { get; set; } = 40;
    public int RuneCarvedChance { get; set; } = 45;
    public int ArtifactChance { get; set; } = 15;

    // Currency rewards
    public int SilverMarksMin { get; set; } = 100;
    public int SilverMarksMax { get; set; } = 200;

    // Special drops
    public bool DropsUniqueItem { get; set; } = true;
    public string? UniqueItemPool { get; set; }

    // Crafting materials
    public bool DropsCraftingMaterials { get; set; } = true;
    public string? CraftingMaterialPool { get; set; }
}

/// <summary>
/// v0.23.3: Artifact definition (maps to Artifacts table)
/// </summary>
public class ArtifactData
{
    public int ArtifactId { get; set; }
    public string ArtifactName { get; set; } = string.Empty;
    public string ArtifactType { get; set; } = string.Empty;

    // Item properties
    public string? Description { get; set; }
    public string? FlavorText { get; set; }

    // Stats
    public int MightBonus { get; set; }
    public int FinesseBonus { get; set; }
    public int WitsBonus { get; set; }
    public int WillBonus { get; set; }
    public int SturdinessBonus { get; set; }

    public int MaxHpBonus { get; set; }
    public int MaxStaminaBonus { get; set; }
    public int MaxAetherBonus { get; set; }

    public int DefenseBonus { get; set; }
    public int SoakBonus { get; set; }
    public int AccuracyBonus { get; set; }

    // Unique properties
    public string? UniqueEffectName { get; set; }
    public string? UniqueEffectDescription { get; set; }
    public string? UniqueEffectScript { get; set; }

    // Set membership (optional)
    public string? SetName { get; set; }
    public int? SetPieceCount { get; set; }

    // Drop restrictions
    public int? DropsFromBossEncounterId { get; set; }
    public int MinimumTdr { get; set; } = 60;

    // Rarity
    public bool IsUnique { get; set; } = true;
}

/// <summary>
/// v0.23.3: Boss-specific unique item drop (maps to Boss_Unique_Items table)
/// </summary>
public class BossUniqueItemData
{
    public int UniqueItemId { get; set; }
    public int BossEncounterId { get; set; }
    public int ArtifactId { get; set; }

    // Drop configuration
    public int DropChance { get; set; } = 100;
    public int DropCountMin { get; set; } = 1;
    public int DropCountMax { get; set; } = 1;

    // Restrictions
    public bool DropsOncePerCharacter { get; set; }
}

/// <summary>
/// v0.23.3: Set bonus definition (maps to Artifact_Set_Bonuses table)
/// </summary>
public class SetBonusData
{
    public int SetBonusId { get; set; }
    public string SetName { get; set; } = string.Empty;
    public int PiecesRequired { get; set; }

    // Bonus effects
    public string BonusName { get; set; } = string.Empty;
    public string BonusDescription { get; set; } = string.Empty;
    public string? BonusEffectScript { get; set; }

    // Display
    public string? BonusIcon { get; set; }
}
