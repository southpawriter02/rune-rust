using Microsoft.Data.Sqlite;
using RuneAndRust.Core;
using System.Text.Json;
using Serilog;

namespace RuneAndRust.Persistence;

/// <summary>
/// v0.41: Repository for managing alternative starting scenarios
/// </summary>
public class AlternativeStartRepository
{
    private static readonly ILogger _log = Log.ForContext<AlternativeStartRepository>();
    private readonly string _connectionString;

    public AlternativeStartRepository(string connectionString)
    {
        _connectionString = connectionString;
        _log.Debug("AlternativeStartRepository initialized");
        InitializeTables();
    }

    #region Table Initialization

    private void InitializeTables()
    {
        _log.Debug("Initializing alternative start tables");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        // Alternative_Starts table (scenario definitions)
        var createStartsTable = connection.CreateCommand();
        createStartsTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Alternative_Starts (
                start_id TEXT PRIMARY KEY,
                name TEXT NOT NULL,
                description TEXT NOT NULL,
                flavor_text TEXT NOT NULL,
                requirement_description TEXT NOT NULL,
                starting_level INTEGER DEFAULT 1,
                starting_equipment_json TEXT DEFAULT '[]',
                unlocked_specializations_json TEXT DEFAULT '[]',
                starting_legend INTEGER DEFAULT 0,
                starting_resources_json TEXT DEFAULT '{}',
                starting_sector_id INTEGER,
                completed_quests_json TEXT DEFAULT '[]',
                skip_tutorial INTEGER DEFAULT 0,
                hard_mode_enabled INTEGER DEFAULT 0,
                permadeath_enabled INTEGER DEFAULT 0,
                reward_multiplier REAL DEFAULT 1.0,
                timer_visible INTEGER DEFAULT 0
            )
        ";
        createStartsTable.ExecuteNonQuery();

        // Alternative_Start_Progress table (unlock progress per account)
        var createProgressTable = connection.CreateCommand();
        createProgressTable.CommandText = @"
            CREATE TABLE IF NOT EXISTS Alternative_Start_Progress (
                progress_id INTEGER PRIMARY KEY AUTOINCREMENT,
                account_id INTEGER NOT NULL,
                start_id TEXT NOT NULL,
                is_unlocked INTEGER DEFAULT 0,
                unlocked_at TEXT,
                FOREIGN KEY (account_id) REFERENCES Account_Progression(account_id),
                FOREIGN KEY (start_id) REFERENCES Alternative_Starts(start_id),
                UNIQUE (account_id, start_id)
            )
        ";
        createProgressTable.ExecuteNonQuery();

        _log.Information("Alternative start tables initialized");
    }

    #endregion

    #region Alternative Start CRUD

    /// <summary>
    /// Get alternative start by ID
    /// </summary>
    public AlternativeStart? GetById(string startId)
    {
        _log.Debug("Getting alternative start: StartID={StartId}", startId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT * FROM Alternative_Starts
            WHERE start_id = @StartId
        ";
        command.Parameters.AddWithValue("@StartId", startId);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapAlternativeStartFromReader(reader);
        }

        _log.Warning("Alternative start not found: {StartId}", startId);
        return null;
    }

    /// <summary>
    /// Get all alternative starts
    /// </summary>
    public List<AlternativeStart> GetAll()
    {
        _log.Debug("Getting all alternative starts");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Alternative_Starts ORDER BY starting_level, name";

        var starts = new List<AlternativeStart>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            starts.Add(MapAlternativeStartFromReader(reader));
        }

