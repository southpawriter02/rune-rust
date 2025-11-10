using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
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
                level INTEGER NOT NULL,
                current_xp INTEGER NOT NULL,
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
                last_saved TEXT NOT NULL
            )
        ";
        createTableCommand.ExecuteNonQuery();
    }

    public void SaveGame(PlayerCharacter player, WorldState worldState)
    {
        var saveData = new SaveData
        {
            CharacterName = player.Name,
            Class = player.Class,
            Level = player.Level,
            CurrentXP = player.CurrentXP,
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
            LastSaved = DateTime.Now
        };

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO saves (
                character_name, class, level, current_xp,
                might, finesse, wits, will, sturdiness,
                current_hp, max_hp, current_stamina, max_stamina,
                current_room_id, cleared_rooms_json, puzzle_solved, boss_defeated,
                last_saved
            ) VALUES (
                $name, $class, $level, $xp,
                $might, $finesse, $wits, $will, $sturdiness,
                $hp, $maxhp, $stamina, $maxstamina,
                $roomid, $cleared, $puzzle, $boss,
                $saved
            )
        ";

        command.Parameters.AddWithValue("$name", saveData.CharacterName);
        command.Parameters.AddWithValue("$class", saveData.Class.ToString());
        command.Parameters.AddWithValue("$level", saveData.Level);
        command.Parameters.AddWithValue("$xp", saveData.CurrentXP);
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
        command.Parameters.AddWithValue("$saved", saveData.LastSaved.ToString("yyyy-MM-dd HH:mm:ss"));

        command.ExecuteNonQuery();
    }

    public (PlayerCharacter?, WorldState?) LoadGame(string characterName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM saves WHERE character_name = $name";
        command.Parameters.AddWithValue("$name", characterName);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
        {
            return (null, null);
        }

        var saveData = new SaveData
        {
            CharacterName = reader.GetString(reader.GetOrdinal("character_name")),
            Class = Enum.Parse<CharacterClass>(reader.GetString(reader.GetOrdinal("class"))),
            Level = reader.GetInt32(reader.GetOrdinal("level")),
            CurrentXP = reader.GetInt32(reader.GetOrdinal("current_xp")),
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

        // Reconstruct PlayerCharacter
        var player = new PlayerCharacter
        {
            Name = saveData.CharacterName,
            Class = saveData.Class,
            Level = saveData.Level,
            CurrentXP = saveData.CurrentXP,
            XPToNextLevel = CalculateXPToNextLevel(saveData.Level),
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

        // Reconstruct abilities based on class and level (will be set by CharacterFactory)

        // Reconstruct WorldState
        var worldState = new WorldState
        {
            CurrentRoomId = saveData.CurrentRoomId,
            ClearedRoomIds = JsonSerializer.Deserialize<List<int>>(saveData.ClearedRoomsJson) ?? new List<int>(),
            PuzzleSolved = saveData.PuzzleSolved,
            BossDefeated = saveData.BossDefeated
        };

        return (player, worldState);
    }

    public List<SaveInfo> ListSaves()
    {
        var saves = new List<SaveInfo>();

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT character_name, class, level, boss_defeated, last_saved FROM saves ORDER BY last_saved DESC";

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            saves.Add(new SaveInfo
            {
                CharacterName = reader.GetString(0),
                Class = Enum.Parse<CharacterClass>(reader.GetString(1)),
                Level = reader.GetInt32(2),
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

    private int CalculateXPToNextLevel(int level)
    {
        return level switch
        {
            1 => 50,
            2 => 100,
            3 => 150,
            4 => 200,
            _ => 0
        };
    }
}
