using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using RuneAndRust.Core.Quests;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

public class SaveRepository
{
    private static readonly ILogger _log = Log.ForContext<SaveRepository>();
    private readonly string _connectionString;
    private const string DatabaseName = "runeandrust.db";

    public SaveRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, DatabaseName);
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("SaveRepository initialized with database path: {DbPath}", dbPath);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        _log.Debug("Initializing database");

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createTableCommand = connection.CreateCommand();
        createTableCommand.CommandText = @"
            CREATE TABLE IF NOT EXISTS saves (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_name TEXT UNIQUE NOT NULL,
                class TEXT NOT NULL,
                current_milestone INTEGER NOT NULL,
                current_legend INTEGER NOT NULL,
                progression_points INTEGER NOT NULL,
                might INTEGER NOT NULL,
                finesse INTEGER NOT NULL,
                wits INTEGER NOT NULL,
                will INTEGER NOT NULL,
                sturdiness INTEGER NOT NULL,
                current_hp INTEGER NOT NULL,
                max_hp INTEGER NOT NULL,
                current_stamina INTEGER NOT NULL,
                max_stamina INTEGER NOT NULL,
                current_room_id INTEGER NOT NULL,
                cleared_rooms_json TEXT NOT NULL,
                puzzle_solved INTEGER NOT NULL,
                boss_defeated INTEGER NOT NULL,
                equipped_weapon_json TEXT,
                equipped_armor_json TEXT,
                inventory_json TEXT DEFAULT '[]',
                room_items_json TEXT DEFAULT '{}',
                last_saved TEXT NOT NULL
            )
        ";
        createTableCommand.ExecuteNonQuery();

        // Add equipment columns to existing saves (migration for v0.3)
        // Add trauma economy columns (migration for v0.5)
        // Add specialization column (migration for v0.7)
        // Add Adept status effect columns (migration for v0.7)
        // Add consumables and crafting components columns (migration for v0.7)
        // Add NPC & Quest columns (migration for v0.8)
        // Add currency column (migration for v0.9)
        // Add procedural generation columns (migration for v0.10)
        // Add traumas column (migration for v0.15)
        var alterCommands = new[]
        {
            "ALTER TABLE saves ADD COLUMN equipped_weapon_json TEXT",
            "ALTER TABLE saves ADD COLUMN equipped_armor_json TEXT",
            "ALTER TABLE saves ADD COLUMN inventory_json TEXT DEFAULT '[]'",
            "ALTER TABLE saves ADD COLUMN room_items_json TEXT DEFAULT '{}'",
            "ALTER TABLE saves ADD COLUMN psychic_stress INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN corruption INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN rooms_explored_since_rest INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN traumas_json TEXT DEFAULT '[]'",
            "ALTER TABLE saves ADD COLUMN specialization TEXT DEFAULT 'None'",
            "ALTER TABLE saves ADD COLUMN vulnerable_turns INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN analyzed_turns INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN seized_turns INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN is_performing INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN performing_turns INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN current_performance TEXT",
            "ALTER TABLE saves ADD COLUMN inspired_turns INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN silenced_turns INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN temp_hp INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN consumables_json TEXT DEFAULT '[]'",
            "ALTER TABLE saves ADD COLUMN crafting_components_json TEXT DEFAULT '{}'",
            "ALTER TABLE saves ADD COLUMN faction_reputations_json TEXT DEFAULT '{}'",
            "ALTER TABLE saves ADD COLUMN active_quests_json TEXT DEFAULT '[]'",
            "ALTER TABLE saves ADD COLUMN completed_quests_json TEXT DEFAULT '[]'",
            "ALTER TABLE saves ADD COLUMN npc_states_json TEXT DEFAULT '[]'",
            "ALTER TABLE saves ADD COLUMN currency INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN current_dungeon_seed INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN dungeons_completed INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN current_room_string_id TEXT",
            "ALTER TABLE saves ADD COLUMN is_procedural_dungeon INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN current_biome_id TEXT",
            // v0.20: Tactical Combat Grid System
            "ALTER TABLE saves ADD COLUMN position_zone TEXT",
            "ALTER TABLE saves ADD COLUMN position_row TEXT",
            "ALTER TABLE saves ADD COLUMN position_column INTEGER",
            "ALTER TABLE saves ADD COLUMN position_elevation INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN kinetic_energy INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN max_kinetic_energy INTEGER DEFAULT 100",
            // v0.21.1: Advanced Stance System (v2.0 canonical)
            "ALTER TABLE saves ADD COLUMN active_stance_type TEXT DEFAULT 'Calculated'",
            "ALTER TABLE saves ADD COLUMN stance_turns_in_current INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN stance_shifts_remaining INTEGER DEFAULT 1"
        };

        foreach (var alterSql in alterCommands)
        {
            try
            {
                var alterCommand = connection.CreateCommand();
                alterCommand.CommandText = alterSql;
                alterCommand.ExecuteNonQuery();
            }
            catch (SqliteException)
            {
                // Column already exists, ignore
            }
        }

            // v0.19: Create specialization system tables (data-driven architecture)
            CreateSpecializationTables(connection);

            // v0.20: Create tactical combat grid system tables
            CreateBattlefieldGridTables(connection);

            // v0.29.1: Create Muspelheim biome tables
            CreateMuspelheimBiomeTables(connection);

            // v0.30.1: Create Niflheim biome tables
            CreateNiflheimBiomeTables(connection);

            // v0.33.1: Create Faction System tables
            CreateFactionTables(connection);

            // v0.34.1: Create Companion System tables
            CreateCompanionTables(connection);

            // v0.35.1: Create Territory Control & Dynamic World tables
            CreateTerritoryControlTables(connection);

            // v0.41: Create Meta-Progression System tables
            CreateMetaProgressionTables(connection);

            _log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize database");
            throw;
        }
    }

    /// <summary>
    /// v0.41: Create meta-progression system tables
    /// Initializes all repositories and seeds initial data
    /// </summary>
    private void CreateMetaProgressionTables(SqliteConnection connection)
    {
        _log.Debug("Creating meta-progression system tables");

        // Initialize all v0.41 repositories (creates tables)
        var accountProgressionRepo = new AccountProgressionRepository(_connectionString);
        var achievementRepo = new AchievementRepository(_connectionString);
        var cosmeticRepo = new CosmeticRepository(_connectionString);
        var alternativeStartRepo = new AlternativeStartRepository(_connectionString);

        // Seed initial meta-progression content
        var seeder = new MetaProgressionSeeder(_connectionString);
        seeder.SeedAll();

        _log.Information("Meta-progression system tables created and seeded");
    }

    /// <summary>
    /// v0.19: Create specialization system tables for data-driven architecture
    /// </summary>
    private void CreateSpecializationTables(SqliteConnection connection)
    {
        _log.Debug("Creating specialization system tables");

        // Specializations table - stores all specialization metadata
        var createSpecializationsTable = connection.CreateCommand();
        createSpecializationsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Specializations (
                SpecializationID INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                ArchetypeID INTEGER NOT NULL,
                PathType TEXT NOT NULL,
                MechanicalRole TEXT NOT NULL,
                PrimaryAttribute TEXT NOT NULL,
                SecondaryAttribute TEXT,
                Description TEXT,
                Tagline TEXT,
                UnlockRequirementsJson TEXT NOT NULL,
                ResourceSystem TEXT NOT NULL,
                TraumaRisk TEXT NOT NULL,
                IconEmoji TEXT,
                PPCostToUnlock INTEGER DEFAULT 3,
                IsActive INTEGER DEFAULT 1
            )
        ";
        createSpecializationsTable.ExecuteNonQuery();

        // Abilities table - stores all ability definitions
        var createAbilitiesTable = connection.CreateCommand();
        createAbilitiesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Abilities (
                AbilityID INTEGER PRIMARY KEY,
                SpecializationID INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL,
                TierLevel INTEGER NOT NULL,
                PPCost INTEGER NOT NULL,
                PrerequisitesJson TEXT NOT NULL,
                AbilityType TEXT NOT NULL,
                ActionType TEXT NOT NULL,
                TargetType TEXT NOT NULL,
                ResourceCostJson TEXT NOT NULL,
                AttributeUsed TEXT,
                BonusDice INTEGER DEFAULT 0,
                SuccessThreshold INTEGER DEFAULT 2,
                MechanicalSummary TEXT,
                DamageDice INTEGER DEFAULT 0,
                IgnoresArmor INTEGER DEFAULT 0,
                HealingDice INTEGER DEFAULT 0,
                StatusEffectsAppliedJson TEXT,
                StatusEffectsRemovedJson TEXT,
                MaxRank INTEGER DEFAULT 3,
                CostToRank2 INTEGER DEFAULT 5,
                CostToRank3 INTEGER DEFAULT 0,
                CooldownTurns INTEGER DEFAULT 0,
                CooldownType TEXT DEFAULT 'None',
                IsActive INTEGER DEFAULT 1,
                Notes TEXT,
                FOREIGN KEY (SpecializationID) REFERENCES Specializations(SpecializationID)
            )
        ";
        createAbilitiesTable.ExecuteNonQuery();

        // CharacterSpecializations - tracks which specs each character has unlocked
        var createCharacterSpecializationsTable = connection.CreateCommand();
        createCharacterSpecializationsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS CharacterSpecializations (
                CharacterID INTEGER NOT NULL,
                SpecializationID INTEGER NOT NULL,
                UnlockedAt TEXT NOT NULL,
                IsActive INTEGER DEFAULT 1,
                PPSpentInTree INTEGER DEFAULT 0,
                PRIMARY KEY (CharacterID, SpecializationID),
                FOREIGN KEY (SpecializationID) REFERENCES Specializations(SpecializationID)
            )
        ";
        createCharacterSpecializationsTable.ExecuteNonQuery();

        // CharacterAbilities - tracks which abilities each character has learned
        var createCharacterAbilitiesTable = connection.CreateCommand();
        createCharacterAbilitiesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS CharacterAbilities (
                CharacterID INTEGER NOT NULL,
                AbilityID INTEGER NOT NULL,
                UnlockedAt TEXT NOT NULL,
                CurrentRank INTEGER DEFAULT 1,
                TimesUsed INTEGER DEFAULT 0,
                PRIMARY KEY (CharacterID, AbilityID),
                FOREIGN KEY (AbilityID) REFERENCES Abilities(AbilityID)
            )
        ";
        createCharacterAbilitiesTable.ExecuteNonQuery();

        // Create indices for performance
        var createIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_specializations_archetype ON Specializations(ArchetypeID)",
            "CREATE INDEX IF NOT EXISTS idx_abilities_specialization ON Abilities(SpecializationID)",
            "CREATE INDEX IF NOT EXISTS idx_abilities_tier ON Abilities(TierLevel)",
            "CREATE INDEX IF NOT EXISTS idx_character_specs ON CharacterSpecializations(CharacterID)",
            "CREATE INDEX IF NOT EXISTS idx_character_abilities ON CharacterAbilities(CharacterID)"
        };

        foreach (var indexSql in createIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        // v0.26.1: Characters_Fury table - tracks Fury resource for Berserkr specialization
        var createCharactersFuryTable = connection.CreateCommand();
        createCharactersFuryTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Characters_Fury (
                character_id INTEGER PRIMARY KEY,
                current_fury INTEGER NOT NULL DEFAULT 0 CHECK(current_fury >= 0 AND current_fury <= 100),
                max_fury INTEGER NOT NULL DEFAULT 100,
                in_combat INTEGER NOT NULL DEFAULT 0,
                last_fury_gain_timestamp TEXT,
                total_fury_generated INTEGER NOT NULL DEFAULT 0,
                total_fury_spent INTEGER NOT NULL DEFAULT 0,
                unstoppable_fury_triggered INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            )
        ";
        createCharactersFuryTable.ExecuteNonQuery();

        // Create indices for Characters_Fury table
        var createFuryIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_fury_character ON Characters_Fury(character_id)",
            "CREATE INDEX IF NOT EXISTS idx_fury_in_combat ON Characters_Fury(in_combat)"
        };

        foreach (var indexSql in createFuryIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        // v0.28.1: Characters_SpiritBargains table - tracks Spirit Bargain mechanics for Seidkona specialization
        var createCharactersSpiritBargainsTable = connection.CreateCommand();
        createCharactersSpiritBargainsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Characters_SpiritBargains (
                character_id INTEGER PRIMARY KEY,
                total_bargains_triggered INTEGER NOT NULL DEFAULT 0,
                total_bargains_attempted INTEGER NOT NULL DEFAULT 0,
                bargain_success_rate REAL NOT NULL DEFAULT 0.0,
                fickle_fortune_rank INTEGER NOT NULL DEFAULT 0,
                in_moment_of_clarity INTEGER NOT NULL DEFAULT 0,
                clarity_turns_remaining INTEGER NOT NULL DEFAULT 0,
                clarity_uses_this_combat INTEGER NOT NULL DEFAULT 0,
                forced_bargain_used_this_combat INTEGER NOT NULL DEFAULT 0,
                psychic_resonance_bonus_active INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            )
        ";
        createCharactersSpiritBargainsTable.ExecuteNonQuery();

        // Create indices for Characters_SpiritBargains table
        var createSpiritBargainIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_spirit_bargains_character ON Characters_SpiritBargains(character_id)",
            "CREATE INDEX IF NOT EXISTS idx_spirit_bargains_clarity ON Characters_SpiritBargains(in_moment_of_clarity)"
        };

        foreach (var indexSql in createSpiritBargainIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        // v0.28.2: Characters_EchoChains table - tracks Echo Chain mechanics for Echo-Caller specialization
        var createCharactersEchoChainsTable = connection.CreateCommand();
        createCharactersEchoChainsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Characters_EchoChains (
                character_id INTEGER PRIMARY KEY,
                total_echoes_triggered INTEGER NOT NULL DEFAULT 0,
                total_echo_chains_triggered INTEGER NOT NULL DEFAULT 0,
                echo_cascade_rank INTEGER NOT NULL DEFAULT 0,
                echo_chain_range INTEGER NOT NULL DEFAULT 1,
                echo_chain_damage_multiplier REAL NOT NULL DEFAULT 0.5,
                echo_chain_max_targets INTEGER NOT NULL DEFAULT 1,
                silence_weapon_uses_this_combat INTEGER NOT NULL DEFAULT 0,
                created_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE
            )
        ";
        createCharactersEchoChainsTable.ExecuteNonQuery();

        // Create indices for Characters_EchoChains table
        var createEchoChainIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_echo_chains_character ON Characters_EchoChains(character_id)"
        };

        foreach (var indexSql in createEchoChainIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        _log.Information("Specialization system tables created successfully");
    }

    /// <summary>
    /// v0.20: Create tactical combat grid system tables
    /// </summary>
    private void CreateBattlefieldGridTables(SqliteConnection connection)
    {
        _log.Debug("Creating battlefield grid system tables");

        // battlefield_grids table - stores grid metadata
        var createGridsTable = connection.CreateCommand();
        createGridsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS battlefield_grids (
                grid_id TEXT PRIMARY KEY,
                combat_id TEXT,
                columns INTEGER NOT NULL,
                created_at TEXT NOT NULL
            )
        ";
        createGridsTable.ExecuteNonQuery();

        // battlefield_tiles table - stores individual tile state
        var createTilesTable = connection.CreateCommand();
        createTilesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS battlefield_tiles (
                tile_id TEXT PRIMARY KEY,
                grid_id TEXT NOT NULL,
                zone TEXT NOT NULL CHECK(zone IN ('Player', 'Enemy')),
                row TEXT NOT NULL CHECK(row IN ('Front', 'Back')),
                column_index INTEGER NOT NULL,
                elevation INTEGER DEFAULT 0,
                tile_type TEXT NOT NULL CHECK(tile_type IN ('Normal', 'HighGround', 'Glitched')),
                cover_type TEXT NOT NULL CHECK(cover_type IN ('None', 'Physical', 'Metaphysical', 'Both')),
                glitch_type TEXT CHECK(glitch_type IN ('Flickering', 'InvertedGravity', 'Looping')),
                glitch_severity INTEGER CHECK(glitch_severity BETWEEN 1 AND 3),
                is_occupied INTEGER DEFAULT 0,
                occupant_id TEXT,
                FOREIGN KEY (grid_id) REFERENCES battlefield_grids(grid_id)
            )
        ";
        createTilesTable.ExecuteNonQuery();

        // battlefield_traps table - stores active traps
        var createTrapsTable = connection.CreateCommand();
        createTrapsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS battlefield_traps (
                trap_id TEXT PRIMARY KEY,
                trap_name TEXT NOT NULL,
                tile_id TEXT NOT NULL,
                owner_id TEXT NOT NULL,
                turns_remaining INTEGER NOT NULL,
                is_visible INTEGER DEFAULT 0,
                effect_type TEXT NOT NULL CHECK(effect_type IN ('Damage', 'Status', 'Debuff', 'AreaEffect')),
                effect_data TEXT NOT NULL,
                trigger_type TEXT NOT NULL CHECK(trigger_type IN ('OnEnter', 'OnExit', 'Manual')),
                created_at TEXT NOT NULL,
                FOREIGN KEY (tile_id) REFERENCES battlefield_tiles(tile_id)
            )
        ";
        createTrapsTable.ExecuteNonQuery();

        // Create indices for performance
        var createIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_battlefield_grids_combat ON battlefield_grids(combat_id)",
            "CREATE INDEX IF NOT EXISTS idx_tiles_grid ON battlefield_tiles(grid_id)",
            "CREATE INDEX IF NOT EXISTS idx_tiles_position ON battlefield_tiles(grid_id, zone, row, column_index)",
            "CREATE INDEX IF NOT EXISTS idx_traps_tile ON battlefield_traps(tile_id)",
            "CREATE INDEX IF NOT EXISTS idx_traps_owner ON battlefield_traps(owner_id)"
        };

        foreach (var indexSql in createIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        _log.Information("Battlefield grid system tables created successfully");
    }

    /// <summary>
    /// v0.29.1: Create Muspelheim biome tables for database-driven biome system
    /// </summary>
    private void CreateMuspelheimBiomeTables(SqliteConnection connection)
    {
        _log.Debug("Creating Muspelheim biome system tables");

        // Table: Biomes - Core biome metadata
        var createBiomesTable = connection.CreateCommand();
        createBiomesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Biomes (
                biome_id INTEGER PRIMARY KEY,
                biome_name TEXT NOT NULL UNIQUE,
                biome_description TEXT,
                z_level_restriction TEXT,
                ambient_condition_id INTEGER,
                min_character_level INTEGER DEFAULT 1,
                max_character_level INTEGER DEFAULT 12,
                is_active INTEGER DEFAULT 1,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP
            )
        ";
        createBiomesTable.ExecuteNonQuery();

        // Table: Biome_RoomTemplates
        var createRoomTemplatesTable = connection.CreateCommand();
        createRoomTemplatesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Biome_RoomTemplates (
                template_id INTEGER PRIMARY KEY AUTOINCREMENT,
                biome_id INTEGER NOT NULL,
                template_name TEXT NOT NULL,
                template_description TEXT,
                room_size_category TEXT CHECK(room_size_category IN ('Small', 'Medium', 'Large', 'XLarge')),
                min_connections INTEGER DEFAULT 1,
                max_connections INTEGER DEFAULT 4,
                can_be_entrance INTEGER DEFAULT 0,
                can_be_exit INTEGER DEFAULT 0,
                can_be_hub INTEGER DEFAULT 0,
                hazard_density TEXT CHECK(hazard_density IN ('None', 'Low', 'Medium', 'High', 'Extreme')),
                enemy_spawn_weight INTEGER DEFAULT 100,
                resource_spawn_chance REAL DEFAULT 0.3,
                wfc_adjacency_rules TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
            )
        ";
        createRoomTemplatesTable.ExecuteNonQuery();

        // Table: Biome_EnvironmentalFeatures (structure only, content in v0.29.2)
        var createEnvFeaturesTable = connection.CreateCommand();
        createEnvFeaturesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Biome_EnvironmentalFeatures (
                feature_id INTEGER PRIMARY KEY AUTOINCREMENT,
                biome_id INTEGER NOT NULL,
                feature_name TEXT NOT NULL,
                feature_type TEXT CHECK(feature_type IN ('Hazard', 'Terrain', 'Ambient')),
                feature_description TEXT,
                damage_per_turn INTEGER DEFAULT 0,
                damage_type TEXT,
                tile_coverage_percent REAL DEFAULT 0,
                is_destructible INTEGER DEFAULT 0,
                blocks_movement INTEGER DEFAULT 0,
                blocks_line_of_sight INTEGER DEFAULT 0,
                hazard_density_category TEXT,
                special_rules TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
            )
        ";
        createEnvFeaturesTable.ExecuteNonQuery();

        // Table: Biome_EnemySpawns (structure only, content in v0.29.3)
        var createEnemySpawnsTable = connection.CreateCommand();
        createEnemySpawnsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Biome_EnemySpawns (
                spawn_id INTEGER PRIMARY KEY AUTOINCREMENT,
                biome_id INTEGER NOT NULL,
                enemy_name TEXT NOT NULL,
                enemy_type TEXT NOT NULL,
                spawn_weight INTEGER DEFAULT 100,
                min_level INTEGER DEFAULT 1,
                max_level INTEGER DEFAULT 12,
                spawn_rules_json TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
            )
        ";
        createEnemySpawnsTable.ExecuteNonQuery();

        // Table: Biome_ResourceDrops
        var createResourceDropsTable = connection.CreateCommand();
        createResourceDropsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Biome_ResourceDrops (
                resource_drop_id INTEGER PRIMARY KEY AUTOINCREMENT,
                biome_id INTEGER NOT NULL,
                resource_name TEXT NOT NULL,
                resource_description TEXT,
                resource_tier INTEGER CHECK(resource_tier >= 1 AND resource_tier <= 5),
                rarity TEXT CHECK(rarity IN ('Common', 'Uncommon', 'Rare', 'Epic', 'Legendary')),
                base_drop_chance REAL DEFAULT 0.1,
                min_quantity INTEGER DEFAULT 1,
                max_quantity INTEGER DEFAULT 1,
                requires_special_node INTEGER DEFAULT 0,
                weight INTEGER DEFAULT 100,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE
            )
        ";
        createResourceDropsTable.ExecuteNonQuery();

        // Table: Characters_BiomeStatus
        var createBiomeStatusTable = connection.CreateCommand();
        createBiomeStatusTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Characters_BiomeStatus (
                status_id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_id INTEGER NOT NULL,
                biome_id INTEGER NOT NULL,
                first_entry_timestamp TEXT,
                total_time_seconds INTEGER DEFAULT 0,
                rooms_explored INTEGER DEFAULT 0,
                enemies_defeated INTEGER DEFAULT 0,
                heat_damage_taken INTEGER DEFAULT 0,
                times_died_to_heat INTEGER DEFAULT 0,
                resources_collected INTEGER DEFAULT 0,
                has_reached_surtur INTEGER DEFAULT 0,
                last_updated TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
                FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE CASCADE,
                UNIQUE(character_id, biome_id)
            )
        ";
        createBiomeStatusTable.ExecuteNonQuery();

        // Create indices for performance
        var createIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_biomes_name ON Biomes(biome_name)",
            "CREATE INDEX IF NOT EXISTS idx_biomes_active ON Biomes(is_active)",
            "CREATE INDEX IF NOT EXISTS idx_biome_room_templates_biome ON Biome_RoomTemplates(biome_id)",
            "CREATE INDEX IF NOT EXISTS idx_biome_room_templates_size ON Biome_RoomTemplates(room_size_category)",
            "CREATE INDEX IF NOT EXISTS idx_biome_env_features_biome ON Biome_EnvironmentalFeatures(biome_id)",
            "CREATE INDEX IF NOT EXISTS idx_biome_env_features_type ON Biome_EnvironmentalFeatures(feature_type)",
            "CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_biome ON Biome_EnemySpawns(biome_id)",
            "CREATE INDEX IF NOT EXISTS idx_biome_enemy_spawns_type ON Biome_EnemySpawns(enemy_type)",
            "CREATE INDEX IF NOT EXISTS idx_biome_resources_biome ON Biome_ResourceDrops(biome_id)",
            "CREATE INDEX IF NOT EXISTS idx_biome_resources_tier ON Biome_ResourceDrops(resource_tier)",
            "CREATE INDEX IF NOT EXISTS idx_biome_resources_rarity ON Biome_ResourceDrops(rarity)",
            "CREATE INDEX IF NOT EXISTS idx_biome_status_character ON Characters_BiomeStatus(character_id)",
            "CREATE INDEX IF NOT EXISTS idx_biome_status_biome ON Characters_BiomeStatus(biome_id)"
        };

        foreach (var indexSql in createIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        // Seed Muspelheim biome data
        SeedMuspelheimBiomeData(connection);

        _log.Information("Muspelheim biome system tables created successfully");
    }

    /// <summary>
    /// v0.29.1: Seed Muspelheim biome data (biome entry, room templates, resources)
    /// </summary>
    private void SeedMuspelheimBiomeData(SqliteConnection connection)
    {
        _log.Debug("Seeding Muspelheim biome data");

        // Insert Muspelheim biome
        var insertBiome = connection.CreateCommand();
        insertBiome.CommandText = @"
            INSERT OR IGNORE INTO Biomes (
                biome_id, biome_name, biome_description, z_level_restriction,
                ambient_condition_id, min_character_level, max_character_level, is_active
            ) VALUES (
                4, 'Muspelheim',
                'Catastrophic geothermal failure zone where containment systems have collapsed and thermal regulators have liquefied. Industrial forges and magma-tap stations vent raw heat into the ruins.',
                '[Roots]', 1004, 7, 12, 1
            )
        ";
        insertBiome.ExecuteNonQuery();

        // Insert Room Templates
        var roomTemplates = new[]
        {
            ("Geothermal Control Chamber", "Octagonal command center with defunct thermal monitoring stations and shattered observation glass. Central control console radiates heat. Multiple exits lead to auxiliary systems.", "Large", 3, 5, 1, 0, 1, "Medium", 120, 0.5, "{\"allow\": [\"Lava Flow Corridor\", \"Equipment Bay\", \"Heat Exchanger Platform\"], \"forbid\": [\"Containment Breach Zone\"]}"),
            ("Lava Flow Corridor", "Narrow passage bisected by molten slag river. Catwalks provide precarious crossing points. Heat shimmer distorts vision at range.", "Small", 2, 2, 0, 0, 0, "High", 80, 0.1, "{\"allow\": [\"Geothermal Control Chamber\", \"Collapsed Forge Floor\", \"Emergency Coolant Junction\"], \"forbid\": [\"Containment Breach Zone\", \"Molten Slag Repository\"]}"),
            ("Collapsed Forge Floor", "Multi-tiered industrial platform partially collapsed into molten pit below. Structurally unstable catwalks and exposed rebar create vertical combat zones.", "Medium", 2, 3, 0, 0, 0, "Extreme", 150, 0.2, "{\"allow\": [\"Lava Flow Corridor\", \"Heat Exchanger Platform\"], \"forbid\": [\"Containment Breach Zone\"]}"),
            ("Scorched Equipment Bay", "Industrial storage chamber filled with heat-warped machinery and blackened supply crates. Residual thermal energy makes salvage dangerous but rewarding.", "Medium", 1, 3, 0, 0, 0, "Low", 60, 0.8, "{\"allow\": [\"Geothermal Control Chamber\", \"Emergency Coolant Junction\"], \"forbid\": [\"Molten Slag Repository\", \"Containment Breach Zone\"]}"),
            ("Molten Slag Repository", "Waste containment chamber overflowing with liquefied industrial byproducts. Islands of stable ground surrounded by glowing slag. Legendary materials solidify in cooler pockets.", "Large", 1, 2, 0, 0, 0, "Extreme", 40, 0.9, "{\"allow\": [\"Heat Exchanger Platform\"], \"forbid\": [\"Lava Flow Corridor\", \"Scorched Equipment Bay\", \"Geothermal Control Chamber\"]}"),
            ("Heat Exchanger Platform", "Massive vertical chamber with colossal heat exchange pipes venting superheated steam. Multi-level catwalks provide tactical positioning. Pressure release valves create dynamic hazards.", "XLarge", 2, 4, 0, 0, 0, "High", 110, 0.4, "{\"allow\": [\"Geothermal Control Chamber\", \"Collapsed Forge Floor\", \"Molten Slag Repository\"], \"forbid\": [\"Containment Breach Zone\"]}"),
            ("Containment Breach Zone", "Catastrophic failure site where primary containment vessel has ruptured. Radiation of extreme heat, molten metal geysers, and structural instability. The heart of the meltdown.", "XLarge", 1, 1, 0, 0, 0, "Extreme", 10, 0.0, "{\"allow\": [], \"forbid\": [\"Geothermal Control Chamber\", \"Lava Flow Corridor\", \"Scorched Equipment Bay\", \"Molten Slag Repository\", \"Emergency Coolant Junction\"]}"),
            ("Emergency Coolant Junction", "Crossroads of defunct coolant pipelines. Residual coolant vapor provides brief respite from heat. Chokepoint for tactical defense or ambush.", "Small", 2, 4, 0, 1, 0, "Low", 90, 0.3, "{\"allow\": [\"Lava Flow Corridor\", \"Scorched Equipment Bay\"], \"forbid\": [\"Containment Breach Zone\", \"Molten Slag Repository\"]}")
        };

        foreach (var template in roomTemplates)
        {
            var insertTemplate = connection.CreateCommand();
            insertTemplate.CommandText = @"
                INSERT OR IGNORE INTO Biome_RoomTemplates (
                    biome_id, template_name, template_description, room_size_category,
                    min_connections, max_connections, can_be_entrance, can_be_exit, can_be_hub,
                    hazard_density, enemy_spawn_weight, resource_spawn_chance, wfc_adjacency_rules
                ) VALUES (4, $name, $desc, $size, $minConn, $maxConn, $entrance, $exit, $hub, $hazard, $enemyWeight, $resourceChance, $adjacency)
            ";
            insertTemplate.Parameters.AddWithValue("$name", template.Item1);
            insertTemplate.Parameters.AddWithValue("$desc", template.Item2);
            insertTemplate.Parameters.AddWithValue("$size", template.Item3);
            insertTemplate.Parameters.AddWithValue("$minConn", template.Item4);
            insertTemplate.Parameters.AddWithValue("$maxConn", template.Item5);
            insertTemplate.Parameters.AddWithValue("$entrance", template.Item6);
            insertTemplate.Parameters.AddWithValue("$exit", template.Item7);
            insertTemplate.Parameters.AddWithValue("$hub", template.Item8);
            insertTemplate.Parameters.AddWithValue("$hazard", template.Item9);
            insertTemplate.Parameters.AddWithValue("$enemyWeight", template.Item10);
            insertTemplate.Parameters.AddWithValue("$resourceChance", template.Item11);
            insertTemplate.Parameters.AddWithValue("$adjacency", template.Item12);
            insertTemplate.ExecuteNonQuery();
        }

        // Insert Resource Drops
        var resources = new[]
        {
            // Tier 3
            ("Star-Metal Ore", "Heat-forged metallic ore with unusual crystalline structure. Used in high-temperature weapon and armor crafting.", 3, "Uncommon", 0.25, 1, 3, 0, 120),
            ("Obsidian Shards", "Volcanic glass fragments formed from rapidly cooled slag. Razor-sharp edges ideal for cutting tools and projectiles.", 3, "Common", 0.35, 2, 5, 0, 150),
            ("Hardened Servomotors", "Heat-resistant mechanical components from defunct industrial systems. Useful for equipment maintenance and advanced crafting.", 3, "Uncommon", 0.20, 1, 2, 1, 80),
            // Tier 4
            ("Heart of the Inferno", "Runic catalyst supercharged by extreme heat exposure. Glows with inner fire. Highly sought for aetheric weaving.", 4, "Rare", 0.08, 1, 1, 0, 40),
            ("Molten Core Fragment", "Superheated core sample from failed containment system. Radiates constant thermal energy. Handle with ablative gloves.", 4, "Rare", 0.12, 1, 1, 1, 50),
            ("Thermal Regulator Component", "Intact component from pre-Glitch thermal management system. Rare find. Used in advanced environmental protection gear.", 4, "Rare", 0.10, 1, 1, 1, 35),
            // Tier 5
            ("Surtur Engine Core", "Legendary power core from Jotun-Forged warmachine. Pulsates with residual energy. Centerpiece for masterwork crafting.", 5, "Legendary", 0.05, 1, 1, 0, 5),
            ("Eternal Ember", "Self-sustaining thermal anomaly contained in crystalline matrix. Never cools. Source unknown. Priceless to artificers.", 5, "Legendary", 0.02, 1, 1, 1, 3),
            ("Ablative Plating Schematic", "Intact technical document detailing pre-Glitch heat shielding technology. Enables crafting of superior Fire Resistance armor.", 5, "Epic", 0.03, 1, 1, 1, 8)
        };

        foreach (var resource in resources)
        {
            var insertResource = connection.CreateCommand();
            insertResource.CommandText = @"
                INSERT OR IGNORE INTO Biome_ResourceDrops (
                    biome_id, resource_name, resource_description, resource_tier, rarity,
                    base_drop_chance, min_quantity, max_quantity, requires_special_node, weight
                ) VALUES (4, $name, $desc, $tier, $rarity, $dropChance, $minQty, $maxQty, $specialNode, $weight)
            ";
            insertResource.Parameters.AddWithValue("$name", resource.Item1);
            insertResource.Parameters.AddWithValue("$desc", resource.Item2);
            insertResource.Parameters.AddWithValue("$tier", resource.Item3);
            insertResource.Parameters.AddWithValue("$rarity", resource.Item4);
            insertResource.Parameters.AddWithValue("$dropChance", resource.Item5);
            insertResource.Parameters.AddWithValue("$minQty", resource.Item6);
            insertResource.Parameters.AddWithValue("$maxQty", resource.Item7);
            insertResource.Parameters.AddWithValue("$specialNode", resource.Item8);
            insertResource.Parameters.AddWithValue("$weight", resource.Item9);
            insertResource.ExecuteNonQuery();
        }

        // Insert Environmental Hazards
        var hazards = new[]
        {
            // Hazard 1: Burning Ground
            ("[Burning Ground]", "Terrain", "Flames or superheated metal. Deals Fire damage each turn to those standing on it.", 8, "Fire", 15, 0, 0, 0, "Medium", "persistent_fire"),
            // Hazard 2: Chasm/Lava River
            ("[Chasm/Lava River]", "Obstacle", "Impassable molten slag river. Instant death if pushed/moved into. Controllers dream of this.", 999, "Fire", 10, 0, 1, 0, "High", "instant_death_on_enter"),
            // Hazard 3: High-Pressure Steam Vent
            ("[High-Pressure Steam Vent]", "Dynamic", "Pressure valve venting superheated steam. Deals burst damage + Disoriented. Destructible via Environmental Combat.", 16, "Fire", 5, 1, 0, 1, "High", "applies_disoriented,destructible"),
            // Hazard 4: Volatile Gas Pocket
            ("[Volatile Gas Pocket]", "Explosive", "Combustible gas pocket. Explodes when Fire damage dealt nearby (3-tile radius), causing 4d6 AoE Fire damage.", 24, "Fire", 3, 1, 0, 0, "Extreme", "chain_reaction,aoe_radius_3"),
            // Hazard 5: Scorched Metal Plating
            ("[Scorched Metal Plating]", "Terrain", "Heat-warped structural plating. Movement cost doubled (10 ft becomes 5 ft effective). No damage.", 0, null, 20, 0, 0, 0, "Low", "movement_cost_doubled"),
            // Hazard 6: Molten Slag Pool
            ("[Molten Slag Pool]", "Terrain", "Shallow pool of cooling slag. Deals Fire damage and applies [Slowed] (-50% movement speed).", 4, "Fire", 8, 0, 0, 0, "Medium", "applies_slowed"),
            // Hazard 7: Collapsing Catwalk
            ("[Collapsing Catwalk]", "Dynamic", "Unstable walkway. 20% chance per turn of collapse if occupied. Combatant falls to [Chasm] below.", 999, "Physical", 5, 0, 0, 0, "Extreme", "collapse_chance_20,fall_to_chasm"),
            // Hazard 8: Thermal Mirage
            ("[Thermal Mirage]", "Vision", "Heat shimmer distorts vision. Ranged attacks through affected tiles suffer -2 penalty.", 0, null, 25, 0, 0, 0, "Low", "ranged_attack_penalty_2")
        };

        foreach (var hazard in hazards)
        {
            var insertHazard = connection.CreateCommand();
            insertHazard.CommandText = @"
                INSERT OR IGNORE INTO Biome_EnvironmentalFeatures (
                    biome_id, feature_name, feature_type, feature_description,
                    damage_per_turn, damage_type, tile_coverage_percent,
                    is_destructible, blocks_movement, blocks_line_of_sight,
                    hazard_density_category, special_rules
                ) VALUES (4, $name, $type, $desc, $damage, $damageType, $coverage, $destructible, $blocksMove, $blocksLOS, $density, $rules)
            ";
            insertHazard.Parameters.AddWithValue("$name", hazard.Item1);
            insertHazard.Parameters.AddWithValue("$type", hazard.Item2);
            insertHazard.Parameters.AddWithValue("$desc", hazard.Item3);
            insertHazard.Parameters.AddWithValue("$damage", hazard.Item4);
            insertHazard.Parameters.AddWithValue("$damageType", hazard.Item5 ?? (object)DBNull.Value);
            insertHazard.Parameters.AddWithValue("$coverage", hazard.Item6);
            insertHazard.Parameters.AddWithValue("$destructible", hazard.Item7);
            insertHazard.Parameters.AddWithValue("$blocksMove", hazard.Item8);
            insertHazard.Parameters.AddWithValue("$blocksLOS", hazard.Item9);
            insertHazard.Parameters.AddWithValue("$density", hazard.Item10);
            insertHazard.Parameters.AddWithValue("$rules", hazard.Item11);
            insertHazard.ExecuteNonQuery();
        }

        // Insert Enemy Spawns (v0.29.3)
        var enemySpawns = new[]
        {
            // Enemy 1: Forge-Hardened Undying
            ("Forge-Hardened Undying", "Undying", 7, 9, 150, "{\"fire_resistance\": 75, \"ice_resistance\": -50, \"tags\": [\"brittle_on_ice\", \"heat_immune\"]}"),
            // Enemy 2: Magma Elemental
            ("Magma Elemental", "Construct", 8, 11, 80, "{\"fire_resistance\": 100, \"ice_resistance\": -30, \"physical_resistance\": 25, \"tags\": [\"burning_trail\", \"death_explosion\", \"brittle_on_ice\", \"heat_immune\", \"construct\"]}"),
            // Enemy 3: Rival Berserker
            ("Rival Berserker", "Humanoid", 9, 12, 60, "{\"fire_resistance\": 50, \"ice_resistance\": -25, \"tags\": [\"fury_resource\", \"brittle_on_ice\", \"heat_adapted\"]}"),
            // Enemy 4: Surtur's Herald (Boss)
            ("Surtur's Herald", "Boss", 12, 12, 1, "{\"fire_resistance\": 90, \"ice_resistance\": -40, \"physical_resistance\": 50, \"lightning_resistance\": 50, \"tags\": [\"boss\", \"legendary_resistances\", \"brittle_on_ice\", \"heat_immune\", \"multi_phase\"]}"),
            // Enemy 5: Iron-Bane Crusader
            ("Iron-Bane Crusader", "Humanoid", 10, 12, 20, "{\"fire_resistance\": 60, \"ice_resistance\": 0, \"corruption_resistance\": 75, \"tags\": [\"construct_hunter\", \"brittle_on_ice\", \"heat_adapted\", \"potential_ally\"]}")
        };

        foreach (var enemy in enemySpawns)
        {
            var insertEnemy = connection.CreateCommand();
            insertEnemy.CommandText = @"
                INSERT OR IGNORE INTO Biome_EnemySpawns (
                    biome_id, enemy_name, enemy_type,
                    min_level, max_level, spawn_weight, spawn_rules_json
                ) VALUES (4, $name, $type, $minLevel, $maxLevel, $spawnWeight, $rules)
            ";
            insertEnemy.Parameters.AddWithValue("$name", enemy.Item1);
            insertEnemy.Parameters.AddWithValue("$type", enemy.Item2);
            insertEnemy.Parameters.AddWithValue("$minLevel", enemy.Item3);
            insertEnemy.Parameters.AddWithValue("$maxLevel", enemy.Item4);
            insertEnemy.Parameters.AddWithValue("$spawnWeight", enemy.Item5);
            insertEnemy.Parameters.AddWithValue("$rules", enemy.Item6);
            insertEnemy.ExecuteNonQuery();
        }

        _log.Information("Muspelheim biome data seeded: 1 biome, 8 room templates, 9 resources, 8 environmental hazards, 5 enemy types");
    }

    /// <summary>
    /// v0.30.1: Create Niflheim biome tables (tables already created by Muspelheim, just seed data)
    /// </summary>
    private void CreateNiflheimBiomeTables(SqliteConnection connection)
    {
        _log.Debug("Creating Niflheim biome system (seeding data)");

        // Tables already created by CreateMuspelheimBiomeTables
        // Just seed Niflheim-specific data
        SeedNiflheimBiomeData(connection);

        _log.Information("Niflheim biome system created successfully");
    }

    /// <summary>
    /// v0.30.1-v0.30.3: Seed Niflheim biome data
    /// NOTE: For full data, execute SQL files: v0.30.1, v0.30.2, v0.30.3
    /// This is a minimal seeding for database initialization
    /// </summary>
    private void SeedNiflheimBiomeData(SqliteConnection connection)
    {
        _log.Debug("Seeding Niflheim biome data (minimal seed)");

        // v0.30.1: Insert Niflheim biome
        var insertBiome = connection.CreateCommand();
        insertBiome.CommandText = @"
            INSERT OR IGNORE INTO Biomes (
                biome_id, biome_name, biome_description, z_level_restriction,
                ambient_condition_id, min_character_level, max_character_level, is_active
            ) VALUES (
                5, 'Niflheim',
                'The Cryo-Facilities - catastrophic coolant system failure has transformed this zone into an absolute zero wasteland. Ruptured cryogenic containment has flash-frozen entire chambers, preserving machinery and victims alike in crystalline stasis.',
                '[Roots,Canopy]', NULL, 7, 12, 1
            )
        ";
        insertBiome.ExecuteNonQuery();

        // NOTE: Full database seeding (room templates, hazards, enemies, resources)
        // should be done by executing the following SQL files:
        // - Data/v0.30.1_niflheim_schema.sql (8 room templates, 9 resources)
        // - Data/v0.30.2_environmental_hazards.sql (9 hazards, 2 conditions)
        // - Data/v0.30.3_enemy_definitions.sql (5 enemy types, 7 spawn variants)
        //
        // To execute manually:
        //   sqlite3 runeandrust.db < Data/v0.30.1_niflheim_schema.sql
        //   sqlite3 runeandrust.db < Data/v0.30.2_environmental_hazards.sql
        //   sqlite3 runeandrust.db < Data/v0.30.3_environmental_hazards.sql
        //
        // Or integrate full seeding here following the Muspelheim pattern above.

        _log.Information("Niflheim biome minimal seed complete: 1 biome entry created");
        _log.Warning("For full Niflheim content, execute SQL files: v0.30.1, v0.30.2, v0.30.3");
    }

    /// <summary>
    /// v0.33.1: Create Faction System tables and seed base faction definitions
    /// </summary>
    private void CreateFactionTables(SqliteConnection connection)
    {
        _log.Debug("Creating Faction System tables");

        // Table: Factions
        var createFactionsTable = connection.CreateCommand();
        createFactionsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Factions (
                faction_id INTEGER PRIMARY KEY,
                faction_name TEXT NOT NULL UNIQUE,
                display_name TEXT NOT NULL,
                philosophy TEXT,
                description TEXT,
                primary_location TEXT,
                allied_factions TEXT,
                enemy_factions TEXT,
                created_at TEXT DEFAULT CURRENT_TIMESTAMP
            )
        ";
        createFactionsTable.ExecuteNonQuery();

        // Table: Characters_FactionReputations
        var createReputationsTable = connection.CreateCommand();
        createReputationsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Characters_FactionReputations (
                reputation_id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_id INTEGER NOT NULL,
                faction_id INTEGER NOT NULL,
                reputation_value INTEGER DEFAULT 0 CHECK(reputation_value BETWEEN -100 AND 100),
                reputation_tier TEXT CHECK(reputation_tier IN ('Hated', 'Hostile', 'Neutral', 'Friendly', 'Allied', 'Exalted')),
                last_modified TEXT DEFAULT CURRENT_TIMESTAMP,
                FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
                FOREIGN KEY (faction_id) REFERENCES Factions(faction_id),
                UNIQUE(character_id, faction_id)
            )
        ";
        createReputationsTable.ExecuteNonQuery();

        // Table: Faction_Quests
        var createFactionQuestsTable = connection.CreateCommand();
        createFactionQuestsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Faction_Quests (
                faction_quest_id INTEGER PRIMARY KEY AUTOINCREMENT,
                quest_id TEXT NOT NULL,
                faction_id INTEGER NOT NULL,
                required_reputation INTEGER DEFAULT 0,
                reputation_reward INTEGER DEFAULT 0,
                is_repeatable INTEGER DEFAULT 0,
                FOREIGN KEY (faction_id) REFERENCES Factions(faction_id)
            )
        ";
        createFactionQuestsTable.ExecuteNonQuery();

        // Table: Faction_Rewards
        var createFactionRewardsTable = connection.CreateCommand();
        createFactionRewardsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Faction_Rewards (
                reward_id INTEGER PRIMARY KEY AUTOINCREMENT,
                faction_id INTEGER NOT NULL,
                reward_type TEXT CHECK(reward_type IN ('Equipment', 'Consumable', 'Service', 'Ability', 'Discount')),
                reward_name TEXT NOT NULL,
                reward_description TEXT,
                required_reputation INTEGER DEFAULT 0,
                reward_data TEXT,
                FOREIGN KEY (faction_id) REFERENCES Factions(faction_id)
            )
        ";
        createFactionRewardsTable.ExecuteNonQuery();

        // Create indices for performance
        var createIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_factions_name ON Factions(faction_name)",
            "CREATE INDEX IF NOT EXISTS idx_char_faction_rep_character ON Characters_FactionReputations(character_id)",
            "CREATE INDEX IF NOT EXISTS idx_char_faction_rep_faction ON Characters_FactionReputations(faction_id)",
            "CREATE INDEX IF NOT EXISTS idx_char_faction_rep_tier ON Characters_FactionReputations(reputation_tier)",
            "CREATE INDEX IF NOT EXISTS idx_faction_quests_faction ON Faction_Quests(faction_id)",
            "CREATE INDEX IF NOT EXISTS idx_faction_quests_quest_id ON Faction_Quests(quest_id)",
            "CREATE INDEX IF NOT EXISTS idx_faction_quests_rep_req ON Faction_Quests(required_reputation)",
            "CREATE INDEX IF NOT EXISTS idx_faction_rewards_faction ON Faction_Rewards(faction_id)",
            "CREATE INDEX IF NOT EXISTS idx_faction_rewards_type ON Faction_Rewards(reward_type)",
            "CREATE INDEX IF NOT EXISTS idx_faction_rewards_rep_req ON Faction_Rewards(required_reputation)"
        };

        foreach (var indexSql in createIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        // Seed base faction definitions
        SeedFactionData(connection);

        _log.Information("Faction System tables created successfully");
    }

    /// <summary>
    /// v0.33.1: Seed the 5 major faction definitions
    /// </summary>
    private void SeedFactionData(SqliteConnection connection)
    {
        _log.Debug("Seeding faction data");

        var factionSeeds = new[]
        {
            // Faction 1: Iron-Banes
            @"INSERT OR IGNORE INTO Factions (faction_id, faction_name, display_name, philosophy, description, primary_location, allied_factions, enemy_factions)
              VALUES (1, 'IronBanes', 'Iron-Banes',
                'The Undying are corrupted processes that must be purged. Every autonomous construct following 800-year-old protocols is a threat to coherent reality. We follow purification protocols to restore system integrity.',
                'Anti-Undying specialists who hunt corrupted constructs and prevent Runic Blight spread. Not religious zealots, but methodical anti-corruption technicians following purification protocols developed after the Glitch.',
                'Trunk/Roots/Muspelheim', 'RustClans', 'GodSleeperCultists')",

            // Faction 2: God-Sleeper Cultists
            @"INSERT OR IGNORE INTO Factions (faction_id, faction_name, display_name, philosophy, description, primary_location, allied_factions, enemy_factions)
              VALUES (2, 'GodSleeperCultists', 'God-Sleeper Cultists',
                'The Jötun-Forged are sleeping gods awaiting the signal to awaken. Their dormancy is sacred. We are the caretakers, the faithful, the ones who will be there when they rise. Do not harm the sleepers.',
                'Cargo cultists who interpret Jötun logic core broadcasts as divine messages. They protect dormant Jötun-Forged and establish temples in Jötunheim. Their faith is a misinterpretation of corrupted psychic broadcasts.',
                'Jotunheim', 'Independents', 'IronBanes')",

            // Faction 3: Jötun-Readers
            @"INSERT OR IGNORE INTO Factions (faction_id, faction_name, display_name, philosophy, description, primary_location, allied_factions, enemy_factions)
              VALUES (3, 'JotunReaders', 'Jötun-Readers',
                'Knowledge is the only path to understanding the Glitch. Every corrupted log, every fragmented database, every Jötun logic core—these are the keys to comprehension. We preserve, we study, we learn.',
                'Data archaeologists and system analysts dedicated to recovering Pre-Glitch knowledge. They study corrupted systems to understand the Great Silence and archive all recovered data. Knowledge is their highest value.',
                'Alfheim', 'RustClans', '')",

            // Faction 4: Rust-Clans
            @"INSERT OR IGNORE INTO Factions (faction_id, faction_name, display_name, philosophy, description, primary_location, allied_factions, enemy_factions)
              VALUES (4, 'RustClans', 'Rust-Clans',
                'Survival first. No ideology, no worship, no grand theories. We scavenge, we trade, we defend our territory. The world crashed—we''re still here. That''s what matters.',
                'Pragmatic Midgard survivors focused on resource acquisition and trade networks. They cooperate with both Iron-Banes and Jötun-Readers when beneficial, prioritizing practical survival over ideology.',
                'Midgard', 'IronBanes,JotunReaders', '')",

            // Faction 5: Independents
            @"INSERT OR IGNORE INTO Factions (faction_id, faction_name, display_name, philosophy, description, primary_location, allied_factions, enemy_factions)
              VALUES (5, 'Independents', 'Independents',
                'Factions are chains. We walk our own path.',
                'Unaffiliated individuals who reject faction membership. They maintain neutrality in faction conflicts and value personal freedom over collective identity. Gaining reputation with Independents requires actively declining other faction offers.',
                'All', '', '')"
        };

        foreach (var seedSql in factionSeeds)
        {
            var seedCommand = connection.CreateCommand();
            seedCommand.CommandText = seedSql;
            seedCommand.ExecuteNonQuery();
        }

        _log.Information("Faction data seeded: 5 factions created");
        _log.Information("For faction quests and rewards, execute SQL file: Data/v0.33.1_faction_schema.sql");
    }

    /// <summary>
    /// v0.34.1: Create Companion System tables
    /// </summary>
    private void CreateCompanionTables(SqliteConnection connection)
    {
        _log.Debug("Creating Companion System tables");

        // Table: Companions
        var createCompanionsTable = connection.CreateCommand();
        createCompanionsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Companions (
                companion_id INTEGER PRIMARY KEY,
                companion_name TEXT NOT NULL UNIQUE,
                display_name TEXT NOT NULL,

                archetype TEXT NOT NULL CHECK(archetype IN ('Warrior', 'Adept', 'Mystic')),
                faction_affiliation TEXT,
                background_summary TEXT NOT NULL,
                personality_traits TEXT NOT NULL,

                recruitment_location TEXT NOT NULL,
                required_faction TEXT,
                required_reputation_tier TEXT,
                required_reputation_value INTEGER,
                recruitment_quest_id INTEGER,

                base_might INTEGER NOT NULL DEFAULT 10,
                base_finesse INTEGER NOT NULL DEFAULT 10,
                base_sturdiness INTEGER NOT NULL DEFAULT 10,
                base_wits INTEGER NOT NULL DEFAULT 10,
                base_will INTEGER NOT NULL DEFAULT 10,

                base_max_hp INTEGER NOT NULL DEFAULT 30,
                base_defense INTEGER NOT NULL DEFAULT 10,
                base_soak INTEGER NOT NULL DEFAULT 0,

                resource_type TEXT NOT NULL CHECK(resource_type IN ('Stamina', 'Aether Pool')),
                base_max_resource INTEGER NOT NULL DEFAULT 100,

                combat_role TEXT NOT NULL,
                default_stance TEXT NOT NULL DEFAULT 'aggressive' CHECK(default_stance IN ('aggressive', 'defensive', 'passive')),

                starting_abilities TEXT NOT NULL,

                personal_quest_id INTEGER,
                personal_quest_title TEXT,

                created_at TEXT DEFAULT CURRENT_TIMESTAMP
            )
        ";
        createCompanionsTable.ExecuteNonQuery();

        // Table: Characters_Companions
        var createCharactersCompanionsTable = connection.CreateCommand();
        createCharactersCompanionsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Characters_Companions (
                character_companion_id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_id INTEGER NOT NULL,
                companion_id INTEGER NOT NULL,

                is_recruited INTEGER NOT NULL DEFAULT 0,
                recruited_at TEXT,
                is_in_party INTEGER NOT NULL DEFAULT 0,

                current_hp INTEGER NOT NULL,
                current_resource INTEGER NOT NULL,
                is_incapacitated INTEGER NOT NULL DEFAULT 0,

                current_stance TEXT NOT NULL DEFAULT 'aggressive' CHECK(current_stance IN ('aggressive', 'defensive', 'passive')),

                created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                updated_at TEXT DEFAULT CURRENT_TIMESTAMP,

                FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
                FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
                UNIQUE(character_id, companion_id)
            )
        ";
        createCharactersCompanionsTable.ExecuteNonQuery();

        // Table: Companion_Progression
        var createCompanionProgressionTable = connection.CreateCommand();
        createCompanionProgressionTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Companion_Progression (
                progression_id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_id INTEGER NOT NULL,
                companion_id INTEGER NOT NULL,

                current_level INTEGER NOT NULL DEFAULT 1,
                current_legend INTEGER NOT NULL DEFAULT 0,
                legend_to_next_level INTEGER NOT NULL DEFAULT 100,

                override_might INTEGER,
                override_finesse INTEGER,
                override_sturdiness INTEGER,
                override_wits INTEGER,
                override_will INTEGER,

                override_max_hp INTEGER,
                override_defense INTEGER,
                override_soak INTEGER,

                equipped_weapon_id INTEGER,
                equipped_armor_id INTEGER,
                equipped_accessory_id INTEGER,

                unlocked_abilities TEXT NOT NULL DEFAULT '[]',

                updated_at TEXT DEFAULT CURRENT_TIMESTAMP,

                FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
                FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
                UNIQUE(character_id, companion_id)
            )
        ";
        createCompanionProgressionTable.ExecuteNonQuery();

        // Table: Companion_Quests
        var createCompanionQuestsTable = connection.CreateCommand();
        createCompanionQuestsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Companion_Quests (
                companion_quest_id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_id INTEGER NOT NULL,
                companion_id INTEGER NOT NULL,
                quest_id INTEGER NOT NULL,

                is_unlocked INTEGER NOT NULL DEFAULT 0,
                is_started INTEGER NOT NULL DEFAULT 0,
                is_completed INTEGER NOT NULL DEFAULT 0,

                loyalty_reward INTEGER DEFAULT 0,

                unlocked_at TEXT,
                started_at TEXT,
                completed_at TEXT,

                FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
                FOREIGN KEY (companion_id) REFERENCES Companions(companion_id),
                UNIQUE(character_id, companion_id, quest_id)
            )
        ";
        createCompanionQuestsTable.ExecuteNonQuery();

        // Table: Companion_Abilities (separate from player Abilities table)
        var createCompanionAbilitiesTable = connection.CreateCommand();
        createCompanionAbilitiesTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Companion_Abilities (
                ability_id INTEGER PRIMARY KEY,
                ability_name TEXT NOT NULL,
                owner TEXT NOT NULL,
                description TEXT NOT NULL,
                resource_cost_type TEXT,
                resource_cost INTEGER DEFAULT 0,
                duration_turns INTEGER DEFAULT 0,
                range_tiles INTEGER DEFAULT 0,
                target_type TEXT NOT NULL,
                range_type TEXT NOT NULL,
                damage_type TEXT,
                special_effects TEXT,
                conditions TEXT,
                ability_category TEXT DEFAULT 'companion_ability',
                created_at TEXT DEFAULT CURRENT_TIMESTAMP
            )
        ";
        createCompanionAbilitiesTable.ExecuteNonQuery();

        // Create indices for performance
        var createIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_companions_faction ON Companions(required_faction)",
            "CREATE INDEX IF NOT EXISTS idx_companions_location ON Companions(recruitment_location)",
            "CREATE INDEX IF NOT EXISTS idx_char_companions_character ON Characters_Companions(character_id)",
            "CREATE INDEX IF NOT EXISTS idx_char_companions_party ON Characters_Companions(character_id, is_in_party)",
            "CREATE INDEX IF NOT EXISTS idx_companion_prog_character ON Companion_Progression(character_id)",
            "CREATE INDEX IF NOT EXISTS idx_companion_quests_char ON Companion_Quests(character_id)",
            "CREATE INDEX IF NOT EXISTS idx_companion_quests_companion ON Companion_Quests(companion_id)",
            "CREATE INDEX IF NOT EXISTS idx_companion_abilities_owner ON Companion_Abilities(owner)"
        };

        foreach (var indexSql in createIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        // Seed base companion definitions and abilities
        SeedCompanionData(connection);

        _log.Information("Companion System tables created successfully");
    }

    /// <summary>
    /// v0.34.1: Seed the 6 recruitable companions and their 18 abilities
    /// </summary>
    private void SeedCompanionData(SqliteConnection connection)
    {
        _log.Debug("Seeding companion data");

        // Seed 6 companions
        var companionSeeds = new[]
        {
            // Companion 1: Kára Ironbreaker (Iron-Bane Tank)
            @"INSERT OR IGNORE INTO Companions VALUES (
                34001, 'Kara_Ironbreaker', 'Kára Ironbreaker',
                'Warrior', 'Iron-Bane',
                'Former security protocol enforcer turned Undying hunter. Lost her entire squad to a Draugr Juggernaut ambush. Methodical, disciplined, carries survivor''s guilt. Sees every Undying destroyed as atonement.',
                'Stoic, duty-driven, distrusts magic',
                'Trunk (Iron-Bane Enclave)', 'Iron-Bane', 'Friendly', 25, NULL,
                14, 10, 15, 9, 8,
                45, 12, 3,
                'Stamina', 120,
                'Tank', 'defensive',
                '[34101, 34102, 34103]',
                NULL, 'The Last Protocol',
                CURRENT_TIMESTAMP
            )",

            // Companion 2: Finnr the Rust-Sage (Jötun-Reader Support)
            @"INSERT OR IGNORE INTO Companions VALUES (
                34002, 'Finnr_Rust_Sage', 'Finnr the Rust-Sage',
                'Mystic', 'Jotun-Reader',
                'Scholar obsessed with Pre-Glitch knowledge. Believes understanding the Glitch is the only path forward. Socially awkward, brilliant, dangerously curious about forbidden data.',
                'Curious, verbose, oblivious to danger',
                'Alfheim Archives', 'Jotun-Reader', 'Friendly', 25, NULL,
                8, 10, 9, 15, 14,
                28, 10, 1,
                'Aether Pool', 150,
                'Support', 'defensive',
                '[34201, 34202, 34203]',
                NULL, 'The Forlorn Archive',
                CURRENT_TIMESTAMP
            )",

            // Companion 3: Bjorn Scrap-Hand (Rust-Clan Utility)
            @"INSERT OR IGNORE INTO Companions VALUES (
                34003, 'Bjorn_Scrap_Hand', 'Bjorn Scrap-Hand',
                'Adept', 'Rust-Clan',
                'Pragmatic scavenger who survived by being useful. Can fix anything, build weapons from scrap, knows where to find resources. No ideology—just survival.',
                'Practical, cynical, surprisingly loyal',
                'Midgard Trade Outpost', 'Rust-Clan', 'Neutral', 0, NULL,
                11, 12, 12, 14, 9,
                35, 11, 2,
                'Stamina', 110,
                'Utility', 'aggressive',
                '[34301, 34302, 34303]',
                NULL, 'The Old Workshop',
                CURRENT_TIMESTAMP
            )",

            // Companion 4: Valdis the Forlorn-Touched (Independent Glass Cannon)
            @"INSERT OR IGNORE INTO Companions VALUES (
                34004, 'Valdis_Forlorn_Touched', 'Valdis the Forlorn-Touched',
                'Mystic', 'Independent',
                'Seidkona who communes with Forlorn too deeply. Hears voices, sees ghosts, balances on edge of Breaking. Powerful but unstable—high-risk, high-reward companion.',
                'Haunted, prophetic, unpredictable',
                'Niflheim Frozen Ruins', NULL, NULL, NULL, NULL,
                7, 9, 7, 12, 16,
                24, 9, 0,
                'Aether Pool', 180,
                'DPS', 'aggressive',
                '[34401, 34402, 34403]',
                NULL, 'Breaking the Voices',
                CURRENT_TIMESTAMP
            )",

            // Companion 5: Runa Shield-Sister (Independent Tank)
            @"INSERT OR IGNORE INTO Companions VALUES (
                34005, 'Runa_Shield_Sister', 'Runa Shield-Sister',
                'Warrior', 'Independent',
                'Mercenary guard who protects those weaker than herself. No faction loyalties—only a personal code. Straightforward, protective, suspicious of authority.',
                'Honorable, protective, independent',
                'Jotunheim Assembly Yards', NULL, NULL, NULL, NULL,
                13, 11, 16, 10, 9,
                50, 13, 4,
                'Stamina', 130,
                'Tank', 'defensive',
                '[34501, 34502, 34503]',
                NULL, 'The Broken Oath',
                CURRENT_TIMESTAMP
            )",

            // Companion 6: Einar the God-Touched (God-Sleeper DPS)
            @"INSERT OR IGNORE INTO Companions VALUES (
                34006, 'Einar_God_Touched', 'Einar the God-Touched',
                'Warrior', 'God-Sleeper',
                'Cargo cultist who believes Jötun-Forged are sleeping gods. Zealous, powerful near Jötun corpses, sees your journey as divine will. Dangerous fanaticism mixed with genuine combat skill.',
                'Zealous, charismatic, sees omens everywhere',
                'Jotunheim Temple (Einherjar Torso-Cave)', 'God-Sleeper', 'Friendly', 25, NULL,
                16, 10, 13, 8, 11,
                42, 11, 2,
                'Stamina', 140,
                'DPS', 'aggressive',
                '[34601, 34602, 34603]',
                NULL, 'Awaken the Sleeper',
                CURRENT_TIMESTAMP
            )"
        };

        foreach (var seedSql in companionSeeds)
        {
            var seedCommand = connection.CreateCommand();
            seedCommand.CommandText = seedSql;
            seedCommand.ExecuteNonQuery();
        }

        // Seed 18 companion abilities (3 per companion)
        var abilitySeeds = new[]
        {
            // Kára Ironbreaker Abilities
            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34101, 'Shield Bash', 'Kara_Ironbreaker',
                'Bash target with shield, dealing 1d8 + MIGHT damage and stunning for 1 turn.',
                'Stamina', 5, 1, 1, 'single_target',
                'melee', 'Physical', NULL, 'Stun: 1 turn',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34102, 'Taunt', 'Kara_Ironbreaker',
                'Force target enemy to attack Kára for 2 turns.',
                'Stamina', 10, 1, 2, 'single_target',
                'ranged', NULL, NULL, 'Taunt: 2 turns',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34103, 'Purification Strike', 'Kara_Ironbreaker',
                'Powerful strike dealing 2d6 + MIGHT damage. +2d6 bonus damage vs. Undying.',
                'Stamina', 15, 2, 1, 'single_target',
                'melee', 'Physical', '+2d6 vs. Undying', NULL,
                'companion_ability', CURRENT_TIMESTAMP
            )",

            // Finnr Rust-Sage Abilities
            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34201, 'Aetheric Bolt', 'Finnr_Rust_Sage',
                'Fire bolt of Aetheric energy dealing 2d6 + WILL damage at range.',
                'Aether Pool', 20, 2, 6, 'single_target',
                'ranged', 'Magic', NULL, NULL,
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34202, 'Data Analysis', 'Finnr_Rust_Sage',
                'Analyze target enemy, revealing weaknesses. Allies gain +2 to hit and +1d6 damage vs. that enemy for 3 turns.',
                'Aether Pool', 30, 3, 6, 'single_target',
                'ranged', NULL, NULL, 'Weakness Revealed: 3 turns',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34203, 'Runic Shield', 'Finnr_Rust_Sage',
                'Grant ally +3 Defense for 3 turns.',
                'Aether Pool', 25, 3, 6, 'single_target',
                'ranged', NULL, NULL, 'Defense +3: 3 turns',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            // Bjorn Scrap-Hand Abilities
            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34301, 'Improvised Repair', 'Bjorn_Scrap_Hand',
                'Use scrap materials to heal ally for 2d8 HP.',
                'Stamina', 20, 2, 3, 'single_target',
                'ranged', 'Healing', NULL, NULL,
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34302, 'Scrap Grenade', 'Bjorn_Scrap_Hand',
                'Throw improvised explosive dealing 2d6 damage to all enemies in 2x2 area.',
                'Stamina', 25, 3, 5, 'area_2x2',
                'ranged', 'Physical', 'AOE', NULL,
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34303, 'Resourceful', 'Bjorn_Scrap_Hand',
                'Passive: When Bjorn is in party, increase loot quality by 10%.',
                NULL, 0, 0, 0, 'self',
                'passive', NULL, 'Loot +10%', NULL,
                'companion_ability', CURRENT_TIMESTAMP
            )",

            // Valdis Forlorn-Touched Abilities
            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34401, 'Spirit Bolt', 'Valdis_Forlorn_Touched',
                'Unleash psychic blast dealing 3d6 + WILL damage.',
                'Aether Pool', 30, 3, 6, 'single_target',
                'ranged', 'Psychic', NULL, NULL,
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34402, 'Forlorn Whisper', 'Valdis_Forlorn_Touched',
                'Channel terrifying Forlorn voices. Target is feared and skips their next turn.',
                'Aether Pool', 40, 4, 5, 'single_target',
                'ranged', 'Psychic', NULL, 'Fear: Skip 1 turn',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34403, 'Fragile Mind', 'Valdis_Forlorn_Touched',
                'Passive: +25% spell damage, -25% max HP.',
                NULL, 0, 0, 0, 'self',
                'passive', NULL, 'Glass Cannon', NULL,
                'companion_ability', CURRENT_TIMESTAMP
            )",

            // Runa Shield-Sister Abilities
            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34501, 'Defensive Stance', 'Runa_Shield_Sister',
                'Adopt defensive posture. Gain +5 Defense but deal -2 damage for 3 turns.',
                'Stamina', 15, 3, 0, 'self',
                'self', NULL, NULL, 'Defense +5, Damage -2: 3 turns',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34502, 'Interpose', 'Runa_Shield_Sister',
                'Step in front of ally. All damage to that ally this turn redirected to Runa.',
                'Stamina', 10, 1, 3, 'single_target',
                'melee', NULL, NULL, 'Redirect damage: 1 turn',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34503, 'Shield Wall', 'Runa_Shield_Sister',
                'Raise shield to protect entire party. All allies gain +2 Defense for 2 turns.',
                'Stamina', 25, 2, 0, 'all_allies',
                'self', NULL, NULL, 'Party Defense +2: 2 turns',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            // Einar God-Touched Abilities
            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34601, 'Berserker Rage', 'Einar_God_Touched',
                'Enter berserk state. Gain +4 MIGHT for 3 turns but take +2 damage from all sources.',
                'Stamina', 20, 3, 0, 'self',
                'self', NULL, NULL, 'MIGHT +4, Vulnerability +2: 3 turns',
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34602, 'Jotun Attunement', 'Einar_God_Touched',
                'Passive: When within 10 tiles of Jötun corpse, gain +4 to all attributes.',
                NULL, 0, 0, 0, 'self',
                'passive', NULL, 'Conditional +4 all stats', NULL,
                'companion_ability', CURRENT_TIMESTAMP
            )",

            @"INSERT OR IGNORE INTO Companion_Abilities VALUES (
                34603, 'Reckless Strike', 'Einar_God_Touched',
                'Powerful overhead strike dealing 3d6 + MIGHT damage. Lowers own Defense by -3 for 1 turn.',
                'Stamina', 15, 2, 1, 'single_target',
                'melee', 'Physical', NULL, 'Self Defense -3: 1 turn',
                'companion_ability', CURRENT_TIMESTAMP
            )"
        };

        foreach (var seedSql in abilitySeeds)
        {
            var seedCommand = connection.CreateCommand();
            seedCommand.CommandText = seedSql;
            seedCommand.ExecuteNonQuery();
        }

        _log.Information("Companion data seeded: 6 companions and 18 abilities created");
    }

    public void SaveGame(PlayerCharacter player, WorldState worldState, bool isProcedurallyGenerated = false)
    {
        var roomIdentifier = isProcedurallyGenerated
            ? worldState.CurrentRoomStringId ?? "unknown"
            : worldState.CurrentRoomId.ToString();

        _log.Information("Saving game: Character={CharacterName}, Class={Class}, Milestone={Milestone}, Room={RoomId}, Procedural={IsProc}",
            player.Name, player.Class, player.CurrentMilestone, roomIdentifier, isProcedurallyGenerated);

        var startTime = DateTime.Now;

        try
        {
            // Serialize equipment (v0.3)
        var equippedWeaponJson = player.EquippedWeapon != null
            ? JsonSerializer.Serialize(player.EquippedWeapon)
            : null;

        var equippedArmorJson = player.EquippedArmor != null
            ? JsonSerializer.Serialize(player.EquippedArmor)
            : null;

        var inventoryJson = JsonSerializer.Serialize(player.Inventory);

        // Serialize consumables and crafting components (v0.7)
        var consumablesJson = JsonSerializer.Serialize(player.Consumables);
        var craftingComponentsJson = JsonSerializer.Serialize(player.CraftingComponents);

        // Serialize traumas (v0.15)
        var traumasJson = JsonSerializer.Serialize(player.Traumas);

        // Serialize room items (v0.3)
        // Note: WorldState no longer tracks rooms directly, using empty dict for compatibility
        var roomItemsDict = new Dictionary<int, List<Equipment>>();
        var roomItemsJson = JsonSerializer.Serialize(roomItemsDict);

        // Serialize faction reputations, quests, and NPC states (v0.8)
        var factionReputationsJson = JsonSerializer.Serialize(player.FactionReputations.Reputations);
        var activeQuestsJson = JsonSerializer.Serialize(player.ActiveQuests);
        var completedQuestsJson = JsonSerializer.Serialize(player.CompletedQuests);

        // Collect all NPCs from all rooms
        // Note: WorldState no longer tracks rooms/NPCs directly, using empty list for compatibility
        var allNPCs = new List<NPC>();
        var npcStatesJson = JsonSerializer.Serialize(allNPCs);

        var saveData = new SaveData
        {
            CharacterName = player.Name,
            Class = player.Class,
            Specialization = player.Specialization, // v0.7
            CurrentMilestone = player.CurrentMilestone,
            CurrentLegend = player.CurrentLegend,
            ProgressionPoints = player.ProgressionPoints,
            Might = player.Attributes.Might,
            Finesse = player.Attributes.Finesse,
            Wits = player.Attributes.Wits,
            Will = player.Attributes.Will,
            Sturdiness = player.Attributes.Sturdiness,
            CurrentHP = player.HP,
            MaxHP = player.MaxHP,
            CurrentStamina = player.Stamina,
            MaxStamina = player.MaxStamina,
            Currency = player.Currency, // v0.9
            PsychicStress = player.PsychicStress,
            Corruption = player.Corruption,
            RoomsExploredSinceRest = player.RoomsExploredSinceRest,
            TraumasJson = traumasJson, // v0.15
            // v0.7: Adept status effects
            VulnerableTurnsRemaining = player.VulnerableTurnsRemaining,
            AnalyzedTurnsRemaining = player.AnalyzedTurnsRemaining,
            SeizedTurnsRemaining = player.SeizedTurnsRemaining,
            IsPerforming = player.IsPerforming,
            PerformingTurnsRemaining = player.PerformingTurnsRemaining,
            CurrentPerformance = player.CurrentPerformance,
            InspiredTurnsRemaining = player.InspiredTurnsRemaining,
            SilencedTurnsRemaining = player.SilencedTurnsRemaining,
            TempHP = player.TempHP,
            CurrentRoomId = worldState.CurrentRoomId,
            ClearedRoomsJson = JsonSerializer.Serialize(worldState.ClearedRoomIds),
            PuzzleSolved = worldState.PuzzleSolved,
            BossDefeated = worldState.BossDefeated,
            EquippedWeaponJson = equippedWeaponJson,
            EquippedArmorJson = equippedArmorJson,
            InventoryJson = inventoryJson,
            RoomItemsJson = roomItemsJson,
            ConsumablesJson = consumablesJson,
            CraftingComponentsJson = craftingComponentsJson,
            FactionReputationsJson = factionReputationsJson,
            ActiveQuestsJson = activeQuestsJson,
            CompletedQuestsJson = completedQuestsJson,
            NPCStatesJson = npcStatesJson,
            // v0.10: Procedural generation
            IsProceduralDungeon = isProcedurallyGenerated,
            CurrentDungeonSeed = 0, // WorldState no longer tracks dungeon seed directly
            CurrentRoomStringId = worldState?.CurrentRoomStringId,
            DungeonsCompleted = worldState?.DungeonsCompleted ?? 0,
            // v0.21.1: Advanced Stance System (v2.0 canonical)
            ActiveStanceType = player.ActiveStance?.Type.ToString() ?? "Calculated",
            StanceTurnsInCurrent = player.StanceTurnsInCurrent,
            StanceShiftsRemaining = player.StanceShiftsRemaining,
            LastSaved = DateTime.Now
        };

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO saves (
                character_name, class, specialization, current_milestone, current_legend, progression_points,
                might, finesse, wits, will, sturdiness,
                current_hp, max_hp, current_stamina, max_stamina, currency,
                psychic_stress, corruption, rooms_explored_since_rest, traumas_json,
                vulnerable_turns, analyzed_turns, seized_turns, is_performing, performing_turns, current_performance,
                inspired_turns, silenced_turns, temp_hp,
                current_room_id, cleared_rooms_json, puzzle_solved, boss_defeated,
                equipped_weapon_json, equipped_armor_json, inventory_json, room_items_json,
                consumables_json, crafting_components_json,
                faction_reputations_json, active_quests_json, completed_quests_json, npc_states_json,
                is_procedural_dungeon, current_dungeon_seed, current_room_string_id, dungeons_completed, current_biome_id,
                active_stance_type, stance_turns_in_current, stance_shifts_remaining,
                last_saved
            ) VALUES (
                $name, $class, $spec, $milestone, $legend, $pp,
                $might, $finesse, $wits, $will, $sturdiness,
                $hp, $maxhp, $stamina, $maxstamina, $currency,
                $stress, $corruption, $roomsrest, $traumas,
                $vulnturns, $analyzedturns, $seizedturns, $isperforming, $perfturns, $perfname,
                $inspiredturns, $silencedturns, $temphp,
                $roomid, $cleared, $puzzle, $boss,
                $eqweapon, $eqarmor, $inventory, $roomitems,
                $consumables, $craftingcomponents,
                $factionreps, $activequests, $completedquests, $npcstates,
                $isproc, $dungeonseed, $roomstringid, $dungeonscompleted, $biomeid,
                $activestancetype, $stanceturns, $stanceshifts,
                $saved
            )
        ";

        command.Parameters.AddWithValue("$name", saveData.CharacterName);
        command.Parameters.AddWithValue("$class", saveData.Class.ToString());
        command.Parameters.AddWithValue("$spec", saveData.Specialization.ToString());
        command.Parameters.AddWithValue("$milestone", saveData.CurrentMilestone);
        command.Parameters.AddWithValue("$legend", saveData.CurrentLegend);
        command.Parameters.AddWithValue("$pp", saveData.ProgressionPoints);
        command.Parameters.AddWithValue("$might", saveData.Might);
        command.Parameters.AddWithValue("$finesse", saveData.Finesse);
        command.Parameters.AddWithValue("$wits", saveData.Wits);
        command.Parameters.AddWithValue("$will", saveData.Will);
        command.Parameters.AddWithValue("$sturdiness", saveData.Sturdiness);
        command.Parameters.AddWithValue("$hp", saveData.CurrentHP);
        command.Parameters.AddWithValue("$maxhp", saveData.MaxHP);
        command.Parameters.AddWithValue("$stamina", saveData.CurrentStamina);
        command.Parameters.AddWithValue("$maxstamina", saveData.MaxStamina);
        command.Parameters.AddWithValue("$currency", saveData.Currency); // v0.9
        command.Parameters.AddWithValue("$stress", saveData.PsychicStress);
        command.Parameters.AddWithValue("$corruption", saveData.Corruption);
        command.Parameters.AddWithValue("$roomsrest", saveData.RoomsExploredSinceRest);
        command.Parameters.AddWithValue("$traumas", saveData.TraumasJson);
        // v0.7: Adept status effects
        command.Parameters.AddWithValue("$vulnturns", saveData.VulnerableTurnsRemaining);
        command.Parameters.AddWithValue("$analyzedturns", saveData.AnalyzedTurnsRemaining);
        command.Parameters.AddWithValue("$seizedturns", saveData.SeizedTurnsRemaining);
        command.Parameters.AddWithValue("$isperforming", saveData.IsPerforming ? 1 : 0);
        command.Parameters.AddWithValue("$perfturns", saveData.PerformingTurnsRemaining);
        command.Parameters.AddWithValue("$perfname", (object?)saveData.CurrentPerformance ?? DBNull.Value);
        command.Parameters.AddWithValue("$inspiredturns", saveData.InspiredTurnsRemaining);
        command.Parameters.AddWithValue("$silencedturns", saveData.SilencedTurnsRemaining);
        command.Parameters.AddWithValue("$temphp", saveData.TempHP);
        command.Parameters.AddWithValue("$roomid", saveData.CurrentRoomId);
        command.Parameters.AddWithValue("$cleared", saveData.ClearedRoomsJson);
        command.Parameters.AddWithValue("$puzzle", saveData.PuzzleSolved ? 1 : 0);
        command.Parameters.AddWithValue("$boss", saveData.BossDefeated ? 1 : 0);
        command.Parameters.AddWithValue("$eqweapon", (object?)saveData.EquippedWeaponJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$eqarmor", (object?)saveData.EquippedArmorJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$inventory", saveData.InventoryJson);
        command.Parameters.AddWithValue("$roomitems", saveData.RoomItemsJson);
        command.Parameters.AddWithValue("$consumables", saveData.ConsumablesJson);
        command.Parameters.AddWithValue("$craftingcomponents", saveData.CraftingComponentsJson);
        command.Parameters.AddWithValue("$factionreps", saveData.FactionReputationsJson);
        command.Parameters.AddWithValue("$activequests", saveData.ActiveQuestsJson);
        command.Parameters.AddWithValue("$completedquests", saveData.CompletedQuestsJson);
        command.Parameters.AddWithValue("$npcstates", saveData.NPCStatesJson);
        command.Parameters.AddWithValue("$isproc", saveData.IsProceduralDungeon ? 1 : 0);
        command.Parameters.AddWithValue("$dungeonseed", saveData.CurrentDungeonSeed);
        command.Parameters.AddWithValue("$roomstringid", (object?)saveData.CurrentRoomStringId ?? DBNull.Value);
        command.Parameters.AddWithValue("$dungeonscompleted", saveData.DungeonsCompleted);
        command.Parameters.AddWithValue("$biomeid", DBNull.Value); // WorldState no longer tracks dungeon biome directly
        // v0.21.1: Advanced Stance System
        command.Parameters.AddWithValue("$activestancetype", saveData.ActiveStanceType);
        command.Parameters.AddWithValue("$stanceturns", saveData.StanceTurnsInCurrent);
        command.Parameters.AddWithValue("$stanceshifts", saveData.StanceShiftsRemaining);
        command.Parameters.AddWithValue("$saved", saveData.LastSaved.ToString("yyyy-MM-dd HH:mm:ss"));

        command.ExecuteNonQuery();

            var duration = (DateTime.Now - startTime).TotalMilliseconds;
            _log.Information("Game saved successfully: Character={CharacterName}, Duration={Duration}ms",
                player.Name, duration);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to save game: Character={CharacterName}", player.Name);
            throw;
        }
    }

    public (PlayerCharacter?, WorldState?, string?, string?, int?, string?) LoadGame(string characterName)
    {
        _log.Information("Loading game: Character={CharacterName}", characterName);

        var startTime = DateTime.Now;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM saves WHERE character_name = $name";
            command.Parameters.AddWithValue("$name", characterName);

            using var reader = command.ExecuteReader();

            if (!reader.Read())
            {
                _log.Warning("Save not found: Character={CharacterName}", characterName);
                return (null, null, null, null, null, null);
            }

        var saveData = new SaveData
        {
            CharacterName = reader.GetString(reader.GetOrdinal("character_name")),
            Class = Enum.Parse<CharacterClass>(reader.GetString(reader.GetOrdinal("class"))),
            CurrentMilestone = reader.GetInt32(reader.GetOrdinal("current_milestone")),
            CurrentLegend = reader.GetInt32(reader.GetOrdinal("current_legend")),
            ProgressionPoints = reader.GetInt32(reader.GetOrdinal("progression_points")),
            Might = reader.GetInt32(reader.GetOrdinal("might")),
            Finesse = reader.GetInt32(reader.GetOrdinal("finesse")),
            Wits = reader.GetInt32(reader.GetOrdinal("wits")),
            Will = reader.GetInt32(reader.GetOrdinal("will")),
            Sturdiness = reader.GetInt32(reader.GetOrdinal("sturdiness")),
            CurrentHP = reader.GetInt32(reader.GetOrdinal("current_hp")),
            MaxHP = reader.GetInt32(reader.GetOrdinal("max_hp")),
            CurrentStamina = reader.GetInt32(reader.GetOrdinal("current_stamina")),
            MaxStamina = reader.GetInt32(reader.GetOrdinal("max_stamina")),
            CurrentRoomId = reader.GetInt32(reader.GetOrdinal("current_room_id")),
            ClearedRoomsJson = reader.GetString(reader.GetOrdinal("cleared_rooms_json")),
            PuzzleSolved = reader.GetInt32(reader.GetOrdinal("puzzle_solved")) == 1,
            BossDefeated = reader.GetInt32(reader.GetOrdinal("boss_defeated")) == 1
        };

        // Load specialization (v0.7) - handle missing column for backward compatibility
        try
        {
            saveData.Specialization = Enum.Parse<Specialization>(reader.GetString(reader.GetOrdinal("specialization")));
        }
        catch { saveData.Specialization = Specialization.None; }

        // Load currency (v0.9) - handle missing column for backward compatibility
        try
        {
            saveData.Currency = reader.GetInt32(reader.GetOrdinal("currency"));
        }
        catch { saveData.Currency = 0; }

        // Load trauma economy data (v0.5) - handle missing columns for backward compatibility
        try
        {
            saveData.PsychicStress = reader.GetInt32(reader.GetOrdinal("psychic_stress"));
        }
        catch { saveData.PsychicStress = 0; }

        try
        {
            saveData.Corruption = reader.GetInt32(reader.GetOrdinal("corruption"));
        }
        catch { saveData.Corruption = 0; }

        try
        {
            saveData.RoomsExploredSinceRest = reader.GetInt32(reader.GetOrdinal("rooms_explored_since_rest"));
        }
        catch { saveData.RoomsExploredSinceRest = 0; }

        // Load traumas (v0.15) - handle missing column for backward compatibility
        try
        {
            var traumasOrdinal = reader.GetOrdinal("traumas_json");
            saveData.TraumasJson = reader.IsDBNull(traumasOrdinal) ? "[]" : reader.GetString(traumasOrdinal);
        }
        catch { saveData.TraumasJson = "[]"; }

        // Load v0.7 Adept status effects - handle missing columns for backward compatibility
        try { saveData.VulnerableTurnsRemaining = reader.GetInt32(reader.GetOrdinal("vulnerable_turns")); }
        catch { saveData.VulnerableTurnsRemaining = 0; }

        try { saveData.AnalyzedTurnsRemaining = reader.GetInt32(reader.GetOrdinal("analyzed_turns")); }
        catch { saveData.AnalyzedTurnsRemaining = 0; }

        try { saveData.SeizedTurnsRemaining = reader.GetInt32(reader.GetOrdinal("seized_turns")); }
        catch { saveData.SeizedTurnsRemaining = 0; }

        try { saveData.IsPerforming = reader.GetInt32(reader.GetOrdinal("is_performing")) == 1; }
        catch { saveData.IsPerforming = false; }

        try { saveData.PerformingTurnsRemaining = reader.GetInt32(reader.GetOrdinal("performing_turns")); }
        catch { saveData.PerformingTurnsRemaining = 0; }

        try
        {
            var perfOrdinal = reader.GetOrdinal("current_performance");
            saveData.CurrentPerformance = reader.IsDBNull(perfOrdinal) ? null : reader.GetString(perfOrdinal);
        }
        catch { saveData.CurrentPerformance = null; }

        try { saveData.InspiredTurnsRemaining = reader.GetInt32(reader.GetOrdinal("inspired_turns")); }
        catch { saveData.InspiredTurnsRemaining = 0; }

        try { saveData.SilencedTurnsRemaining = reader.GetInt32(reader.GetOrdinal("silenced_turns")); }
        catch { saveData.SilencedTurnsRemaining = 0; }

        try { saveData.TempHP = reader.GetInt32(reader.GetOrdinal("temp_hp")); }
        catch { saveData.TempHP = 0; }

        // Load procedural generation data (v0.10) - handle missing columns for backward compatibility
        try { saveData.IsProceduralDungeon = reader.GetInt32(reader.GetOrdinal("is_procedural_dungeon")) == 1; }
        catch { saveData.IsProceduralDungeon = false; }

        try { saveData.CurrentDungeonSeed = reader.GetInt32(reader.GetOrdinal("current_dungeon_seed")); }
        catch { saveData.CurrentDungeonSeed = 0; }

        try { saveData.DungeonsCompleted = reader.GetInt32(reader.GetOrdinal("dungeons_completed")); }
        catch { saveData.DungeonsCompleted = 0; }

        try
        {
            var roomStringIdOrdinal = reader.GetOrdinal("current_room_string_id");
            saveData.CurrentRoomStringId = reader.IsDBNull(roomStringIdOrdinal) ? null : reader.GetString(roomStringIdOrdinal);
        }
        catch { saveData.CurrentRoomStringId = null; }

        // Load equipment data (v0.3) - handle missing columns for backward compatibility
        try
        {
            var weaponOrdinal = reader.GetOrdinal("equipped_weapon_json");
            saveData.EquippedWeaponJson = reader.IsDBNull(weaponOrdinal) ? null : reader.GetString(weaponOrdinal);
        }
        catch { /* Column doesn't exist in old saves */ }

        try
        {
            var armorOrdinal = reader.GetOrdinal("equipped_armor_json");
            saveData.EquippedArmorJson = reader.IsDBNull(armorOrdinal) ? null : reader.GetString(armorOrdinal);
        }
        catch { /* Column doesn't exist in old saves */ }

        try
        {
            var inventoryOrdinal = reader.GetOrdinal("inventory_json");
            saveData.InventoryJson = reader.IsDBNull(inventoryOrdinal) ? "[]" : reader.GetString(inventoryOrdinal);
        }
        catch { saveData.InventoryJson = "[]"; }

        try
        {
            var roomItemsOrdinal = reader.GetOrdinal("room_items_json");
            saveData.RoomItemsJson = reader.IsDBNull(roomItemsOrdinal) ? "{}" : reader.GetString(roomItemsOrdinal);
        }
        catch { saveData.RoomItemsJson = "{}"; }

        // Load consumables and crafting components (v0.7) - handle missing columns for backward compatibility
        try
        {
            var consumablesOrdinal = reader.GetOrdinal("consumables_json");
            saveData.ConsumablesJson = reader.IsDBNull(consumablesOrdinal) ? "[]" : reader.GetString(consumablesOrdinal);
        }
        catch { saveData.ConsumablesJson = "[]"; }

        try
        {
            var craftingComponentsOrdinal = reader.GetOrdinal("crafting_components_json");
            saveData.CraftingComponentsJson = reader.IsDBNull(craftingComponentsOrdinal) ? "{}" : reader.GetString(craftingComponentsOrdinal);
        }
        catch { saveData.CraftingComponentsJson = "{}"; }

        // Load faction reputations, quests, and NPC states (v0.8) - handle missing columns for backward compatibility
        try
        {
            var factionRepsOrdinal = reader.GetOrdinal("faction_reputations_json");
            saveData.FactionReputationsJson = reader.IsDBNull(factionRepsOrdinal) ? "{}" : reader.GetString(factionRepsOrdinal);
        }
        catch { saveData.FactionReputationsJson = "{}"; }

        try
        {
            var activeQuestsOrdinal = reader.GetOrdinal("active_quests_json");
            saveData.ActiveQuestsJson = reader.IsDBNull(activeQuestsOrdinal) ? "[]" : reader.GetString(activeQuestsOrdinal);
        }
        catch { saveData.ActiveQuestsJson = "[]"; }

        try
        {
            var completedQuestsOrdinal = reader.GetOrdinal("completed_quests_json");
            saveData.CompletedQuestsJson = reader.IsDBNull(completedQuestsOrdinal) ? "[]" : reader.GetString(completedQuestsOrdinal);
        }
        catch { saveData.CompletedQuestsJson = "[]"; }

        try
        {
            var npcStatesOrdinal = reader.GetOrdinal("npc_states_json");
            saveData.NPCStatesJson = reader.IsDBNull(npcStatesOrdinal) ? "[]" : reader.GetString(npcStatesOrdinal);
        }
        catch { saveData.NPCStatesJson = "[]"; }

        // Load v0.21.1 Advanced Stance System (v2.0 canonical) - handle missing columns for backward compatibility
        try
        {
            var stanceTypeOrdinal = reader.GetOrdinal("active_stance_type");
            saveData.ActiveStanceType = reader.IsDBNull(stanceTypeOrdinal) ? "Calculated" : reader.GetString(stanceTypeOrdinal);
        }
        catch { saveData.ActiveStanceType = "Calculated"; }

        try { saveData.StanceTurnsInCurrent = reader.GetInt32(reader.GetOrdinal("stance_turns_in_current")); }
        catch { saveData.StanceTurnsInCurrent = 0; }

        try { saveData.StanceShiftsRemaining = reader.GetInt32(reader.GetOrdinal("stance_shifts_remaining")); }
        catch { saveData.StanceShiftsRemaining = 1; }

        // Reconstruct PlayerCharacter
        var player = new PlayerCharacter
        {
            Name = saveData.CharacterName,
            Class = saveData.Class,
            Specialization = saveData.Specialization, // v0.7
            CurrentMilestone = saveData.CurrentMilestone,
            CurrentLegend = saveData.CurrentLegend,
            ProgressionPoints = saveData.ProgressionPoints,
            LegendToNextMilestone = CalculateLegendToNextMilestone(saveData.CurrentMilestone),
            Attributes = new Attributes(
                might: saveData.Might,
                finesse: saveData.Finesse,
                wits: saveData.Wits,
                will: saveData.Will,
                sturdiness: saveData.Sturdiness
            ),
            HP = saveData.CurrentHP,
            MaxHP = saveData.MaxHP,
            Stamina = saveData.CurrentStamina,
            MaxStamina = saveData.MaxStamina,
            Currency = saveData.Currency, // v0.9
            PsychicStress = saveData.PsychicStress,
            Corruption = saveData.Corruption,
            RoomsExploredSinceRest = saveData.RoomsExploredSinceRest,
            // v0.7: Restore Adept status effects
            VulnerableTurnsRemaining = saveData.VulnerableTurnsRemaining,
            AnalyzedTurnsRemaining = saveData.AnalyzedTurnsRemaining,
            SeizedTurnsRemaining = saveData.SeizedTurnsRemaining,
            IsPerforming = saveData.IsPerforming,
            PerformingTurnsRemaining = saveData.PerformingTurnsRemaining,
            CurrentPerformance = saveData.CurrentPerformance,
            InspiredTurnsRemaining = saveData.InspiredTurnsRemaining,
            SilencedTurnsRemaining = saveData.SilencedTurnsRemaining,
            TempHP = saveData.TempHP,
            AP = 10 // Always restore to max AP
        };

        // Reconstruct equipment (v0.3)
        if (!string.IsNullOrEmpty(saveData.EquippedWeaponJson))
        {
            try
            {
                player.EquippedWeapon = JsonSerializer.Deserialize<Equipment>(saveData.EquippedWeaponJson);
            }
            catch { /* Failed to deserialize weapon */ }
        }

        if (!string.IsNullOrEmpty(saveData.EquippedArmorJson))
        {
            try
            {
                player.EquippedArmor = JsonSerializer.Deserialize<Equipment>(saveData.EquippedArmorJson);
            }
            catch { /* Failed to deserialize armor */ }
        }

        try
        {
            player.Inventory = JsonSerializer.Deserialize<List<Equipment>>(saveData.InventoryJson) ?? new List<Equipment>();
        }
        catch
        {
            player.Inventory = new List<Equipment>();
        }

        // Reconstruct consumables and crafting components (v0.7)
        try
        {
            player.Consumables = JsonSerializer.Deserialize<List<Consumable>>(saveData.ConsumablesJson) ?? new List<Consumable>();
        }
        catch
        {
            player.Consumables = new List<Consumable>();
        }

        try
        {
            player.CraftingComponents = JsonSerializer.Deserialize<Dictionary<ComponentType, int>>(saveData.CraftingComponentsJson) ?? new Dictionary<ComponentType, int>();
        }
        catch
        {
            player.CraftingComponents = new Dictionary<ComponentType, int>();
        }

        // Reconstruct traumas (v0.15)
        try
        {
            player.Traumas = JsonSerializer.Deserialize<List<Trauma>>(saveData.TraumasJson) ?? new List<Trauma>();
        }
        catch
        {
            player.Traumas = new List<Trauma>();
        }

        // Reconstruct faction reputations and quests (v0.8)
        try
        {
            var factionReps = JsonSerializer.Deserialize<Dictionary<FactionType, int>>(saveData.FactionReputationsJson);
            if (factionReps != null)
            {
                player.FactionReputations.Reputations = factionReps;
            }
        }
        catch
        {
            // Failed to deserialize faction reputations - start with defaults
        }

        try
        {
            player.ActiveQuests = JsonSerializer.Deserialize<List<Quest>>(saveData.ActiveQuestsJson) ?? new List<Quest>();
        }
        catch
        {
            player.ActiveQuests = new List<Quest>();
        }

        try
        {
            player.CompletedQuests = JsonSerializer.Deserialize<List<Quest>>(saveData.CompletedQuestsJson) ?? new List<Quest>();
        }
        catch
        {
            player.CompletedQuests = new List<Quest>();
        }

        // Reconstruct v0.21.1 Advanced Stance System (v2.0 canonical)
        try
        {
            player.ActiveStance = saveData.ActiveStanceType switch
            {
                // v2.0 canonical names
                "Aggressive" => Stance.CreateAggressiveStance(),
                "Defensive" => Stance.CreateDefensiveStance(),
                "Calculated" => Stance.CreateCalculatedStance(),
                "Evasive" => Stance.CreateEvasiveStance(),
                // Legacy names for backward compatibility
                "Offensive" => Stance.CreateAggressiveStance(),
                "Balanced" => Stance.CreateCalculatedStance(),
                _ => Stance.CreateCalculatedStance()
            };
            player.StanceTurnsInCurrent = saveData.StanceTurnsInCurrent;
            player.StanceShiftsRemaining = saveData.StanceShiftsRemaining;
        }
        catch
        {
            player.ActiveStance = Stance.CreateCalculatedStance();
            player.StanceTurnsInCurrent = 0;
            player.StanceShiftsRemaining = 1;
        }

        // Reconstruct abilities based on class and level (will be set by CharacterFactory)

        // Reconstruct WorldState
        var worldState = new WorldState
        {
            CurrentRoomId = saveData.CurrentRoomId,
            ClearedRoomIds = JsonSerializer.Deserialize<List<int>>(saveData.ClearedRoomsJson) ?? new List<int>(),
            PuzzleSolved = saveData.PuzzleSolved,
            BossDefeated = saveData.BossDefeated,
            // v0.10: Procedural generation
            CurrentRoomStringId = saveData.CurrentRoomStringId,
            DungeonsCompleted = saveData.DungeonsCompleted
        };

        // Extract biome ID from saved data for dungeon regeneration
        string? biomeId = null;
        if (saveData.IsProceduralDungeon)
        {
            try
            {
                var biomeIdOrdinal = reader.GetOrdinal("current_biome_id");
                biomeId = reader.IsDBNull(biomeIdOrdinal) ? null : reader.GetString(biomeIdOrdinal);
            }
            catch { biomeId = null; }
        }

            var duration = (DateTime.Now - startTime).TotalMilliseconds;
            _log.Information("Game loaded successfully: Character={CharacterName}, Class={Class}, Milestone={Milestone}, Procedural={IsProc}, Duration={Duration}ms",
                player.Name, player.Class, player.CurrentMilestone, saveData.IsProceduralDungeon, duration);

            // Return room items JSON, NPC states JSON, dungeon seed (if procedural), and biome ID for restoration
            // (v0.3, v0.8, v0.10)
            int? dungeonSeed = saveData.IsProceduralDungeon ? saveData.CurrentDungeonSeed : null;
            return (player, worldState, saveData.RoomItemsJson, saveData.NPCStatesJson, dungeonSeed, biomeId);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to load game: Character={CharacterName}", characterName);
            throw;
        }
    }

    /// <summary>
    /// Restore room items from save data (v0.3, v0.10: supports procedural dungeons)
    /// </summary>
    public void RestoreRoomItems(Dictionary<string, Room> rooms, string roomItemsJson)
    {
        if (string.IsNullOrEmpty(roomItemsJson) || roomItemsJson == "{}")
        {
            _log.Debug("No room items to restore");
            return;
        }

        try
        {
            // Try to deserialize as int-keyed dictionary (legacy saves)
            var roomItemsDict = JsonSerializer.Deserialize<Dictionary<int, List<Equipment>>>(roomItemsJson);
            if (roomItemsDict == null) return;

            int itemCount = 0;
            foreach (var kvp in roomItemsDict)
            {
                var roomId = kvp.Key;
                var items = kvp.Value;

                // For legacy handcrafted worlds, find room by integer ID
                // For procedural worlds, this won't work (procedural rooms use string IDs)
                var room = rooms.Values.FirstOrDefault(r => r.Id == roomId);
                if (room != null)
                {
                    room.ItemsOnGround.Clear();
                    room.ItemsOnGround.AddRange(items);
                    itemCount += items.Count;
                }
            }

            _log.Information("Restored room items: {RoomCount} rooms, {ItemCount} items total",
                roomItemsDict.Count, itemCount);
        }
        catch (Exception ex)
        {
            // Failed to restore room items - not critical, player can continue without them
            _log.Warning(ex, "Failed to restore room items");
        }

        // Note: For procedural dungeons (v0.10), room items are NOT persisted because
        // dungeons are regenerated from seed. Future enhancement could serialize room
        // items with string room IDs for procedural dungeon persistence.
    }

    /// <summary>
    /// Restore NPC states from save data (v0.8)
    /// </summary>
    public void RestoreNPCStates(Dictionary<string, Room> rooms, string npcStatesJson)
    {
        if (string.IsNullOrEmpty(npcStatesJson) || npcStatesJson == "[]")
        {
            _log.Debug("No NPC states to restore");
            return;
        }

        try
        {
            var savedNPCs = JsonSerializer.Deserialize<List<NPC>>(npcStatesJson);
            if (savedNPCs == null) return;

            // Clear all NPCs from all rooms first
            foreach (var room in rooms.Values)
            {
                room.NPCs.Clear();
            }

            // Restore NPCs to their rooms with saved state
            foreach (var npc in savedNPCs)
            {
                var room = rooms.Values.FirstOrDefault(r => r.Id == npc.RoomId);
                if (room != null)
                {
                    room.NPCs.Add(npc);
                }
            }

            _log.Information("Restored NPC states: {NpcCount} NPCs", savedNPCs.Count);
        }
        catch (Exception ex)
        {
            // Failed to restore NPC states - not critical, but NPCs will reset to default state
            _log.Warning(ex, "Failed to restore NPC states");
        }
    }

    public List<SaveInfo> ListSaves()
    {
        var saves = new List<SaveInfo>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT character_name, class, current_milestone, boss_defeated, last_saved FROM saves ORDER BY last_saved DESC";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            saves.Add(new SaveInfo
            {
                CharacterName = reader.GetString(0),
                Class = Enum.Parse<CharacterClass>(reader.GetString(1)),
                CurrentMilestone = reader.GetInt32(2),
                BossDefeated = reader.GetInt32(3) == 1,
                LastPlayed = DateTime.Parse(reader.GetString(4))
            });
        }

        return saves;
    }

    public void DeleteSave(string characterName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM saves WHERE character_name = $name";
        command.Parameters.AddWithValue("$name", characterName);

        command.ExecuteNonQuery();
    }

    public bool SaveExists(string characterName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM saves WHERE character_name = $name";
        command.Parameters.AddWithValue("$name", characterName);

        var count = (long)command.ExecuteScalar()!;
        return count > 0;
    }

    /// <summary>
    /// v0.35.1: Create Territory Control & Dynamic World tables and seed initial data
    /// </summary>
    private void CreateTerritoryControlTables(SqliteConnection connection)
    {
        _log.Debug("Creating Territory Control & Dynamic World tables");

        // Table: Worlds
        var createWorldsTable = connection.CreateCommand();
        createWorldsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Worlds (
                world_id INTEGER PRIMARY KEY AUTOINCREMENT,
                world_name TEXT NOT NULL UNIQUE,
                world_description TEXT,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )
        ";
        createWorldsTable.ExecuteNonQuery();

        // Table: Sectors
        var createSectorsTable = connection.CreateCommand();
        createSectorsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Sectors (
                sector_id INTEGER PRIMARY KEY AUTOINCREMENT,
                world_id INTEGER NOT NULL,
                sector_name TEXT NOT NULL,
                sector_description TEXT,
                biome_id INTEGER,
                z_level TEXT,
                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

                FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
                FOREIGN KEY (biome_id) REFERENCES Biomes(biome_id) ON DELETE SET NULL,
                UNIQUE(world_id, sector_name)
            )
        ";
        createSectorsTable.ExecuteNonQuery();

        // Table: Faction_Territory_Control
        var createTerritoryControlTable = connection.CreateCommand();
        createTerritoryControlTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Faction_Territory_Control (
                territory_control_id INTEGER PRIMARY KEY AUTOINCREMENT,
                world_id INTEGER NOT NULL,
                sector_id INTEGER NOT NULL,
                faction_name TEXT NOT NULL,
                influence_value REAL NOT NULL DEFAULT 0.0,
                control_state TEXT NOT NULL CHECK(control_state IN ('Stable', 'Contested', 'War', 'Independent', 'Ruined')),
                last_updated TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

                FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
                FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
                FOREIGN KEY (faction_name) REFERENCES Factions(faction_name) ON DELETE CASCADE,
                UNIQUE(world_id, sector_id, faction_name)
            )
        ";
        createTerritoryControlTable.ExecuteNonQuery();

        // Table: Faction_Wars
        var createFactionWarsTable = connection.CreateCommand();
        createFactionWarsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Faction_Wars (
                war_id INTEGER PRIMARY KEY AUTOINCREMENT,
                world_id INTEGER NOT NULL,
                sector_id INTEGER NOT NULL,
                faction_a TEXT NOT NULL,
                faction_b TEXT NOT NULL,
                war_start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                war_end_date TIMESTAMP,
                war_balance REAL DEFAULT 0.0,
                is_active BOOLEAN NOT NULL DEFAULT 1,
                victor TEXT,
                collateral_damage INTEGER DEFAULT 0,

                FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
                FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
                FOREIGN KEY (faction_a) REFERENCES Factions(faction_name) ON DELETE CASCADE,
                FOREIGN KEY (faction_b) REFERENCES Factions(faction_name) ON DELETE CASCADE,

                CHECK(faction_a != faction_b),
                CHECK(war_balance BETWEEN -100 AND 100)
            )
        ";
        createFactionWarsTable.ExecuteNonQuery();

        // Table: World_Events
        var createWorldEventsTable = connection.CreateCommand();
        createWorldEventsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS World_Events (
                event_id INTEGER PRIMARY KEY AUTOINCREMENT,
                world_id INTEGER NOT NULL,
                sector_id INTEGER,
                event_type TEXT NOT NULL CHECK(event_type IN (
                    'Faction_War', 'Incursion', 'Supply_Raid', 'Diplomatic_Shift',
                    'Catastrophe', 'Awakening_Ritual', 'Excavation_Discovery',
                    'Purge_Campaign', 'Scavenger_Caravan'
                )),
                affected_faction TEXT,
                event_title TEXT NOT NULL,
                event_description TEXT NOT NULL,
                event_start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                event_end_date TIMESTAMP,
                event_duration_days INTEGER DEFAULT 7,
                is_resolved BOOLEAN NOT NULL DEFAULT 0,
                player_influenced BOOLEAN NOT NULL DEFAULT 0,
                outcome TEXT,
                influence_change REAL DEFAULT 0.0,

                FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
                FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
                FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name) ON DELETE CASCADE
            )
        ";
        createWorldEventsTable.ExecuteNonQuery();

        // Table: Player_Territorial_Actions
        var createPlayerActionsTable = connection.CreateCommand();
        createPlayerActionsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Player_Territorial_Actions (
                action_id INTEGER PRIMARY KEY AUTOINCREMENT,
                character_id INTEGER NOT NULL,
                world_id INTEGER NOT NULL,
                sector_id INTEGER NOT NULL,
                action_type TEXT NOT NULL CHECK(action_type IN (
                    'Kill_Enemy', 'Complete_Quest', 'Defend_Territory', 'Sabotage',
                    'Escort_Caravan', 'Destroy_Hazard', 'Activate_Artifact'
                )),
                affected_faction TEXT NOT NULL,
                influence_delta REAL NOT NULL,
                action_timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                notes TEXT,

                FOREIGN KEY (character_id) REFERENCES saves(id) ON DELETE CASCADE,
                FOREIGN KEY (world_id) REFERENCES Worlds(world_id) ON DELETE CASCADE,
                FOREIGN KEY (sector_id) REFERENCES Sectors(sector_id) ON DELETE CASCADE,
                FOREIGN KEY (affected_faction) REFERENCES Factions(faction_name) ON DELETE CASCADE,

                CHECK(influence_delta BETWEEN -10.0 AND 10.0)
            )
        ";
        createPlayerActionsTable.ExecuteNonQuery();

        // Create indices for performance
        var createIndices = new[]
        {
            "CREATE INDEX IF NOT EXISTS idx_worlds_name ON Worlds(world_name)",
            "CREATE INDEX IF NOT EXISTS idx_sectors_world ON Sectors(world_id)",
            "CREATE INDEX IF NOT EXISTS idx_sectors_name ON Sectors(sector_name)",
            "CREATE INDEX IF NOT EXISTS idx_sectors_biome ON Sectors(biome_id)",
            "CREATE INDEX IF NOT EXISTS idx_territory_control_sector ON Faction_Territory_Control(sector_id)",
            "CREATE INDEX IF NOT EXISTS idx_territory_control_faction ON Faction_Territory_Control(faction_name)",
            "CREATE INDEX IF NOT EXISTS idx_territory_control_state ON Faction_Territory_Control(control_state)",
            "CREATE INDEX IF NOT EXISTS idx_territory_control_influence ON Faction_Territory_Control(influence_value DESC)",
            "CREATE INDEX IF NOT EXISTS idx_wars_active ON Faction_Wars(is_active) WHERE is_active = 1",
            "CREATE INDEX IF NOT EXISTS idx_wars_sector ON Faction_Wars(sector_id)",
            "CREATE INDEX IF NOT EXISTS idx_wars_date ON Faction_Wars(war_start_date DESC)",
            "CREATE INDEX IF NOT EXISTS idx_events_active ON World_Events(is_resolved) WHERE is_resolved = 0",
            "CREATE INDEX IF NOT EXISTS idx_events_sector ON World_Events(sector_id)",
            "CREATE INDEX IF NOT EXISTS idx_events_faction ON World_Events(affected_faction)",
            "CREATE INDEX IF NOT EXISTS idx_events_type ON World_Events(event_type)",
            "CREATE INDEX IF NOT EXISTS idx_player_actions_character ON Player_Territorial_Actions(character_id)",
            "CREATE INDEX IF NOT EXISTS idx_player_actions_sector ON Player_Territorial_Actions(sector_id)",
            "CREATE INDEX IF NOT EXISTS idx_player_actions_faction ON Player_Territorial_Actions(affected_faction)",
            "CREATE INDEX IF NOT EXISTS idx_player_actions_date ON Player_Territorial_Actions(action_timestamp DESC)"
        };

        foreach (var indexSql in createIndices)
        {
            var indexCommand = connection.CreateCommand();
            indexCommand.CommandText = indexSql;
            indexCommand.ExecuteNonQuery();
        }

        // Seed initial territory data
        SeedTerritoryData(connection);

        _log.Information("Territory Control & Dynamic World tables created successfully");
    }

    /// <summary>
    /// v0.35.1: Seed initial world, sectors, territory distribution, wars, and events
    /// </summary>
    private void SeedTerritoryData(SqliteConnection connection)
    {
        _log.Debug("Seeding territory control data");

        // Seed World: Aethelgard
        var seedWorld = connection.CreateCommand();
        seedWorld.CommandText = @"
            INSERT OR IGNORE INTO Worlds (world_id, world_name, world_description)
            VALUES (1, 'Aethelgard', 'The World-Tree, an ancient structure containing the remnants of pre-Glitch civilization. A failing technological megastructure where reality coherence decreases with depth.')
        ";
        seedWorld.ExecuteNonQuery();

        // Seed 10 Sectors
        var sectorSeeds = new[]
        {
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (1, 1, 'Midgard', 'The Trunk level - main operational zones and habitable sectors. Neutral ground where Rust-Clans establish their trade networks.', NULL, 'Trunk')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (2, 1, 'Muspelheim', 'Volcanic geothermal sectors with extreme temperatures. Iron-Banes patrol heavily to contain Runic Blight spreading from corrupted forges.', 3, 'Roots')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (3, 1, 'Niflheim', 'Cryo-facilities frozen in catastrophic failure. Data repositories make this contested ground between Jötun-Readers and Rust-Clans.', 5, 'Roots')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (4, 1, 'Alfheim', 'Ancient archives and data preservation zones. Jötun-Readers maintain primary research facilities here.', NULL, 'Canopy')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (5, 1, 'Jotunheim', 'Assembly yards containing dormant Jötun-Forged constructs. Sacred ground for God-Sleeper cultists.', NULL, 'Trunk')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (6, 1, 'Svartalfheim', 'Deep salvage zones rich in scrap and components. Rust-Clan stronghold with established trade routes.', NULL, 'Roots')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (7, 1, 'Vanaheim', 'Transition zones between Trunk and Canopy. Contested between Independents and Iron-Banes.', NULL, 'Trunk')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (8, 1, 'Helheim', 'Corrupted sectors with high Undying presence. Dangerous independent territory.', NULL, 'Roots')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (9, 1, 'Asgard', 'Upper command structures near Canopy. Contested between God-Sleepers and Jötun-Readers.', NULL, 'Canopy')",
            @"INSERT OR IGNORE INTO Sectors (sector_id, world_id, sector_name, sector_description, biome_id, z_level)
              VALUES (10, 1, 'Valhalla', 'Historical defense installations. Iron-Bane operational headquarters.', NULL, 'Trunk')"
        };

        foreach (var sectorSeed in sectorSeeds)
        {
            var seedCommand = connection.CreateCommand();
            seedCommand.CommandText = sectorSeed;
            seedCommand.ExecuteNonQuery();
        }

        // Seed Territory Control (10 sectors × 5 factions = 50 records)
        // NOTE: Using database faction names (IronBanes, GodSleeperCultists, JotunReaders, RustClans, Independents)
        var territorySeeds = new[]
        {
            // Sector 1: Midgard - Independent
            "(1, 1, 'RustClans', 35.0, 'Independent')",
            "(1, 1, 'IronBanes', 30.0, 'Independent')",
            "(1, 1, 'JotunReaders', 20.0, 'Independent')",
            "(1, 1, 'GodSleeperCultists', 10.0, 'Independent')",
            "(1, 1, 'Independents', 5.0, 'Independent')",

            // Sector 2: Muspelheim - Iron-Bane Stable Control
            "(1, 2, 'IronBanes', 65.0, 'Stable')",
            "(1, 2, 'RustClans', 20.0, 'Stable')",
            "(1, 2, 'JotunReaders', 10.0, 'Stable')",
            "(1, 2, 'GodSleeperCultists', 3.0, 'Stable')",
            "(1, 2, 'Independents', 2.0, 'Stable')",

            // Sector 3: Niflheim - Contested
            "(1, 3, 'JotunReaders', 48.0, 'Contested')",
            "(1, 3, 'RustClans', 45.0, 'Contested')",
            "(1, 3, 'IronBanes', 5.0, 'Contested')",
            "(1, 3, 'GodSleeperCultists', 2.0, 'Contested')",
            "(1, 3, 'Independents', 0.0, 'Contested')",

            // Sector 4: Alfheim - Jötun-Reader Stable Control
            "(1, 4, 'JotunReaders', 70.0, 'Stable')",
            "(1, 4, 'Independents', 15.0, 'Stable')",
            "(1, 4, 'GodSleeperCultists', 10.0, 'Stable')",
            "(1, 4, 'RustClans', 3.0, 'Stable')",
            "(1, 4, 'IronBanes', 2.0, 'Stable')",

            // Sector 5: Jötunheim - God-Sleeper Stable Control
            "(1, 5, 'GodSleeperCultists', 60.0, 'Stable')",
            "(1, 5, 'Independents', 25.0, 'Stable')",
            "(1, 5, 'JotunReaders', 10.0, 'Stable')",
            "(1, 5, 'RustClans', 3.0, 'Stable')",
            "(1, 5, 'IronBanes', 2.0, 'Stable')",

            // Sector 6: Svartalfheim - Rust-Clan Stable Control
            "(1, 6, 'RustClans', 62.0, 'Stable')",
            "(1, 6, 'Independents', 20.0, 'Stable')",
            "(1, 6, 'IronBanes', 10.0, 'Stable')",
            "(1, 6, 'JotunReaders', 5.0, 'Stable')",
            "(1, 6, 'GodSleeperCultists', 3.0, 'Stable')",

            // Sector 7: Vanaheim - Contested
            "(1, 7, 'Independents', 50.0, 'Contested')",
            "(1, 7, 'IronBanes', 42.0, 'Contested')",
            "(1, 7, 'RustClans', 5.0, 'Contested')",
            "(1, 7, 'JotunReaders', 2.0, 'Contested')",
            "(1, 7, 'GodSleeperCultists', 1.0, 'Contested')",

            // Sector 8: Helheim - Independent
            "(1, 8, 'Independents', 38.0, 'Independent')",
            "(1, 8, 'IronBanes', 25.0, 'Independent')",
            "(1, 8, 'GodSleeperCultists', 20.0, 'Independent')",
            "(1, 8, 'RustClans', 12.0, 'Independent')",
            "(1, 8, 'JotunReaders', 5.0, 'Independent')",

            // Sector 9: Asgard - Contested
            "(1, 9, 'GodSleeperCultists', 46.0, 'Contested')",
            "(1, 9, 'JotunReaders', 44.0, 'Contested')",
            "(1, 9, 'Independents', 8.0, 'Contested')",
            "(1, 9, 'RustClans', 2.0, 'Contested')",
            "(1, 9, 'IronBanes', 0.0, 'Contested')",

            // Sector 10: Valhalla - Iron-Bane Stable Control
            "(1, 10, 'IronBanes', 68.0, 'Stable')",
            "(1, 10, 'Independents', 18.0, 'Stable')",
            "(1, 10, 'RustClans', 10.0, 'Stable')",
            "(1, 10, 'JotunReaders', 3.0, 'Stable')",
            "(1, 10, 'GodSleeperCultists', 1.0, 'Stable')"
        };

        var territoryBulkInsert = connection.CreateCommand();
        territoryBulkInsert.CommandText = @"
            INSERT OR IGNORE INTO Faction_Territory_Control (world_id, sector_id, faction_name, influence_value, control_state)
            VALUES " + string.Join(", ", territorySeeds);
        territoryBulkInsert.ExecuteNonQuery();

        // Seed initial war in Niflheim
        var seedWar = connection.CreateCommand();
        seedWar.CommandText = @"
            INSERT OR IGNORE INTO Faction_Wars (war_id, world_id, sector_id, faction_a, faction_b, war_balance, is_active)
            VALUES (1, 1, 3, 'JotunReaders', 'RustClans', 3.0, 1)
        ";
        seedWar.ExecuteNonQuery();

        // Seed initial world events
        var eventSeeds = new[]
        {
            @"INSERT OR IGNORE INTO World_Events (event_id, world_id, sector_id, event_type, affected_faction, event_title, event_description, event_duration_days)
              VALUES (1, 1, 3, 'Faction_War', 'JotunReaders', 'The Data Wars', 'Jötun-Readers and Rust-Clans clash over control of ancient data repositories in Niflheim. The scavengers want the salvage, the archaeologists want the knowledge. Player actions could tip the balance.', 12)",
            @"INSERT OR IGNORE INTO World_Events (event_id, world_id, sector_id, event_type, affected_faction, event_title, event_description, event_duration_days)
              VALUES (2, 1, 9, 'Awakening_Ritual', 'GodSleeperCultists', 'The Awakening', 'God-Sleeper cultists attempt to reactivate dormant Jötun-Forged constructs in Asgard. If successful, they will gain significant territorial control. If disrupted, their influence will collapse.', 7)",
            @"INSERT OR IGNORE INTO World_Events (event_id, world_id, sector_id, event_type, affected_faction, event_title, event_description, event_duration_days)
              VALUES (3, 1, 8, 'Purge_Campaign', 'IronBanes', 'The Last Protocol', 'Iron-Banes launch systematic purge of Undying in Helheim. Their protocols demand complete eradication of corrupted processes. Success could establish Iron-Bane presence in this independent sector.', 10)"
        };

        foreach (var eventSeed in eventSeeds)
        {
            var seedCommand = connection.CreateCommand();
            seedCommand.CommandText = eventSeed;
            seedCommand.ExecuteNonQuery();
        }

        _log.Information("Territory data seeded: 1 world (Aethelgard), 10 sectors, 50 influence records, 1 war, 3 events");
        _log.Information("For complete territory control functionality, implement services in v0.35.2-v0.35.4");
    }

    private int CalculateLegendToNextMilestone(int currentMilestone)
    {
        // Adjusted formula for v0.1: (CurrentMilestone × 50) + 100
        return (currentMilestone * 50) + 100;
    }
}