        _log.Information("Retrieved {Count} alternative starts", starts.Count);
        return starts;
    }

    /// <summary>
    /// Get unlocked alternative starts for account
    /// </summary>
    public List<AlternativeStart> GetUnlockedStarts(int accountId)
    {
        _log.Debug("Getting unlocked alternative starts: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT s.*
            FROM Alternative_Starts s
            INNER JOIN Alternative_Start_Progress p
                ON s.start_id = p.start_id
            WHERE p.account_id = @AccountId AND p.is_unlocked = 1
            ORDER BY s.starting_level, s.name
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        var starts = new List<AlternativeStart>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var start = MapAlternativeStartFromReader(reader);
            start.IsUnlocked = true;
            starts.Add(start);
        }

        _log.Information("Retrieved {Count} unlocked alternative starts for account {AccountId}",
            starts.Count, accountId);
        return starts;
    }

    /// <summary>
    /// Get all alternative starts with unlock status for account
    /// </summary>
    public List<AlternativeStart> GetAllWithUnlockStatus(int accountId)
    {
        _log.Debug("Getting all alternative starts with unlock status: AccountID={AccountId}", accountId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                s.*,
                COALESCE(p.is_unlocked, 0) as is_unlocked,
                p.unlocked_at
            FROM Alternative_Starts s
            LEFT JOIN Alternative_Start_Progress p
                ON s.start_id = p.start_id AND p.account_id = @AccountId
            ORDER BY s.starting_level, s.name
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);

        var starts = new List<AlternativeStart>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var start = MapAlternativeStartFromReader(reader);
            start.IsUnlocked = reader.GetInt32(reader.GetOrdinal("is_unlocked")) == 1;
            starts.Add(start);
        }

        _log.Information("Retrieved {Count} alternative starts with unlock status", starts.Count);
        return starts;
    }

    #endregion

    #region Alternative Start Progress

    /// <summary>
    /// Check if alternative start is unlocked
    /// </summary>
    public bool IsUnlocked(int accountId, string startId)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT is_unlocked
            FROM Alternative_Start_Progress
            WHERE account_id = @AccountId AND start_id = @StartId
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@StartId", startId);

        var result = command.ExecuteScalar();
        return result != null && Convert.ToInt32(result) == 1;
    }

    /// <summary>
    /// Unlock alternative start for account
    /// </summary>
    public void UnlockStart(int accountId, string startId)
    {
        _log.Information("Unlocking alternative start: AccountID={AccountId}, StartID={StartId}",
            accountId, startId);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT OR REPLACE INTO Alternative_Start_Progress
                (account_id, start_id, is_unlocked, unlocked_at)
            VALUES (@AccountId, @StartId, 1, @UnlockedAt)
        ";
        command.Parameters.AddWithValue("@AccountId", accountId);
        command.Parameters.AddWithValue("@StartId", startId);
        command.Parameters.AddWithValue("@UnlockedAt", DateTime.UtcNow.ToString("O"));

        command.ExecuteNonQuery();

        _log.Information("Alternative start unlocked successfully");
    }

    #endregion

    #region Mapping Helpers

    private AlternativeStart MapAlternativeStartFromReader(SqliteDataReader reader)
    {
        var equipmentJson = reader.GetString(reader.GetOrdinal("starting_equipment_json"));
        var specsJson = reader.GetString(reader.GetOrdinal("unlocked_specializations_json"));
        var resourcesJson = reader.GetString(reader.GetOrdinal("starting_resources_json"));
        var questsJson = reader.GetString(reader.GetOrdinal("completed_quests_json"));

        var equipment = JsonSerializer.Deserialize<List<string>>(equipmentJson) ?? new List<string>();
        var specs = JsonSerializer.Deserialize<List<string>>(specsJson) ?? new List<string>();
        var resources = JsonSerializer.Deserialize<Dictionary<string, int>>(resourcesJson) ?? new Dictionary<string, int>();
        var quests = JsonSerializer.Deserialize<List<string>>(questsJson) ?? new List<string>();

        int? startingSectorId = null;
        if (!reader.IsDBNull(reader.GetOrdinal("starting_sector_id")))
        {
            startingSectorId = reader.GetInt32(reader.GetOrdinal("starting_sector_id"));
        }

        return new AlternativeStart
        {
            StartId = reader.GetString(reader.GetOrdinal("start_id")),
            Name = reader.GetString(reader.GetOrdinal("name")),
            Description = reader.GetString(reader.GetOrdinal("description")),
            FlavorText = reader.GetString(reader.GetOrdinal("flavor_text")),
            RequirementDescription = reader.GetString(reader.GetOrdinal("requirement_description")),
            StartingLevel = reader.GetInt32(reader.GetOrdinal("starting_level")),
            StartingEquipment = equipment,
            UnlockedSpecializations = specs,
            StartingLegend = reader.GetInt32(reader.GetOrdinal("starting_legend")),
            StartingResources = resources,
            StartingSectorId = startingSectorId,
            CompletedQuests = quests,
            SkipTutorial = reader.GetInt32(reader.GetOrdinal("skip_tutorial")) == 1,
            HardModeEnabled = reader.GetInt32(reader.GetOrdinal("hard_mode_enabled")) == 1,
            PermadeathEnabled = reader.GetInt32(reader.GetOrdinal("permadeath_enabled")) == 1,
            RewardMultiplier = (float)reader.GetDouble(reader.GetOrdinal("reward_multiplier")),
            TimerVisible = reader.GetInt32(reader.GetOrdinal("timer_visible")) == 1
        };
    }

    #endregion
}
