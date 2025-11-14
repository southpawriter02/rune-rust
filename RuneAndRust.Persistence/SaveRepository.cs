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
            // v0.21.1: Advanced Stance System
            "ALTER TABLE saves ADD COLUMN active_stance_type TEXT DEFAULT 'Balanced'",
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

            _log.Information("Database initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize database");
            throw;
        }
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
            // v0.21.1: Advanced Stance System
            ActiveStanceType = player.ActiveStance?.Type.ToString() ?? "Balanced",
            StanceTurnsInCurrent = 0, // TODO: Add tracking field to PlayerCharacter
            StanceShiftsRemaining = 1, // TODO: Add tracking field to PlayerCharacter
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

        // Load v0.21.1 Advanced Stance System - handle missing columns for backward compatibility
        try
        {
            var stanceTypeOrdinal = reader.GetOrdinal("active_stance_type");
            saveData.ActiveStanceType = reader.IsDBNull(stanceTypeOrdinal) ? "Balanced" : reader.GetString(stanceTypeOrdinal);
        }
        catch { saveData.ActiveStanceType = "Balanced"; }

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

        // Reconstruct v0.21.1 Advanced Stance System
        try
        {
            player.ActiveStance = saveData.ActiveStanceType switch
            {
                "Defensive" => Stance.CreateDefensiveStance(),
                "Offensive" => Stance.CreateOffensiveStance(),
                "Evasive" => Stance.CreateEvasiveStance(),
                "Balanced" => Stance.CreateBalancedStance(),
                _ => Stance.CreateBalancedStance()
            };
            // TODO: Restore StanceTurnsInCurrent and StanceShiftsRemaining when PlayerCharacter has these fields
        }
        catch
        {
            player.ActiveStance = Stance.CreateBalancedStance();
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

    private int CalculateLegendToNextMilestone(int currentMilestone)
    {
        // Adjusted formula for v0.1: (CurrentMilestone × 50) + 100
        return (currentMilestone * 50) + 100;
    }
}
