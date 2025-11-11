using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using RuneAndRust.Engine;
using System.Text.Json;

namespace RuneAndRust.Persistence;

public class SaveRepository
{
    private readonly string _connectionString;
    private const string DatabaseName = "runeandrust.db";

    public SaveRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, DatabaseName);
        _connectionString = $"Data Source={dbPath}";

        InitializeDatabase();
    }

    private void InitializeDatabase()
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
        var alterCommands = new[]
        {
            "ALTER TABLE saves ADD COLUMN equipped_weapon_json TEXT",
            "ALTER TABLE saves ADD COLUMN equipped_armor_json TEXT",
            "ALTER TABLE saves ADD COLUMN inventory_json TEXT DEFAULT '[]'",
            "ALTER TABLE saves ADD COLUMN room_items_json TEXT DEFAULT '{}'",
            "ALTER TABLE saves ADD COLUMN psychic_stress INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN corruption INTEGER DEFAULT 0",
            "ALTER TABLE saves ADD COLUMN rooms_explored_since_rest INTEGER DEFAULT 0",
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
            "ALTER TABLE saves ADD COLUMN npc_states_json TEXT DEFAULT '[]'"
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
    }

    public void SaveGame(PlayerCharacter player, WorldState worldState, GameWorld? world = null)
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

        // Serialize room items (v0.3)
        var roomItemsDict = new Dictionary<int, List<Equipment>>();
        if (world != null)
        {
            foreach (var kvp in world.Rooms)
            {
                var room = kvp.Value;
                if (room.ItemsOnGround.Count > 0)
                {
                    roomItemsDict[room.Id] = room.ItemsOnGround;
                }
            }
        }
        var roomItemsJson = JsonSerializer.Serialize(roomItemsDict);

        // Serialize faction reputations, quests, and NPC states (v0.8)
        var factionReputationsJson = JsonSerializer.Serialize(player.FactionReputations.Reputations);
        var activeQuestsJson = JsonSerializer.Serialize(player.ActiveQuests);
        var completedQuestsJson = JsonSerializer.Serialize(player.CompletedQuests);

        // Collect all NPCs from all rooms
        var allNPCs = new List<NPC>();
        if (world != null)
        {
            foreach (var kvp in world.Rooms)
            {
                allNPCs.AddRange(kvp.Value.NPCs);
            }
        }
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
            PsychicStress = player.PsychicStress,
            Corruption = player.Corruption,
            RoomsExploredSinceRest = player.RoomsExploredSinceRest,
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
            LastSaved = DateTime.Now
        };

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO saves (
                character_name, class, specialization, current_milestone, current_legend, progression_points,
                might, finesse, wits, will, sturdiness,
                current_hp, max_hp, current_stamina, max_stamina,
                psychic_stress, corruption, rooms_explored_since_rest,
                vulnerable_turns, analyzed_turns, seized_turns, is_performing, performing_turns, current_performance,
                inspired_turns, silenced_turns, temp_hp,
                current_room_id, cleared_rooms_json, puzzle_solved, boss_defeated,
                equipped_weapon_json, equipped_armor_json, inventory_json, room_items_json,
                consumables_json, crafting_components_json,
                faction_reputations_json, active_quests_json, completed_quests_json, npc_states_json,
                last_saved
            ) VALUES (
                $name, $class, $spec, $milestone, $legend, $pp,
                $might, $finesse, $wits, $will, $sturdiness,
                $hp, $maxhp, $stamina, $maxstamina,
                $stress, $corruption, $roomsrest,
                $vulnturns, $analyzedturns, $seizedturns, $isperforming, $perfturns, $perfname,
                $inspiredturns, $silencedturns, $temphp,
                $roomid, $cleared, $puzzle, $boss,
                $eqweapon, $eqarmor, $inventory, $roomitems,
                $consumables, $craftingcomponents,
                $factionreps, $activequests, $completedquests, $npcstates,
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
        command.Parameters.AddWithValue("$stress", saveData.PsychicStress);
        command.Parameters.AddWithValue("$corruption", saveData.Corruption);
        command.Parameters.AddWithValue("$roomsrest", saveData.RoomsExploredSinceRest);
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
        command.Parameters.AddWithValue("$saved", saveData.LastSaved.ToString("yyyy-MM-dd HH:mm:ss"));

        command.ExecuteNonQuery();
    }

    public (PlayerCharacter?, WorldState?, string?, string?) LoadGame(string characterName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM saves WHERE character_name = $name";
        command.Parameters.AddWithValue("$name", characterName);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return (null, null, null);
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

        // Reconstruct abilities based on class and level (will be set by CharacterFactory)

        // Reconstruct WorldState
        var worldState = new WorldState
        {
            CurrentRoomId = saveData.CurrentRoomId,
            ClearedRoomIds = JsonSerializer.Deserialize<List<int>>(saveData.ClearedRoomsJson) ?? new List<int>(),
            PuzzleSolved = saveData.PuzzleSolved,
            BossDefeated = saveData.BossDefeated
        };

        // Return room items JSON and NPC states JSON for restoration (v0.3, v0.8)
        return (player, worldState, saveData.RoomItemsJson, saveData.NPCStatesJson);
    }

    /// <summary>
    /// Restore room items from save data (v0.3)
    /// </summary>
    public void RestoreRoomItems(GameWorld world, string roomItemsJson)
    {
        if (string.IsNullOrEmpty(roomItemsJson) || roomItemsJson == "{}")
        {
            return;
        }

        try
        {
            var roomItemsDict = JsonSerializer.Deserialize<Dictionary<int, List<Equipment>>>(roomItemsJson);
            if (roomItemsDict == null) return;

            foreach (var kvp in roomItemsDict)
            {
                var roomId = kvp.Key;
                var items = kvp.Value;

                // Find the room by ID
                var room = world.Rooms.Values.FirstOrDefault(r => r.Id == roomId);
                if (room != null)
                {
                    room.ItemsOnGround.Clear();
                    room.ItemsOnGround.AddRange(items);
                }
            }
        }
        catch
        {
            // Failed to restore room items - not critical, player can continue without them
        }
    }

    /// <summary>
    /// Restore NPC states from save data (v0.8)
    /// </summary>
    public void RestoreNPCStates(GameWorld world, string npcStatesJson)
    {
        if (string.IsNullOrEmpty(npcStatesJson) || npcStatesJson == "[]")
        {
            return;
        }

        try
        {
            var savedNPCs = JsonSerializer.Deserialize<List<NPC>>(npcStatesJson);
            if (savedNPCs == null) return;

            // Clear all NPCs from all rooms first
            foreach (var room in world.Rooms.Values)
            {
                room.NPCs.Clear();
            }

            // Restore NPCs to their rooms with saved state
            foreach (var npc in savedNPCs)
            {
                var room = world.Rooms.Values.FirstOrDefault(r => r.Id == npc.RoomId);
                if (room != null)
                {
                    room.NPCs.Add(npc);
                }
            }
        }
        catch
        {
            // Failed to restore NPC states - not critical, but NPCs will reset to default state
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
