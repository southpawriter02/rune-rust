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
        var alterCommands = new[]
        {
            "ALTER TABLE saves ADD COLUMN equipped_weapon_json TEXT",
            "ALTER TABLE saves ADD COLUMN equipped_armor_json TEXT",
            "ALTER TABLE saves ADD COLUMN inventory_json TEXT DEFAULT '[]'",
            "ALTER TABLE saves ADD COLUMN room_items_json TEXT DEFAULT '{}'"
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

        var saveData = new SaveData
        {
            CharacterName = player.Name,
            Class = player.Class,
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
            CurrentRoomId = worldState.CurrentRoomId,
            ClearedRoomsJson = JsonSerializer.Serialize(worldState.ClearedRoomIds),
            PuzzleSolved = worldState.PuzzleSolved,
            BossDefeated = worldState.BossDefeated,
            EquippedWeaponJson = equippedWeaponJson,
            EquippedArmorJson = equippedArmorJson,
            InventoryJson = inventoryJson,
            RoomItemsJson = roomItemsJson,
            LastSaved = DateTime.Now
        };

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO saves (
                character_name, class, current_milestone, current_legend, progression_points,
                might, finesse, wits, will, sturdiness,
                current_hp, max_hp, current_stamina, max_stamina,
                current_room_id, cleared_rooms_json, puzzle_solved, boss_defeated,
                equipped_weapon_json, equipped_armor_json, inventory_json, room_items_json,
                last_saved
            ) VALUES (
                $name, $class, $milestone, $legend, $pp,
                $might, $finesse, $wits, $will, $sturdiness,
                $hp, $maxhp, $stamina, $maxstamina,
                $roomid, $cleared, $puzzle, $boss,
                $eqweapon, $eqarmor, $inventory, $roomitems,
                $saved
            )
        ";

        command.Parameters.AddWithValue("$name", saveData.CharacterName);
        command.Parameters.AddWithValue("$class", saveData.Class.ToString());
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
        command.Parameters.AddWithValue("$roomid", saveData.CurrentRoomId);
        command.Parameters.AddWithValue("$cleared", saveData.ClearedRoomsJson);
        command.Parameters.AddWithValue("$puzzle", saveData.PuzzleSolved ? 1 : 0);
        command.Parameters.AddWithValue("$boss", saveData.BossDefeated ? 1 : 0);
        command.Parameters.AddWithValue("$eqweapon", (object?)saveData.EquippedWeaponJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$eqarmor", (object?)saveData.EquippedArmorJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$inventory", saveData.InventoryJson);
        command.Parameters.AddWithValue("$roomitems", saveData.RoomItemsJson);
        command.Parameters.AddWithValue("$saved", saveData.LastSaved.ToString("yyyy-MM-dd HH:mm:ss"));

        command.ExecuteNonQuery();
    }

    public (PlayerCharacter?, WorldState?, string?) LoadGame(string characterName)
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

        // Reconstruct PlayerCharacter
        var player = new PlayerCharacter
        {
            Name = saveData.CharacterName,
            Class = saveData.Class,
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

        // Reconstruct abilities based on class and level (will be set by CharacterFactory)

        // Reconstruct WorldState
        var worldState = new WorldState
        {
            CurrentRoomId = saveData.CurrentRoomId,
            ClearedRoomIds = JsonSerializer.Deserialize<List<int>>(saveData.ClearedRoomsJson) ?? new List<int>(),
            PuzzleSolved = saveData.PuzzleSolved,
            BossDefeated = saveData.BossDefeated
        };

        // Return room items JSON for restoration (v0.3)
        return (player, worldState, saveData.RoomItemsJson);
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
