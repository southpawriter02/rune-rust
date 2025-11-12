using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.13: Repository for managing persistent world state changes.
/// Implements delta-based storage for efficient room state reconstruction.
/// </summary>
public class WorldStateRepository
{
    private static readonly ILogger _log = Log.ForContext<WorldStateRepository>();
    private readonly string _connectionString;
    private const string DatabaseName = "runeandrust.db";

    public WorldStateRepository(string? dataDirectory = null)
    {
        var dbPath = Path.Combine(dataDirectory ?? Environment.CurrentDirectory, DatabaseName);
        _connectionString = $"Data Source={dbPath}";

        _log.Debug("WorldStateRepository initialized with database path: {DbPath}", dbPath);

        InitializeWorldStateTable();
    }

    /// <summary>
    /// Create world_state_changes table if it doesn't exist
    /// </summary>
    private void InitializeWorldStateTable()
    {
        _log.Debug("Initializing world_state_changes table");

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS world_state_changes (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    save_id INTEGER NOT NULL,
                    sector_seed TEXT NOT NULL,
                    room_id TEXT NOT NULL,
                    change_type TEXT NOT NULL,
                    target_id TEXT NOT NULL,
                    change_data TEXT NOT NULL,
                    timestamp TEXT NOT NULL,
                    turn_number INTEGER NOT NULL,
                    schema_version INTEGER NOT NULL DEFAULT 1,

                    FOREIGN KEY (save_id) REFERENCES saves(id) ON DELETE CASCADE
                )
            ";
            createTableCommand.ExecuteNonQuery();

            // Create indexes for efficient querying
            var createIndexCommands = new[]
            {
                "CREATE INDEX IF NOT EXISTS idx_world_state_save_sector ON world_state_changes(save_id, sector_seed)",
                "CREATE INDEX IF NOT EXISTS idx_world_state_save_room ON world_state_changes(save_id, room_id)",
                "CREATE INDEX IF NOT EXISTS idx_world_state_timestamp ON world_state_changes(timestamp)"
            };

            foreach (var indexSql in createIndexCommands)
            {
                var indexCommand = connection.CreateCommand();
                indexCommand.CommandText = indexSql;
                indexCommand.ExecuteNonQuery();
            }

            _log.Information("World state changes table initialized successfully");
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to initialize world_state_changes table");
            throw;
        }
    }

    /// <summary>
    /// Record a world state change to the database
    /// </summary>
    public void RecordChange(WorldStateChange change)
    {
        if (change == null)
        {
            _log.Warning("Attempted to record null WorldStateChange");
            return;
        }

        _log.Debug("Recording world state change: SaveId={SaveId}, RoomId={RoomId}, Type={ChangeType}, Target={TargetId}",
            change.SaveId, change.RoomId, change.ChangeType, change.TargetId);

        var startTime = DateTime.Now;

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO world_state_changes (
                    save_id, sector_seed, room_id, change_type, target_id, change_data,
                    timestamp, turn_number, schema_version
                ) VALUES (
                    $saveId, $sectorSeed, $roomId, $changeType, $targetId, $changeData,
                    $timestamp, $turnNumber, $schemaVersion
                )
            ";

            command.Parameters.AddWithValue("$saveId", change.SaveId);
            command.Parameters.AddWithValue("$sectorSeed", change.SectorSeed);
            command.Parameters.AddWithValue("$roomId", change.RoomId);
            command.Parameters.AddWithValue("$changeType", change.ChangeType.ToString());
            command.Parameters.AddWithValue("$targetId", change.TargetId);
            command.Parameters.AddWithValue("$changeData", change.ChangeData);
            command.Parameters.AddWithValue("$timestamp", change.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            command.Parameters.AddWithValue("$turnNumber", change.TurnNumber);
            command.Parameters.AddWithValue("$schemaVersion", change.SchemaVersion);

            command.ExecuteNonQuery();

            var duration = (DateTime.Now - startTime).TotalMilliseconds;
            _log.Information("World state change recorded: Type={ChangeType}, Target={TargetId}, Room={RoomId}, Duration={Duration}ms",
                change.ChangeType, change.TargetId, change.RoomId, duration);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to record world state change: RoomId={RoomId}, Type={ChangeType}",
                change.RoomId, change.ChangeType);
            throw;
        }
    }

    /// <summary>
    /// Get all world state changes for a specific save (character)
    /// </summary>
    public List<WorldStateChange> GetChangesForSave(int saveId)
    {
        _log.Debug("Retrieving all world state changes for save: SaveId={SaveId}", saveId);

        var startTime = DateTime.Now;
        var changes = new List<WorldStateChange>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, save_id, sector_seed, room_id, change_type, target_id, change_data,
                       timestamp, turn_number, schema_version
                FROM world_state_changes
                WHERE save_id = $saveId
                ORDER BY timestamp ASC
            ";
            command.Parameters.AddWithValue("$saveId", saveId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                changes.Add(new WorldStateChange
                {
                    Id = reader.GetInt32(0),
                    SaveId = reader.GetInt32(1),
                    SectorSeed = reader.GetString(2),
                    RoomId = reader.GetString(3),
                    ChangeType = Enum.Parse<WorldStateChangeType>(reader.GetString(4)),
                    TargetId = reader.GetString(5),
                    ChangeData = reader.GetString(6),
                    Timestamp = DateTime.Parse(reader.GetString(7)),
                    TurnNumber = reader.GetInt32(8),
                    SchemaVersion = reader.GetInt32(9)
                });
            }

            var duration = (DateTime.Now - startTime).TotalMilliseconds;
            _log.Information("Retrieved world state changes for save: SaveId={SaveId}, ChangeCount={Count}, Duration={Duration}ms",
                saveId, changes.Count, duration);

            return changes;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to retrieve world state changes for save: SaveId={SaveId}", saveId);
            throw;
        }
    }

    /// <summary>
    /// Get world state changes for a specific room in a save
    /// </summary>
    public List<WorldStateChange> GetChangesForRoom(int saveId, string sectorSeed, string roomId)
    {
        _log.Debug("Retrieving world state changes: SaveId={SaveId}, SectorSeed={SectorSeed}, RoomId={RoomId}",
            saveId, sectorSeed, roomId);

        var startTime = DateTime.Now;
        var changes = new List<WorldStateChange>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, save_id, sector_seed, room_id, change_type, target_id, change_data,
                       timestamp, turn_number, schema_version
                FROM world_state_changes
                WHERE save_id = $saveId AND sector_seed = $sectorSeed AND room_id = $roomId
                ORDER BY timestamp ASC
            ";
            command.Parameters.AddWithValue("$saveId", saveId);
            command.Parameters.AddWithValue("$sectorSeed", sectorSeed);
            command.Parameters.AddWithValue("$roomId", roomId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                changes.Add(new WorldStateChange
                {
                    Id = reader.GetInt32(0),
                    SaveId = reader.GetInt32(1),
                    SectorSeed = reader.GetString(2),
                    RoomId = reader.GetString(3),
                    ChangeType = Enum.Parse<WorldStateChangeType>(reader.GetString(4)),
                    TargetId = reader.GetString(5),
                    ChangeData = reader.GetString(6),
                    Timestamp = DateTime.Parse(reader.GetString(7)),
                    TurnNumber = reader.GetInt32(8),
                    SchemaVersion = reader.GetInt32(9)
                });
            }

            var duration = (DateTime.Now - startTime).TotalMilliseconds;
            _log.Debug("Retrieved room changes: SaveId={SaveId}, RoomId={RoomId}, Count={Count}, Duration={Duration}ms",
                saveId, roomId, changes.Count, duration);

            return changes;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to retrieve room changes: SaveId={SaveId}, RoomId={RoomId}",
                saveId, roomId);
            throw;
        }
    }

    /// <summary>
    /// Get world state changes for a specific sector
    /// </summary>
    public List<WorldStateChange> GetChangesForSector(int saveId, string sectorSeed)
    {
        _log.Debug("Retrieving world state changes for sector: SaveId={SaveId}, SectorSeed={SectorSeed}",
            saveId, sectorSeed);

        var startTime = DateTime.Now;
        var changes = new List<WorldStateChange>();

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT id, save_id, sector_seed, room_id, change_type, target_id, change_data,
                       timestamp, turn_number, schema_version
                FROM world_state_changes
                WHERE save_id = $saveId AND sector_seed = $sectorSeed
                ORDER BY timestamp ASC
            ";
            command.Parameters.AddWithValue("$saveId", saveId);
            command.Parameters.AddWithValue("$sectorSeed", sectorSeed);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                changes.Add(new WorldStateChange
                {
                    Id = reader.GetInt32(0),
                    SaveId = reader.GetInt32(1),
                    SectorSeed = reader.GetString(2),
                    RoomId = reader.GetString(3),
                    ChangeType = Enum.Parse<WorldStateChangeType>(reader.GetString(4)),
                    TargetId = reader.GetString(5),
                    ChangeData = reader.GetString(6),
                    Timestamp = DateTime.Parse(reader.GetString(7)),
                    TurnNumber = reader.GetInt32(8),
                    SchemaVersion = reader.GetInt32(9)
                });
            }

            var duration = (DateTime.Now - startTime).TotalMilliseconds;
            _log.Information("Retrieved sector changes: SaveId={SaveId}, SectorSeed={SectorSeed}, Count={Count}, Duration={Duration}ms",
                saveId, sectorSeed, changes.Count, duration);

            return changes;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to retrieve sector changes: SaveId={SaveId}, SectorSeed={SectorSeed}",
                saveId, sectorSeed);
            throw;
        }
    }

    /// <summary>
    /// Get the save ID for a character name (helper method)
    /// </summary>
    public int? GetSaveIdForCharacter(string characterName)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT id FROM saves WHERE character_name = $name";
            command.Parameters.AddWithValue("$name", characterName);

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : null;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get save ID for character: {CharacterName}", characterName);
            return null;
        }
    }

    /// <summary>
    /// Delete all world state changes for a save (when save is deleted)
    /// </summary>
    public void DeleteChangesForSave(int saveId)
    {
        _log.Information("Deleting all world state changes for save: SaveId={SaveId}", saveId);

        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM world_state_changes WHERE save_id = $saveId";
            command.Parameters.AddWithValue("$saveId", saveId);

            int deletedCount = command.ExecuteNonQuery();

            _log.Information("Deleted world state changes: SaveId={SaveId}, Count={Count}",
                saveId, deletedCount);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to delete world state changes for save: SaveId={SaveId}", saveId);
            throw;
        }
    }

    /// <summary>
    /// Get count of changes for a room (for performance monitoring)
    /// </summary>
    public int GetChangeCountForRoom(int saveId, string roomId)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*)
                FROM world_state_changes
                WHERE save_id = $saveId AND room_id = $roomId
            ";
            command.Parameters.AddWithValue("$saveId", saveId);
            command.Parameters.AddWithValue("$roomId", roomId);

            var count = (long)command.ExecuteScalar()!;
            return (int)count;
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to get change count for room: SaveId={SaveId}, RoomId={RoomId}",
                saveId, roomId);
            return 0;
        }
    }
}
